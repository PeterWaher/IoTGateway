using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Push
{
	/// <summary>
	/// Push Notification Client
	/// </summary>
	public class PushNotificationClient : XmppExtension
	{
		/// <summary>
		/// http://waher.se/Schema/PushNotification.xsd
		/// </summary>
		public const string MessagePushNamespace = "http://waher.se/Schema/PushNotification.xsd";

		/// <summary>
		/// Push Notification Client
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public PushNotificationClient(XmppClient Client)
			: base(Client)
		{
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => Array.Empty<string>();

		#region New Token

		/// <summary>
		/// Reports a new push token to the server.
		/// </summary>
		/// <param name="Token">Token received from the push service.</param>
		/// <param name="Service">Service providing the token.</param>
		/// <param name="ClientType">Client Type.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task NewToken(string Token, PushMessagingService Service, ClientType ClientType, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<newToken xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("' service='");
			Xml.Append(Service.ToString());
			Xml.Append("' clientType='");
			Xml.Append(ClientType.ToString());
			Xml.Append("' token='");
			Xml.Append(XML.Encode(Token));
			Xml.Append("'/>");

			return this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Reports a new push token to the server.
		/// </summary>
		/// <param name="Token">Token received from the push service.</param>
		/// <param name="Service">Service providing the token.</param>
		/// <param name="ClientType">Client Type.</param>
		public async Task NewTokenAsync(string Token, PushMessagingService Service, ClientType ClientType)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.NewToken(Token, Service, ClientType, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to report new token."));

				return Task.CompletedTask;
			}, null);

			await Result.Task;
		}

		#endregion

		#region Remove Token

		/// <summary>
		/// Removes the last push token from the server.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task RemoveToken(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<removeToken xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("'/>");

			return this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Removes the last push token from the server.
		/// </summary>
		public async Task RemoveTokenAsync()
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.RemoveToken((Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to remove token."));

				return Task.CompletedTask;
			}, null);

			await Result.Task;
		}

		#endregion

		#region Clear Rules

		/// <summary>
		/// Clears available push notification rules for the client.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task ClearRules(EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<clearRules xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("'/>");

			return this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Clears available push notification rules for the client.
		/// </summary>
		public async Task ClearRulesAsync()
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.ClearRules((Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to clear available rules."));

				return Task.CompletedTask;
			}, null);

			await Result.Task;
		}

		#endregion

		#region Add Push Notification Rule

		/// <summary>
		/// Adds a push notification rule to the client account.
		/// </summary>
		/// <param name="MessageType">Rule applies to messages of this type.</param>
		/// <param name="LocalName">Rule applies only if the content of the message matches this local name.</param>
		/// <param name="Namespace">Rule applies only if the namespace of the message matches this namespace.</param>
		/// <param name="Channel">The push notification will be sent to this channel.</param>
		/// <param name="MessageVariable">Variable to put the Message XML in, before patternmatching or content script is executed.</param>
		/// <param name="PatternMatchingScript">Optional pattern-matching script. It will be applied to the incoming message, and
		/// can be used to populate variables that will later be used to construct the push notification content.</param>
		/// <param name="ContentScript">Script creating the content of the push notification.
		/// 
		/// If Content results in a string, it will be sent as a simple notification.
		/// 
		/// If Content results in an object, use the property names defined by Firebase XMPP API, to configure properties:
		/// https://firebase.google.com/docs/cloud-messaging/xmpp-server-ref?authuser=0#downstream-xmpp-messages-json
		/// 
		/// All other properties defined in the resulting object, will be treated as data tags in the notification.
		/// </param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task AddRule(MessageType MessageType, string LocalName, string Namespace, string Channel, string MessageVariable,
			string PatternMatchingScript, string ContentScript, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<addRule xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("' type='");

			if (MessageType != MessageType.Normal)
				Xml.Append(MessageType.ToString().ToLower());

			Xml.Append("' localName='");
			Xml.Append(XML.Encode(LocalName));
			Xml.Append("' namespace='");
			Xml.Append(XML.Encode(Namespace));
			Xml.Append("' channel='");
			Xml.Append(XML.Encode(Channel));
			Xml.Append("' variable='");
			Xml.Append(XML.Encode(MessageVariable));
			Xml.Append("'>");

			if (!string.IsNullOrEmpty(PatternMatchingScript))
			{
				Xml.Append("<PatternMatching>");
				Xml.Append(XML.Encode(PatternMatchingScript));
				Xml.Append("</PatternMatching>");
			}

			if (!string.IsNullOrEmpty(ContentScript))
			{
				Xml.Append("<Content>");
				Xml.Append(XML.Encode(ContentScript));
				Xml.Append("</Content>");
			}

			Xml.Append("</addRule>");

			return this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Adds a push notification rule to the client account.
		/// </summary>
		/// <param name="MessageType">Rule applies to messages of this type.</param>
		/// <param name="LocalName">Rule applies only if the content of the message matches this local name.</param>
		/// <param name="Namespace">Rule applies only if the namespace of the message matches this namespace.</param>
		/// <param name="Channel">The push notification will be sent to this channel.</param>
		/// <param name="MessageVariable">Variable to put the incoming message content in, before script is executed.</param>
		/// <param name="PatternMatchingScript">Optional pattern-matching script. It will be applied to the incoming message, and
		/// can be used to populate variables that will later be used to construct the push notification content.</param>
		/// <param name="ContentScript">Script creating the content of the push notification.
		/// 
		/// If Content results in a string, it will be sent as a simple notification.
		/// 
		/// If Content results in an object, use the property names defined by Firebase XMPP API, to configure properties:
		/// https://firebase.google.com/docs/cloud-messaging/xmpp-server-ref?authuser=0#downstream-xmpp-messages-json
		/// 
		/// All other properties defined in the resulting object, will be treated as data tags in the notification.
		/// </param>
		public async Task AddRuleAsync(MessageType MessageType, string LocalName, string Namespace, string Channel, string MessageVariable,
			string PatternMatchingScript, string ContentScript)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.AddRule(MessageType, LocalName, Namespace, Channel, MessageVariable, PatternMatchingScript, ContentScript, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to add push notification rule."));

				return Task.CompletedTask;
			}, null);

			await Result.Task;
		}

		#endregion

		#region Remove Push Notification Rule

		/// <summary>
		/// Removes a push notification rule from the client account.
		/// </summary>
		/// <param name="MessageType">Rule applies to messages of this type.</param>
		/// <param name="LocalName">Rule applies only if the content of the message matches this local name.</param>
		/// <param name="Namespace">Rule applies only if the namespace of the message matches this namespace.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task RemoveRule(MessageType MessageType, string LocalName, string Namespace, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<removeRule xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("' type='");

			if (MessageType != MessageType.Normal)
				Xml.Append(MessageType.ToString().ToLower());

			Xml.Append("' localName='");
			Xml.Append(XML.Encode(LocalName));
			Xml.Append("' namespace='");
			Xml.Append(XML.Encode(Namespace));
			Xml.Append("'/>");

			return this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Removes a push notification rule from the client account.
		/// </summary>
		/// <param name="MessageType">Rule applies to messages of this type.</param>
		/// <param name="LocalName">Rule applies only if the content of the message matches this local name.</param>
		/// <param name="Namespace">Rule applies only if the namespace of the message matches this namespace.</param>
		public async Task RemoveRuleAsync(MessageType MessageType, string LocalName, string Namespace)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.RemoveRule(MessageType, LocalName, Namespace, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to remove push notification rule."));

				return Task.CompletedTask;
			}, null);

			await Result.Task;
		}

		#endregion

	}
}
