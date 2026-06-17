using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Content.Images;
using Waher.Events;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Client;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Abstract base class for HTTP-based Model Context Protocol (MCP) server resource.
	/// </summary>
	public abstract class HttpMcpServerResource : JsonRpcWebService
	{
		private const int PageSize = 20;
		private readonly Dictionary<string, Tool> tools = new Dictionary<string, Tool>();

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
			this.Name = Name;
			this.Title = Title;
			this.Version = Version;
			this.Description = Description;
			this.Icons = new Icons(Icons);
			this.WebSiteUri = WebSiteUri;
			this.Instructions = Instructions;

			if (this.Icons.Empty)
			{
				Icon[] DefaultIcons = GetDefaultIcons();
				if (DefaultIcons.Length > 0)
					this.Icons = new Icons(DefaultIcons);
			}

			foreach (MethodInfo Method in this.GetType().GetMethods(BindingFlags.Instance |
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (Method.GetCustomAttribute<McpServerToolAttribute>() is
					McpServerToolAttribute McpServerToolAttribute)
				{
					this.RegisterTool(Method, McpServerToolAttribute);
				}
			}
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
		/// Name of server.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Title of server.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Version of server.
		/// </summary>
		public string Version { get; private set; }

		/// <summary>
		/// Description of server.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Icons of server.
		/// </summary>
		public Icons Icons { get; private set; }

		/// <summary>
		/// Website URI of server.
		/// </summary>
		public Uri WebSiteUri { get; private set; }

		/// <summary>
		/// Instructions for server.
		/// </summary>
		public string Instructions { get; private set; }

		/// <summary>
		/// Registers a MCP Server tool.
		/// </summary>
		/// <param name="Method"></param>
		/// <param name="Attributes"></param>
		/// <exception cref="Exception"></exception>
		public void RegisterTool(MethodInfo Method, McpServerToolAttribute Attributes)
		{
			lock (this.tools)
			{
				string Name = Method.Name;

				if (this.tools.ContainsKey(Name))
					throw new Exception("Tool already registered: " + Name);

				this.tools[Name] = new Tool(Method, Attributes.Title,
					Attributes.Description, Attributes.IconsMethod,
					Attributes.CanModifyEnvironment, Attributes.CanDestroyEnvironment,
					Attributes.Idempotent, Attributes.OpenWorldAccess);
			}

			// TODO: Send notification to clients about new tool.
			// TODO: Declare tool notification in capabilities.
		}

		/// <summary>
		/// Gets default icons, if any.
		/// </summary>
		/// <returns>Array of default icons. Empty, if none found.</returns>
		public static Icon[] GetDefaultIcons()
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
		public static Uri? GetDefaultWebSite()
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
						{ "name", this.Name },
						{ "title", this.Title },
						{ "version", this.Version },
						{ "description", this.Description },
						{ "icons", this.Icons.ToJson() },
						{ "websiteUrl", this.WebSiteUri.ToString() }
					}
				},
				{ "instructions", this.Instructions }
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
		protected async Task<Dictionary<string, object>> Tools_List(HttpRequest Request,
			string? Cursor = null)
		{
			int Offset = 0;
			int MaxCount = PageSize;

			if (!string.IsNullOrEmpty(Cursor))
			{
				if (!int.TryParse(Cursor, out Offset) || Offset < 0)
					throw new Exception("Invalid cursor.");
			}

			ChunkedList<Tool> Tools = new ChunkedList<Tool>();
			Dictionary<string, object>[] ToolsJson;
			int Next = Offset + MaxCount;

			Dictionary<string, object> Result = new Dictionary<string, object>();

			lock (this.tools)
			{
				foreach (Tool Tool in this.tools.Values)
				{
					if (MaxCount <= 0)
					{
						Result["nextCursor"] = Next.ToString();
						break;
					}

					if (Offset > 0)
					{
						Offset--;
						continue;
					}

					Tools.Add(Tool);
					MaxCount--;
				}
			}

			int i = 0;
			int c = Tools.Count;

			ToolsJson = new Dictionary<string, object>[c];

			foreach (Tool Tool in Tools)
				ToolsJson[i++] = await Tool.ToJson(this);

			Result["tools"] = ToolsJson;

			return Result;
		}

		[JsonRpcMethod]
		protected Dictionary<string, object> Prompts_List(HttpRequest Request,
			string? Cursor = null)
		{
			// TODO: prompts/list

			return new Dictionary<string, object>()
			{
				{ "prompts", Array.Empty<Dictionary<string, object>>() }
			};
		}

		[JsonRpcMethod]
		protected Dictionary<string, object> Resources_List(HttpRequest Request,
			string? Cursor = null)
		{
			// TODO: resources/list

			return new Dictionary<string, object>()
			{
				{ "resources", Array.Empty<Dictionary<string, object>>() }
			};
		}
	}
}
