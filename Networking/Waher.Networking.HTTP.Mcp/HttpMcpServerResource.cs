using System;
using System.Collections.Generic;
using Waher.Content.Images;
using Waher.Events;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Abstract base class for HTTP-based Model Context Protocol (MCP) server resource.
	/// </summary>
	public abstract class HttpMcpServerResource : JsonRpcWebService
	{
		private readonly string name;
		private readonly string title;
		private readonly string version;
		private readonly string description;
		private readonly string instructions;
		private readonly Icons icons;
		private readonly Uri webSiteUri;

		/// <summary>
		/// Abstract base class for HTTP-based Model Context Protocol (MCP) server resource.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Name">Name of server.</param>
		/// <param name="Title">Title of server.</param>
		/// <param name="Version">Version of server.</param>
		/// <param name="Description">Description of server.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="Instructions">Instructions for server.</param>
		public HttpMcpServerResource(string ResourceName, string Name, string Title,
			string Version, string Description, Icon[] Icons, Uri WebSiteUri,
			string Instructions)
			: base(ResourceName, true, false)
		{
			this.name = Name;
			this.title = Title;
			this.version = Version;
			this.description = Description;
			this.icons = new Icons(Icons);
			this.webSiteUri = WebSiteUri;
			this.instructions = Instructions;
		}

		/// <summary>
		/// Protocol version of client, if available.
		/// </summary>
		public string? ClientProtocolVersion { get; private set; }

		/// <summary>
		/// Client capabilities, if available.
		/// </summary>
		public ClientCapabilities? ClientCapabilities { get; private set; }

		/// <summary>
		/// Information about client, if available.
		/// </summary>
		public Implementation? ClientInformation { get; private set; }

		/// <summary>
		/// If Server-Sent Events (SSE) are supported by the resource.
		/// </summary>
		public override bool SupportsServerSentEvents => true;

		/// <summary>
		/// Gets default icons, if any.
		/// </summary>
		/// <returns>Array of default icons. Empty, if none found.</returns>
		protected static Icon[] GetDefaultIcons()
		{
			if (Types.TryGetModuleParameter("FavIcon", out string Url))
			{
				return new Icon[]
				{
					new Icon(new Uri(Url), ImageCodec.ContentTypeIcon, null, null)
				};
			}
			else
				return Array.Empty<Icon>();
		}

		/// <summary>
		/// Gets a URI to the default web site, if any.
		/// </summary>
		/// <returns>URI, if available, null if not.</returns>
		protected static Uri? GetDefaultWebSite()
		{
			if (Types.TryGetModuleParameter("HomePage", out string Url) &&
				Uri.TryCreate(Url, UriKind.Absolute, out Uri WebSiteUri))
			{
				return WebSiteUri;
			}
			else
				return null;
		}

		/// <summary>
		/// MCP initialize method. Called by client to initialize connection and exchange 
		/// information about capabilities.
		/// </summary>
		/// <param name="ProtocolVersion">Protocol Version</param>
		/// <param name="Capabilities">Client capabilities</param>
		/// <param name="ClientInfo">Client information</param>
		/// <returns>Server capabilities and information.</returns>
		[JsonRpcMethod]
		protected Dictionary<string, object> Initialize(
			string ProtocolVersion,
			Dictionary<string, object> Capabilities,
			Dictionary<string, object> ClientInfo)
		{
			this.ClientProtocolVersion = ProtocolVersion;

			if (ClientCapabilities.TryParse(Capabilities, out ClientCapabilities CapabilitiesParsed))
				this.ClientCapabilities = CapabilitiesParsed;

			if (Implementation.TryParse(ClientInfo, out Implementation ClientInfoParsed))
				this.ClientInformation = ClientInfoParsed;

			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "protocolVersion", "2025-11-25" },
				{ "capabilities", new Dictionary<string, object>()
					{
						{ "prompts", new Dictionary<string, object>()
							{
								{ "listChanged", false }	// TODO (for instance, when configuring or editing what prompts are available)
							}
						},
						{ "resources", new Dictionary<string, object>()
							{
								{ "subscribe", false },
								{ "listChanged", false }	// TODO (for instance, when available resource change)
							}
						},
						{ "tools", new Dictionary<string, object>()
							{
								{ "listChanged", false }	// TODO (for instance, when configuring what tools are available)
							}
						},
						{ "logging", new Dictionary<string, object>() },
						{ "completions", new Dictionary<string, object>() },
						{ "tasks", new Dictionary<string, object>()
							{
								{ "list", new Dictionary<string, object>() },
								{ "cancel", new Dictionary<string, object>() },
								{ "requests", new Dictionary<string, object>()
									{
										{ "tools", new Dictionary<string, object>()
											{
												{ "call", new Dictionary<string, object>() }
											}
										}
									}
								}
							}
						},
						{ "experimental", new Dictionary<string, object>() }
					}
				},
				{ "serverInfo", new Dictionary<string,object>()
					{
						{ "name", this.name },
						{ "title", this.title },
						{ "version", this.version },
						{ "description", this.description },
						{ "icons", this.icons.ToJson() },
						{ "websiteUrl", this.webSiteUri.ToString() }
					}
				},
				{ "instructions", this.instructions }
			};

			return Result;
		}

		[JsonRpcMethod]
		protected void Notifications_Initialized(HttpRequest Request)
		{
			Log.Informational("MCP client initialized: " + Request.RemoteEndPoint,
				this.ResourceName, Request.RemoteEndPoint, "McpInitialized");
		}

		[JsonRpcMethod]
		protected Dictionary<string, object> Tools_List(HttpRequest Request,
			string? Cursor = null)
		{
			// TODO 

			return new Dictionary<string, object>()
			{
				{ "tools", Array.Empty<Dictionary<string, object>>() }
			};
		}

		[JsonRpcMethod]
		protected Dictionary<string, object> Prompts_List(HttpRequest Request,
			string? Cursor = null)
		{
			// TODO 

			return new Dictionary<string, object>()
			{
				{ "prompts", Array.Empty<Dictionary<string, object>>() }
			};
		}

		[JsonRpcMethod]
		protected Dictionary<string, object> Resources_List(HttpRequest Request,
			string? Cursor = null)
		{
			// TODO 

			return new Dictionary<string, object>()
			{
				{ "resources", Array.Empty<Dictionary<string, object>>() }
			};
		}
	}
}
