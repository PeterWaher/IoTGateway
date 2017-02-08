using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.PeerToPeer;
using Waher.Networking.Sniffers;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Peer connection state.
	/// </summary>
	public class PeerState : ITextTransportLayer
	{
		private UTF8Encoding encoding = new UTF8Encoding(false, false);
		private StringBuilder fragment = new StringBuilder();
		private XmppState state = XmppState.StreamNegotiation;
		private PeerConnection peer;
		private XmppServerlessMessaging parent;
		private XmppClient xmppClient;
		private LinkedList<KeyValuePair<PeerConnectionEventHandler, object>> callbacks = null;
		private int inputState = 0;
		private int inputDepth = 0;
		private string parentBareJid;
		private string streamHeader;
		private string streamFooter;
		private string streamId;
		private double version;
		private string remoteBareJid;
		private bool headerSent = false;

		/// <summary>
		/// Event raised when a text packet has been sent.
		/// </summary>
		public event TextEventHandler OnSent = null;

		/// <summary>
		/// Event raised when a text packet (XML fragment) has been received.
		/// </summary>
		public event TextEventHandler OnReceived = null;

		public PeerState(PeerConnection Peer, XmppServerlessMessaging Parent)
		{
			this.parent = Parent;
			this.peer = Peer;
			this.parentBareJid = Parent.BareJid;

			this.Init();
		}

		public PeerState(PeerConnection Peer, XmppServerlessMessaging Parent, string RemoteJID, string StreamHeader, string StreamFooter,
			string StreamId, double Version, PeerConnectionEventHandler Callback, object State)
		{
			this.parent = Parent;
			this.peer = Peer;
			this.remoteBareJid = RemoteJID;
			this.streamHeader = StreamHeader;
			this.streamFooter = StreamFooter;
			this.streamId = StreamId;
			this.version = Version;

			this.callbacks = new LinkedList<KeyValuePair<PeerConnectionEventHandler, object>>();
			this.callbacks.AddLast(new KeyValuePair<PeerConnectionEventHandler, object>(Callback, State));

			if (this.peer != null)
				this.Init();
		}

		public void AddCallback(PeerConnectionEventHandler Callback, object State)
		{
			if (this.callbacks == null)
				this.callbacks = new LinkedList<KeyValuePair<PeerConnectionEventHandler, object>>();

			this.callbacks.AddLast(new KeyValuePair<PeerConnectionEventHandler, object>(Callback, State));
		}

		private void Init()
		{
			this.peer.OnSent += Peer_OnSent;
			this.peer.OnReceived += Peer_OnReceived;
			this.peer.OnClosed += Peer_OnClosed;
		}

		public void Peer_OnReceived(object Sender, byte[] Packet)
		{
			string s = this.encoding.GetString(Packet, 0, Packet.Length);

			if (this.xmppClient == null)
				this.parent.ReceiveText(s);

			if (!this.ParseIncoming(s))
			{
				// TODO: Don't continue to read (for instance if switch to TLS is to be done.)
			}
		}

		private bool ParseIncoming(string s)
		{
			bool Result = true;

			foreach (char ch in s)
			{
				switch (this.inputState)
				{
					case 0:     // Waiting for <?
						if (ch == '<')
						{
							this.fragment.Append(ch);
							if (this.fragment.Length > 4096)
							{
								this.ToError();
								return false;
							}
							else
								this.inputState++;
						}
						else if (ch > ' ')
						{
							this.ToError();
							return false;
						}
						break;

					case 1:     // Waiting for ? or >
						this.fragment.Append(ch);
						if (this.fragment.Length > 4096)
						{
							this.ToError();
							return false;
						}
						else if (ch == '?')
							this.inputState++;
						else if (ch == '>')
						{
							this.inputState = 5;
							this.inputDepth = 1;
							this.ProcessStream(this.fragment.ToString());
							this.fragment.Clear();
						}
						break;

					case 2:     // Waiting for ?>
						this.fragment.Append(ch);
						if (this.fragment.Length > 4096)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
							this.inputState++;
						break;

					case 3:     // Waiting for <stream
						this.fragment.Append(ch);
						if (this.fragment.Length > 4096)
						{
							this.ToError();
							return false;
						}
						else if (ch == '<')
							this.inputState++;
						else if (ch > ' ')
						{
							this.ToError();
							return false;
						}
						break;

					case 4:     // Waiting for >
						this.fragment.Append(ch);
						if (this.fragment.Length > 4096)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							this.inputState++;
							this.inputDepth = 1;
							this.ProcessStream(this.fragment.ToString());
							this.fragment.Clear();
						}
						break;

					case 5: // Waiting for <
						if (ch == '<')
						{
							this.fragment.Append(ch);
							this.inputState++;
						}

						else if (this.inputDepth > 1)
							this.fragment.Append(ch);
						else if (ch > ' ')
						{
							this.ToError();
							return false;
						}
						break;

					case 6: // Second character in tag
						this.fragment.Append(ch);
						if (ch == '/')
							this.inputState++;
						else
							this.inputState += 2;
						break;

					case 7: // Waiting for end of closing tag
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth--;
							if (this.inputDepth < 1)
							{
								this.ToError();
								return false;
							}
							else
							{
								if (this.inputDepth == 1)
								{
									if (!this.ProcessFragment(this.fragment.ToString()))
										Result = false;

									this.fragment.Clear();
								}

								if (this.inputState > 0)
									this.inputState = 5;
							}
						}
						break;

					case 8: // Wait for end of start tag
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 5;
						}
						else if (ch == '/')
							this.inputState++;
						break;

					case 9: // Check for end of childless tag.
						this.fragment.Append(ch);
						if (ch == '>')
						{
							if (this.inputDepth == 1)
							{
								if (!this.ProcessFragment(this.fragment.ToString()))
									Result = false;

								this.fragment.Clear();
							}

							if (this.inputState != 0)
								this.inputState = 5;
						}
						else
							this.inputState--;
						break;

					default:
						break;
				}
			}

			return Result;
		}

		private void ToError()
		{
			this.inputState = -1;
			this.state = XmppState.Error;

			this.CallCallbacks();

			if (this.peer != null)
			{
				this.peer.Dispose();
				this.peer = null;
			}
		}

		private void ProcessStream(string Xml)
		{
			try
			{
				int i = Xml.IndexOf("?>");
				if (i >= 0)
					Xml = Xml.Substring(i + 2).TrimStart();

				this.streamHeader = Xml;

				i = Xml.IndexOf(":stream");
				if (i < 0)
					this.streamFooter = "</stream>";
				else
					this.streamFooter = "</" + Xml.Substring(1, i - 1) + ":stream>";

				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(Xml + this.streamFooter);

				if (Doc.DocumentElement.LocalName != "stream")
					throw new XmppException("Invalid stream.", Doc.DocumentElement);

				XmlElement Stream = Doc.DocumentElement;

				this.version = XML.Attribute(Stream, "version", 0.0);
				this.streamId = XML.Attribute(Stream, "id");
				this.remoteBareJid = XML.Attribute(Stream, "from");

				if (this.version < 1.0)
					throw new XmppException("Version not supported.", Stream);

				if (this.parentBareJid != XML.Attribute(Stream, "to"))
					throw new XmppException("Invalid destination JID.", Stream);

				this.state = XmppState.Authenticating;

				string Header = "<?xml version='1.0'?><stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' from='" +
					this.parentBareJid + "' to='" + this.remoteBareJid + "' version='1.0'>";

				try
				{
					this.parent.AuthenticatePeer(this.peer, this.remoteBareJid);
				}
				catch (Exception ex)
				{
					this.parent.Error(ex.Message);

					Header += "<stream:error><invalid-from xmlns='urn:ietf:params:xml:ns:xmpp-streams'/></stream:error></stream:stream>";

					this.headerSent = true;
					this.Send(Header, (sender,e)=>
					{
						this.ToError();
					});

					return;
				}

				this.xmppClient = new XmppClient(this, this.state, Header, "</stream:stream>", this.parentBareJid);
				this.xmppClient.SendFromAddress = true;
				this.parent.NewXmppClient(this.xmppClient, this.parentBareJid, this.remoteBareJid);
				this.parent.PeerAuthenticated(this);

				this.xmppClient.OnStateChanged += this.XmppClient_OnStateChanged;

				if (!this.headerSent)
				{
					this.headerSent = true;
					this.Send(Header);

					this.state = XmppState.Connected;
				}
			}
			catch (Exception ex)
			{
				this.parent.Exception(ex);
				this.ToError();
			}
		}

		private void XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			this.state = NewState;

			if (NewState == XmppState.Connected)
				this.CallCallbacks();
			else if (this.callbacks != null && (NewState == XmppState.Error || NewState == XmppState.Offline))
			{
				if (this.parent != null)
					this.parent.PeerClosed(this);

				this.xmppClient.Dispose();
				this.xmppClient = null;

				this.CallCallbacks();
			}
		}

		/// <summary>
		/// Current connection state.
		/// </summary>
		public XmppState State
		{
			get
			{
				if (this.xmppClient != null)
					return this.xmppClient.State;
				else
					return this.state;
			}
		}

		internal bool HeaderSent
		{
			get { return this.headerSent; }
			set { this.headerSent = value; }
		}

		/// <summary>
		/// Peer-to-peer connection object.
		/// </summary>
		public PeerConnection Peer
		{
			get { return this.peer; }
			internal set
			{
				this.peer = value;
				this.Init();
			}
		}

		/// <summary>
		/// XMPP client.
		/// </summary>
		public XmppClient XmppClient
		{
			get { return this.xmppClient; }
		}

		/// <summary>
		/// Parent object.
		/// </summary>
		public XmppServerlessMessaging Parent
		{
			get { return this.parent; }
		}

		/// <summary>
		/// Remote Bare JID
		/// </summary>
		public string RemoteBareJid
		{
			get { return this.remoteBareJid; }
		}

		internal bool HasCallbacks
		{
			get { return this.callbacks != null; }
		}

		internal void CallCallbacks()
		{
			if (this.callbacks != null)
			{
				foreach (KeyValuePair<PeerConnectionEventHandler, object> P in this.callbacks)
				{
					try
					{
						P.Key(this, new PeerConnectionEventArgs(this.xmppClient, P.Value, this.parentBareJid, this.remoteBareJid));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

				this.callbacks = null;
			}
		}

		private bool ProcessFragment(string Xml)
		{
			TextEventHandler h = this.OnReceived;
			if (h != null)
			{
				bool Result;

				try
				{
					Result = h(this, Xml);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					Result = false;
				}

				if (Result && this.callbacks != null)
					this.CallCallbacks();

				return Result;
			}
			else
				return false;
		}

		private void Peer_OnClosed(object sender, EventArgs e)
		{
			this.parent.PeerClosed(this);
			this.parent = null;
			this.peer = null;
		}

		/// <summary>
		/// CLoses the connection.
		/// </summary>
		public void Close()
		{
			if (this.peer != null)
			{
				try
				{
					this.peer.Dispose();
				}
				catch (Exception)
				{
					// Ignore.
				}

				this.peer = null;
			}

			if (this.xmppClient != null)
			{
				IDisposable Disposable;

				foreach (ISniffer Sniffer in this.xmppClient.Sniffers)
				{
					this.xmppClient.Remove(Sniffer);

					Disposable = Sniffer as IDisposable;
					if (Disposable != null)
					{
						try
						{
							Disposable.Dispose();
						}
						catch (Exception)
						{
							// Ignore
						}
					}
				}

				try
				{
					this.xmppClient.Dispose();
				}
				catch (Exception)
				{
					// Ignore.
				}

				this.xmppClient = null;
			}
		}

		private void Peer_OnSent(object Sender, byte[] Packet)
		{
			TextEventHandler h = this.OnSent;
			if (h != null)
			{
				try
				{
					string s = this.encoding.GetString(Packet);
					h(this, s);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Sends a packet.
		/// </summary>
		/// <param name="Packet"></param>
		public void Send(string Packet)
		{
			this.Send(Packet, null);
		}

		/// <summary>
		/// Sends a packet.
		/// </summary>
		/// <param name="Packet"></param>
		/// <param name="Callback">Optional method to call when packet has been sent.</param>
		///	<param name="State">State object to pass on to the callback method.</param>
		public void Send(string Packet, EventHandler Callback)
		{
			byte[] Data = this.encoding.GetBytes(Packet);
			this.peer.SendTcp(Data, Callback);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.Close();
		}
	}
}
