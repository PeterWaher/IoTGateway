using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for subscription list callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SubscriptionsEventHandler(object Sender, SubscriptionsEventArgs e);

	/// <summary>
	/// Event arguments for subscription list callback events.
	/// </summary>
	public class SubscriptionsEventArgs : NodeEventArgs
    {
		private Subscription[] subscriptions;

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
		public Subscription[] Subscriptions
		{
			get { return this.subscriptions; }
		}
    }
}
