using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.PubSub.Events
{
	/// <summary>
	/// Event argument for subscription event notification events.
	/// </summary>
	public class SubscriptionNotificationEventArgs : MessageEventArgs
    {
		private readonly string nodeName;
		private readonly string jid;
		private readonly NodeSubscriptionStatus status;

		/// <summary>
		/// Event argument for subscription event notification events.
		/// </summary>
		/// <param name="NodeName">Node name.</param>
		/// <param name="Jid">JID of subscriber.</param>
		/// <param name="Status">Subscription status</param>
		/// <param name="e">Message event arguments</param>
		public SubscriptionNotificationEventArgs(string NodeName, string Jid,
			NodeSubscriptionStatus Status, MessageEventArgs e)
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
		/// New subscription status.
		/// </summary>
		public NodeSubscriptionStatus Status => this.status;

	}
}
