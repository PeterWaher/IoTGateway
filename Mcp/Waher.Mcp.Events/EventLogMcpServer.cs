using System;
using System.Collections.Generic;
using Waher.Events;
using Waher.Networking.HTTP.Mcp;

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
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		public void LogDebug(
			string Message, 
			string Object = "", 
			string Actor = "",
			EventLevel Level = EventLevel.Minor, 
			string Facility = "", 
			string Module = "",
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
