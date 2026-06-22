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
			"Logs an event for debug purposes to the event log.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogDebug(
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

			[McpStringParameter("Facility", "The subsystem or external component that is the source of the event.")]
			string Facility = "", 

			[McpStringParameter("Module", "The module or component within the source that is the source of the event.")]
			string Module = "",

			[McpStringParameter("Meta Data", "Additional meta data that may be of interest to an observer, to log with the event, as key-value pairs.")]
			Dictionary<string, object>? MetaData = null)
		{
			int c = MetaData?.Count ?? 0;
			KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[c];

			if (c > 0)
			{
				int i = 0;

				foreach (KeyValuePair<string, object> P in MetaData!)
					Tags[i++] = P;
			}

			Log.Debug(Message, Object, Actor, "McpEvent", Level, Facility, Module, Tags);
		}
	}
}
