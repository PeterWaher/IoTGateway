using Waher.Content.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Provisioning.Events
{
	/// <summary>
	/// Event arguments for IsFriend events.
	/// </summary>
	public abstract class QuestionEventArgs : MessageEventArgs
	{
		private readonly ProvisioningClient client;
		private readonly string jid;
		private string remoteJid;
		private string key;

		/// <summary>
		/// Event arguments for IsFriend events.
		/// </summary>
		/// <param name="Client">XMPP Client used.</param>
		/// <param name="e">Message with request.</param>
		public QuestionEventArgs(ProvisioningClient Client, MessageEventArgs e)
			: base(e)
		{
			this.client = Client;
			this.jid = XML.Attribute(e.Content, "jid");
			this.remoteJid = XML.Attribute(e.Content, "remoteJid");
			this.key = XML.Attribute(e.Content, "key");
		}

		/// <summary>
		/// Provisioning Client used.
		/// </summary>
		public ProvisioningClient Client => this.client;

		/// <summary>
		/// JID.
		/// </summary>
		public string JID => this.jid;

		/// <summary>
		/// JID of device requesting presence subscription of the device.
		/// </summary>
		public string RemoteJID
		{
			get => this.remoteJid;
			set => this.remoteJid = value;
		}

		/// <summary>
		/// Event key.
		/// </summary>
		public string Key
		{
			get => this.key;
			set => this.key = value;
		}
	}
}
