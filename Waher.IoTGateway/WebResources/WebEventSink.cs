using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Security;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Sending events to the corresponding web page(s).
	/// </summary>
	public class WebEventSink : EventSink
	{
		private readonly DateTime created = DateTime.Now;
		private readonly DateTime expires;
		private readonly string[] privileges;
		private readonly string userVariable;
		private readonly string resource;
		private string[] tabIds = null;
		private DateTime tabIdTimestamp = DateTime.MinValue;

		/// <summary>
		/// Sending sniffer events to the corresponding web page(s).
		/// </summary>
		/// <param name="SinkId">Sniffer ID</param>
		/// <param name="PageResource">Resource of page displaying the sniffer.</param>
		/// <param name="MaxLife">Maximum life of sniffer.</param>
		/// <param name="UserVariable">Event is only pushed to clients with a session contining a variable 
		/// named <paramref name="UserVariable"/> having a value derived from <see cref="IUser"/>.</param>
		/// <param name="Privileges">Event is only pushed to clients with a user variable having the following set of privileges.</param>
		public WebEventSink(string SinkId, string PageResource, TimeSpan MaxLife, string UserVariable, params string[] Privileges)
			: base(SinkId)
		{
			this.expires = DateTime.Now.Add(MaxLife);
			this.resource = PageResource;
			this.tabIds = null;
			this.userVariable = UserVariable;
			this.privileges = Privileges;
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override async Task Queue(Event Event)
		{
			try
			{
				DateTime Now = DateTime.Now;

				if ((Now - this.tabIdTimestamp).TotalSeconds > 2 || this.tabIds is null || this.tabIds.Length == 0)
				{
					this.tabIds = ClientEvents.GetTabIDsForLocation(this.resource, true);
					this.tabIdTimestamp = Now;
				}

				Dictionary<string, object>[] Tags;

				if (Event.Tags is null)
					Tags = null;
				else
				{
					int i, c = Event.Tags.Length;
					Tags = new Dictionary<string, object>[c];

					for (i = 0; i < c; i++)
					{
						Tags[i] = new Dictionary<string, object>()
						{
							{ "name", Event.Tags[i].Key },
							{ "value", Event.Tags[i].Value }
						};
					}
				}

				string Data = JSON.Encode(new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("date", XML.Encode(Event.Timestamp.Date,true)),
					new KeyValuePair<string, object>("time", Event.Timestamp.TimeOfDay.ToString()),
					new KeyValuePair<string, object>("type", Event.Type.ToString()),
					new KeyValuePair<string, object>("level", Event.Level.ToString()),
					new KeyValuePair<string, object>("id", Event.EventId),
					new KeyValuePair<string, object>("object", Event.Object),
					new KeyValuePair<string, object>("actor", Event.Actor),
					new KeyValuePair<string, object>("module", Event.Module),
					new KeyValuePair<string, object>("facility", Event.Facility),
					new KeyValuePair<string, object>("message", Event.Message),
					new KeyValuePair<string, object>("stackTrace", Event.StackTrace),
					new KeyValuePair<string, object>("tags", Tags)
				}, false);

				int Tabs = await ClientEvents.PushEvent(this.tabIds, "NewEvent", Data, true, this.userVariable, this.privileges);

				if (Now >= this.expires || (Tabs <= 0 && (Now - this.created).TotalSeconds >= 5))
				{
					await ClientEvents.PushEvent(this.tabIds, "SinkClosed", string.Empty, false, this.userVariable, this.privileges);

					Log.Unregister(this);
					this.Dispose();
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
