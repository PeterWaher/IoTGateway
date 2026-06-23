using System;
using System.Collections.Generic;
using Waher.Events;
using Waher.Networking.HTTP.Mcp;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Events
{
	public class EventLogMcpServer : HttpMcpServerResource
	{
		/// <summary>
		/// Model Context Protocol (MCP) server resource for the Event Log.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public EventLogMcpServer(string ResourceName)
			: this(ResourceName,
				  GetDefaultIcons(), GetDefaultWebSite()
				  ?? new Uri("https://www.nuget.org/packages/Waher.Events/"))
		{
		}

		/// <summary>
		/// Model Context Protocol (MCP) server resource for the Event Log.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		public EventLogMcpServer(string ResourceName, Icon[] Icons, Uri WebSiteUri)
			: this(ResourceName,
				  "EventLog",   // Name
				  "Event Log",  // Title
				  typeof(EventLogMcpServer).Assembly.GetName().Version.ToString(),
				  "A Model Context Protocol (MCP) server resource permitting MCP clients " +
				  "to log events to the Event Log.",
				  Icons,
				  WebSiteUri,
				  "Significant events should be logged to the Event Log to facilitate " +
				  "troubleshooting. Personal information should be removed from events " +
				  "if not necessary for cybersecurity reasons, depending on context.")
		{
		}

		/// <summary>
		/// Model Context Protocol (MCP) server resource for the Event Log.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Name">Name of server.</param>
		/// <param name="Title">Title of server.</param>
		/// <param name="Version">Version of server.</param>
		/// <param name="Description">Description of server.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="Instructions">Instructions for server.</param>
		public EventLogMcpServer(string ResourceName, string Name, string Title,
			string Version, string Description, Icon[] Icons, Uri WebSiteUri,
			string Instructions)
			: base(ResourceName, Name, Title, Version, Description, Icons, WebSiteUri,
				Instructions)
		{
		}

		[McpServerTool(
			"Log Debug Event",  // Title
			"Logs an event for debug purposes to the event log. The purpose of debug events is to highlight technical information to developers, for troubleshooting.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public Dictionary<string, object?> LogDebug(
			[McpStringParameter("Event Message", "The body text of the event to log.")]
			string Message,

			[McpStringParameter("Object", "The object associated with the event, if any.")]
			string Object = "",

			[McpStringParameter("Actor", "The subject, or actor, performing the action resulting in the event being logged.")]
			string Actor = "",

			[McpStringParameter("Event Level", "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.")]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter("Event ID", "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.", 32)]
			string EventId = "McpEvent",

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "", 

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Debug(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
			return LogResponse();
		}

		[McpServerTool(
			"Log Informational Event",  // Title
			"Logs an informational event to the event log. The purpose of informational events is to provide a record of normal operational tasks being performed or events occurring.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public Dictionary<string, object?> LogInformational(
			[McpStringParameter("Event Message", "The body text of the event to log.")]
			string Message,

			[McpStringParameter("Object", "The object associated with the event, if any.")]
			string Object = "",

			[McpStringParameter("Actor", "The subject, or actor, performing the action resulting in the event being logged.")]
			string Actor = "",

			[McpStringParameter("Event Level", "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.")]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter("Event ID", "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.", 32)]
			string EventId = "McpEvent",

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "",

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Informational(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
			return LogResponse();
		}

		[McpServerTool(
			"Log Notice Event",  // Title
			"Logs an notice event to the event log. A Notice represents a significant condition or change that administrators should be aware of.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public Dictionary<string, object?> LogNotice(
			[McpStringParameter("Event Message", "The body text of the event to log.")]
			string Message,

			[McpStringParameter("Object", "The object associated with the event, if any.")]
			string Object = "",

			[McpStringParameter("Actor", "The subject, or actor, performing the action resulting in the event being logged.")]
			string Actor = "",

			[McpStringParameter("Event Level", "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.")]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter("Event ID", "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.", 32)]
			string EventId = "McpEvent",

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "",

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Notice(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
			return LogResponse();
		}

		[McpServerTool(
			"Log Warning Event",  // Title
			"Logs a warning event to the event log. Warning events warn operators of conditions that may lead to errors if they are not properly managed.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public Dictionary<string, object?> LogWarning(
			[McpStringParameter("Event Message", "The body text of the event to log.")]
			string Message,

			[McpStringParameter("Object", "The object associated with the event, if any.")]
			string Object = "",

			[McpStringParameter("Actor", "The subject, or actor, performing the action resulting in the event being logged.")]
			string Actor = "",

			[McpStringParameter("Event Level", "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.")]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter("Event ID", "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.", 32)]
			string EventId = "McpEvent",

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "",

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Warning(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
			return LogResponse();
		}

		[McpServerTool(
			"Log Error Event",  // Title
			"Logs an error event to the event log. Error events inform operators of normal error conditions. A normal error condition is typically an expected error.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public Dictionary<string, object?> LogError(
			[McpStringParameter("Event Message", "The body text of the event to log.")]
			string Message,

			[McpStringParameter("Object", "The object associated with the event, if any.")]
			string Object = "",

			[McpStringParameter("Actor", "The subject, or actor, performing the action resulting in the event being logged.")]
			string Actor = "",

			[McpStringParameter("Event Level", "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.")]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter("Event ID", "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.", 32)]
			string EventId = "McpEvent",

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "",

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Error(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
			return LogResponse();
		}

		[McpServerTool(
			"Log Critical Error Event",  // Title
			"Logs a critical error event to the event log. A critical error is an error so great that it could escalate into something graver if not addressed. Typically, a Critical error is typically an unexpected error.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public Dictionary<string, object?> LogCritical(
			[McpStringParameter("Event Message", "The body text of the event to log.")]
			string Message,

			[McpStringParameter("Object", "The object associated with the event, if any.")]
			string Object = "",

			[McpStringParameter("Actor", "The subject, or actor, performing the action resulting in the event being logged.")]
			string Actor = "",

			[McpStringParameter("Event Level", "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.")]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter("Event ID", "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.", 32)]
			string EventId = "McpEvent",

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "",

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Critical(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
			return LogResponse();
		}

		[McpServerTool(
			"Log Alert Event",  // Title
			"Logs an alert event to the event log. An alert error is so grave, that action must be taken immediately.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public Dictionary<string, object?> LogAlert(
			[McpStringParameter("Event Message", "The body text of the event to log.")]
			string Message,

			[McpStringParameter("Object", "The object associated with the event, if any.")]
			string Object = "",

			[McpStringParameter("Actor", "The subject, or actor, performing the action resulting in the event being logged.")]
			string Actor = "",

			[McpStringParameter("Event Level", "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.")]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter("Event ID", "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.", 32)]
			string EventId = "McpEvent",

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "",

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Alert(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
			return LogResponse();
		}

		[McpServerTool(
			"Log Emergency Event",  // Title
			"Logs an emergency event to the event log. An emergency error signals the system is unusable, or will become unusable if action is not taken immediately.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public Dictionary<string, object?> LogEmergency(
			[McpStringParameter("Event Message", "The body text of the event to log.")]
			string Message,

			[McpStringParameter("Object", "The object associated with the event, if any.")]
			string Object = "",

			[McpStringParameter("Actor", "The subject, or actor, performing the action resulting in the event being logged.")]
			string Actor = "",

			[McpStringParameter("Event Level", "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.")]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter("Event ID", "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.", 32)]
			string EventId = "McpEvent",

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "",

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Emergency(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
			return LogResponse();
		}

		private static Dictionary<string, object?> LogResponse()
		{
			return new Dictionary<string, object?>() { { "logged", true } };
		}

		private static KeyValuePair<string, object>[] GetTags(Dictionary<string, object>? MetaData)
		{
			int c = MetaData?.Count ?? 0;
			KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[c];

			if (c > 0)
			{
				int i = 0;

				foreach (KeyValuePair<string, object> P in MetaData!)
					Tags[i++] = P;
			}

			return Tags;
		}
	}
}
