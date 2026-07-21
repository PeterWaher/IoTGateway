using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Networking.HTTP.Mcp;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;

namespace Waher.Mcp.Files
{
	/// <summary>
	/// MCP Server resource for access to account-specific file storage.
	/// </summary>
	[OAuthResourceName("Account-Specific File Storage MCP Server")]
	[McpScopeRoot("MCP:Files")]
	public class FileStorageMcpServer : HttpMcpServerResource
	{
		internal const string BasePrivilege = OAuthResource.OAuthScopePrivilegePrefix + "MCP.Files";
		internal const string ToolsPrivilege = BasePrivilege + ".Tools";
		internal const string ListPrivilege = ToolsPrivilege + ".List";
		internal const string ReadPrivilege = ToolsPrivilege + ".Read";
		internal const string WritePrivilege = ToolsPrivilege + ".Write";
		internal const string DeletePrivilege = ToolsPrivilege + ".Delete";

		private readonly string rootFolder;

		/// <summary>
		/// MCP Server resource for access to account-specific file storage.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="RootFolder">Root folder that will host the account-specific 
		/// file storage.</param>
		public FileStorageMcpServer(string ResourceName, string RootFolder)
			: this(ResourceName, RootFolder,
				  GetDefaultIcons(), GetDefaultWebSite()
				  ?? new Uri("https://www.nuget.org/packages/Waher.Events/"))
		{
		}

		/// <summary>
		/// MCP Server resource for access to account-specific file storage.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="RootFolder">Root folder that will host the account-specific 
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		public FileStorageMcpServer(string ResourceName, string RootFolder, Icon[] Icons,
			Uri WebSiteUri)
			: this(ResourceName,
				  RootFolder,
				  "FileStorage",   // Name
				  "File Storage",  // Title
				  typeof(FileStorageMcpServer).Assembly.GetName().Version.ToString(),
				  "A Model Context Protocol (MCP) server resource permitting MCP clients " +
				  "to store and manage persistant files in an account-specific file storage.",
				  Icons,
				  WebSiteUri,
				  "Use the resource list to get access to folders and files in the " +
				  "account-specific root folder. Tools are available to list sub-folders " +
				  "files in folders. Tools are also available to read, update and delete " +
				  "files and folders.")
		{
		}

		/// <summary>
		/// MCP Server resource for access to account-specific file storage.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="RootFolder">Root folder that will host the account-specific 
		/// <param name="Name">Name of server.</param>
		/// <param name="Title">Title of server.</param>
		/// <param name="Version">Version of server.</param>
		/// <param name="Description">Description of server.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="Instructions">Instructions for server.</param>
		public FileStorageMcpServer(string ResourceName, string RootFolder, string Name, 
			string Title, string Version, string Description, Icon[] Icons, Uri WebSiteUri,
			string Instructions)
			: base(ResourceName, Name, Title, Version, Description, Icons, WebSiteUri,
				Instructions)
		{
			this.rootFolder = Path.GetFullPath(RootFolder);

			if (!Directory.Exists(this.rootFolder))
				Directory.CreateDirectory(this.rootFolder);
		}

		/// <summary>
		/// Gets available resources.
		/// </summary>
		/// <returns>Array of resources.</returns>
		public override Task<Resource[]> GetResources()
		{
			return Task.FromResult(Array.Empty<Resource>());
		}

	}
}
