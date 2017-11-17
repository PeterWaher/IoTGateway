using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event arguments for IsFriend events.
	/// </summary>
	public abstract class QuestionEventArgs : MessageEventArgs
	{
		private XmppClient client;
		private string jid;
		private string remoteJid;
		private string key;

		/// <summary>
		/// Event arguments for IsFriend events.
		/// </summary>
		/// <param name="Client">XMPP Client used.</param>
		/// <param name="e">Message with request.</param>
		public QuestionEventArgs(XmppClient Client, MessageEventArgs e)
			: base(e)
		{
			this.client = Client;
			this.jid = XML.Attribute(e.Content, "jid");
			this.remoteJid = XML.Attribute(e.Content, "remoteJid");
			this.key = XML.Attribute(e.Content, "key");
		}

		/// <summary>
		/// XMPP Client used.
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// JID.
		/// </summary>
		public string JID
		{
			get { return this.jid; }
		}

		/// <summary>
		/// JID of device requesting presence subscription of the device.
		/// </summary>
		public string RemoteJID
		{
			get { return this.remoteJid; }
			set { this.remoteJid = value; }
		}

		/// <summary>
		/// Event key.
		/// </summary>
		public string Key
		{
			get { return this.key; }
			set { this.key = value; }
		}

	}
}
