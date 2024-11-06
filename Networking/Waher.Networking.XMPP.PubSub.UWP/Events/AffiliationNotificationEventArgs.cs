using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.PubSub.Events
{
	/// <summary>
	/// Event argument for affiliation event notification events.
	/// </summary>
	public class AffiliationNotificationEventArgs : MessageEventArgs
    {
		private readonly string nodeName;
		private readonly string jid;
		private readonly AffiliationStatus status;

		/// <summary>
		/// Event argument for affiliation event notification events.
		/// </summary>
		/// <param name="NodeName">Node name.</param>
		/// <param name="Jid">JID of subscriber.</param>
		/// <param name="Status">Affiliation status</param>
		/// <param name="e">Message event arguments</param>
		public AffiliationNotificationEventArgs(string NodeName, string Jid,
			AffiliationStatus Status, MessageEventArgs e)
			: base(e)
		{
			this.nodeName = NodeName;
			this.jid = Jid;
			this.status = Status;
		}

		/// <summary>
		/// Name of node.
		/// </summary>
		public string NodeName => this.nodeName;

		/// <summary>
		/// JID of subscriber.
		/// </summary>
		public string Jid => this.jid;

		/// <summary>
		/// New affiliation status.
		/// </summary>
		public AffiliationStatus Status => this.status;
	}
}
