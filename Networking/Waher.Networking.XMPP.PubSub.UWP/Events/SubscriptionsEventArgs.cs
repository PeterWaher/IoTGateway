using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.PubSub.Events
{
	/// <summary>
	/// Event arguments for subscription list callback events.
	/// </summary>
	public class SubscriptionsEventArgs : NodeEventArgs
    {
		private readonly Subscription[] subscriptions;

		/// <summary>
		/// Event arguments for subscription list callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Subscriptions">Available subscriptions.</param>
		/// <param name="e">IQ result event arguments.</param>
		public SubscriptionsEventArgs(string NodeName, Subscription[] Subscriptions, IqResultEventArgs e)
			: base(NodeName, e)
		{
			this.subscriptions = Subscriptions;
		}

		/// <summary>
		/// Available subscriptions.
		/// </summary>
		public Subscription[] Subscriptions => this.subscriptions;
    }
}
