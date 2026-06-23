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

		[McpDateTimeParameter("Timestamp", "Timestamp of event, in UTC.")]
		public DateTime Timestamp { get; }

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

		[McpStringParameter(EventLogMcpServer.EventLevelTitle, EventLogMcpServer.EventLevelDescription)]
		[McpEnumValue(EventLevel.Minor, "Minor Event")]
		[McpEnumValue(EventLevel.Medium, "Medium Event")]
		[McpEnumValue(EventLevel.Major, "Major Event")]
		public EventLevel Level { get; }

		[McpStringParameter(EventLogMcpServer.MessageTitle, EventLogMcpServer.MessageDescription)]
		public string Message { get; }

		[McpStringParameter(EventLogMcpServer.ObjectTitle, EventLogMcpServer.ObjectDescription, 0, 128)]
		public string Object { get; }

		[McpStringParameter(EventLogMcpServer.ActorTitle, EventLogMcpServer.ActorDescription, 0, 128)]
		public string Actor { get; }

		[McpStringParameter(EventLogMcpServer.EventIdTitle, EventLogMcpServer.EventIdDescription, 0, 32)]
		public string EventId { get; }

		[McpStringParameter(EventLogMcpServer.FacilityTitle, EventLogMcpServer.FacilityDescription, 0, 256)]
		public string Facility { get; }

		[McpStringParameter(EventLogMcpServer.ModuleTitle, EventLogMcpServer.ModuleDescription, 0, 256)]
		public string Module { get; }

		[McpParameter("Stack Trace", "Stack trace of the event, if any.")]
		public string StackTrace { get; }

		[McpParameter("Tags", "Variable set of tags providing event-specific information.")]
		public Tag[] Tags { get; }
	}
}
