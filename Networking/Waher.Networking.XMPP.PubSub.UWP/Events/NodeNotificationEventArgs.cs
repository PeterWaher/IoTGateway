using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for node event notificaction event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments</param>
	public delegate Task NodeNotificationEventHandler(object Sender, NodeNotificationEventArgs e);

	/// <summary>
	/// Event argument for node notification events.
	/// </summary>
	public class NodeNotificationEventArgs : MessageEventArgs
    {
		private readonly string nodeName;
		private readonly string subscriptionId;

		/// <summary>
		/// Event argument for item notification events.
		/// </summary>
		/// <param name="NodeName">Node name.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="e">Message event arguments</param>
		public NodeNotificationEventArgs(string NodeName, string SubscriptionId, MessageEventArgs e)
			: base(e)
		{
			this.nodeName = NodeName;
			this.subscriptionId = SubscriptionId;
		}

		/// <summary>
		/// Event argument for item notification events.
		/// </summary>
		/// <param name="e">Message event arguments</param>
		public NodeNotificationEventArgs(NodeNotificationEventArgs e)
			: base(e)
		{
			this.nodeName = e.nodeName;
			this.subscriptionId = e.subscriptionId;
		}

		/// <summary>
		/// Name of node.
		/// </summary>
		public string NodeName
		{
			get { return this.nodeName; }
		}

		/// <summary>
		/// Subscription identity, if provided in event notification.
		/// </summary>
		public string SubscriptionId
		{
			get { return this.subscriptionId; }
		}

	}
}
