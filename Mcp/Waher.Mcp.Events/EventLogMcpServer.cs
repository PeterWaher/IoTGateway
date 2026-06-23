using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Events.Persistence;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Mcp;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Collections;

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

		internal const string McpEventId = "McpEvent";
		internal const string MessageTitle = "Event Message";
		internal const string MessageDescription = "The body text of the logged event.";
		internal const string ObjectTitle = "Object";
		internal const string ObjectDescription = "The object associated with the event, if any.";
		internal const string ActorTitle = "Actor";
		internal const string ActorDescription = "The subject, or actor, performing the action resulting in the event being logged.";
		internal const string EventLevelTitle = "Event Level";
		internal const string EventLevelDescription = "The level of the event being logged. Minor events occur frequently, Medium events update something, or reports something could cause a major event, Major events adds or destroys something, or reports something important is out of order.";
		internal const string EventIdTitle = "Event ID";
		internal const string EventIdDescription = "Optional Event ID for the event. Event IDs are used to identify a specific type of event, and is used collect related information in reports.";
		internal const string FacilityTitle = "Facility";
		internal const string FacilityDescription = "The subsystem or external component that is the source of the event.";
		internal const string ModuleTitle = "Module";
		internal const string ModuleDescription = "The module or component within the source that is the source of the event.";

		[McpServerTool(
			"Log Debug Event",  // Title
			"Logs an event for debug purposes to the event log. The purpose of debug events is to highlight technical information to developers, for troubleshooting.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogDebug(
			[McpStringParameter(MessageTitle, MessageDescription)]
			string Message,

			[McpStringParameter(ObjectTitle, ObjectDescription, 0, 128)]
			string Object = "",

			[McpStringParameter(ActorTitle, ActorDescription, 0, 128)]
			string Actor = "",

			[McpStringParameter(EventLevelTitle, EventLevelDescription)]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter(EventIdTitle, EventIdDescription, 0, 32)]
			string EventId = McpEventId,

			[McpStringParameter(FacilityTitle, FacilityDescription, 0, 256)]
			string Facility = "",

			[McpStringParameter(ModuleTitle, ModuleDescription, 0, 256)]
			string Module = "",

			[McpParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			[McpMetaDataArgument]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Debug(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
		}

		[McpServerTool(
			"Log Informational Event",  // Title
			"Logs an informational event to the event log. The purpose of informational events is to provide a record of normal operational tasks being performed or events occurring.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogInformational(
			[McpStringParameter(MessageTitle, MessageDescription)]
			string Message,

			[McpStringParameter(ObjectTitle, ObjectDescription, 0, 128)]
			string Object = "",

			[McpStringParameter(ActorTitle, ActorDescription, 0, 128)]
			string Actor = "",

			[McpStringParameter(EventLevelTitle, EventLevelDescription)]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter(EventIdTitle, EventIdDescription, 0, 32)]
			string EventId = McpEventId,

			[McpStringParameter(FacilityTitle, FacilityDescription, 0, 256)]
			string Facility = "",

			[McpStringParameter(ModuleTitle, ModuleDescription, 0, 256)]
			string Module = "",

			[McpParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			[McpMetaDataArgument]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Informational(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
		}

		[McpServerTool(
			"Log Notice Event",  // Title
			"Logs an notice event to the event log. A Notice represents a significant condition or change that administrators should be aware of.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogNotice(
			[McpStringParameter(MessageTitle, MessageDescription)]
			string Message,

			[McpStringParameter(ObjectTitle, ObjectDescription, 0, 128)]
			string Object = "",

			[McpStringParameter(ActorTitle, ActorDescription, 0, 128)]
			string Actor = "",

			[McpStringParameter(EventLevelTitle, EventLevelDescription)]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter(EventIdTitle, EventIdDescription, 0, 32)]
			string EventId = McpEventId,

			[McpStringParameter(FacilityTitle, FacilityDescription, 0, 256)]
			string Facility = "",

			[McpStringParameter(ModuleTitle, ModuleDescription, 0, 256)]
			string Module = "",

			[McpParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			[McpMetaDataArgument]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Notice(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
		}

		[McpServerTool(
			"Log Warning Event",  // Title
			"Logs a warning event to the event log. Warning events warn operators of conditions that may lead to errors if they are not properly managed.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogWarning(
			[McpStringParameter(MessageTitle, MessageDescription)]
			string Message,

			[McpStringParameter(ObjectTitle, ObjectDescription, 0, 128)]
			string Object = "",

			[McpStringParameter(ActorTitle, ActorDescription, 0, 128)]
			string Actor = "",

			[McpStringParameter(EventLevelTitle, EventLevelDescription)]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter(EventIdTitle, EventIdDescription, 0, 32)]
			string EventId = McpEventId,

			[McpStringParameter(FacilityTitle, FacilityDescription, 0, 256)]
			string Facility = "",

			[McpStringParameter(ModuleTitle, ModuleDescription, 0, 256)]
			string Module = "",

			[McpParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			[McpMetaDataArgument]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Warning(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
		}

		[McpServerTool(
			"Log Error Event",  // Title
			"Logs an error event to the event log. Error events inform operators of normal error conditions. A normal error condition is typically an expected error.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogError(
			[McpStringParameter(MessageTitle, MessageDescription)]
			string Message,

			[McpStringParameter(ObjectTitle, ObjectDescription, 0, 128)]
			string Object = "",

			[McpStringParameter(ActorTitle, ActorDescription, 0, 128)]
			string Actor = "",

			[McpStringParameter(EventLevelTitle, EventLevelDescription)]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter(EventIdTitle, EventIdDescription, 0, 32)]
			string EventId = McpEventId,

			[McpStringParameter(FacilityTitle, FacilityDescription, 0, 256)]
			string Facility = "",

			[McpStringParameter(ModuleTitle, ModuleDescription, 0, 256)]
			string Module = "",

			[McpParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			[McpMetaDataArgument]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Error(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
		}

		[McpServerTool(
			"Log Critical Error Event",  // Title
			"Logs a critical error event to the event log. A critical error is an error so great that it could escalate into something graver if not addressed. Typically, a Critical error is typically an unexpected error.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogCritical(
			[McpStringParameter(MessageTitle, MessageDescription)]
			string Message,

			[McpStringParameter(ObjectTitle, ObjectDescription, 0, 128)]
			string Object = "",

			[McpStringParameter(ActorTitle, ActorDescription, 0, 128)]
			string Actor = "",

			[McpStringParameter(EventLevelTitle, EventLevelDescription)]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter(EventIdTitle, EventIdDescription, 0, 32)]
			string EventId = McpEventId,

			[McpStringParameter(FacilityTitle, FacilityDescription, 0, 256)]
			string Facility = "",

			[McpStringParameter(ModuleTitle, ModuleDescription, 0, 256)]
			string Module = "",

			[McpParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			[McpMetaDataArgument]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Critical(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
		}

		[McpServerTool(
			"Log Alert Event",  // Title
			"Logs an alert event to the event log. An alert error is so grave, that action must be taken immediately.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogAlert(
			[McpStringParameter(MessageTitle, MessageDescription)]
			string Message,

			[McpStringParameter(ObjectTitle, ObjectDescription, 0, 128)]
			string Object = "",

			[McpStringParameter(ActorTitle, ActorDescription, 0, 128)]
			string Actor = "",

			[McpStringParameter(EventLevelTitle, EventLevelDescription)]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter(EventIdTitle, EventIdDescription, 0, 32)]
			string EventId = McpEventId,

			[McpStringParameter(FacilityTitle, FacilityDescription, 0, 256)]
			string Facility = "",

			[McpStringParameter(ModuleTitle, ModuleDescription, 0, 256)]
			string Module = "",

			[McpParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			[McpMetaDataArgument]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Alert(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
		}

		[McpServerTool(
			"Log Emergency Event",  // Title
			"Logs an emergency event to the event log. An emergency error signals the system is unusable, or will become unusable if action is not taken immediately.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogEmergency(
			[McpStringParameter(MessageTitle, MessageDescription)]
			string Message,

			[McpStringParameter(ObjectTitle, ObjectDescription, 0, 128)]
			string Object = "",

			[McpStringParameter(ActorTitle, ActorDescription, 0, 128)]
			string Actor = "",

			[McpStringParameter(EventLevelTitle, EventLevelDescription)]
			[McpEnumValue(EventLevel.Minor, "Minor Event")]
			[McpEnumValue(EventLevel.Medium, "Medium Event")]
			[McpEnumValue(EventLevel.Major, "Major Event")]
			EventLevel Level = EventLevel.Minor,

			[McpStringParameter(EventIdTitle, EventIdDescription, 0, 32)]
			string EventId = McpEventId,

			[McpStringParameter(FacilityTitle, FacilityDescription, 0, 256)]
			string Facility = "",

			[McpStringParameter(ModuleTitle, ModuleDescription, 0, 256)]
			string Module = "",

			[McpParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			[McpMetaDataArgument]
			Dictionary<string, object>? MetaData = null)
		{
			Log.Emergency(Message, Object, Actor, EventId, Level, Facility, Module, GetTags(MetaData));
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

		[McpServerTool(
			"Search for Events",  // Title
			"Performs a search for events in the event log.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[return: McpParameter("Search Result", "Result set of the search for events, as well as information about if more events are available, and the offset to the next set of events.")]
		public async Task<SearchResult> Search(HttpRequest Request,

			[McpIntegerParameter("Offset", "Offset into search result set where returned events begin.", 0, int.MaxValue)]
			int Offset = 0,

			[McpIntegerParameter("Max Count", "Maximum number of results to return.", 1, 100)]
			int MaxCount = 20,

			[McpDateTimeParameter("From", "Start date/time for search, in UTC. If provided, only events newer than or equal to this point in time will be returned.")]
			DateTime? From = null,

			[McpDateTimeParameter("To", "End date/time for search, in UTC. If provided, only events older than or equal to this point in time will be returned.")]
			DateTime? To = null,

			[McpStringParameter("Message", "Message to search for. If provided, only events containing this message text will be returned.")]
			string? Message = null,

			[McpStringParameter("Object", "Object to search for. If provided, only events with this object will be returned. This property represents the object associated with the events.")]
			string? Object = null,

			[McpStringParameter("Actor", "Actor to search for. If provided, only events with this actor will be returned. This property represents the subject, or actor, performing the action resulting in the events being logged.")]
			string? Actor = null,

			[McpStringParameter("EventId", "Event ID to search for. If provided, only events with this event ID will be returned. Event IDs are used to identify a specific type of event, and is used collect related information in reports.")]
			string? EventId = null,

			[McpStringParameter("Module", "Module ID to search for. If provided, only events with this module will be returned. This property represents the module or component within the source that is the source of the event.")]
			string? Module = null,

			[McpStringParameter("Facility", "Facility ID to search for. If provided, only events with this facility will be returned. this property represents the subsystem or external component that is the source of the event.")]
			string? Facility = null,

			[McpParameter("Debug Events", "If provided, determines if debug events are to be included in the result. The purpose of debug events is to highlight technical information to developers, for troubleshooting.")]
			bool? Debug = null,

			[McpParameter("Informational Events", "If provided, determines if informational events are to be included in the result. The purpose of informational events is to provide a record of normal operational tasks being performed or events occurring.")]
			bool? Informational = null,

			[McpParameter("Notice Events", "If provided, determines if notice events are to be included in the result. A Notice represents a significant condition or change that administrators should be aware of.")]
			bool? Notice = null,

			[McpParameter("Warning Events", "If provided, determines if warning events are to be included in the result. Warning events warn operators of conditions that may lead to errors if they are not properly managed.")]
			bool? Warning = null,

			[McpParameter("Error Events", "If provided, determines if error events are to be included in the result. Error events inform operators of normal error conditions. A normal error condition is typically an expected error.")]
			bool? Error = null,

			[McpParameter("Critical Events", "If provided, determines if critical error events are to be included in the result. A critical error is an error so great that it could escalate into something graver if not addressed. Typically, a Critical error is typically an unexpected error.")]
			bool? Critical = null,

			[McpParameter("Alert Events", "If provided, determines if alerts events are to be included in the result. An alert error is so grave, that action must be taken immediately.")]
			bool? Alert = null,

			[McpParameter("Emergency Events", "If provided, determines if emergencies events are to be included in the result. An emergency error signals the system is unusable, or will become unusable if action is not taken immediately.")]
			bool? Emergency = null,

			[McpParameter("Minor Events", "If provided, determines if minor events are to be included in the result. Minor events occur frequently.")]
			bool? Minor = null,

			[McpParameter("Medium Events", "If provided, determines if medium events are to be included in the result. Medium events update something, or reports something could cause a major event.")]
			bool? Medium = null,

			[McpParameter("Major Events", "If provided, determines if major events are to be included in the result. Major events adds or destroys something, or reports something important is out of order.")]
			bool? Major = null)
		{
			if (From.HasValue && To.HasValue && From.Value > To.Value)
			{
				DateTime? Temp = To;
				To = From;
				From = Temp;
			}

			bool AllTypes =
				(!Debug.HasValue || Debug.Value) &&
				(!Informational.HasValue || Informational.Value) &&
				(!Notice.HasValue || Notice.Value) &&
				(!Warning.HasValue || Warning.Value) &&
				(!Error.HasValue || Error.Value) &&
				(!Critical.HasValue || Critical.Value) &&
				(!Alert.HasValue || Alert.Value) &&
				(!Emergency.HasValue || Emergency.Value);

			bool AllLevels =
				(!Minor.HasValue || Minor.Value) &&
				(!Medium.HasValue || Medium.Value) &&
				(!Major.HasValue || Major.Value);

			ChunkedList<KeyValuePair<string, object>> Tags = new ChunkedList<KeyValuePair<string, object>>()
					{
						new KeyValuePair<string, object>("From", From.ToString()),
						new KeyValuePair<string, object>("To", To.ToString()),
					};

			if (!string.IsNullOrEmpty(Object))
				Tags.Add(new KeyValuePair<string, object>("Object", Object));

			if (!string.IsNullOrEmpty(Actor))
				Tags.Add(new KeyValuePair<string, object>("Actor", Actor));

			if (!string.IsNullOrEmpty(EventId))
				Tags.Add(new KeyValuePair<string, object>("EventId", EventId));

			if (!string.IsNullOrEmpty(Module))
				Tags.Add(new KeyValuePair<string, object>("Module", Module));

			if (!string.IsNullOrEmpty(Facility))
				Tags.Add(new KeyValuePair<string, object>("Facility", Facility));

			if (!AllTypes)
			{
				if (Debug.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Debug", Debug));

				if (Informational.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Informational", Informational));

				if (Notice.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Notice", Notice));

				if (Warning.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Warning", Warning));

				if (Error.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Error", Error));

				if (Critical.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Critical", Critical));

				if (Alert.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Alert", Alert));

				if (Emergency.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Emergency", Emergency));
			}

			if (!AllLevels)
			{
				if (Minor.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Minor", Minor));

				if (Medium.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Medium", Medium));

				if (Major.HasValue)
					Tags.Add(new KeyValuePair<string, object>("Major", Major));
			}

			Log.Informational("Searching event log.",
				string.Empty,
				Request.RemoteEndPoint,
				McpEventId,
				Tags.ToArray());


			ChunkedList<Filter> Filters = new ChunkedList<Filter>()
			{
				new FilterFieldGreaterOrEqualTo("Timestamp", From),
				new FilterFieldLesserOrEqualTo("Timestamp", To)
			};

			AddFilter(Filters, "Object", Object);
			AddFilter(Filters, "Actor", Actor);
			AddFilter(Filters, "EventId", EventId);
			AddFilter(Filters, "Module", Module);
			AddFilter(Filters, "Facility", Facility);

			if (!AllTypes)
			{
				List<Filter> Ors = new List<Filter>();

				AddFilter(Ors, "Type", Debug, EventType.Debug);
				AddFilter(Ors, "Type", Informational, EventType.Informational);
				AddFilter(Ors, "Type", Notice, EventType.Notice);
				AddFilter(Ors, "Type", Warning, EventType.Warning);
				AddFilter(Ors, "Type", Error, EventType.Error);
				AddFilter(Ors, "Type", Critical, EventType.Critical);
				AddFilter(Ors, "Type", Alert, EventType.Alert);
				AddFilter(Ors, "Type", Emergency, EventType.Emergency);

				Filters.Add(new FilterOr(Ors.ToArray()));
			}

			if (!AllLevels)
			{
				List<Filter> Ors = new List<Filter>();

				AddFilter(Ors, "Level", Minor, EventLevel.Minor);
				AddFilter(Ors, "Level", Medium, EventLevel.Medium);
				AddFilter(Ors, "Level", Major, EventLevel.Major);

				Filters.Add(new FilterOr(Ors.ToArray()));
			}

			if (!string.IsNullOrEmpty(Message))
			{
				Filters.Add(new FilterCustom<PersistedEvent>(e => 
					e.Message.Contains(Message, StringComparison.CurrentCultureIgnoreCase)));
			}

			int NextOffset = Offset + MaxCount;
			int Left = MaxCount;
			bool More = false;
			ChunkedList<Event> Events = new ChunkedList<Event>();
			IEnumerable<PersistedEvent> RecordSet = await Database.Find<PersistedEvent>(
				Offset, MaxCount + 1, new FilterAnd(Filters.ToArray()), "-Timestamp");

			foreach (PersistedEvent Event in RecordSet)
			{
				if (Left-- > 0)
					Events.Add(new Event(Event));
				else
				{
					More = true;
					break;
				}
			}

			return new SearchResult(More, More ? NextOffset : (int?)null, Events.ToArray());
		}

		private static void AddFilter(ChunkedList<Filter> Filters, string ParameterName, string? Value)
		{
			if (!string.IsNullOrEmpty(Value))
			{
				if (Value.IndexOf('*') >= 0)
					Filters.Add(new FilterFieldLikeRegEx(ParameterName, CommonTypes.RegexStringEncode(Value.Replace("*", "__WILDCARD__")).Replace("__WILDCARD__", ".*")));
				else
					Filters.Add(new FilterFieldEqualTo(ParameterName, Value));
			}
		}

		private static void AddFilter(List<Filter> Filters, string ParameterName, bool? Value, Enum EnumValue)
		{
			if (Value.HasValue && Value.Value)
				Filters.Add(new FilterFieldEqualTo(ParameterName, EnumValue));
		}

	}
}
