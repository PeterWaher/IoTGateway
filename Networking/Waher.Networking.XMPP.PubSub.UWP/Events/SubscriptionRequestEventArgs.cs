using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for subscription request event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments</param>
	public delegate Task SubscriptionRequestEventHandler(object Sender, SubscriptionRequestEventArgs e);

	/// <summary>
	/// Event argument for item notification events.
	/// </summary>
	public class SubscriptionRequestEventArgs : NodeNotificationEventArgs
    {
		private readonly string jid;
		private readonly DataForm form;

		/// <summary>
		/// Event argument for item notification events.
		/// </summary>
		/// <param name="NodeName">Node name.</param>
		/// <param name="Jid">JID of subscriber.</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="e">Message event arguments</param>
		public SubscriptionRequestEventArgs(string NodeName, string Jid, string SubscriptionId,
			MessageFormEventArgs e)
			: base(NodeName, SubscriptionId, e)
		{
			this.jid = Jid;
			this.form = e.Form;
		}

		/// <summary>
		/// JID of subscriber
		/// </summary>
		public string Jid
		{
			get { return this.jid; }
		}

		/// <summary>
		/// Form embedded in message.
		/// </summary>
		public DataForm Form
		{
			get { return this.form; }
		}

		/// <summary>
		/// Approves the request.
		/// </summary>
		public void Approve()
		{
			this.form["pubsub#allow"]?.SetValue("true");
			this.form.Submit();
		}

		/// <summary>
		/// Rejects the request.
		/// </summary>
		public void Reject()
		{
			this.form["pubsub#allow"]?.SetValue("false");
			this.form.Submit();
		}

		/// <summary>
		/// Cancels the request.
		/// </summary>
		public void Cancel()
		{
			this.form.Cancel();
		}

	}
}
