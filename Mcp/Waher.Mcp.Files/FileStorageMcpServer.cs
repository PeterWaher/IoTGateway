using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Mcp;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.HTTP.Mcp.Model.Resources;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Mcp.Files
{
	/// <summary>
	/// MCP Server resource for access to account-specific file storage.
	/// </summary>
	[OAuthResourceName("Account-Specific File Storage MCP Server")]
	[McpScopeRoot("MCP:Files")]
	public class FileStorageMcpServer : HttpMcpServerResource, IDisposable
	{
		internal const string BasePrivilege = OAuthResource.OAuthScopePrivilegePrefix + "MCP.Files";
		internal const string ToolsPrivilege = BasePrivilege + ".Tools";
		internal const string ListPrivilege = ToolsPrivilege + ".List";
		internal const string ReadPrivilege = ToolsPrivilege + ".Read";
		internal const string WritePrivilege = ToolsPrivilege + ".Write";
		internal const string DeletePrivilege = ToolsPrivilege + ".Delete";

		private readonly Dictionary<string, UsageRec> users;
		private readonly string rootFolder;
		private readonly FileSystemWatcher watcher;
		private readonly int userNameStart;
		private bool disposed = false;

		/// <summary>
		/// MCP Server resource for access to account-specific file storage.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="RootFolder">Root folder that will host the account-specific 
		/// files and folders.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public FileStorageMcpServer(string ResourceName, string RootFolder, 
			ISnifferSet? SnifferSet)
			: this(ResourceName, RootFolder,
				  GetDefaultIcons(), GetDefaultWebSite()
				  ?? new Uri("https://www.nuget.org/packages/Waher.Events/"), 
				  SnifferSet)
		{
		}

		/// <summary>
		/// MCP Server resource for access to account-specific file storage.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="RootFolder">Root folder that will host the account-specific 
		/// files and folders.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public FileStorageMcpServer(string ResourceName, string RootFolder, Icon[] Icons,
			Uri WebSiteUri, ISnifferSet? SnifferSet)
			: this(ResourceName,
				  RootFolder,
				  "FileStorage",   // Name
				  "File Storage",  // Title
				  typeof(FileStorageMcpServer).Assembly.GetName().Version.ToString(),
				  "A Model Context Protocol (MCP) server resource permitting MCP clients " +
				  "to store and manage persistant files in an account-specific file storage.",
				  Icons,
				  WebSiteUri,
				  "Use the resource list to get access to all files associated with the " +
				  "account. Files may be organized in folders. Tools are available to " +
				  "read, update and delete files and folders. No executable files must " +
				  "be stored in file storage. Resource URIs are all local and unique to " +
				  "the agent. They cannot and must not be shared.", 
				  SnifferSet)
		{
		}

		/// <summary>
		/// MCP Server resource for access to account-specific file storage.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="RootFolder">Root folder that will host the account-specific 
		/// files and folders.</param>
		/// <param name="Name">Name of server.</param>
		/// <param name="Title">Title of server.</param>
		/// <param name="Version">Version of server.</param>
		/// <param name="Description">Description of server.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="Instructions">Instructions for server.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public FileStorageMcpServer(string ResourceName, string RootFolder, string Name,
			string Title, string Version, string Description, Icon[] Icons, Uri WebSiteUri,
			string Instructions, ISnifferSet? SnifferSet)
			: base(ResourceName, Name, Title, Version, Description, Icons, WebSiteUri,
				Instructions, SnifferSet)
		{
			this.rootFolder = Path.GetFullPath(RootFolder);

			if (!Directory.Exists(this.rootFolder))
				Directory.CreateDirectory(this.rootFolder);

			this.users = new Dictionary<string, UsageRec>();
			this.userNameStart = this.rootFolder.Length;

			if (!this.rootFolder.EndsWith(Path.DirectorySeparatorChar) &&
				!this.rootFolder.EndsWith('/'))
			{
				this.userNameStart++;
			}

			this.watcher = new FileSystemWatcher(this.rootFolder, "*.*")
			{
				IncludeSubdirectories = true,
				EnableRaisingEvents = true,
				InternalBufferSize = 65536,
				NotifyFilter =
					NotifyFilters.Attributes |
					NotifyFilters.CreationTime |
					NotifyFilters.DirectoryName |
					NotifyFilters.FileName |
					NotifyFilters.LastAccess |
					NotifyFilters.LastWrite |
					NotifyFilters.Security |
					NotifyFilters.Size
			};
			this.watcher.Created += this.FileCreated;
			this.watcher.Renamed += this.FileRenamed;
			this.watcher.Deleted += this.FileDeleted;
			this.watcher.Changed += this.FileUpdated;
		}

		private class UsageRec
		{
			public IUser? User;
			public Session? Session;
			public string? Url;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.watcher.Dispose();
			}
		}

		/// <summary>
		/// If resources published by the MCP Server require authentication. If true, 
		/// the client must authenticate before resources can be listed or read.
		/// </summary>
		public override bool ResourcesRequireAuthentication => true;

		/// <summary>
		/// If the MCP server has resource capabilities.
		/// </summary>
		public override bool HasResources => true;

		/// <summary>
		/// Gets available resources.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Array of resources.</returns>
		public override Task<Resource[]> GetResources(HttpRequest Request, IUser? User,
			Session? Session)
		{
			if (User is null)
				return Task.FromResult(Array.Empty<Resource>());

			string UserName = User.UserName;
			string Folder = Path.Combine(this.rootFolder, UserName);
			if (!Directory.Exists(Folder))
			{
				Directory.CreateDirectory(Folder);
				return Task.FromResult(Array.Empty<Resource>());
			}

			string[] Files = Directory.GetFiles(Folder, "*.*", SearchOption.AllDirectories);
			int i, c = Files.Length;
			Resource[] Resources = new Resource[c];
			string ResourceName = Request.Header.GetURL(false, false);

			lock (this.users)
			{
				this.users[UserName] = new UsageRec()
				{
					User = User,
					Url = ResourceName,
					Session = Session
				};
			}

			for (i = 0; i < c; i++)
			{
				string FullFileName = Files[i];
				string FileName = FullFileName[(Folder.Length + 1)..];
				string Uri = CreateFileUrl(ResourceName, FileName);
				FileInfo FileInfo = new FileInfo(FullFileName);

				Resources[i] = new FileResource(FileName, string.Empty, string.Empty,
					new Uri(Uri), FullFileName, null, FileInfo.Length);
			}

			return Task.FromResult(Resources);
		}

		/// <summary>
		/// Tries to get a resource, given its URI.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Resource, if found (and user has access rights to it), null otherwise.</returns>
		public override Task<Resource?> TryGetResource(HttpRequest Request, IUser? User,
			Uri Uri, Session? Session)
		{
			if (User is null)
				return Task.FromResult<Resource?>(null);

			string UserName = User.UserName;
			string FileName = Uri.OriginalString;
			string ResourceName = Request.Header.GetURL(false, false);

			if (!FileName.StartsWith(ResourceName))
				return Task.FromResult<Resource?>(null);

			int c = ResourceName.Length;
			if (!ResourceName.EndsWith('/'))
				c++;

			FileName = HttpUtility.UrlDecode(FileName[c..]).Replace('/', Path.DirectorySeparatorChar);

			lock (this.users)
			{
				this.users[UserName] = new UsageRec()
				{
					User = User,
					Url = ResourceName,
					Session = Session
				};
			}

			string Folder = Path.Combine(this.rootFolder, UserName);
			if (!Directory.Exists(Folder))
			{
				Directory.CreateDirectory(Folder);
				return Task.FromResult<Resource?>(null);
			}

			FileName = Path.Combine(Folder, FileName);
			if (!File.Exists(FileName))
				return Task.FromResult<Resource?>(null);

			FileInfo FileInfo = new FileInfo(FileName);

			return Task.FromResult<Resource?>(new FileResource(FileName, string.Empty,
				string.Empty, Uri, FileName, null, FileInfo.Length));
		}

		private void FileCreated(object sender, FileSystemEventArgs e)
		{
			if (this.TryGetUser(e.FullPath, out UsageRec? Usage, out _))
				this.ResourcesUpdated(Usage.User!);
		}

		private void FileDeleted(object sender, FileSystemEventArgs e)
		{
			if (this.TryGetUser(e.FullPath, out UsageRec? Usage, out string? LocalFileName))
			{
				Usage.Session?.Unsubscribe(CreateFileUrl(Usage.Url, LocalFileName));
				this.ResourcesUpdated(Usage.User!);
			}
		}

		private static string CreateFileUrl(string? BaseUrl, string LocalFileName)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(BaseUrl);

			LocalFileName = LocalFileName.Replace(Path.DirectorySeparatorChar, '/');

			foreach (string Part in LocalFileName.Split('/'))
			{
				sb.Append('/');
				sb.Append(HttpUtility.UrlEncode(Part));
			}

			return sb.ToString();
		}

		private void FileRenamed(object sender, RenamedEventArgs e)
		{
			if (this.TryGetUser(e.OldFullPath, out UsageRec? Usage, out string? LocalFileName))
			{
				Usage.Session?.Unsubscribe(CreateFileUrl(Usage.Url, LocalFileName));
				this.ResourcesUpdated(Usage.User!);
			}
		}

		private bool TryGetUser(string FileName, [NotNullWhen(true)] out UsageRec? Rec,
			[NotNullWhen(true)] out string? LocalFileName)
		{
			Rec = null;
			LocalFileName = null;

			if (!FileName.StartsWith(this.rootFolder))
				return false;

			int i = FileName.IndexOf(Path.DirectorySeparatorChar, this.userNameStart);
			if (i < 0)
				return false;

			string UserName = FileName[this.userNameStart..i];
			LocalFileName = FileName[(i + 1)..];

			lock (this.users)
			{
				if (!this.users.TryGetValue(UserName, out Rec))
					return false;
			}

			return true;
		}

		private void FileUpdated(object sender, FileSystemEventArgs e)
		{
			if (this.TryGetUser(e.FullPath, out UsageRec? Rec, out string? LocalFileName))
			{
				string Url = Rec.Url + '/' + LocalFileName.Replace(Path.DirectorySeparatorChar, '/');

				this.ResourceUpdated(Rec.User!, new Uri(Url));
			}
		}

	}
}
