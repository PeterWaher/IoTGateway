using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.PubSub.Events
{
	/// <summary>
	/// Event arguments for node subscription options callback events.
	/// </summary>
	public class SubscriptionOptionsEventArgs : DataFormEventArgs
    {
		private readonly string nodeName;
		private readonly string jid;
		private readonly SubscriptionOptions options;

		/// <summary>
		/// Event arguments for node subscription options callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Jid">JID of subscription.</param>
		/// <param name="Options">Subscription options, if available.</param>
		/// <param name="e">IQ result event arguments.</param>
		public SubscriptionOptionsEventArgs(string NodeName, string Jid, SubscriptionOptions Options,
			DataFormEventArgs e)
			: base(e.Form, e)
		{
			this.nodeName = NodeName;
			this.jid = Jid;
			this.options = Options;
		}

		/// <summary>
		/// Node name.
		/// </summary>
		public string NodeName => this.nodeName;

		/// <summary>
		/// JID of subscription.
		/// </summary>
		public string Jid => this.jid;

		/// <summary>
		/// Subscriptions options, if available.
		/// </summary>
		public SubscriptionOptions Options => this.options;
	}
}
