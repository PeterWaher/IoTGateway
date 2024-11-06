using System.Threading.Tasks;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.PubSub.Events
{
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
		public string Jid => this.jid;

		/// <summary>
		/// Form embedded in message.
		/// </summary>
		public DataForm Form => this.form;

		/// <summary>
		/// Approves the request.
		/// </summary>
		public async Task Approve()
		{
			Field Field = this.form["pubsub#allow"];
			if (!(Field is null))
				await Field.SetValue("true");

			await this.form.Submit();
		}

		/// <summary>
		/// Rejects the request.
		/// </summary>
		public async Task Reject()
		{
			Field Field = this.form["pubsub#allow"];
			if (!(Field is null))
				await Field.SetValue("false");

			await this.form.Submit();
		}

		/// <summary>
		/// Cancels the request.
		/// </summary>
		public Task Cancel()
		{
			return this.form.Cancel();
		}

	}
}
