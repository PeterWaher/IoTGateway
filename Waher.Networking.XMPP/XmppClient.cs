#define LineListener

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Threading;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Connection error event handler.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Exception">Information about error received.</param>
	public delegate void XmppExceptionEventHandler(XmppClient Sender, Exception Exception);

	/// <summary>
	/// Event handler for state change events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewState">New state reported.</param>
	public delegate void StateChangedEventHandler(XmppClient Sender, XmppState NewState);


	/// <summary>
	/// Manages an XMPP client connection. Implements XMPP, as defined in
	/// https://tools.ietf.org/html/rfc6120
	/// https://tools.ietf.org/html/rfc6121
	/// https://tools.ietf.org/html/rfc6122
	/// </summary>
	public class XmppClient : IDisposable
	{
		private const int BufferSize = 16384;
		private const int KeepAliveTimeSeconds = 30;

		private LinkedList<byte[]> outputQueue = new LinkedList<byte[]>();
		private byte[] buffer = new byte[BufferSize];
		private TcpClient client = null;
		private Stream stream = null;
		private Timer secondTimer = null;
		private DateTime nextPing = DateTime.MinValue;
		private UTF8Encoding encoding = new UTF8Encoding(false, false);
		private StringBuilder fragment = new StringBuilder();
		private XmppState state;
		private string host;
		private string language;
		private string baseJid;
		private string fullJid;
		private string userName;
		private string password;
		private int port;
		private int keepAliveSeconds;
		private int inputState = 0;
		private int inputDepth = 0;
		private bool trustServer = false;
		private bool isWriting = false;

		/// <summary>
		/// Manages an XMPP client connection. Implements XMPP, as defined in
		/// https://tools.ietf.org/html/rfc6120
		/// https://tools.ietf.org/html/rfc6121
		/// https://tools.ietf.org/html/rfc6122
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="Tls">If TLS is used to encrypt communication.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		public XmppClient(string Host, int Port, string UserName, string Password, string Language)
		{
			this.host = Host;
			this.port = Port;
			this.userName = UserName;
			this.password = Password;
			this.language = Language;
			this.state = XmppState.Connecting;
			this.client = new TcpClient();
			this.client.BeginConnect(Host, Port, this.ConnectCallback, null);
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				this.client.EndConnect(ar);
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
				return;
			}

			this.stream = new NetworkStream(this.client.Client, false);

			this.baseJid = this.userName + "@" + this.host;
			this.BeginWrite("<?xml version='1.0'?><stream:stream from='" + this.baseJid + "' to='" + this.host +
				"' version='1.0' xml:lang='" + this.language + "' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>");

			this.inputState = 0;
			this.BeginRead();
		}

		private void ConnectionError(Exception ex)
		{
			XmppExceptionEventHandler h = this.OnConnectionError;
			if (h != null)
			{
				try
				{
					h(this, ex);
				}
				catch (Exception ex2)
				{
#if LineListener
					Console.Out.WriteLine("Ex: " + ex2.Message);
#endif
				}
			}

			this.Error(ex);

			this.State = XmppState.Error;
		}

		private void Error(Exception ex)
		{
			XmppExceptionEventHandler h = this.OnError;
			if (h != null)
			{
				try
				{
					h(this, ex);
				}
				catch (Exception ex2)
				{
#if LineListener
					Console.Out.WriteLine("Ex: " + ex2.Message);
#endif
				}
			}
		}

		/// <summary>
		/// Event raised when a connection to a broker could not be made.
		/// </summary>
		public event XmppExceptionEventHandler OnConnectionError = null;

		/// <summary>
		/// Event raised when an error was encountered.
		/// </summary>
		public event XmppExceptionEventHandler OnError = null;

		/// <summary>
		/// Host or IP address of XMPP server.
		/// </summary>
		public string Host
		{
			get { return this.host; }
		}

		/// <summary>
		/// Port number to connect to.
		/// </summary>
		public int Port
		{
			get { return this.port; }
		}

		/// <summary>
		/// If server should be trusted, regardless if the operating system could validate its certificate or not.
		/// </summary>
		public bool TrustServer
		{
			get { return this.trustServer; }
			set { this.trustServer = value; }
		}

		/// <summary>
		/// Current state of connection.
		/// </summary>
		public XmppState State
		{
			get { return this.state; }
			internal set
			{
				if (this.state != value)
				{
					this.state = value;

					StateChangedEventHandler h = this.OnStateChanged;
					if (h != null)
					{
						try
						{
							h(this, value);
						}
						catch (Exception ex)
						{
#if LineListener
							Console.Out.WriteLine("Ex: " + ex.Message);
#endif
						}
					}
				}
			}
		}

		/// <summary>
		/// Event raised whenever the internal state of the connection changes.
		/// </summary>
		public event StateChangedEventHandler OnStateChanged = null;

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		public void Dispose()
		{
			/*if (this.state == XmppState.Connected)
				this.DISCONNECT();*/

			if (this.outputQueue != null)
			{
				lock (this.outputQueue)
				{
					this.outputQueue.Clear();
				}
			}

			/*if (this.contentCache != null)
			{
				lock (this.contentCache)
				{
					this.contentCache.Clear();
				}
			}*/

			if (this.secondTimer != null)
			{
				this.secondTimer.Dispose();
				this.secondTimer = null;
			}

			if (this.stream != null)
			{
				this.stream.Dispose();
				this.stream = null;
			}

			if (this.client != null)
			{
				this.client.Close();
				this.client = null;
			}
		}

		private void BeginWrite(string Xml)
		{
#if LineListener
			Console.Out.WriteLine("Tx: " + Xml);
#endif
			byte[] Packet = this.encoding.GetBytes(Xml);

			lock (this.outputQueue)
			{
				if (this.isWriting)
					this.outputQueue.AddLast(Packet);
				else
					this.DoBeginWriteLocked(Packet);
			}
		}

		private void DoBeginWriteLocked(byte[] Packet)
		{
			this.stream.BeginWrite(Packet, 0, Packet.Length, this.EndWrite, null);
			this.isWriting = true;
		}

		private void EndWrite(IAsyncResult ar)
		{
			if (this.stream == null)
				return;

			try
			{
				this.stream.EndWrite(ar);

				this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);

				lock (this.outputQueue)
				{
					LinkedListNode<byte[]> Next = this.outputQueue.First;

					if (Next == null)
						this.isWriting = false;
					else
					{
						this.outputQueue.RemoveFirst();
						this.DoBeginWriteLocked(Next.Value);
					}
				}
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);

				lock (this.outputQueue)
				{
					this.outputQueue.Clear();
					this.isWriting = false;
				}
			}
		}

		private void BeginRead()
		{
			this.stream.BeginRead(this.buffer, 0, BufferSize, this.EndRead, null);
		}

		private void EndRead(IAsyncResult ar)
		{
			string s;
			int NrRead;

			if (this.stream == null)
				return;

			try
			{
				NrRead = this.stream.EndRead(ar);
				if (NrRead > 0)
				{
					s = this.encoding.GetString(this.buffer, 0, NrRead);
					this.stream.BeginRead(this.buffer, 0, BufferSize, this.EndRead, null);
				}
				else
					return;
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
				return;
			}

#if LineListener
			Console.Out.WriteLine("Rx: " + s);
#endif

			foreach (char ch in s)
			{
				switch (this.inputState)
				{
					case 0:		// Waiting for <?
						if (ch == '<')
						{
							this.fragment.Append(ch);
							this.inputState++;
						}
						else if (ch > ' ')
						{
							this.inputState = -1;
							if (this.stream != null)
							{
								this.stream.Close();
								this.client.Close();
							}
							this.State = XmppState.Error;
							return;
						}
						break;

					case 1:		// Waiting for ? or >
						this.fragment.Append(ch);
						if (ch == '?')
							this.inputState++;
						else if (ch == '>')
						{
							this.inputState = 5;
							this.inputDepth = 1;
							this.ProcessStream(this.fragment.ToString() + "</stream>");
							this.fragment.Clear();
						}
						break;

					case 2:		// Waiting for ?>
						this.fragment.Append(ch);
						if (ch == '>')
							this.inputState++;
						break;

					case 3:		// Waiting for <stream
						this.fragment.Append(ch);
						if (ch == '<')
							this.inputState++;
						else if (ch > ' ')
						{
							this.inputState = -1;
							if (this.stream != null)
							{
								this.stream.Close();
								this.client.Close();
							}
							this.State = XmppState.Error;
							return;
						}
						break;

					case 4:		// Waiting for >
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputState++;
							this.inputDepth = 1;
							this.ProcessStream(this.fragment.ToString() + "</stream>");
							this.fragment.Clear();
						}
						break;

					case 5:	// Waiting for <
						if (ch == '<')
						{
							this.fragment.Append(ch);
							this.inputState++;
						}
							
						else if (this.inputDepth > 1)
							this.fragment.Append(ch);
						else if (ch > ' ')
						{
							this.inputState = -1;
							if (this.stream != null)
							{
								this.stream.Close();
								this.client.Close();
							}
							this.State = XmppState.Error;
							return;
						}
						break;

					case 6:	// Second character in tag
						this.fragment.Append(ch);
						if (ch == '/')
							this.inputState++;
						else
							this.inputState += 2;
						break;

					case 7:	// Waiting for end of closing tag
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth--;
							if (this.inputDepth < 1)
							{
								this.inputState = -1;
								if (this.stream != null)
								{
									this.stream.Close();
									this.client.Close();
								}
								this.State = XmppState.Offline;
								return;
							}
							else
							{
								if (this.inputDepth == 1)
								{
									this.ProcessFragment(this.fragment.ToString());
									this.fragment.Clear();
								}

								this.inputState = 5;
							}
						}
						break;

					case 8:	// Wait for end of start tag
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 5;
						}
						else if (ch == '/')
							this.inputState++;
						break;

					case 9:	// Check for end of childless tag.
						this.fragment.Append(ch);
						if (ch == '>')
						{
							if (this.inputDepth == 1)
							{
								this.ProcessFragment(this.fragment.ToString());
								this.fragment.Clear();
							}

							this.inputState = 5;
						}
						else
							this.inputState--;
						break;

					default:
						break;
				}
			}
		}

		private void ProcessStream(string Xml)
		{
			Console.Out.WriteLine("<stream> element received: " + Xml);
		}

		private void ProcessFragment(string Xml)
		{
			Console.Out.WriteLine("Fragment received: " + Xml);
		}

	}
}
