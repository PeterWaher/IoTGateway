using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Events.WebHook
{
	/// <summary>
	/// Event sink sending events to a remote service using POST.
	/// </summary>
	public class WebHookSink : EventSink
	{
		private readonly Uri callbackUrl;

		/// <summary>
		/// Event sink sending events to a remote service using POST.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="CallbackUrl">Webhook Callback URL</param>
		public WebHookSink(string ObjectID, string CallbackUrl)
			: base(ObjectID)
		{
			this.callbackUrl = new Uri(CallbackUrl);
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			Dictionary<string, object> Payload = new Dictionary<string, object>()
			{
				{ "Timestamp", Event.Timestamp.ToUniversalTime() },
				{ "Type", Event.Type },
				{ "Level", Event.Level },
				{ "EventId", Event.EventId },
				{ "Object", Event.Object },
				{ "Actor", Event.Actor },
				{ "Facility", Event.Facility },
				{ "Module", Event.Module },
				{ "Message", Event.Message },
				{ "Tags", Event.Tags },
				{ "StackTrace", Event.StackTrace }
			};

			return InternetContent.PostAsync(this.callbackUrl, Payload);
		}
	}
}
