using System.Threading.Tasks;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Provisioning.Events
{
	/// <summary>
	/// Event arguments for IsFriend events.
	/// </summary>
	public class IsFriendEventArgs : QuestionEventArgs
	{
		/// <summary>
		/// Event arguments for IsFriend events.
		/// </summary>
		/// <param name="Client">Provisioning Client used.</param>
		/// <param name="e">Message with request.</param>
		public IsFriendEventArgs(ProvisioningClient Client, MessageEventArgs e)
			: base(Client, e)
		{
		}

		/// <summary>
		/// Accepts the friendship.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task Accept(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, RuleRange.Caller, Callback, State);
		}

		/// <summary>
		/// Rejects the friendship.
		/// </summary>
		public Task Reject(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, RuleRange.Caller, Callback, State);
		}

		/// <summary>
		/// Accepts the friendship, and similar future requests from the entire remote domain.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task AcceptForEntireDomain(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, RuleRange.Domain, Callback, State);
		}

		/// <summary>
		/// Rejects the friendship, and similar future requests from the entire remote domain.
		/// </summary>
		public Task RejectForEntireDomain(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, RuleRange.Domain, Callback, State);
		}

		/// <summary>
		/// Accepts the friendship, and all future requests.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task AcceptForAll(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(true, RuleRange.All, Callback, State);
		}

		/// <summary>
		/// Rejects the friendship, and all future requests.
		/// </summary>
		public Task RejectForAll(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Respond(false, RuleRange.All, Callback, State);
		}

		private Task Respond(bool IsFriend, RuleRange Range, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.Client.IsFriendResponse(this.From, this.JID, this.RemoteJID, this.Key, IsFriend, Range, Callback, State);
		}
	}
}
