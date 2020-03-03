using System;
using System.Collections.Generic;

namespace Waher.Events
{
	/// <summary>
	/// Generic exception, with meta-data for logging.
	/// </summary>
	public class GenericException : Exception, IEventActor, IEventFacility, IEventId, IEventLevel, IEventModule, IEventObject, IEventTags, IEventType
	{
		/// <summary>
		/// Generic exception, with meta-data for logging.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public GenericException(string Message, EventType? Type = null, string Object = null, string Actor = null, string EventId = null, 
			EventLevel? Level = null, string Facility = null, string Module = null, params KeyValuePair<string, object>[] Tags)
			: base(Message)
		{
			this.Type = Type;
			this.Object = Object;
			this.Actor = Actor;
			this.EventId = EventId;
			this.Level = Level;
			this.Facility = Facility;
			this.Module = Module;
			this.Tags = Tags;
		}
		/// <summary>
		/// Generic exception, with meta-data for logging.
		/// </summary>
		/// <param name="Message">Free-text event message.</param>
		/// <param name="InnerException">Inner Exception.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Object">Object related to the event.</param>
		/// <param name="Actor">Actor responsible for the action causing the event.</param>
		/// <param name="EventId">Computer-readable Event ID identifying type of even.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Facility">Facility can be either a facility in the network sense or in the system sense.</param>
		/// <param name="Module">Module where the event is reported.</param>
		/// <param name="Tags">Variable set of tags providing event-specific information.</param>
		public GenericException(string Message, Exception InnerException, EventType? Type = null, string Object = null, 
			string Actor = null, string EventId = null, EventLevel? Level = null, string Facility = null, string Module = null, 
			params KeyValuePair<string, object>[] Tags)
			: base(Message, InnerException)
		{
			this.Type = Type;
			this.Object = Object;
			this.Actor = Actor;
			this.EventId = EventId;
			this.Level = Level;
			this.Facility = Facility;
			this.Module = Module;
			this.Tags = Tags;
		}

		/// <summary>
		/// Actor identifier related to the object.
		/// </summary>
		public string Actor { get; }

		/// <summary>
		/// Facility identifier related to the object.
		/// </summary>
		public string Facility { get; }

		/// <summary>
		/// Event identifier related to the object.
		/// </summary>
		public string EventId { get; }

		/// <summary>
		/// Event level.
		/// </summary>
		public EventLevel? Level { get; }

		/// <summary>
		/// Module identifier related to the object.
		/// </summary>
		public string Module { get; }

		/// <summary>
		/// Object identifier related to the object.
		/// </summary>
		public string Object { get; }

		/// <summary>
		/// Tags related to the object.
		/// </summary>
		public KeyValuePair<string, object>[] Tags { get; }

		/// <summary>
		/// Event type.
		/// </summary>
		public EventType? Type { get; }
	}
}
