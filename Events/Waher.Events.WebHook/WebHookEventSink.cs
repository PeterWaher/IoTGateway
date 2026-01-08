using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Security;

namespace Waher.Events.WebHook
{
	/// <summary>
	/// Event sink sending events to a remote service using POST.
	/// </summary>
	public class WebHookEventSink : EventSink, ITlsCertificateEndpoint
	{
		private readonly Uri callbackUrl;
		private X509Certificate certificate;

		/// <summary>
		/// Event sink sending events to a remote service using POST.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="CallbackUrl">Webhook Callback URL</param>
		public WebHookEventSink(string ObjectID, string CallbackUrl)
			: this(ObjectID, CallbackUrl, null)
		{
		}

		/// <summary>
		/// Event sink sending events to a remote service using POST.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="CallbackUrl">Webhook Callback URL</param>
		/// <param name="Certificate">Certificate to use if mTLS is negotiated.</param>
		public WebHookEventSink(string ObjectID, string CallbackUrl, X509Certificate Certificate)
			: base(ObjectID)
		{
			this.callbackUrl = new Uri(CallbackUrl);
			this.certificate = Certificate;
		}

		/// <summary>
		/// Updates the certificate used in mTLS negotiation.
		/// </summary>
		/// <param name="Certificate">Updated Certificate</param>
		public void UpdateCertificate(X509Certificate Certificate)
		{
			this.certificate = Certificate;
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

			return InternetContent.PostAsync(this.callbackUrl, Payload, this.certificate);
		}
	}
}
