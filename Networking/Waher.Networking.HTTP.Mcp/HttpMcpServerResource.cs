using System;
using System.Collections.Generic;
using Waher.Networking.HTTP.JsonRpc;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// HTTP-based Model Context Protocol (MCP) server resource.
	/// </summary>
	public class HttpMcpServerResource : JsonRpcWebService
	{
		private delegate Dictionary<string, object> InitializeDelegate(
			string ProtocolVersion,
			Dictionary<string, object> Capabilities,
			Dictionary<string, object> ClientInfo);

		private readonly string name;
		private readonly string title;
		private readonly string version;
		private readonly string description;
		private readonly string instructions;
		private readonly Icons icons;
		private readonly Uri webSiteUri;

		/// <summary>
		/// HTTP-based Model Context Protocol (MCP) resource.
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

			this.Register((InitializeDelegate)this.Initialize);
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

		private Dictionary<string, object> Initialize(
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
						{ "experimental", new Dictionary<string, object>() },
						{ "logging", new Dictionary<string, object>() },
						{ "completions", new Dictionary<string, object>() },
						{ "prompts", new Dictionary<string, object>()
							{
								{ "listChanged", false }
							}
						},
						{ "resources", new Dictionary<string, object>()
							{
								{ "subscribe", false },
								{ "listChanged", false }
							}
						},
						{ "tools", new Dictionary<string, object>()
							{
								{ "listChanged", false }
							}
						},
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
					}
				},
				{ "serverInfo", new Dictionary<string,object>()
					{
						{ "name", this.name },
						{ "title", this.title },
						{ "version", this.version },
						{ "description", this.description },
						{ "icons", this.icons.ToJson() },
						{ "websiteUrl", this.webSiteUri }
					}
				},
				{ "instructions", this.instructions }
			};

			return Result;
		}
	}
}
