using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking;
using Waher.Networking.PeerToPeer;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Peer connection state.
	/// </summary>
	public class PeerState
	{
		private UTF8Encoding encoding = new UTF8Encoding(false, false);
		private StringBuilder fragment = new StringBuilder();
		private XmppState state = XmppState.StreamNegotiation;
		private PeerConnection peer;
		private XmppServerlessMessaging parent;
		private int inputState = 0;
		private int inputDepth = 0;
		private string streamHeader;
		private string streamFooter;
		private string streamId;
		private double version;
		private string remoteBareJid;

		public PeerState(PeerConnection Peer, XmppServerlessMessaging Parent)
		{
			this.parent = Parent;
			this.peer = Peer;

			this.peer.OnSent += Peer_OnSent;
			this.peer.OnReceived += Peer_OnReceived;
			this.peer.OnClosed += Peer_OnClosed;
		}

		public void Peer_OnReceived(object Sender, byte[] Packet)
		{
			string s = this.encoding.GetString(Packet, 0, Packet.Length);

			// TODO: Sniffer.

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
						if (this.fragment.Length>4096)
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

			this.peer.Dispose();
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

				if (this.parent.BareJid != XML.Attribute(Stream, "to"))
					throw new XmppException("Invalid destination JID.", Stream);

				this.state = XmppState.Authenticating;

				if (!this.parent.AuthenticatePeer(this.Peer, this.remoteBareJid))
					throw new XmppException("Invalid source JID.", Stream);

				this.state = XmppState.Connected;
				this.parent.PeerAuthenticated(this);
			}
			catch (Exception)
			{
				this.ToError();
			}
		}

		/// <summary>
		/// Current connection state.
		/// </summary>
		public XmppState State
		{
			get { return this.state; }
		}

		/// <summary>
		/// Peer-to-peer connection object.
		/// </summary>
		public PeerConnection Peer
		{
			get { return this.peer; }
		}

		/// <summary>
		/// Parent object.
		/// </summary>
		public XmppServerlessMessaging Parent
		{
			get { return this.parent; }
		}

		public string RemoteBareJid
		{
			get { return this.remoteBareJid; }
		}

		public void Close()
		{
			// TODO
		}

		private bool ProcessFragment(string Xml)
		{
			// TODO
			return true;
		}

		private void Peer_OnClosed(object sender, EventArgs e)
		{
			this.parent.PeerClosed(this);
			this.parent = null;
			this.peer = null;
		}

		private void Peer_OnSent(object Sender, byte[] Packet)
		{
			// TODO
		}
	}
}
