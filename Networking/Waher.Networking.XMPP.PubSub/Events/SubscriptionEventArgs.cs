using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for node subscription callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SubscriptionEventHandler(object Sender, SubscriptionEventArgs e);

	/// <summary>
	/// Event arguments for node subscription callback events.
	/// </summary>
    public class SubscriptionEventArgs : SubscriptionOptionsEventArgs
	{
		private OptionsAvailability availability;
		private DateTime expires;
		private string subscriptionId;

		/// <summary>
		/// Event arguments for node subscription callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID of subscription.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Options">Subscription options, if available.</param>
		/// <param name="Availability">If options are supported.</param>
		/// <param name="Expires">When the subscription expires.</param>
		/// <param name="e">IQ result event arguments.</param>
		public SubscriptionEventArgs(string NodeName, string Jid, string SubscriptionId,
			SubscriptionOptions Options, OptionsAvailability Availability, DateTime Expires, 
			DataFormEventArgs e)
			: base(NodeName, Jid, Options, e)
		{
			this.availability = Availability;
			this.expires = Expires;
			this.subscriptionId = SubscriptionId;
		}

		/// <summary>
		/// If options are supported
		/// </summary>
		public OptionsAvailability OptionsSupported
		{
			get { return this.availability; }
		}

		/// <summary>
		/// When the subscription expires.
		/// </summary>
		public DateTime Expires
		{
			get { return this.expires; }
		}

		/// <summary>
		/// Subscription ID
		/// </summary>
		public string SubscriptionId
		{
			get { return this.subscriptionId; }
		}
	}
}
