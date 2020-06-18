using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for subscription event notificaction event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments</param>
	public delegate Task SubscriptionNotificationEventHandler(object Sender, SubscriptionNotificationEventArgs e);

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
		public string NodeName
		{
			get { return this.nodeName; }
		}

		/// <summary>
		/// JID of subscriber.
		/// </summary>
		public string Jid
		{
			get { return this.jid; }
		}

		/// <summary>
		/// New subscription status.
		/// </summary>
		public NodeSubscriptionStatus Status
		{
			get { return this.status; }
		}

	}
}
