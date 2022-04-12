using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Push
{
	/// <summary>
	/// Push Notification Client
	/// </summary>
	public class PushNotificationClient : XmppExtension
	{
		/// <summary>
		/// http://waher.se/Schema/MessagePush.xsd
		/// </summary>
		public const string MessagePushNamespace = "http://waher.se/Schema/MessagePush.xsd";

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
		public override string[] Extensions => new string[0];

		#region New Token

		/// <summary>
		/// Reports a new push token to the server.
		/// </summary>
		/// <param name="Token">Token received from the push service.</param>
		/// <param name="Service">Service providing the token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void NewToken(string Token, PushMessagingService Service, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<newToken xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("' service='");
			Xml.Append(Service.ToString());
			Xml.Append("' token='");
			Xml.Append(XML.Encode(Token));
			Xml.Append("'/>");

			this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Reports a new push token to the server.
		/// </summary>
		/// <param name="Token">Token received from the push service.</param>
		/// <param name="Service">Service providing the token.</param>
		public Task NewTokenAsync(string Token, PushMessagingService Service)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			this.NewToken(Token, Service, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to report new token."));

				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		#endregion
		
		#region Remove Token

		/// <summary>
		/// Removes the last push token from the server.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void RemoveToken(IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<removeToken xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("'/>");

			this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Removes the last push token from the server.
		/// </summary>
		public Task RemoveTokenAsync()
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			this.RemoveToken((sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to remove token."));

				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		#endregion

		#region Clear Rules

		/// <summary>
		/// Clears available push notification rules for the client.
		/// </summary>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ClearRules(IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<clearRules xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("'/>");

			this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Clears available push notification rules for the client.
		/// </summary>
		public Task ClearRulesAsync()
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			this.ClearRules((sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to clear available rules."));

				return Task.CompletedTask;
			}, null);

			return Result.Task;
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
		/// <param name="ContentScript">Script creating the content of the push notification.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void AddRule(string MessageType, string LocalName, string Namespace, string Channel, string MessageVariable, 
			string PatternMatchingScript, string ContentScript, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<addRule xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("' type='");
			Xml.Append(XML.Encode(MessageType));
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

			this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Adds a push notification rule to the client account.
		/// </summary>
		/// <param name="MessageType">Rule applies to messages of this type.</param>
		/// <param name="LocalName">Rule applies only if the content of the message matches this local name.</param>
		/// <param name="Namespace">Rule applies only if the namespace of the message matches this namespace.</param>
		/// <param name="Channel">The push notification will be sent to this channel.</param>
		/// <param name="PatternMatchingScript">Optional pattern-matching script. It will be applied to the incoming message, and
		/// can be used to populate variables that will later be used to construct the push notification content.</param>
		/// <param name="ContentScript">Script creating the content of the push notification.</param>
		public Task AddRuleAsync(string MessageType, string LocalName, string Namespace, string Channel, string MessageVariable, 
			string PatternMatchingScript, string ContentScript)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			this.AddRule(MessageType, LocalName, Namespace, Channel, MessageVariable, PatternMatchingScript, ContentScript, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to add push notification rule."));

				return Task.CompletedTask;
			}, null);

			return Result.Task;
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
		public void RemoveRule(string MessageType, string LocalName, string Namespace, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<removeRule xmlns='");
			Xml.Append(MessagePushNamespace);
			Xml.Append("' type='");
			Xml.Append(XML.Encode(MessageType));
			Xml.Append("' localName='");
			Xml.Append(XML.Encode(LocalName));
			Xml.Append("' namespace='");
			Xml.Append(XML.Encode(Namespace));
			Xml.Append("'/>");

			this.client.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Removes a push notification rule from the client account.
		/// </summary>
		/// <param name="MessageType">Rule applies to messages of this type.</param>
		/// <param name="LocalName">Rule applies only if the content of the message matches this local name.</param>
		/// <param name="Namespace">Rule applies only if the namespace of the message matches this namespace.</param>
		public Task RemoveRuleAsync(string MessageType, string LocalName, string Namespace)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			this.RemoveRule(MessageType, LocalName, Namespace, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to remove push notification rule."));

				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		#endregion

	}
}
