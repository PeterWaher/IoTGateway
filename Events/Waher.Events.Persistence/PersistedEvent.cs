using System;
using System.Collections.Generic;
using Waher.Persistence.Attributes;

namespace Waher.Events.Persistence
{
	/// <summary>
	/// Class representing a persisted event.
	/// </summary>
	[CollectionName("EventLog")]
	[TypeName(TypeNameSerialization.None)]
	[ArchivingTime("ArchiveDays")]
	[Index("Timestamp")]
	[Index("Object", "Timestamp")]
	[Index("Actor", "Timestamp")]
	[Index("EventId", "Timestamp")]
	[Index("Facility", "Timestamp")]
	[Index("Type", "Timestamp")]
	public class PersistedEvent
	{
		private Guid objectId = Guid.Empty;
		private DateTime timestamp = DateTime.MinValue;
		private EventType type = EventType.Informational;
		private EventLevel level = EventLevel.Minor;
		private string message = string.Empty;
		private string obj = string.Empty;
		private string actor = string.Empty;
		private string eventId = string.Empty;
		private string module = string.Empty;
		private string facility = string.Empty;
		private string stackTrace = string.Empty;
		private PersistedTag[] tags = null;

		/// <summary>
		/// Class representing a persisted event.
		/// </summary>
		public PersistedEvent()
		{
		}

		/// <summary>
		/// Class representing a persisted event.
		/// </summary>
		/// <param name="Event">Event to store.</param>
		public PersistedEvent(Event Event)
		{
			this.timestamp = Event.Timestamp;
			this.type = Event.Type;
			this.message = Event.Message;
			this.obj = Event.Object;
			this.actor = Event.Actor;
			this.eventId = Event.EventId;
			this.level = Event.Level;
			this.facility = Event.Facility;
			this.module = Event.Module;
			this.stackTrace = Event.StackTrace;

			if (Event.Tags is null)
				this.tags = null;
			else
			{
				int i, c = Event.Tags.Length;

				this.tags = new PersistedTag[c];

				for (i = 0; i < c; i++)
				{
					this.tags[i] = new PersistedTag()
					{
						Name = Event.Tags[i].Key,
						Value = Event.Tags[i].Value
					};
				}
			}
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public Guid ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Timestamp
		{
			get { return this.timestamp; }
			set { this.timestamp = value; }
		}

		/// <summary>
		/// Type of event.
		/// </summary>
		[DefaultValue(EventType.Informational)]
		public EventType Type
		{
			get { return this.type; }
			set { this.type = value; }
		}

		/// <summary>
		/// Event Level.
		/// </summary>
		[DefaultValue(EventLevel.Minor)]
		public EventLevel Level
		{
			get { return this.level; }
			set { this.level = value; }
		}

		/// <summary>
		/// Free-text event message.
		/// </summary>
		public string Message
		{
			get { return this.message; }
			set { this.message = value; }
		}

		/// <summary>
		/// First row of <see cref="Message"/>.
		/// </summary>
		[IgnoreMember]
		public string MessageFirstRow
		{
			get
			{
				string s = this.message.Trim();
				int i = s.IndexOf('\n');
				int j = s.IndexOf('\r');

				if (i < 0)
					i = j;
				else if (j >= 0 && j < i)
					i = j;

				if (i < 0)
					return s;
				else
					return s.Substring(0, i).TrimEnd();
			}
		}

		/// <summary>
		/// Object related to the event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Object
		{
			get { return this.obj; }
			set { this.obj = value; }
		}

		/// <summary>
		/// Actor responsible for the action causing the event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Actor
		{
			get { return this.actor; }
			set { this.actor = value; }
		}

		/// <summary>
		/// Computer-readable Event ID identifying type of even.
		/// </summary>
		[DefaultValueStringEmpty]
		public string EventId
		{
			get { return this.eventId; }
			set { this.eventId = value; }
		}

		/// <summary>
		/// Facility can be either a facility in the network sense or in the system sense.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Facility
		{
			get { return this.facility; }
			set { this.facility = value; }
		}

		/// <summary>
		/// Module where the event is reported.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Module
		{
			get { return this.module; }
			set { this.module = value; }
		}

		/// <summary>
		/// Stack Trace of event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string StackTrace
		{
			get { return this.stackTrace; }
			set { this.stackTrace = value; }
		}

		/// <summary>
		/// Variable set of tags providing event-specific information.
		/// </summary>
		[DefaultValueNull]
		public PersistedTag[] Tags
		{
			get { return this.tags; }
			set { this.tags = value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.message;
		}

		/// <summary>
		/// Number of days to archive event.
		/// </summary>
		public int ArchiveDays
		{
			get => PersistedEventLog.ArchiveDays;
		}
	}
}
