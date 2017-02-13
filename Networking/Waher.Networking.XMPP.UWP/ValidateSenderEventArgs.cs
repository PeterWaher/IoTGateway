using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Delegate for sender validation event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ValidateSenderEventHandler(object Sender, ValidateSenderEventArgs e);

	/// <summary>
	/// Event arguments for sender validation events.
	/// </summary>
	public class ValidateSenderEventArgs : EventArgs
	{
		private XmlElement stanza;
		private string from;
		private string fromBareJid;
		private bool accepted = false;
		private bool rejected = false;

		/// <summary>
		/// Event arguments for message events.
		/// </summary>
		public ValidateSenderEventArgs(XmlElement Stanza, string From)
			: this(Stanza, From, XmppClient.GetBareJID(From))
		{
		}

		/// <summary>
		/// Event arguments for message events.
		/// </summary>
		public ValidateSenderEventArgs(XmlElement Stanza, string From, string FromBareJid)
		{
			this.stanza = Stanza;
			this.from = From;
			this.fromBareJid = FromBareJid;
		}

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
