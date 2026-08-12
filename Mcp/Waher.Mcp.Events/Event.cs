using System;
using Waher.Events;
using Waher.Events.Persistence;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Events
{
	/// <summary>
	/// Information about an event in the event log.
	/// </summary>
	public class Event
	{
		/// <summary>
		/// Information about an event in the event log.
		/// </summary>
		/// <param name="Event">Persisted event.</param>
		public Event(PersistedEvent Event)
		{
			this.ObjectId = Event.ObjectId.ToString();
			this.Timestamp = Event.Timestamp;
			this.Type = Event.Type;
			this.Level = Event.Level;
			this.Message = Event.Message;
			this.Object = Event.Object;
			this.Actor = Event.Actor;
			this.EventId = Event.EventId;
			this.Facility = Event.Facility;
			this.Module = Event.Module;
			this.StackTrace = Event.StackTrace;

			int i, c = Event.Tags?.Length ?? 0;
			this.Tags = new Tag[c];

			for (i = 0; i < c; i++)
				this.Tags[i] = new Tag(Event.Tags![i]);
		}

		/// <summary>
		/// ID of event object in persistent storage.
		/// </summary>
		[McpStringParameter("Object ID", "ID of event object in persistent storage.")]
		public string ObjectId { get; }

		/// <summary>
		/// Timestamp of event, in UTC.
		/// </summary>
		[McpDateTimeParameter("Timestamp", "Timestamp of event, in UTC.")]
		public DateTime Timestamp { get; }

		/// <summary>
		/// Type of event.
		/// </summary>
		[McpParameter("Event Type", "Type of event.")]
		[McpEnumValue(EventType.Debug, "Debug event. Debug events highlight technical information to developers, for troubleshooting.")]
		[McpEnumValue(EventType.Informational, "Informational event. Informational events provide a record of normal operational tasks being performed or events occurring.")]
		[McpEnumValue(EventType.Notice, "Notice event. Notice events highlight important information that may require attention.")]
		[McpEnumValue(EventType.Warning, "Warning event. Warning events highlight potential issues that may require attention.")]
		[McpEnumValue(EventType.Error, "Error event. Error events indicate a problem that has occurred.")]
		[McpEnumValue(EventType.Critical, "Critical event. Critical events indicate errors so great that it could escalate into something graver if not addressed.")]
		[McpEnumValue(EventType.Alert, "Alert event. Alert events indicate a condition that requires immediate action.")]
		[McpEnumValue(EventType.Emergency, "Emergency event. Emergency events signals the system is unusable, or will become unusable if action is not taken immediately.")]
		public EventType Type { get; }

		/// <summary>
		/// The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.
		/// </summary>
		[McpParameter(EventLogMcpServer.EventLevelTitle, EventLogMcpServer.EventLevelDescription)]
		[McpEnumValue(EventLevel.Minor, "Minor Event")]
		[McpEnumValue(EventLevel.Medium, "Medium Event")]
		[McpEnumValue(EventLevel.Major, "Major Event")]
		public EventLevel Level { get; }

		/// <summary>
		/// The body text of the logged event.
		/// </summary>
		[McpStringParameter(EventLogMcpServer.MessageTitle, EventLogMcpServer.MessageDescription)]
		public string Message { get; }

		/// <summary>
		/// The object associated with the event, if any.
		/// </summary>
		[McpStringParameter(EventLogMcpServer.ObjectTitle, EventLogMcpServer.ObjectDescription, 0, 128)]
		public string Object { get; }

		/// <summary>
		/// The subject, or actor, performing the action resulting in the event being logged.
		/// </summary>
		[McpStringParameter(EventLogMcpServer.ActorTitle, EventLogMcpServer.ActorDescription, 0, 128)]
		public string Actor { get; }

		/// <summary>
		/// Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.
		/// </summary>
		[McpStringParameter(EventLogMcpServer.EventIdTitle, EventLogMcpServer.EventIdDescription, 0, 32)]
		public string EventId { get; }

		/// <summary>
		/// The subsystem or external component that is the source of the event.
		/// </summary>
		[McpStringParameter(EventLogMcpServer.FacilityTitle, EventLogMcpServer.FacilityDescription, 0, 256)]
		public string Facility { get; }

		/// <summary>
		/// The module or component within the source that is the source of the event.
		/// </summary>
		[McpStringParameter(EventLogMcpServer.ModuleTitle, EventLogMcpServer.ModuleDescription, 0, 256)]
		public string Module { get; }

		/// <summary>
		/// Stack trace of the event, if any.
		/// </summary>
		[McpParameter("Stack Trace", "Stack trace of the event, if any.")]
		public string StackTrace { get; }

		/// <summary>
		/// Varaiable set of tags providing event-specific information.
		/// </summary>
		[McpParameter("Tags", "Variable set of tags providing event-specific information.")]
		public Tag[] Tags { get; }
	}
}
