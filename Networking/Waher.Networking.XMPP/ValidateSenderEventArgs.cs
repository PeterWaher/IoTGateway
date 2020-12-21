using System;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Delegate for sender validation event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task ValidateSenderEventHandler(object Sender, ValidateSenderEventArgs e);

	/// <summary>
	/// Event arguments for sender validation events.
	/// </summary>
	public class ValidateSenderEventArgs : EventArgs
	{
		private readonly XmppClient client;
		private readonly XmlElement stanza;
		private readonly IqEventArgs iqStanza;
		private readonly MessageEventArgs messageStanza;
		private string from;
		private string fromBareJid;
		private bool accepted = false;
		private bool rejected = false;

		/// <summary>
		/// Event arguments for message events.
		/// </summary>
		public ValidateSenderEventArgs(XmppClient Client, XmlElement Stanza, string From, IqEventArgs IqStanza, MessageEventArgs MessageStanza)
			: this(Client, Stanza, From, XmppClient.GetBareJID(From), IqStanza, MessageStanza)
		{
		}

		/// <summary>
		/// Event arguments for message events.
		/// </summary>
		public ValidateSenderEventArgs(XmppClient Client, XmlElement Stanza, string From, string FromBareJid, IqEventArgs IqStanza, MessageEventArgs MessageStanza)
		{
			this.client = Client;
			this.stanza = Stanza;
			this.from = From;
			this.fromBareJid = FromBareJid;
			this.iqStanza = IqStanza;
			this.messageStanza = MessageStanza;
		}

		/// <summary>
		/// XMPP Client.
		/// </summary>
		public XmppClient Client { get { return this.client; } }

		/// <summary>
		/// The stanza.
		/// </summary>
		public XmlElement Stanza { get { return this.stanza; } }

		/// <summary>
		/// From where the stanza was received.
		/// </summary>
		public string From
		{
			get { return this.from; }
			set
			{
				this.from = value;
				this.fromBareJid = XmppClient.GetBareJID(value);
			}
		}

		/// <summary>
		/// Bare JID of resource sending the stanza.
		/// </summary>
		public string FromBareJID
		{
			get { return this.fromBareJid; }
		}

		/// <summary>
		/// IQ Stanza, or null if message stanza.
		/// </summary>
		public IqEventArgs IqStanza
		{
			get { return this.iqStanza; }
		}

		/// <summary>
		/// Message Stanza, or null if iq stanza.
		/// </summary>
		public MessageEventArgs MessageStanza
		{
			get { return this.messageStanza; }
		}

		/// <summary>
		/// Called from an event handler to accept the sender.
		/// </summary>
		public void Accept()
		{
			this.accepted = true;
		}

		/// <summary>
		/// Called from an event handler to reject the sender.
		/// </summary>
		public void Reject()
		{
			this.rejected = true;
		}

		/// <summary>
		/// If an event handler accepts the sender.
		/// </summary>
		public bool Accepted
		{
			get { return this.accepted; }
		}

		/// <summary>
		/// If an event handler accepts the sender.
		/// </summary>
		public bool Rejected
		{
			get { return this.rejected; }
		}
	}
}
