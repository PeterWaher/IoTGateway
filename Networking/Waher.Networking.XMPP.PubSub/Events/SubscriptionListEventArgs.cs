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
	public delegate void SubscriptionListEventHandler(object Sender, SubscriptionListEventArgs e);

	/// <summary>
	/// Event arguments for subscription list callback events.
	/// </summary>
	public class SubscriptionListEventArgs : NodeEventArgs
    {
		private Dictionary<string, NodeSubscriptionStatus> subscriptions;

		/// <summary>
		/// Event arguments for node callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Subscriptions">Available subscriptions.</param>
		/// <param name="e">IQ result event arguments.</param>
		public SubscriptionListEventArgs(string NodeName, 
			Dictionary<string,NodeSubscriptionStatus> Subscriptions, IqResultEventArgs e)
			: base(NodeName, e)
		{
			this.subscriptions = Subscriptions;
		}

		/// <summary>
		/// Available subscriptions.
		/// </summary>
		public Dictionary<string, NodeSubscriptionStatus> Subscriptions
		{
			get { return this.subscriptions; }
		}
    }
}
