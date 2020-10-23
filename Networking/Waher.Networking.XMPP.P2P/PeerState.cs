using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content.Xml;
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
		private const int MaxFragmentSize = 1000000;

		private readonly UTF8Encoding encoding = new UTF8Encoding(false, false);
		private readonly StringBuilder fragment = new StringBuilder();
		private int fragmentLength = 0;
		private XmppState state = XmppState.StreamNegotiation;
		private PeerConnection peer;
		private XmppServerlessMessaging parent;
		private XmppClient xmppClient;
		private LinkedList<KeyValuePair<PeerConnectionEventHandler, object>> callbacks = null;
		private DateTime lastActivity = DateTime.Now;
		private int inputState = 0;
		private int inputDepth = 0;
		private readonly string parentFullJid;
		private string streamHeader;
		private string streamFooter;
		private string streamId;
		private double version;
		private string remoteFullJid;
		private bool headerSent = false;

		/// <summary>
		/// Event raised when a text packet has been sent.
		/// </summary>
		public event TextEventHandler OnSent = null;

		/// <summary>
		/// Event raised when a text packet (XML fragment) has been received.
		/// </summary>
		public event TextEventHandler OnReceived = null;

		/// <summary>
		/// Peer connection state.
		/// </summary>
		/// <param name="Peer">Peer connection.</param>
		/// <param name="Parent">Parent object.</param>
		public PeerState(PeerConnection Peer, XmppServerlessMessaging Parent)
		{
			this.parent = Parent;
			this.peer = Peer;
			this.parentFullJid = Parent.FullJid;

			this.AddPeerHandlers();
		}

		/// <summary>
		/// Peer connection state.
		/// </summary>
		/// <param name="Peer">Peer connection.</param>
		/// <param name="Parent">Parent object.</param>
		/// <param name="RemoteFullJID">Remote Full JID</param>
		/// <param name="StreamHeader">Stream header</param>
		/// <param name="StreamFooter">Stream footer</param>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Version">Protocol version</param>
		/// <param name="Callback">Callback method</param>
		/// <param name="State">State object</param>
		public PeerState(PeerConnection Peer, XmppServerlessMessaging Parent, string RemoteFullJID, string StreamHeader, string StreamFooter,
			string StreamId, double Version, PeerConnectionEventHandler Callback, object State)
		{
			this.parent = Parent;
			this.peer = Peer;
			this.remoteFullJid = RemoteFullJID;
			this.streamHeader = StreamHeader;
			this.streamFooter = StreamFooter;
			this.streamId = StreamId;
			this.version = Version;
			this.parentFullJid = Parent.FullJid;

			this.callbacks = new LinkedList<KeyValuePair<PeerConnectionEventHandler, object>>();
			this.callbacks.AddLast(new KeyValuePair<PeerConnectionEventHandler, object>(Callback, State));

			this.AddPeerHandlers();
		}

		internal void AddCallback(PeerConnectionEventHandler Callback, object State)
		{
			if (this.callbacks is null)
				this.callbacks = new LinkedList<KeyValuePair<PeerConnectionEventHandler, object>>();

			this.callbacks.AddLast(new KeyValuePair<PeerConnectionEventHandler, object>(Callback, State));
		}

		private void AddPeerHandlers()
		{
			if (this.peer != null)
			{
				this.peer.OnSent += Peer_OnSent;
				this.peer.OnReceived += Peer_OnReceived;
				this.peer.OnClosed += Peer_OnClosed;
			}
		}

		/// <summary>
		/// If reading has been paused.
		/// </summary>
		public bool Paused => this.peer.Paused;

		/// <summary>
		/// Continues a paused connection.
		/// </summary>
		public void Continue()
		{
			this.peer.Continue();
		}

		private void RemoveHandlers()
		{
			if (this.peer != null)
			{
				this.peer.OnSent -= Peer_OnSent;
				this.peer.OnReceived -= Peer_OnReceived;
				this.peer.OnClosed -= Peer_OnClosed;
			}
		}

		/// <summary>
		/// Data received from a peer.
		/// </summary>
		/// <param name="Sender">Sender of event</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte read.</param>
		/// <param name="Count">Number of bytes read.</param>
		/// <returns>If the process should be continued.</returns>
		public Task<bool> Peer_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			string s = this.encoding.GetString(Buffer, Offset, Count);

			this.lastActivity = DateTime.Now;
			if (this.xmppClient is null)
				this.parent.ReceiveText(s);

			return this.ParseIncoming(s);
		}

		private async Task<bool> ParseIncoming(string s)
		{
			bool Result = true;

			foreach (char ch in s)
			{
				switch (this.inputState)
				{
					case 0:     // Waiting for first <
						if (ch == '<')
						{
							this.fragment.Append(ch);
							if (++this.fragmentLength > MaxFragmentSize)
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
						if (++this.fragmentLength > MaxFragmentSize)
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
							this.fragmentLength = 0;
						}
						break;

					case 2:     // In processing instruction. Waiting for ?>
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
							this.inputState++;
						break;

					case 3:     // Waiting for <stream
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
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
						if (++this.fragmentLength > MaxFragmentSize)
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
							this.fragmentLength = 0;
						}
						break;

					case 5: // Waiting for start element.
						if (ch == '<')
						{
							this.fragment.Append(ch);
							if (++this.fragmentLength > MaxFragmentSize)
							{
								this.ToError();
								return false;
							}
							else
								this.inputState++;
						}
						else if (this.inputDepth > 1)
						{
							this.fragment.Append(ch);
							if (++this.fragmentLength > MaxFragmentSize)
							{
								this.ToError();
								return false;
							}
						}
						else if (ch > ' ')
						{
							this.ToError();
							return false;
						}
						break;

					case 6: // Second character in tag
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '/')
							this.inputState++;
						else if (ch == '!')
							this.inputState = 13;
						else
							this.inputState += 2;
						break;

					case 7: // Waiting for end of closing tag
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
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
									if (!await this.ProcessFragment(this.fragment.ToString()))
										Result = false;

									this.fragment.Clear();
									this.fragmentLength = 0;
								}

								if (this.inputState > 0)
									this.inputState = 5;
							}
						}
						break;

					case 8: // Wait for end of start tag
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 5;
						}
						else if (ch == '/')
							this.inputState++;
						else if (ch <= ' ')
							this.inputState += 2;
						break;

					case 9: // Check for end of childless tag.
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							if (this.inputDepth == 1)
							{
								if (!await this.ProcessFragment(this.fragment.ToString()))
									Result = false;

								this.fragment.Clear();
								this.fragmentLength = 0;
							}

							if (this.inputState != 0)
								this.inputState = 5;
						}
						else
							this.inputState--;
						break;

					case 10:    // Check for attributes.
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 5;
						}
						else if (ch == '/')
							this.inputState--;
						else if (ch == '"')
							this.inputState++;
						else if (ch == '\'')
							this.inputState += 2;
						break;

					case 11:    // Double quote attribute.
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '"')
							this.inputState--;
						break;

					case 12:    // Single quote attribute.
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '\'')
							this.inputState -= 2;
						break;

					case 13:    // Third character in start of comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '-')
							this.inputState++;
						else if (ch == '[')
							this.inputState = 18;
						else
						{
							this.ToError();
							return false;
						}
						break;

					case 14:    // Fourth character in start of comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '-')
							this.inputState++;
						else
						{
							this.ToError();
							return false;
						}
						break;

					case 15:    // In comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '-')
							this.inputState++;
						break;

					case 16:    // Second character in end of comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '-')
							this.inputState++;
						else
							this.inputState--;
						break;

					case 17:    // Third character in end of comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
							this.inputState = 5;
						else
							this.inputState -= 2;
						break;

					case 18:    // Fourth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == 'C')
							this.inputState++;
						else
						{
							this.ToError();
							return false;
						}
						break;

					case 19:    // Fifth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == 'D')
							this.inputState++;
						else
						{
							this.ToError();
							return false;
						}
						break;

					case 20:    // Sixth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == 'A')
							this.inputState++;
						else
						{
							this.ToError();
							return false;
						}
						break;

					case 21:    // Seventh character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == 'T')
							this.inputState++;
						else
						{
							this.ToError();
							return false;
						}
						break;

					case 22:    // Eighth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == 'A')
							this.inputState++;
						else
						{
							this.ToError();
							return false;
						}
						break;

					case 23:    // Ninth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '[')
							this.inputState++;
						else
						{
							this.ToError();
							return false;
						}
						break;

					case 24:    // In CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == ']')
							this.inputState++;
						break;

					case 25:    // Second character in end of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == ']')
							this.inputState++;
						else
							this.inputState--;
						break;

					case 26:    // Third character in end of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							this.ToError();
							return false;
						}
						else if (ch == '>')
							this.inputState = 5;
						else
							this.inputState -= 2;
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

			this.peer?.Dispose();
			this.peer = null;
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

				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.LoadXml(Xml + this.streamFooter);

				if (Doc.DocumentElement.LocalName != "stream")
					throw new XmppException("Invalid stream.", Doc.DocumentElement);

				XmlElement Stream = Doc.DocumentElement;

				this.version = XML.Attribute(Stream, "version", 0.0);
				this.streamId = XML.Attribute(Stream, "id");
				this.remoteFullJid = XML.Attribute(Stream, "from");

				if (this.version < 1.0)
					throw new XmppException("Version not supported.", Stream);

				if (this.parentFullJid != XML.Attribute(Stream, "to"))
					throw new XmppException("Invalid destination JID.", Stream);

				this.state = XmppState.Authenticating;

				string Header = "<?xml version='1.0'?><stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' from='" +
					this.parentFullJid + "' to='" + this.remoteFullJid + "' version='1.0'>";

				try
				{
					this.parent.AuthenticatePeer(this.peer, this.remoteFullJid);
				}
				catch (Exception ex)
				{
					this.parent.Exception(ex);

					Header += "<stream:error><invalid-from xmlns='urn:ietf:params:xml:ns:xmpp-streams'/>" +
						"<text xmlns='urn:ietf:params:xml:ns:xmpp-streams'>" + XML.Encode(ex.Message) +
						"</text></stream:error></stream:stream>";

					this.headerSent = true;
					this.SendAsync(Header, (sender, e) =>
					{
						this.ToError();
					});

					return;
				}

				if (!this.headerSent)
				{
					this.headerSent = true;
					this.SendAsync(Header);
				}

				this.state = XmppState.Connected;
				this.xmppClient = new XmppClient(this, this.state, Header, "</stream:stream>", this.parentFullJid,
					typeof(XmppServerlessMessaging).GetTypeInfo().Assembly)
				{
					SendFromAddress = true
				};

				this.parent.PeerAuthenticated(this);
				this.parent.NewXmppClient(this.xmppClient, this.parentFullJid, this.remoteFullJid);

				this.xmppClient.OnStateChanged += this.XmppClient_OnStateChanged;

				this.CallCallbacks();
			}
			catch (Exception ex)
			{
				this.parent.Exception(ex);
				this.ToError();
			}
		}

		private Task XmppClient_OnStateChanged(object _, XmppState NewState)
		{
			this.state = NewState;

			if (NewState == XmppState.Connected)
				this.CallCallbacks();
			else if (NewState == XmppState.Error || NewState == XmppState.Offline)
			{
				if (this.parent != null)
					this.parent.PeerClosed(this);

				if (this.xmppClient != null)
				{
					this.xmppClient.Dispose();
					this.xmppClient = null;
				}

				this.CallCallbacks();
			}

			return Task.CompletedTask;
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
				this.RemoveHandlers();
				this.peer = value;
				this.AddPeerHandlers();
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
		/// Remote Full JID
		/// </summary>
		public string RemoteFullJid
		{
			get { return this.remoteFullJid; }
		}

		internal bool HasCallbacks
		{
			get { return this.callbacks != null; }
		}

		internal void ClearCallbacks()
		{
			this.callbacks = null;
		}

		internal void CallCallbacks()
		{
			if (this.callbacks != null)
			{
				foreach (KeyValuePair<PeerConnectionEventHandler, object> P in this.callbacks)
				{
					try
					{
						P.Key(this, new PeerConnectionEventArgs(this.xmppClient, P.Value, this.parentFullJid, this.remoteFullJid));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

				this.callbacks = null;
			}
		}

		private Task<bool> ProcessFragment(string Xml)
		{
			Task<bool> Result;
			TextEventHandler h = this.OnReceived;

			if (h is null)
				Result = Task.FromResult<bool>(false);
			else
			{
				try
				{
					Result = h(this, Xml);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					Result = Task.FromResult<bool>(false);
				}

				//if (Result && this.callbacks != null)
				//	this.CallCallbacks();
			}

			return Result;
		}

		private void Peer_OnClosed(object sender, EventArgs e)
		{
			this.parent.PeerClosed(this);
			this.parent = null;
			this.peer = null;

			if (this.callbacks != null)
				this.CallCallbacks();
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
				foreach (ISniffer Sniffer in this.xmppClient.Sniffers)
				{
					this.xmppClient.Remove(Sniffer);

					if (Sniffer is IDisposable Disposable)
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

		private Task Peer_OnSent(object Sender, byte[] Buffer, int Offset, int Count)
		{
			TextEventHandler h = this.OnSent;
			if (!(h is null))
			{
				try
				{
					string s = this.encoding.GetString(Buffer, Offset, Count);
					h(this, s);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			return Task.FromResult<bool>(true);
		}

		/// <summary>
		/// Sends a packet.
		/// </summary>
		/// <param name="Packet"></param>
		public Task<bool> SendAsync(string Packet)
		{
			return this.SendAsync(Packet, null);
		}

		/// <summary>
		/// Sends a packet.
		/// </summary>
		/// <param name="Packet"></param>
		/// <param name="Callback">Optional method to call when packet has been sent.</param>
		public Task<bool> SendAsync(string Packet, EventHandler Callback)
		{
			byte[] Data = this.encoding.GetBytes(Packet);

			if (this.peer != null)
			{
				this.peer.SendTcp(Data, Callback);
				this.lastActivity = DateTime.Now;
			}

			return Task.FromResult<bool>(true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.Close();
		}

		/// <summary>
		/// Seconds since object was active.
		/// </summary>
		public double AgeSeconds
		{
			get
			{
				return (DateTime.Now - this.lastActivity).TotalSeconds;
			}
		}
	}
}
