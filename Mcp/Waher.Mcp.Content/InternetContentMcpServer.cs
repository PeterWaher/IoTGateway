using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.Mcp;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Mcp.Content
{
	/// <summary>
	/// MCP Server resource for access to Internet Content.
	/// </summary>
	[OAuthResourceName("Internet Content MCP Server")]
	[McpScopeRoot("MCP:InternetContent")]
	public class InternetContentMcpServer : HttpMcpServerResource
	{
		internal const string BasePrivilege = OAuthResource.OAuthScopePrivilegePrefix + "MCP.InternetContent";
		internal const string ToolsPrivilege = BasePrivilege + ".Tools";
		internal const string GetPrivilege = ToolsPrivilege + ".Get";
		internal const string PostPrivilege = ToolsPrivilege + ".Post";
		internal const string PutPrivilege = ToolsPrivilege + ".Put";
		internal const string DeletePrivilege = ToolsPrivilege + ".Delete";
		internal const string QueryPrivilege = ToolsPrivilege + ".Query";

		/// <summary>
		/// MCP Server resource for access to Internet Content.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public InternetContentMcpServer(string ResourceName)
			: this(ResourceName,
				  GetDefaultIcons(), GetDefaultWebSite()
				  ?? new Uri("https://www.nuget.org/packages/Waher.Events/"))
		{
		}

		/// <summary>
		/// MCP Server resource for access to Internet Content.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		public InternetContentMcpServer(string ResourceName, Icon[] Icons, Uri WebSiteUri)
			: this(ResourceName,
				  "InternetContent",   // Name
				  "Internet Content",  // Title
				  typeof(InternetContentMcpServer).Assembly.GetName().Version.ToString(),
				  "A Model Context Protocol (MCP) server resource permitting MCP clients " +
				  "to access Internet Content using GET, POST, PUT, DELETE and QUERY methods, " +
				  "as well as encoding and decoding Internet Content.",
				  Icons,
				  WebSiteUri,
				  "Use the tools provided to access Internet Content securely. " +
				  "All requests are logged, and monitored for security purposes. " +
				  "Access to resources must use secure secure URI schemes such as HTTPS " +
				  "instead of unsecure ones such as HTTP.")
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
		public InternetContentMcpServer(string ResourceName, string Name, string Title,
			string Version, string Description, Icon[] Icons, Uri WebSiteUri,
			string Instructions)
			: base(ResourceName, Name, Title, Version, Description, Icons, WebSiteUri,
				Instructions)
		{
		}

		/// <summary>
		/// Retrieves content from the Internet, by accessing a resource using the GET method.
		/// </summary>
		/// <param name="Uri">The URI of the resource to access.</param>
		/// <param name="Accept">The Accept header to use when retrieving the resource.</param>
		/// <param name="AcceptLanguage">The Accept-Language header to use when retrieving the resource.</param>
		/// <param name="Timeout">Optional timeout in milliseconds for the request.</param>
		/// <param name="AdditionalHeaders">Additional headers to include in the request.</param>
		/// <returns>The content received and decoded.</returns>
		[McpServerTool(
			"Get",  // Title
			"Retrieves content from the Internet, by accessing a resource using the GET method.",   // Description
			"",     // IconsMethod, use default icons
			false,  // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			true,	// Idempotent
			true)]	// OpenWorldAccess
		[RequiredPrivilege(GetPrivilege)]
		[return: McpParameter("Result", "Content received and decoded.")]
		public async Task<object> Get(
			[McpUriParameter("URI", "URI of the resource to access.")]
			Uri Uri,

			[McpStringParameter("Accept", "Accept header to use when accessing the resource. It informs the web server what Internet Content-Type you are expecting the content of the response to be enceded with.")]
			string Accept = "*/*",

			[McpStringParameter("Accept-Language", "Optional Accept-Language header to use when accessing the resource. It informs the web server what language you expect human-readable content to be written in.")]
			string AcceptLanguage = "",

			[McpIntegerParameter("Timeout", "Optional timeout in milliseconds for the request.", 1, 60000)]
			int? Timeout = null,

			[McpParameter("AdditionalHeaders", "Additional headers to include in the request.")]
			Dictionary<string, string>? AdditionalHeaders = null)
		{
			ContentResponse Content = await InternetContent.GetAsync(Uri, Certificate,
				Timeout ?? InternetContent.DefaultTimeout, 
				GetHeaders(Accept, AcceptLanguage, AdditionalHeaders));

			Content.AssertOk();

			return Content.Decoded;
		}

		private static X509Certificate? Certificate
		{
			get
			{
				if (Types.TryGetModuleParameter("X509", out object Obj) &&
					Obj is X509Certificate Certificate)
				{
					return Certificate;
				}
				else
					return null;
			}
		}

		private static KeyValuePair<string, string>[] GetHeaders(string Accept,
			string AcceptLanguage, Dictionary<string, string>? AdditionalHeaders)
		{
			ChunkedList<KeyValuePair<string, string>> Headers =
				new ChunkedList<KeyValuePair<string, string>>(2 + (AdditionalHeaders?.Count ?? 0));

			if (!string.IsNullOrEmpty(Accept))
				Headers.Add(new KeyValuePair<string, string>("Accept", Accept));

			if (!string.IsNullOrEmpty(AcceptLanguage))
				Headers.Add(new KeyValuePair<string, string>("Accept-Language", AcceptLanguage));

			if (!(AdditionalHeaders is null))
				Headers.AddRange(AdditionalHeaders);

			return Headers.ToArray();
		}

		/// <summary>
		/// Posts information to a resource on the Internet using the POST method, and returns 
		/// the content that is returned.
		/// </summary>
		/// <param name="Uri">The URI of the resource to access.</param>
		/// <param name="Payload">The payload to post to the resource.</param>
		/// <param name="Accept">The Accept header to use when retrieving the resource.</param>
		/// <param name="AcceptLanguage">The Accept-Language header to use when retrieving the resource.</param>
		/// <param name="Timeout">Optional timeout in milliseconds for the request.</param>
		/// <param name="AdditionalHeaders">Additional headers to include in the request.</param>
		/// <returns>The content received and decoded.</returns>
		[McpServerTool(
			"Post",  // Title
			"Posts information to a resource on the Internet using the POST method, and returns the content that is returned.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,	// CanDestroyEnvironment
			false,  // Idempotent
			true)]	// OpenWorldAccess
		[RequiredPrivilege(GetPrivilege)]
		[return: McpParameter("Result", "Content received and decoded.")]
		public async Task<object> Post(
			[McpUriParameter("URI", "URI of the resource to access.")]
			Uri Uri,

			[McpParameter("Payload", "The payload to post to the resource.")]
			object? Payload = null,

			[McpStringParameter("Accept", "Accept header to use when accessing the resource. It informs the web server what Internet Content-Type you are expecting the content of the response to be enceded with.")]
			string Accept = "*/*",

			[McpStringParameter("Accept-Language", "Optional Accept-Language header to use when accessing the resource. It informs the web server what language you expect human-readable content to be written in.")]
			string AcceptLanguage = "",

			[McpIntegerParameter("Timeout", "Optional timeout in milliseconds for the request.", 1, 60000)]
			int? Timeout = null,

			[McpParameter("AdditionalHeaders", "Additional headers to include in the request.")]
			Dictionary<string, string>? AdditionalHeaders = null)
		{
			ContentResponse Content = await InternetContent.PostAsync(Uri, Payload,
				Certificate, Timeout ?? InternetContent.DefaultTimeout,
				GetHeaders(Accept, AcceptLanguage, AdditionalHeaders));

			Content.AssertOk();

			return Content.Decoded;
		}

	}
}
