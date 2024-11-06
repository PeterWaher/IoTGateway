using System;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.PubSub.Events
{
	/// <summary>
	/// Event arguments for node subscription callback events.
	/// </summary>
    public class SubscriptionEventArgs : SubscriptionOptionsEventArgs
	{
		private readonly OptionsAvailability availability;
		private readonly DateTime expires;
		private readonly string subscriptionId;
		private readonly NodeSubscriptionStatus status;

		/// <summary>
		/// Event arguments for node subscription callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID of subscription.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Options">Subscription options, if available.</param>
		/// <param name="Availability">If options are supported.</param>
		/// <param name="Expires">When the subscription expires.</param>
		/// <param name="Status">Status of subscription.</param>
		/// <param name="e">IQ result event arguments.</param>
		public SubscriptionEventArgs(string NodeName, string Jid, string SubscriptionId,
			SubscriptionOptions Options, OptionsAvailability Availability, DateTime Expires,
			NodeSubscriptionStatus Status, DataFormEventArgs e)
			: base(NodeName, Jid, Options, e)
		{
			this.availability = Availability;
			this.expires = Expires;
			this.subscriptionId = SubscriptionId;
			this.status = Status;
		}

		/// <summary>
		/// If options are supported
		/// </summary>
		public OptionsAvailability OptionsSupported => this.availability;

		/// <summary>
		/// When the subscription expires.
		/// </summary>
		public DateTime Expires => this.expires;

		/// <summary>
		/// Subscription ID
		/// </summary>
		public string SubscriptionId => this.subscriptionId;

		/// <summary>
		/// Status of subscripton.
		/// </summary>
		public NodeSubscriptionStatus Status => this.status;
	}
}
