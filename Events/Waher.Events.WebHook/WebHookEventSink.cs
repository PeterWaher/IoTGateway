using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Cache;
using Waher.Runtime.Collections;
using Waher.Security;

namespace Waher.Events.WebHook
{
	/// <summary>
	/// Event sink sending events to a remote service using POST.
	/// </summary>
	public class WebHookEventSink : EventSink, ITlsCertificateEndpoint
	{
		private Cache<string, Rec> eventCache = null;
		private X509Certificate certificate;
		private readonly SemaphoreSlim synchObj = new SemaphoreSlim(1);
		private readonly Uri callbackUrl;
		private readonly int maxSecondsUsed;
		private readonly int maxSecondsUnused;
		private readonly bool collectOnType;
		private readonly bool collectOnLevel;
		private readonly bool collectOnEventId;
		private readonly bool collectOnObject;
		private readonly bool collectOnActor;
		private readonly bool collectOnFacility;
		private readonly bool collectOnModule;
		private readonly bool collected;

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
		public WebHookEventSink(string ObjectID, string CallbackUrl,
			X509Certificate Certificate)
			: this(ObjectID, CallbackUrl, Certificate, 0, 0, false, false,
				  false, false, false, false, false)
		{
		}

		/// <summary>
		/// Event sink sending events to a remote service using POST.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="CallbackUrl">Webhook Callback URL</param>
		/// <param name="Certificate">Certificate to use if mTLS is negotiated.</param>
		/// <param name="MaxSecondsUsed">Maximum number of seconds events can be cached before being sent.</param>
		/// <param name="MaxSecondsUnused">Maximum number of seconds events can be cached before being sent, if collection key not referenced.</param>
		/// <param name="CollectOnType">If events should be collected and cached based on type.</param>
		/// <param name="CollectOnLevel">If events should be collected and cached based on level.</param>
		/// <param name="CollectOnEventId">If events should be collected and cached based on event ID.</param>
		/// <param name="CollectOnObject">If events should be collected and cached based on object.</param>
		/// <param name="CollectOnActor">If events should be collected and cached based on actor.</param>
		/// <param name="CollectOnFacility">If events should be collected and cached based on facility.</param>
		/// <param name="CollectOnModule">If events should be collected and cached based on module.</param>
		public WebHookEventSink(string ObjectID, string CallbackUrl, X509Certificate Certificate,
			int MaxSecondsUsed, int MaxSecondsUnused, bool CollectOnType,
			bool CollectOnLevel, bool CollectOnEventId, bool CollectOnObject,
			bool CollectOnActor, bool CollectOnFacility, bool CollectOnModule)
			: base(ObjectID)
		{
			if (MaxSecondsUsed < MaxSecondsUnused)
				throw new ArgumentException("MaxSecondsUsed must be greater than, or equal to, MaxSecondsUnused.", nameof(MaxSecondsUsed));

			this.collected = MaxSecondsUnused > 0;
			this.callbackUrl = new Uri(CallbackUrl);
			this.certificate = Certificate;
			this.maxSecondsUsed = MaxSecondsUsed;
			this.maxSecondsUnused = MaxSecondsUnused;
			this.collectOnType = CollectOnType;
			this.collectOnLevel = CollectOnLevel;
			this.collectOnEventId = CollectOnEventId;
			this.collectOnObject = CollectOnObject;
			this.collectOnActor = CollectOnActor;
			this.collectOnFacility = CollectOnFacility;
			this.collectOnModule = CollectOnModule;

			if (this.collected)
			{
				this.eventCache = new Cache<string, Rec>(int.MaxValue,
					TimeSpan.FromSeconds(this.maxSecondsUsed),
					TimeSpan.FromSeconds(this.maxSecondsUnused));

				this.eventCache.Removed += this.EventCache_Removed;
			}
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync()"/>
		/// </summary>
		public override Task DisposeAsync()
		{
			this.eventCache?.Clear();
			this.eventCache?.Dispose();
			this.eventCache = null;

			return base.DisposeAsync();
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
		public override async Task Queue(Event Event)
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

			if (this.collected)
			{
				await this.synchObj.WaitAsync();
				try
				{
					StringBuilder sb = new StringBuilder();
					bool First = true;

					if (this.collectOnType)
						Append(sb, Event.Type.ToString(), ref First);

					if (this.collectOnLevel)
						Append(sb, Event.Level.ToString(), ref First);

					if (this.collectOnEventId)
						Append(sb, Event.EventId, ref First);

					if (this.collectOnObject)
						Append(sb, Event.Object, ref First);

					if (this.collectOnActor)
						Append(sb, Event.Actor, ref First);

					if (this.collectOnFacility)
						Append(sb, Event.Facility, ref First);

					if (this.collectOnModule)
						Append(sb, Event.Module, ref First);

					string Key = sb.ToString();

					if (!this.eventCache.TryGetValue(Key, out Rec Rec))
					{
						Rec = new Rec()
						{
							First = Event.Timestamp
						};

						this.eventCache[Key] = Rec;
					}

					Rec.Events.Add(Payload);
					Rec.Last = Event.Timestamp;
				}
				finally
				{
					this.synchObj.Release();
				}
			}
			else
				await InternetContent.PostAsync(this.callbackUrl, Payload, this.certificate);
		}

		private static void Append(StringBuilder sb, string Value, ref bool First)
		{
			if (First)
				First = false;
			else
				sb.AppendLine();

			sb.Append(Value);
		}

		private class Rec
		{
			public ChunkedList<Dictionary<string, object>> Events = new ChunkedList<Dictionary<string, object>>();
			public DateTime First;
			public DateTime Last;
		}

		private async Task EventCache_Removed(object Sender, CacheItemEventArgs<string, Rec> e)
		{
			await this.synchObj.WaitAsync();
			try
			{
				Rec Rec = e.Value;
				Dictionary<string, object> Payload = new Dictionary<string, object>()
				{
					{ "First", Rec.First },
					{ "Last", Rec.Last },
					{ "Count", Rec.Events.Count },
					{ "Events", Rec.Events.ToArray() }
				};

				await InternetContent.PostAsync(this.callbackUrl, Payload, this.certificate);
			}
			catch (Exception ex)
			{
				Event Event = new Event(EventType.Critical, ex, this.ObjectID, string.Empty, string.Empty,
					EventLevel.Minor, string.Empty, string.Empty);
				Event.Avoid(this);

				Log.Event(Event);
			}
			finally
			{
				this.synchObj.Release();
			}
		}
	}
}
