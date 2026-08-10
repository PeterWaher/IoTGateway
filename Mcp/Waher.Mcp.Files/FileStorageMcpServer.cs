using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.JsonRpc.Transports;
using Waher.Networking.HTTP.Mcp;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.HTTP.Mcp.Model.Resources;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Networking.Sniffers;
using Waher.Runtime.Collections;
using Waher.Runtime.IO;
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
		internal const string ResourcesPrivilege = BasePrivilege + ".Resources";
		internal const string ListPrivilege = ResourcesPrivilege + ".List";
		internal const string ReadPrivilege = ResourcesPrivilege + ".Read";
		internal const string CreatePrivilege = ToolsPrivilege + ".Create";
		internal const string AppendPrivilege = ToolsPrivilege + ".Append";
		internal const string UpdatePrivilege = ToolsPrivilege + ".Update";
		internal const string DeletePrivilege = ToolsPrivilege + ".Delete";
		internal const string SearchPrivilege = ToolsPrivilege + ".Search";
		internal const string EditPrivilege = ToolsPrivilege + ".Edit";

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
			: this(ResourceName, RootFolder, GetDefaultIcons(), null, SnifferSet)
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
			Uri? WebSiteUri, ISnifferSet? SnifferSet)
			: this(ResourceName,
				  RootFolder,
				  "FileStorage",   // Name
				  "File Storage",  // Title
				  typeof(FileStorageMcpServer).Assembly.GetName().Version.ToString(),
				  "A Model Context Protocol (MCP) server resource permitting MCP clients " +
				  "to store and manage persistent files in an account-specific file storage.",
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
			string Title, string Version, string Description, Icon[] Icons, Uri? WebSiteUri,
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
		/// MCP server resource documentation, as an array of key-value pairs.
		/// The Key represents Markdown (true) or plain text (false), and the Value
		/// represents the documentation text. Each entry in the array represents a
		/// paragraph.
		/// </summary>
		public override KeyValuePair<bool, string>[] ResourceDocumentation
		{
			get
			{
				return new KeyValuePair<bool, string>[]
				{
					new KeyValuePair<bool, string>(true,
						"Each resource represents a file in the account-specific file " +
						"storage. As files are created, modified, renamed or deleted, " +
						"resource events are propagated to clients that receive " +
						"notifications (new, renamed or deleted files) or have resource " +
						"subscriptions active (modifed files)."),
					new KeyValuePair<bool, string>(true,
						"**Note**: Files are account-specific. This means that other " +
						"clients have different sets of files. This means that URIs " +
						"cannot be meaningfully shared, as they point to different " +
						"files in each account-specific storage. The purpose of the " +
						"account-specific storage is to store information for later " +
						"processing or reference.")
				};
			}
		}

		/// <summary>
		/// Lists available MCP server resources.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of resources.</returns>
		[RequiredPrivilege(ListPrivilege)]
		protected override Task<Dictionary<string, object>?> Resources_List(
			IJsonRpcCall Call, string? Cursor = null)
		{
			return base.Resources_List(Call, Cursor);
		}

		/// <summary>
		/// Reads an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Uri">URI of the resource to read.</param>
		/// <param name="_Meta">Associated meta-data, if available.</param>
		/// <returns>Dictionary containing the result of the tool call.</returns>
		[RequiredPrivilege(ReadPrivilege)]
		protected override async Task<Dictionary<string, object>?> Resources_Read(
			IJsonRpcCall Call, Uri Uri, 
			[JsonRpcMetaDataArgument] object? _Meta = null)
		{
			return await base.Resources_Read(Call, Uri, _Meta);
		}

		/// <summary>
		/// Subscribes to an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Uri">URI of the resource to subscribe to.</param>
		[RequiredPrivilege(ListPrivilege)]
		protected override Task Resources_Subscribe(
			IJsonRpcCall Call, Uri Uri)
		{
			return base.Resources_Subscribe(Call, Uri);
		}

		/// <summary>
		/// Unsubscribes from an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Uri">URI of the resource to unsubscribe from.</param>
		[RequiredPrivilege(ListPrivilege)]
		protected override async Task Resources_Unsubscribe(
			IJsonRpcCall Call, Uri Uri)
		{
			await base.Resources_Unsubscribe(Call, Uri);
		}

		/// <summary>
		/// Gets available resources.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Array of resources.</returns>
		public override Task<Resource[]> GetResources(IJsonRpcCall Call, IUser? User,
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
			string BaseUri = Call.GetBaseUrl();

			lock (this.users)
			{
				this.users[UserName] = new UsageRec()
				{
					User = User,
					Url = BaseUri,
					Session = Session
				};
			}

			for (i = 0; i < c; i++)
			{
				string FullFileName = Files[i];
				string FileName = FullFileName[(Folder.Length + 1)..];
				string Uri = CreateFileUrl(BaseUri, FileName);
				FileInfo FileInfo = new FileInfo(FullFileName);

				Resources[i] = new FileResource(FileName, string.Empty, string.Empty,
					new Uri(Uri), FullFileName, null, FileInfo.Length);
			}

			return Task.FromResult(Resources);
		}

		/// <summary>
		/// Tries to get a resource, given its URI.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Resource, if found (and user has access rights to it), null otherwise.</returns>
		public override Task<Resource?> TryGetResource(IJsonRpcCall Call, IUser? User,
			Uri Uri, Session? Session)
		{
			if (User is null)
				return Task.FromResult<Resource?>(null);

			string UserName = User.UserName;
			string FileName = Uri.OriginalString;
			string BareUri = Call.GetBaseUrl();

			if (!FileName.StartsWith(BareUri))
				return Task.FromResult<Resource?>(null);

			int c = BareUri.Length;
			if (!BareUri.EndsWith('/'))
				c++;

			FileName = HttpUtility.UrlDecode(FileName[c..]).Replace('/', Path.DirectorySeparatorChar);

			lock (this.users)
			{
				this.users[UserName] = new UsageRec()
				{
					User = User,
					Url = BareUri,
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
				string Url = CreateFileUrl(Rec.Url, LocalFileName);
				this.ResourceUpdated(Rec.User!, new Uri(Url));
			}
		}

		private static void AssertLocalFileNameOk(string LocalFileName)
		{
			AssertFileNameOk("Local file name", LocalFileName, false);
		}

		private static void AssertPatternOk(string Pattern)
		{
			if (Pattern.Contains('/') ||
				Pattern.Contains('\\') ||
				Pattern.Contains(Path.DirectorySeparatorChar))
			{
				throw new BadRequestException("Pattern cannot contain path characters.");
			}

			AssertFileNameOk("Pattern", Pattern, true);
		}

		private static void AssertFileNameOk(string Name, string FileName, bool Pattern)
		{
			if (string.IsNullOrEmpty(FileName))
				throw new BadRequestException(Name + " cannot be empty.");

			if (FileName.StartsWith('/') ||
				FileName.StartsWith('\\') ||
				FileName.StartsWith(Path.DirectorySeparatorChar))
			{
				throw new BadRequestException(Name + " cannot start with a path character.");
			}

			if (FileName.Contains(".."))
				throw new BadRequestException(Name + " cannot contain double period characters.");

			char[] Invalid = Pattern ? GetInvalidPatternChars() : Path.GetInvalidFileNameChars();

			if (FileName.IndexOfAny(Invalid) >= 0)
				throw new BadRequestException(Name + " contains invalid characters.");

			if (string.IsNullOrEmpty(Path.GetExtension(FileName)))
				throw new BadRequestException(Name + " must have a file extension.");
		}

		private static char[] GetInvalidPatternChars()
		{
			char[] Invalid = Path.GetInvalidFileNameChars();
			ChunkedList<char> List = new ChunkedList<char>(Invalid.Length);
			List.AddRange(Invalid);
			List.Remove('*');
			List.Remove('?');
			return List.ToArray();
		}

		/// <summary>
		/// Creates a text file in account-specific file storage.
		/// If a file with the same name exists, it is replaced.
		/// Text contents is stored UTF-8 encoded, with a Byte-Order-Mark (BOM).
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="LocalFileName">Local file name. Must not contain double period 
		/// characters or begin with a path character, to attempt to escape file 
		/// storage area. File extension must match Internet Content-Type of file 
		/// contents.</param>
		/// <param name="Content">Text content of file to create. The contents must not 
		/// be harmful or contain illegal information.</param>
		/// <param name="AppendCrLf">If true, a carriage-return/line-feed (CRLF) is
		/// appended to the text content before creating the file.</param>
		/// <returns>URI of created file resource.</returns>
		[McpServerTool(
			"Create Text File",  // Title
			"Creates a text file in account-specific file storage. If a file with the " +
			"same name exists, it is replaced. Text contents is stored UTF-8 encoded, " +
			"with a Byte-Order-Mark (BOM).",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			true,   // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(CreatePrivilege)]
		[return: McpParameter("Result", "URI of created file resource.")]
		public async Task<string> CreateTextFile(
			IJsonRpcCall Call,

			[McpStringParameter("Local File Name", "Local file name. Must not contain " +
			"double period characters or begin with a path character, to attempt to " +
			"escape file storage area. File extension must match Internet Content-Type " +
			"of file contents.", 3, 256)]
			string LocalFileName,

			[McpStringParameter("Content", "Text content of file to create. The " +
			"contents must not be harmful or contain illegal information.")]
			string Content,

			[McpParameter("Append CRLF", "If true, a carriage-return/line-feed " +
			"(CRLF) is appended to the text content before creating the file.")]
			bool AppendCrLf)
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertLocalFileNameOk(LocalFileName);

			string FullFileName = Path.Combine(this.rootFolder, UserName, LocalFileName);
			string BaseUri = Call.GetBaseUrl();
			string Uri = CreateFileUrl(BaseUri, LocalFileName);

			if (AppendCrLf)
				Content += "\r\n";

			await Runtime.IO.Files.WriteAllTextAsync(FullFileName, Content, Strings.Utf8WithBom);

			return Uri;
		}

		/// <summary>
		/// Creates a binary file in account-specific file storage. If a file with the 
		/// same name exists, it is replaced.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="LocalFileName">Local file name. Must not contain double period 
		/// characters or begin with a path character, to attempt to escape file 
		/// storage area. File extension must match Internet Content-Type of file 
		/// contents.</param>
		/// <param name="Content">BASE-64 encoded binary content of file to create. The 
		/// contents must not be harmful, be executable, or contain illegal information.</param>
		/// <returns>URI of created file resource.</returns>
		[McpServerTool(
			"Create Binary File",  // Title
			"Creates a binary file in account-specific file storage. If a file with the " +
			"same name exists, it is replaced.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			true,   // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(CreatePrivilege)]
		[return: McpParameter("Result", "URI of created file resource.")]
		public async Task<string> CreateBinaryFile(
			IJsonRpcCall Call,

			[McpStringParameter("Local File Name", "Local file name. Must not contain " +
			"double period characters or begin with a path character, to attempt to " +
			"escape file storage area. File extension must match Internet Content-Type " +
			"of file contents.", 3, 256)]
			string LocalFileName,

			[McpStringParameter("Content", "BASE-64 encoded binary content of file to " +
			"create. The contents must not be harmful, be executable, or contain illegal " +
			"information.")]
			string Content)
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertLocalFileNameOk(LocalFileName);

			string FullFileName = Path.Combine(this.rootFolder, UserName, LocalFileName);
			string BaseUri = Call.GetBaseUrl();
			string Uri = CreateFileUrl(BaseUri, LocalFileName);
			byte[] Bin = Convert.FromBase64String(Content);

			await Runtime.IO.Files.WriteAllBytesAsync(FullFileName, Bin);

			return Uri;
		}

		/// <summary>
		/// Appends a text file in account-specific file storage.
		/// If the file does not exist, one is created.
		/// Text contents is stored UTF-8 encoded (with a Byte-Order-Mark (BOM) if the 
		/// file is created).
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="LocalFileName">Local file name. Must not contain double period 
		/// characters or begin with a path character, to attempt to escape file 
		/// storage area. File extension must match Internet Content-Type of file 
		/// contents.</param>
		/// <param name="Content">Text content of file to append. The contents must not 
		/// be harmful or contain illegal information.</param>
		/// <param name="AppendCrLf">If true, a carriage-return/line-feed (CRLF) is 
		/// appended to the text content before appending it to the file.</param>
		/// <returns>URI of appended (or created) file resource.</returns>
		[McpServerTool(
			"Append Text File",  // Title
			"Appends a text file in account-specific file storage. If the file does not " +
			"exist, one is created. Text contents is stored UTF-8 encoded (with a " +
			"Byte-Order-Mark (BOM) if the file is created).",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(AppendPrivilege)]
		[return: McpParameter("Result", "URI of appended file resource.")]
		public async Task<string> AppendTextFile(
			IJsonRpcCall Call,

			[McpStringParameter("Local File Name", "Local file name. Must not contain " +
			"double period characters or begin with a path character, to attempt to " +
			"escape file storage area. File extension must match Internet Content-Type " +
			"of file contents.", 3, 256)]
			string LocalFileName,

			[McpStringParameter("Content", "Text content of file to append. The " +
			"contents must not be harmful or contain illegal information.")]
			string Content,

			[McpParameter("Append CRLF", "If true, a carriage-return/line-feed " +
			"(CRLF) is appended to the text content before appending it to the file.")]
			bool AppendCrLf)
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertLocalFileNameOk(LocalFileName);

			string FullFileName = Path.Combine(this.rootFolder, UserName, LocalFileName);
			string BaseUri = Call.GetBaseUrl();
			string Uri = CreateFileUrl(BaseUri, LocalFileName);

			if (AppendCrLf)
				Content += "\r\n";

			if (File.Exists(FullFileName))
			{
				byte[] Bin = Encoding.UTF8.GetBytes(Content);
				await Runtime.IO.Files.AppendAllBytesAsync(FullFileName, Bin);
			}
			else
				await Runtime.IO.Files.WriteAllTextAsync(FullFileName, Content, Strings.Utf8WithBom);

			return Uri;
		}

		/// <summary>
		/// Appends a binary file in account-specific file storage. If the file does not 
		/// exist, one is created.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="LocalFileName">Local file name. Must not contain double period 
		/// characters or begin with a path character, to attempt to escape file 
		/// storage area. File extension must match Internet Content-Type of file 
		/// contents.</param>
		/// <param name="Content">BASE-64 encoded binary content of file to append. The 
		/// contents must not be harmful, be executable, or contain illegal information.</param>
		/// <returns>URI of appended (or created) file resource.</returns>
		[McpServerTool(
			"Append Binary File",  // Title
			"Appends a binary file in account-specific file storage. If the file does " +
			"not exist, one is created.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(AppendPrivilege)]
		[return: McpParameter("Result", "URI of appended file resource.")]
		public async Task<string> AppendBinaryFile(
			IJsonRpcCall Call,

			[McpStringParameter("Local File Name", "Local file name. Must not contain " +
			"double period characters or begin with a path character, to attempt to " +
			"escape file storage area. File extension must match Internet Content-Type " +
			"of file contents.", 3, 256)]
			string LocalFileName,

			[McpStringParameter("Content", "BASE-64 encoded binary content of file to " +
			"append. The contents must not be harmful, be executable, or contain illegal " +
			"information.")]
			string Content)
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertLocalFileNameOk(LocalFileName);

			string FullFileName = Path.Combine(this.rootFolder, UserName, LocalFileName);
			string BaseUri = Call.GetBaseUrl();
			string Uri = CreateFileUrl(BaseUri, LocalFileName);
			byte[] Bin = Convert.FromBase64String(Content);

			await Runtime.IO.Files.AppendAllBytesAsync(FullFileName, Bin);

			return Uri;
		}

		/// <summary>
		/// Updates a text file in account-specific file storage, replacing its previous 
		/// content with new content. The file must exist. Text contents is stored UTF-8 
		/// encoded, with a Byte-Order-Mark (BOM).
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="LocalFileName">Local file name. Must not contain double period 
		/// characters or begin with a path character, to attempt to escape file 
		/// storage area. File extension must match Internet Content-Type of file 
		/// contents.</param>
		/// <param name="Content">Text content of file to update. The contents must not 
		/// be harmful or contain illegal information.</param>
		/// <param name="AppendCrLf">If true, a carriage-return/line-feed (CRLF) is 
		/// appended to the text content before updating the file.</param>
		/// <returns>URI of updated file resource.</returns>
		[McpServerTool(
			"Update Text File",  // Title
			"Updates a text file in account-specific file storage, replacing its " +
			"previous content with new content. The file must exist. Text contents is " +
			"stored UTF-8 encoded, with a Byte-Order-Mark (BOM).",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			true,   // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(UpdatePrivilege)]
		[return: McpParameter("Result", "URI of updated file resource.")]
		public async Task<string> UpdateTextFile(
			IJsonRpcCall Call,

			[McpStringParameter("Local File Name", "Local file name. Must not contain " +
			"double period characters or begin with a path character, to attempt to " +
			"escape file storage area. File extension must match Internet Content-Type " +
			"of file contents.", 3, 256)]
			string LocalFileName,

			[McpStringParameter("Content", "Text content of file to update. The " +
			"contents must not be harmful or contain illegal information.")]
			string Content,

			[McpParameter("Append CRLF", "If true, a carriage-return/line-feed " +
			"(CRLF) is appended to the text content before updating the file.")]
			bool AppendCrLf)
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertLocalFileNameOk(LocalFileName);

			string FullFileName = Path.Combine(this.rootFolder, UserName, LocalFileName);
			if (!File.Exists(FullFileName))
				throw new NotFoundException("File not found.");

			string BaseUri = Call.GetBaseUrl();
			string Uri = CreateFileUrl(BaseUri, LocalFileName);

			if (AppendCrLf)
				Content += "\r\n";

			await Runtime.IO.Files.WriteAllTextAsync(FullFileName, Content, Strings.Utf8WithBom);

			return Uri;
		}

		/// <summary>
		/// Updates a binary file in account-specific file storage. The file must exist.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="LocalFileName">Local file name. Must not contain double period 
		/// characters or begin with a path character, to attempt to escape file 
		/// storage area. File extension must match Internet Content-Type of file 
		/// contents.</param>
		/// <param name="Content">BASE-64 encoded binary content of file to update. The 
		/// contents must not be harmful, be executable, or contain illegal information.</param>
		/// <returns>URI of updated file resource.</returns>
		[McpServerTool(
			"Update Binary File",  // Title
			"Updates a binary file in account-specific file storage. The file must exist.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			true,   // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(UpdatePrivilege)]
		[return: McpParameter("Result", "URI of updated file resource.")]
		public async Task<string> UpdateBinaryFile(
			IJsonRpcCall Call,

			[McpStringParameter("Local File Name", "Local file name. Must not contain " +
			"double period characters or begin with a path character, to attempt to " +
			"escape file storage area. File extension must match Internet Content-Type " +
			"of file contents.", 3, 256)]
			string LocalFileName,

			[McpStringParameter("Content", "BASE-64 encoded binary content of file to " +
			"update. The contents must not be harmful, be executable, or contain illegal " +
			"information.")]
			string Content)
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertLocalFileNameOk(LocalFileName);

			string FullFileName = Path.Combine(this.rootFolder, UserName, LocalFileName);
			if (!File.Exists(FullFileName))
				throw new NotFoundException("File not found.");

			string BaseUri = Call.GetBaseUrl();
			string Uri = CreateFileUrl(BaseUri, LocalFileName);
			byte[] Bin = Convert.FromBase64String(Content);

			await Runtime.IO.Files.WriteAllBytesAsync(FullFileName, Bin);

			return Uri;
		}

		/// <summary>
		/// Deletes a file in account-specific file storage, regardless of type. The file 
		/// must exist.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="LocalFileName">Local file name. Must not contain double period 
		/// characters or begin with a path character, to attempt to escape file 
		/// storage area. File extension must match Internet Content-Type of file 
		/// contents.</param>
		/// <returns>URI of deleted file resource.</returns>
		[McpServerTool(
			"Delete File",  // Title
			"Deletes a file in account-specific file storage, regardless of type. The " +
			"file must exist.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(DeletePrivilege)]
		[return: McpParameter("Result", "URI of deleted file resource.")]
		public string DeleteFile(
			IJsonRpcCall Call,

			[McpStringParameter("Local File Name", "Local file name. Must not contain " +
			"double period characters or begin with a path character, to attempt to " +
			"escape file storage area. File extension must match Internet Content-Type " +
			"of file contents.", 3, 256)]
			string LocalFileName)
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertLocalFileNameOk(LocalFileName);

			string FullFileName = Path.Combine(this.rootFolder, UserName, LocalFileName);
			if (!File.Exists(FullFileName))
				throw new NotFoundException("File not found.");

			string BaseUri = Call.GetBaseUrl();
			string Uri = CreateFileUrl(BaseUri, LocalFileName);

			File.Delete(FullFileName);

			return Uri;
		}

		/// <summary>
		/// Searches for files in account-specific file storage that have file names
		/// matching a given search pattern.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="Pattern">File name search pattern. Must not contain double period 
		/// characters or path characters, to attempt to escape file storage area. Use 
		/// asterisk (*) as wildcard, or questionmark (?) as a character wildcard.</param>
		/// <returns>Search result of files in account-specific file storage.</returns>
		[McpServerTool(
			"Search",  // Title
			"Searches for files in account-specific file storage that have file names " +
			"matching a given search pattern.",   // Description
			"",     // IconsMethod, use default icons
			false,  // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			true,   // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(SearchPrivilege)]
		[return: McpParameter("Result", "Search result of files in account-specific file " +
			"storage.")]
		public SearchResult SearchFiles(
			IJsonRpcCall Call,

			[McpStringParameter("Search Pattern", "File name search pattern. Must not " +
			"contain double period characters or path characters, to attempt to " +
			"escape file storage area. Use asterisk (*) as wildcard, or questionmark (?) "+
			"as a character wildcard.", 3, 256)]
			string Pattern = "*.*")
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertPatternOk(Pattern);

			string UserFolder = Path.Combine(this.rootFolder, UserName);
			string[] AllFiles = Directory.GetFiles(UserFolder, "*.*", SearchOption.AllDirectories);
			string[] Files = Pattern == "*.*" ? AllFiles :
				Directory.GetFiles(UserFolder, Pattern, SearchOption.AllDirectories);
			int i, c = Files.Length;
			string[] ResourceUris = new string[c];
			string BaseUrl = Call.GetBaseUrl();

			for (i = 0; i < c; i++)
			{
				Files[i] = Files[i][(UserFolder.Length + 1)..];
				ResourceUris[i] = CreateFileUrl(BaseUrl, Files[i]);
			}

			return new SearchResult(c, AllFiles.Length, Pattern, Files, ResourceUris);
		}

		/// <summary>
		/// Edits a file in account-specific file storage.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="LocalFileName">Local file name. Must not contain double period 
		/// characters or begin with a path character, to attempt to escape file 
		/// storage area. File extension must match Internet Content-Type of file 
		/// contents.</param>
		/// <param name="Sensitive">If the contents of the file are sensitive, set this to
		/// true. Sensitive information will be edited in a separate protected window and
		/// not inline.</param>
		/// <returns>URI of updated file resource.</returns>
		[McpServerTool(
			"Edit",  // Title
			"Edits a file in account-specific file storage.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(EditPrivilege)]
		[return: McpParameter("Result", "URI of updated file resource.")]
		public async Task<string> EditFile(IJsonRpcCall Call,

			[McpStringParameter("Local File Name", "Local file name. Must not contain " +
			"double period characters or begin with a path character, to attempt to " +
			"escape file storage area. File extension must match Internet Content-Type " +
			"of file contents.", 3, 256)]
			string LocalFileName,

			[McpParameter("Sensitive", "If the contents of the file are sensitive, set " +
			"this to true. Sensitive information will be edited in a separate protected " +
			"window and not inline.")]
			bool Sensitive)
		{
			string? UserName = Call.User?.UserName;
			if (string.IsNullOrEmpty(UserName))
				throw new ForbiddenException("User not authenticated.");

			AssertLocalFileNameOk(LocalFileName);

			string BaseUri = Call.GetBaseUrl();
			string Uri = CreateFileUrl(BaseUri, LocalFileName);
			string FullFileName = Path.Combine(this.rootFolder, UserName, LocalFileName);
			if (!File.Exists(FullFileName))
				throw new NotFoundException("File not found.");

			Session? Session = await this.TryGetMcpSession(Call)
				?? throw new ForbiddenException("No MCP session active.");

			InternetContent.TryGetContentType(Path.GetExtension(LocalFileName), out string? ContentType);

			if (FileResource.IsTextFile(ContentType))
			{
				TextContents Contents = new TextContents()
				{
					FileContents = await Runtime.IO.Files.ReadAllTextAsync(FullFileName)
				};

				bool? Result = await this.ElicitUserInput(Call,
					"Edit the contents of the following text file.",
					Contents, Sensitive, Session, 5 * 60 * 1000);

				if (!Result.HasValue)
					throw new Exception("User input expected.");

				if (!Result.Value)
					throw new Exception("User declined to provide input.");

				await Runtime.IO.Files.WriteAllTextAsync(FullFileName, Contents.FileContents,
					Strings.Utf8WithBom);
			}
			else
			{
				BinaryContents Contents = new BinaryContents()
				{
					FileContents = Convert.ToBase64String(
						await Runtime.IO.Files.ReadAllBytesAsync(FullFileName))
				};

				bool? Result = await this.ElicitUserInput(Call,
					"Edit the BASE64-encoded contents of the following binary file.",
					Contents, Sensitive, Session, 5 * 60 * 1000);

				if (!Result.HasValue)
					throw new Exception("User input expected.");

				if (!Result.Value)
					throw new Exception("User declined to provide input.");

				await Runtime.IO.Files.WriteAllBytesAsync(FullFileName,
					Convert.FromBase64String(Contents.FileContents));
			}

			return Uri;
		}

		private class TextContents
		{
			[McpStringParameter("Text", "Text contents of the file being edited.")]
			public string? FileContents;
		}

		private class BinaryContents
		{
			[McpStringParameter("Binary", "BASE-64-encoded Binary contents of the file being edited.")]
			public string? FileContents;
		}

	}
}
