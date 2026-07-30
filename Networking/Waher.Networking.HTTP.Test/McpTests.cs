using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Mcp.Events;
using Waher.Mcp.Files;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Test
{
	/// <summary>
	/// End-to-end unit tests for the HTTP MCP server implementation.
	/// </summary>
	[TestClass]
	[DoNotParallelize]
	public class McpTests : IDynamicUserSource, IThingRegistryUserSource, IDisposable
	{
		private const string BaseUrl = "http://localhost:8081";
		private const string EventLogResource = "/MCP/Events";
		private const string FilesResource = "/MCP/Files";
		private const string ProtocolVersion = "2025-11-25";
		private const string TestUserName = "User";
		private const string TestPassword = "Password";
		private const string PromptUserName = "PromptUser";
		private const string PromptPassword = "PromptPassword";

		private const string EventToolsScope = "MCP:EventLog:Tools";
		private const string EventPromptsScope = "MCP:EventLog:Prompts";
		private const string FilesToolsScope = "MCP:Files:Tools";
		private const string FilesResourcesScope = "MCP:Files:Resources";
		private const string AllScopes = EventToolsScope + " " + EventPromptsScope + " " +
			FilesToolsScope + " " + FilesResourcesScope;

		private Dictionary<string, User> users;
		private HttpServer server;
		private ConsoleEventSink sink = null;
		private XmlFileSniffer xmlSniffer = null;
		private XmlFileSnifferSet xmlSnifferSet = null;
		private JwtFactory jwtFactory = null;
		private OAuth2Environment environment = null;
		private TestEventLogMcpServer eventLogServer = null;
		private FileStorageMcpServer fileStorageServer = null;
		private string fileStorageRoot = null;
		private string accessToken = null;

		[ClassInitialize]
		public static void ClassInitialize(TestContext TestContext)
		{
			if (Directory.Exists("Sniffers"))
			{
				foreach (string FileName in Directory.GetFiles("Sniffers", "*.xml", SearchOption.AllDirectories))
					File.Delete(FileName);
			}
		}

		/// <summary>
		/// Test context.
		/// </summary>
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public async Task TestInitialize()
		{
			string SnifferFileName = this.TestContext.TestName;
			if (string.IsNullOrEmpty(SnifferFileName))
				SnifferFileName = "Sniffer";

			SnifferFileName += ".xml";

			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);

			this.xmlSniffer = new XmlFileSniffer("Sniffers\\HTTP\\" + SnifferFileName,
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				7, BinaryPresentationMethod.ByteCount);

			this.xmlSnifferSet = new XmlFileSnifferSet("Sniffers\\MCP", SnifferFileName,
				TimeSpan.FromHours(1),
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				7, BinaryPresentationMethod.ByteCount);

			this.jwtFactory = JwtFactory.CreateHmacSha256(BaseUrl);
			JwtFactory.ValidateAudience += (sender, e) =>
			{
				foreach (string Audience in e.Audience)
				{
					if (Audience == TestUserName || Audience == PromptUserName)
					{
						e.Acceptable = true;
						break;
					}
				}
			};

			this.server = new HttpServer(8081, this.xmlSniffer);

			await Database.Clear("OAuthRedirectUris");
			await Database.Clear("OAuthClients");

			this.environment = new();
			this.environment.Register(this.jwtFactory);
			this.environment.Register(this);

			Types.SetModuleParameter("JWT", this.jwtFactory);
			Types.SetModuleParameter("Users", this);
			Types.SetModuleParameter("Domain", "localhost");
			Types.SetModuleParameter("Realm", "TestServer");
			Types.SetModuleParameter("HTTP", this.server);
			Types.SetModuleParameter("OAUTH2", this.environment);

			this.server.Register(new ProtectedResourceMetaData(this.environment));
			this.server.Register(new OAuthTokenResource(this.environment));
			this.server.Register(new OAuthIntrospectionResource(this.environment));
			this.server.Register(new OAuthRegistrationResource(this.environment));
			this.server.Register(new OAuthManagementResource(this.environment));
			this.server.Register(new OAuthDeviceAuthorizationResource(this.environment));
			this.server.Register(new OAuthAuthorizeResource(this.environment));
			this.server.Register(new AuthorizationServerMetaData(this.environment));
			this.fileStorageRoot = Path.Combine(Path.GetTempPath(),
				"Waher.Mcp.Tests." + Guid.NewGuid().ToString("N"));
			string UserFolder = Path.Combine(this.fileStorageRoot, TestUserName);
			Directory.CreateDirectory(UserFolder);

			for (int i = 0; i < 25; i++)
			{
				File.WriteAllText(Path.Combine(UserFolder,
					"Resource" + i.ToString("D2") + ".txt"),
					"Resource content " + i.ToString("D2") + ".", Encoding.UTF8);
			}

			File.WriteAllText(Path.Combine(UserFolder, "editable.txt"),
				"Original contents.", Encoding.UTF8);
			File.WriteAllBytes(Path.Combine(UserFolder, "Binary.bin"),
				[0, 1, 2, 3, 254, 255]);

			this.eventLogServer = new TestEventLogMcpServer(EventLogResource, this.xmlSnifferSet);
			this.fileStorageServer = new FileStorageMcpServer(FilesResource,
				this.fileStorageRoot, this.xmlSnifferSet);

			this.server.Register(this.eventLogServer);
			this.server.Register(this.fileStorageServer);

			string Prefix = OAuthResource.OAuthScopePrivilegePrefix;
			this.users = new Dictionary<string, User>()
			{
				{ TestUserName, new User(TestUserName, TestPassword,
					[
						Prefix + EventToolsScope.Replace(':', '.'),
						Prefix + EventPromptsScope.Replace(':', '.'),
						Prefix + FilesToolsScope.Replace(':', '.'),
						Prefix + FilesResourcesScope.Replace(':', '.'),

						Prefix + "MCP.EventLog.Tools.Log.Debug",
						Prefix + "MCP.EventLog.Tools.Log.Information",
						Prefix + "MCP.EventLog.Tools.Log.Notice",
						Prefix + "MCP.EventLog.Tools.Log.Warning",
						Prefix + "MCP.EventLog.Tools.Log.Error",
						Prefix + "MCP.EventLog.Tools.Log.Critical",
						Prefix + "MCP.EventLog.Tools.Log.Alert",
						Prefix + "MCP.EventLog.Tools.Log.Emergency",
						Prefix + "MCP.EventLog.Tools.Search",
						Prefix + "MCP.EventLog.Prompts.FindSensitiveInfo",

						Prefix + "MCP.Files.Resources.List",
						Prefix + "MCP.Files.Resources.Read",
						Prefix + "MCP.Files.Tools.Create",
						Prefix + "MCP.Files.Tools.Append",
						Prefix + "MCP.Files.Tools.Update",
						Prefix + "MCP.Files.Tools.Delete",
						Prefix + "MCP.Files.Tools.Search",
						Prefix + "MCP.Files.Tools.Edit"
					]) },
				{ PromptUserName, new User(PromptUserName, PromptPassword,
					[
						Prefix + EventPromptsScope.Replace(':', '.'),
						Prefix + "MCP.EventLog.Prompts.FindSensitiveInfo"
					]) }
			};

			this.accessToken = await Login(AllScopes);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.server is not null)
			{
				await this.server.DisposeAsync();
				this.server = null;
			}

			if (this.fileStorageServer is not null)
			{
				this.fileStorageServer.Dispose();
				this.fileStorageServer = null;
			}

			this.eventLogServer = null;

			if (this.xmlSniffer is not null)
			{
				await this.xmlSniffer.DisposeAsync();
				this.xmlSniffer = null;
			}

			if (this.sink is not null)
			{
				Log.Unregister(this.sink);
				await this.sink.DisposeAsync();
				this.sink = null;
			}

			if (this.jwtFactory is not null)
			{
				this.jwtFactory.Dispose();
				this.jwtFactory = null;
			}

			if (this.environment is not null)
			{
				this.environment.Dispose();
				this.environment = null;
			}

			if (!string.IsNullOrEmpty(this.fileStorageRoot) &&
				Directory.Exists(this.fileStorageRoot))
			{
				Directory.Delete(this.fileStorageRoot, true);
			}
		}

		public bool Disposed => true;   // Will make it possible to re-register Users module parameter.
		public void Dispose() { }		// Do nothing by default.

		public Task<IUser> TryGetUser(string UserName)
		{
			if (this.users.TryGetValue(UserName, out User User))
				return Task.FromResult<IUser>(User);
			else
				return Task.FromResult<IUser>(null);
		}

		public Task<IRegistration> RegisterUser(IRegistrationRequest RegistrationRequest)
		{
			string UserName;
			string Password;

			do
			{
				UserName = Guid.NewGuid().ToString();
			}
			while (this.users.ContainsKey(UserName));

			Password = Guid.NewGuid().ToString();

			User User = new(UserName, Password, []);
			this.users[User.UserName] = User;

			return Task.FromResult<IRegistration>(
				new Registration(UserName, Password, RegistrationRequest));
		}

		public async Task<IRegistration> UpdateUser(string UserName,
			IRegistrationRequest RegistrationRequest)
		{
			IUser User = await this.TryGetUser(UserName);
			if (User is null)
				return null;

			if (!string.IsNullOrEmpty(RegistrationRequest.ClientSecret))
			{
				if (User is not User TypedUser)
					return null;

				this.users[User.UserName] = new User(TypedUser.UserName,
					RegistrationRequest.ClientSecret, TypedUser.Owner,
					TypedUser.Privileges);
			}

			return new Registration(UserName,
				RegistrationRequest.ClientSecret ?? string.Empty, RegistrationRequest);
		}

		public Task<bool> DeleteUser(string UserName, string RemoteEndPoint)
		{
			return Task.FromResult(this.users.Remove(UserName));
		}

		public Task<IUser> TryGetOwner(IUser Device)
		{
			string OwnerId = (Device as User)?.Owner;
			if (string.IsNullOrEmpty(OwnerId))
				return Task.FromResult<IUser>(null);

			return this.TryGetUser(OwnerId);
		}

		private class Registration(string UserName, string Password,
			IRegistrationRequest Request) : IRegistration
		{
			public string ClientId { get; } = UserName;
			public string ClientSecret { get; } = Password;
			public DateTime? ClientSecretExpiresAt => null;
			public IRegistrationRequest Request { get; } = Request;
		}

		[TestMethod]
		public async Task Test_001_Initialize_EventLog_Server()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> Result = Connection.InitializeResult;

			Assert.AreEqual(ProtocolVersion, Required<string>(Result, "protocolVersion"));
			Dictionary<string, object> Capabilities = RequiredDictionary(Result, "capabilities");
			Assert.IsTrue(Capabilities.ContainsKey("tools"));
			Assert.IsTrue(Capabilities.ContainsKey("prompts"));
			Assert.IsFalse(Capabilities.ContainsKey("resources"));
			Assert.IsTrue(Required<bool>(RequiredDictionary(Capabilities, "tools"), "listChanged"));
			Assert.IsTrue(Required<bool>(RequiredDictionary(Capabilities, "prompts"), "listChanged"));

			Dictionary<string, object> ServerInfo = RequiredDictionary(Result, "serverInfo");
			Assert.AreEqual("EventLog", Required<string>(ServerInfo, "name"));
			Assert.AreEqual("Event Log", Required<string>(ServerInfo, "title"));
			Assert.AreEqual(BaseUrl + EventLogResource,
				Required<string>(ServerInfo, "websiteUrl"));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Result, "instructions")));
		}

		[TestMethod]
		public async Task Test_002_Initialize_File_Server()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			Dictionary<string, object> Result = Connection.InitializeResult;

			Assert.AreEqual(ProtocolVersion, Required<string>(Result, "protocolVersion"));
			Dictionary<string, object> Capabilities = RequiredDictionary(Result, "capabilities");
			Assert.IsTrue(Capabilities.ContainsKey("tools"));
			Assert.IsTrue(Capabilities.ContainsKey("resources"));
			Assert.IsFalse(Capabilities.ContainsKey("prompts"));

			Dictionary<string, object> Resources = RequiredDictionary(Capabilities, "resources");
			Assert.IsTrue(Required<bool>(Resources, "subscribe"));
			Assert.IsTrue(Required<bool>(Resources, "listChanged"));

			Dictionary<string, object> ServerInfo = RequiredDictionary(Result, "serverInfo");
			Assert.AreEqual("FileStorage", Required<string>(ServerInfo, "name"));
			Assert.AreEqual("File Storage", Required<string>(ServerInfo, "title"));
			Assert.AreEqual(BaseUrl + FilesResource,
				Required<string>(ServerInfo, "websiteUrl"));
		}

		[TestMethod]
		public async Task Test_003_Mcp_Session_Header_Is_Required()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			JsonRpcHttpResponse Response = await Connection.CallRawAsync("tools/list",
				[], false);

			Assert.AreEqual(HttpStatusCode.BadRequest, Response.StatusCode, Response.Body);
		}

		[TestMethod]
		public async Task Test_004_OAuth_Scopes_Filter_Mcp_Capabilities()
		{
			string PromptsOnlyToken = await Login(PromptUserName, PromptPassword,
				EventPromptsScope);
			await using McpConnection Connection = await Connect(EventLogResource,
				PromptsOnlyToken, CreateElicitationCapabilities(true, true));

			Dictionary<string, object> ToolResult = await Connection.CallAsync("tools/list",
				[]);
			Assert.IsEmpty(RequiredArray(ToolResult, "tools"));

			Dictionary<string, object> PromptResult = await Connection.CallAsync("prompts/list",
				[]);
			Assert.HasCount(20, RequiredArray(PromptResult, "prompts"));
		}

		[TestMethod]
		public async Task Test_005_Tools_List_Contains_Schemas_And_Annotations()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> Result = await Connection.CallAsync("tools/list",
				[]);

			object[] Tools = RequiredArray(Result, "tools");
			Assert.HasCount(20, Tools);
			Assert.AreEqual("20", Required<string>(Result, "nextCursor"));

			Dictionary<string, object> Tool = FindByName(Tools, "LogInformational");
			Assert.AreEqual("Log Informational Event", Required<string>(Tool, "title"));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Tool, "description")));

			Dictionary<string, object> InputSchema = RequiredDictionary(Tool, "inputSchema");
			Assert.AreEqual("object", Required<string>(InputSchema, "type"));
			Dictionary<string, object> Properties = RequiredDictionary(InputSchema, "properties");
			Dictionary<string, object> Message = RequiredDictionary(Properties, "Message");
			Assert.AreEqual("string", Required<string>(Message, "type"));
			Assert.AreEqual("Event Message", Required<string>(Message, "title"));
			AssertArrayContains(RequiredArray(InputSchema, "required"), "Message");

			Dictionary<string, object> Execution = RequiredDictionary(Tool, "execution");
			Assert.AreEqual("optional", Required<string>(Execution, "taskSupport"));

			Dictionary<string, object> Annotations = RequiredDictionary(Tool, "annotations");
			Assert.IsTrue(Annotations.ContainsKey("readOnlyHint"));
			Assert.IsTrue(Annotations.ContainsKey("destructiveHint"));
			Assert.IsTrue(Annotations.ContainsKey("idempotentHint"));
			Assert.IsTrue(Annotations.ContainsKey("openWorldHint"));
		}

		[TestMethod]
		public async Task Test_006_Tools_Cursors_Return_All_Authorized_Tools()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> First = await Connection.CallAsync("tools/list",
				[]);
			Dictionary<string, object> Second = await Connection.CallAsync("tools/list",
				new Dictionary<string, object>()
				{
					{ "cursor", Required<string>(First, "nextCursor") }
				});

			object[] FirstPage = RequiredArray(First, "tools");
			object[] SecondPage = RequiredArray(Second, "tools");
			Assert.HasCount(20, FirstPage);
			Assert.HasCount(9, SecondPage);
			Assert.IsFalse(Second.ContainsKey("nextCursor"));
			AssertUniqueNames(29, FirstPage, SecondPage);

			Dictionary<string, object> BeyondEnd = await Connection.CallAsync("tools/list",
				new Dictionary<string, object>() { { "cursor", "999" } });
			Assert.IsEmpty(RequiredArray(BeyondEnd, "tools"));
		}

		[TestMethod]
		[DataRow(EventLogResource, "tools/list", "not-a-cursor")]
		[DataRow(EventLogResource, "tools/list", "-1")]
		[DataRow(EventLogResource, "prompts/list", "not-a-cursor")]
		[DataRow(EventLogResource, "prompts/list", "-1")]
		[DataRow(FilesResource, "resources/list", "not-a-cursor")]
		[DataRow(FilesResource, "resources/list", "-1")]
		public async Task Test_007_Invalid_Cursors_Are_Rejected(string Resource,
			string Method, string Cursor)
		{
			await using McpConnection Connection = await this.Connect(Resource);
			JsonRpcHttpResponse Response = await Connection.CallRawAsync(Method,
				new Dictionary<string, object>() { { "cursor", Cursor } });

			Assert.AreEqual(HttpStatusCode.BadRequest, Response.StatusCode, Response.Body);
		}

		[TestMethod]
		public async Task Test_008_Execute_EventLog_Tool()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> Result = await Connection.CallAsync("tools/call",
				new Dictionary<string, object>()
				{
					{ "name", "LogInformational" },
					{ "arguments", new Dictionary<string, object>()
						{
							{ "Message", "MCP unit test event." },
							{ "Object", "McpTests" },
							{ "Actor", TestUserName },
							{ "EventId", "McpUnitTest" }
						}
					}
				});

			AssertMcpOperationSucceeded(Result);
			Assert.IsEmpty(RequiredArray(Result, "content"));
		}

		[TestMethod]
		public async Task Test_009_Execute_File_Tool_And_Verify_Side_Effect()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			Dictionary<string, object> Result = await Connection.CallAsync("tools/call",
				new Dictionary<string, object>()
				{
					{ "name", "CreateTextFile" },
					{ "arguments", new Dictionary<string, object>()
						{
							{ "LocalFileName", "created.txt" },
							{ "Content", "Created by MCP." },
							{ "AppendCrLf", false }
						}
					}
				});

			AssertMcpOperationSucceeded(Result);
			Assert.AreEqual(BaseUrl + FilesResource + "/created.txt", GetSingleTextContent(Result));

			string FullFileName = Path.Combine(this.fileStorageRoot, TestUserName, "created.txt");
			Assert.IsTrue(File.Exists(FullFileName));
			Assert.AreEqual("Created by MCP.", File.ReadAllText(FullFileName));
		}

		[TestMethod]
		public async Task Test_010_File_Tools_List_Contains_Edit_File_Schema()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			Dictionary<string, object> Result = await Connection.CallAsync("tools/list",
				[]);

			object[] Tools = RequiredArray(Result, "tools");
			Assert.HasCount(9, Tools);
			Assert.IsFalse(Result.ContainsKey("nextCursor"));

			Dictionary<string, object> Tool = FindByName(Tools, "EditFile");
			Assert.AreEqual("Edit", Required<string>(Tool, "title"));
			Dictionary<string, object> InputSchema = RequiredDictionary(Tool, "inputSchema");
			Dictionary<string, object> Properties = RequiredDictionary(InputSchema, "properties");
			Assert.AreEqual("string", Required<string>(
				RequiredDictionary(Properties, "LocalFileName"), "type"));
			Assert.AreEqual("boolean", Required<string>(
				RequiredDictionary(Properties, "Sensitive"), "type"));
			object[] RequiredArguments = RequiredArray(InputSchema, "required");
			AssertArrayContains(RequiredArguments, "LocalFileName");
			AssertArrayContains(RequiredArguments, "Sensitive");
		}

		[TestMethod]
		public async Task Test_011_Invalid_Tool_Arguments_Return_Mcp_Error_Result()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> Result = await Connection.CallAsync("tools/call",
				new Dictionary<string, object>()
				{
					{ "name", "LogInformational" },
					{ "arguments", new Dictionary<string, object>() }
				});

			Assert.IsTrue(Required<bool>(Result, "isError"));
			string ErrorText = GetSingleTextContent(Result);
			Assert.IsTrue(ErrorText.Contains("Missing", StringComparison.OrdinalIgnoreCase),
				ErrorText);
		}

		[TestMethod]
		public async Task Test_012_Unknown_Tool_Is_Rejected()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			JsonRpcHttpResponse Response = await Connection.CallRawAsync("tools/call",
				new Dictionary<string, object>()
				{
					{ "name", "NoSuchTool" },
					{ "arguments", new Dictionary<string, object>() }
				});

			Assert.AreEqual(HttpStatusCode.NotFound, Response.StatusCode, Response.Body);
		}

		[TestMethod]
		public async Task Test_013_Prompts_List_Contains_Arguments()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> Result = await Connection.CallAsync("prompts/list",
				[]);

			object[] Prompts = RequiredArray(Result, "prompts");
			Assert.HasCount(20, Prompts);
			Assert.AreEqual("20", Required<string>(Result, "nextCursor"));

			Dictionary<string, object> Prompt = FindByName(Prompts, "FindSensitiveInformation");
			Assert.AreEqual("Find Sensitive Information", Required<string>(Prompt, "title"));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Prompt, "description")));

			object[] Arguments = RequiredArray(Prompt, "arguments");
			Assert.HasCount(10, Arguments);
			Dictionary<string, object> NrDays = FindByName(Arguments, "NrDays");
			Assert.IsFalse(Required<bool>(NrDays, "required"));
			Assert.AreEqual("Number of Days", Required<string>(NrDays, "title"));
		}

		[TestMethod]
		public async Task Test_014_Prompt_Cursors_Return_All_Authorized_Prompts()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> First = await Connection.CallAsync("prompts/list",
				[]);
			Dictionary<string, object> Second = await Connection.CallAsync("prompts/list",
				new Dictionary<string, object>()
				{
					{ "cursor", Required<string>(First, "nextCursor") }
				});

			object[] FirstPage = RequiredArray(First, "prompts");
			object[] SecondPage = RequiredArray(Second, "prompts");
			Assert.HasCount(20, FirstPage);
			Assert.HasCount(1, SecondPage);
			Assert.IsFalse(Second.ContainsKey("nextCursor"));
			AssertUniqueNames(21, FirstPage, SecondPage);
		}

		[TestMethod]
		public async Task Test_015_Get_Prompt_Returns_Role_And_Content_Blocks()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> Result = await Connection.CallAsync("prompts/get",
				new Dictionary<string, object>()
				{
					{ "name", "FindSensitiveInformation" },
					{ "arguments", new Dictionary<string, object>()
						{
							{ "NrDays", 3 },
							{ "EditEvents", true },
							{ "DeleteEvents", false }
						}
					}
				});

			AssertMcpOperationSucceeded(Result);
			object[] Messages = RequiredArray(Result, "messages");
			Assert.HasCount(2, Messages);

			Dictionary<string, object> UserMessage = AsDictionary(Messages[0], "messages[0]");
			Assert.AreEqual("user", Required<string>(UserMessage, "role"));
			string UserText = GetTextBlock(RequiredDictionary(UserMessage, "content"));
			Assert.Contains("last 3 days", UserText);
			Assert.Contains("sensitive information", UserText);

			Dictionary<string, object> AssistantMessage = AsDictionary(Messages[1], "messages[1]");
			Assert.AreEqual("assistant", Required<string>(AssistantMessage, "role"));
			string AssistantText = GetTextBlock(RequiredDictionary(AssistantMessage, "content"));
			Assert.Contains("Search for Events", AssistantText);
			Assert.Contains("Edit Events", AssistantText);
		}

		[TestMethod]
		public async Task Test_016_Invalid_Prompt_Arguments_Return_Mcp_Error_Result()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			Dictionary<string, object> Result = await Connection.CallAsync("prompts/get",
				new Dictionary<string, object>()
				{
					{ "name", "FindSensitiveInformation" },
					{ "arguments", new Dictionary<string, object>()
						{
							{ "NrDays", "not-an-integer" }
						}
					}
				});

			Assert.IsTrue(Required<bool>(Result, "isError"));
			object[] Messages = RequiredArray(Result, "messages");
			Assert.HasCount(1, Messages);
			string ErrorText = GetTextBlock(RequiredDictionary(
				AsDictionary(Messages[0], "messages[0]"), "content"));
			Assert.IsFalse(string.IsNullOrEmpty(ErrorText));
		}

		[TestMethod]
		public async Task Test_017_Unknown_Prompt_Is_Rejected()
		{
			await using McpConnection Connection = await this.Connect(EventLogResource);
			JsonRpcHttpResponse Response = await Connection.CallRawAsync("prompts/get",
				new Dictionary<string, object>()
				{
					{ "name", "NoSuchPrompt" },
					{ "arguments", new Dictionary<string, object>() }
				});

			Assert.AreEqual(HttpStatusCode.NotFound, Response.StatusCode, Response.Body);
		}

		[TestMethod]
		public async Task Test_018_Resources_List_Returns_Resource_Descriptors()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			Dictionary<string, object> Result = await Connection.CallAsync("resources/list",
				[]);

			object[] Resources = RequiredArray(Result, "resources");
			Assert.HasCount(20, Resources);
			Assert.AreEqual("20", Required<string>(Result, "nextCursor"));

			foreach (object Item in Resources)
			{
				Dictionary<string, object> Resource = AsDictionary(Item, "resource");
				Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Resource, "name")));
				Assert.IsTrue(Required<string>(Resource, "uri").StartsWith(
					BaseUrl + FilesResource + "/", StringComparison.Ordinal));
				Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Resource, "mimeType")));
				Assert.IsGreaterThanOrEqualTo(0, Convert.ToInt64(Required<object>(Resource, "size")));
			}
		}

		[TestMethod]
		public async Task Test_019_Resource_Cursors_Return_All_Authorized_Resources()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			Dictionary<string, object> First = await Connection.CallAsync("resources/list",
				[]);
			Dictionary<string, object> Second = await Connection.CallAsync("resources/list",
				new Dictionary<string, object>()
				{
					{ "cursor", Required<string>(First, "nextCursor") }
				});

			object[] FirstPage = RequiredArray(First, "resources");
			object[] SecondPage = RequiredArray(Second, "resources");
			Assert.HasCount(20, FirstPage);
			Assert.HasCount(7, SecondPage);
			Assert.IsFalse(Second.ContainsKey("nextCursor"));
			AssertUniqueNames(27, FirstPage, SecondPage);
		}

		[TestMethod]
		public async Task Test_020_Read_Text_Resource()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			string Uri = BaseUrl + FilesResource + "/Resource05.txt";
			Dictionary<string, object> Result = await Connection.CallAsync("resources/read",
				new Dictionary<string, object>() { { "uri", Uri } });

			object[] Contents = RequiredArray(Result, "contents");
			Assert.HasCount(1, Contents);
			Dictionary<string, object> Content = AsDictionary(Contents[0], "contents[0]");
			Assert.AreEqual(Uri, Required<string>(Content, "uri"));
			Assert.AreEqual("Resource content 05.", Required<string>(Content, "text"));
			Assert.IsTrue(Required<string>(Content, "mimeType").StartsWith(
				"text/plain", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public async Task Test_021_Read_Binary_Resource()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			string Uri = BaseUrl + FilesResource + "/Binary.bin";
			Dictionary<string, object> Result = await Connection.CallAsync("resources/read",
				new Dictionary<string, object>() { { "uri", Uri } });

			object[] Contents = RequiredArray(Result, "contents");
			Assert.HasCount(1, Contents);
			Dictionary<string, object> Content = AsDictionary(Contents[0], "contents[0]");
			Assert.AreEqual(Uri, Required<string>(Content, "uri"));
			Assert.AreEqual(Convert.ToBase64String(new byte[] { 0, 1, 2, 3, 254, 255 }),
				Required<string>(Content, "blob"));
			Assert.AreEqual("application/octet-stream",
				Required<string>(Content, "mimeType"));
		}

		[TestMethod]
		public async Task Test_022_Unknown_Resource_Is_Rejected()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			JsonRpcHttpResponse Response = await Connection.CallRawAsync("resources/read",
				new Dictionary<string, object>()
				{
					{ "uri", BaseUrl + FilesResource + "/missing.txt" }
				});

			Assert.AreEqual(HttpStatusCode.NotFound, Response.StatusCode, Response.Body);
		}

		[TestMethod]
		public async Task Test_023_Form_Elicitation_Accepts_User_Input()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			await using SseConnection Sse = await Connection.OpenSseAsync();

			Task<Dictionary<string, object>> ToolTask = CallEditFile(Connection, false);
			Dictionary<string, object> Elicitation = await Sse.WaitForMethodAsync("elicitation/create");
			Dictionary<string, object> Parameters = RequiredDictionary(Elicitation, "params");

			Assert.AreEqual("form", Required<string>(Parameters, "mode"));
			Assert.IsFalse(Parameters.ContainsKey("url"));
			Assert.IsFalse(Parameters.ContainsKey("elicitationId"));
			Assert.IsFalse(string.IsNullOrEmpty(Required<string>(Parameters, "message")));

			Dictionary<string, object> Schema = RequiredDictionary(Parameters, "requestedSchema");
			Assert.AreEqual("object", Required<string>(Schema, "type"));
			Dictionary<string, object> Properties = RequiredDictionary(Schema, "properties");
			Dictionary<string, object> FileContents = RequiredDictionary(Properties, "FileContents");
			Assert.AreEqual("string", Required<string>(FileContents, "type"));
			Assert.AreEqual("Text", Required<string>(FileContents, "title"));

			await Connection.PostClientResultAsync(Required<object>(Elicitation, "id"),
				new Dictionary<string, object>()
				{
					{ "action", "accept" },
					{ "content", new Dictionary<string, object>()
						{
							{ "FileContents", "Edited using form elicitation." }
						}
					}
				});

			Dictionary<string, object> Result = await ToolTask;
			AssertMcpOperationSucceeded(Result);
			Assert.AreEqual(BaseUrl + FilesResource + "/editable.txt", GetSingleTextContent(Result));
			Assert.AreEqual("Edited using form elicitation.", this.ReadEditableFile());
		}

		[TestMethod]
		[DataRow("decline", "declined")]
		[DataRow("cancel", "expected")]
		public async Task Test_024_Form_Elicitation_Decline_And_Cancel(string Action,
			string ExpectedErrorText)
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			await using SseConnection Sse = await Connection.OpenSseAsync();

			Task<Dictionary<string, object>> ToolTask = CallEditFile(Connection, false);
			Dictionary<string, object> Elicitation = await Sse.WaitForMethodAsync("elicitation/create");
			Assert.AreEqual("form", Required<string>(RequiredDictionary(Elicitation, "params"), "mode"));

			await Connection.PostClientResultAsync(Required<object>(Elicitation, "id"),
				new Dictionary<string, object>() { { "action", Action } });

			Dictionary<string, object> Result = await ToolTask;
			Assert.IsTrue(Required<bool>(Result, "isError"));
			string ErrorText = GetSingleTextContent(Result);
			Assert.IsTrue(ErrorText.Contains(ExpectedErrorText,
				StringComparison.OrdinalIgnoreCase), ErrorText);
			Assert.AreEqual("Original contents.", this.ReadEditableFile());
		}

		[TestMethod]
		[Timeout(10000)]
		public async Task Test_025_Url_Elicitation_Uses_Protected_Form_For_Sensitive_Input()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			await using SseConnection Sse = await Connection.OpenSseAsync();

			Task<Dictionary<string, object>> ToolTask = CallEditFile(Connection, true);
			Dictionary<string, object> Elicitation = await Sse.WaitForMethodAsync("elicitation/create");
			Dictionary<string, object> Parameters = RequiredDictionary(Elicitation, "params");

			Assert.AreEqual("url", Required<string>(Parameters, "mode"));
			Assert.IsFalse(Parameters.ContainsKey("requestedSchema"));
			string ElicitationId = Required<string>(Parameters, "elicitationId");
			string Url = Required<string>(Parameters, "url");
			Assert.AreEqual(Required<object>(Elicitation, "id").ToString(), ElicitationId);
			Assert.AreEqual(BaseUrl + FilesResource + "/" + ElicitationId, Url);

			HtmlFormResponse Form = await Connection.GetInputFormAsync(Url);
			Assert.AreEqual(HttpStatusCode.OK, Form.StatusCode, Form.Body);
			AssertHeaderContains(Form.Headers, "X-Frame-Options", "DENY");
			AssertHeaderContains(Form.Headers, "Content-Security-Policy", "frame-ancestors 'none'");
			AssertHeaderContains(Form.Headers, "Content-Security-Policy", "form-action 'self'");
			AssertHeaderContains(Form.Headers, "Cache-Control", "no-store");
			AssertHeaderContains(Form.Headers, "Pragma", "no-cache");
			Assert.Contains("InputForm", Form.Body);
			Assert.Contains("FileContents", Form.Body);
			Assert.Contains("Original contents.", Form.Body);

			string ParametersToken = ExtractHiddenInput(Form.Body, "_p_");
			await Connection.PostInputFormAsync(Url, ParametersToken, true,
				new Dictionary<string, string>()
				{
					{ "FileContents", "Edited using URL elicitation." }
				});

			Dictionary<string, object> Result = await ToolTask;
			AssertMcpOperationSucceeded(Result);
			Assert.AreEqual("Edited using URL elicitation.", this.ReadEditableFile());

			Dictionary<string, object> Complete = await Sse.WaitForMethodAsync(
				"notifications/elicitation/complete");
			Assert.AreEqual(ElicitationId,
				Required<object>(RequiredDictionary(Complete, "params"), "elicitationId").ToString());
		}

		[TestMethod]
		[Timeout(10000)]
		public async Task Test_026_Url_Elicitation_Can_Be_Cancelled_From_Protected_Form()
		{
			await using McpConnection Connection = await this.Connect(FilesResource);
			await using SseConnection Sse = await Connection.OpenSseAsync();

			Task<Dictionary<string, object>> ToolTask = CallEditFile(Connection, true);
			Dictionary<string, object> Elicitation = await Sse.WaitForMethodAsync("elicitation/create");
			Dictionary<string, object> Parameters = RequiredDictionary(Elicitation, "params");
			string ElicitationId = Required<string>(Parameters, "elicitationId");
			string Url = Required<string>(Parameters, "url");
			HtmlFormResponse Form = await Connection.GetInputFormAsync(Url);
			string ParametersToken = ExtractHiddenInput(Form.Body, "_p_");

			await Connection.PostInputFormAsync(Url, ParametersToken, false,
				[]);

			Dictionary<string, object> Result = await ToolTask;
			Assert.IsTrue(Required<bool>(Result, "isError"));
			Assert.Contains("expected", GetSingleTextContent(Result));
			Assert.AreEqual("Original contents.", this.ReadEditableFile());

			Dictionary<string, object> Complete = await Sse.WaitForMethodAsync(
				"notifications/elicitation/complete");
			Assert.AreEqual(ElicitationId,
				Required<object>(RequiredDictionary(Complete, "params"), "elicitationId").ToString());
		}

		[TestMethod]
		[Timeout(10000)]
		public async Task Test_027_NonSensitive_Elicitation_Falls_Back_To_Url_When_Form_Is_Unsupported()
		{
			Dictionary<string, object> UrlOnlyCapabilities = CreateElicitationCapabilities(false, true);
			await using McpConnection Connection = await Connect(FilesResource,
				this.accessToken, UrlOnlyCapabilities);
			await using SseConnection Sse = await Connection.OpenSseAsync();

			Task<Dictionary<string, object>> ToolTask = CallEditFile(Connection, false);
			Dictionary<string, object> Elicitation = await Sse.WaitForMethodAsync("elicitation/create");
			Dictionary<string, object> Parameters = RequiredDictionary(Elicitation, "params");
			Assert.AreEqual("url", Required<string>(Parameters, "mode"));
			string Url = Required<string>(Parameters, "url");
			HtmlFormResponse Form = await Connection.GetInputFormAsync(Url);
			Assert.AreEqual(HttpStatusCode.OK, Form.StatusCode, Form.Body);
			string ParametersToken = ExtractHiddenInput(Form.Body, "_p_");

			await Connection.PostInputFormAsync(Url, ParametersToken, true,
				new Dictionary<string, string>()
					{
						{ "FileContents", "Edited through URL fallback." }
					});

			Dictionary<string, object> Result = await ToolTask;
			AssertMcpOperationSucceeded(Result);
			Assert.AreEqual("Edited through URL fallback.", this.ReadEditableFile());

			Dictionary<string, object> Complete = await Sse.WaitForMethodAsync(
				"notifications/elicitation/complete");
			Assert.AreEqual(Required<object>(Elicitation, "id").ToString(),
				Required<object>(RequiredDictionary(Complete, "params"), "elicitationId").ToString());
		}

		[TestMethod]
		public async Task Test_028_Elicitation_Requires_Client_Capability()
		{
			await using McpConnection Connection = await Connect(FilesResource,
				this.accessToken, []);
			Dictionary<string, object> Result = await Connection.CallAsync("tools/call",
				new Dictionary<string, object>()
				{
					{ "name", "EditFile" },
					{ "arguments", new Dictionary<string, object>()
						{
							{ "LocalFileName", "editable.txt" },
							{ "Sensitive", false }
						}
					}
				});

			Assert.IsTrue(Required<bool>(Result, "isError"));
			string ErrorText = GetSingleTextContent(Result);
			Assert.IsTrue(ErrorText.Contains("support", StringComparison.OrdinalIgnoreCase),
				ErrorText);
			Assert.AreEqual("Original contents.", this.ReadEditableFile());
		}

		private string ReadEditableFile()
		{
			return File.ReadAllText(Path.Combine(this.fileStorageRoot,
				TestUserName, "editable.txt"));
		}

		private static Task<Dictionary<string, object>> CallEditFile(McpConnection Connection,
			bool Sensitive)
		{
			return Connection.CallAsync("tools/call",
				new Dictionary<string, object>()
				{
					{ "name", "EditFile" },
					{ "arguments", new Dictionary<string, object>()
						{
							{ "LocalFileName", "editable.txt" },
							{ "Sensitive", Sensitive }
						}
					}
				});
		}

		private Task<McpConnection> Connect(string Resource)
		{
			return Connect(Resource, this.accessToken,
				CreateElicitationCapabilities(true, true));
		}

		private static Task<McpConnection> Connect(string Resource, string AccessToken,
			Dictionary<string, object> Capabilities)
		{
			return McpConnection.CreateAsync(BaseUrl + Resource, AccessToken,
				ProtocolVersion, Capabilities);
		}

		private static Dictionary<string, object> CreateElicitationCapabilities(bool Form,
			bool Url)
		{
			Dictionary<string, object> Elicitation = [];

			if (Form)
				Elicitation["form"] = new Dictionary<string, object>();

			if (Url)
				Elicitation["url"] = new Dictionary<string, object>();

			return new Dictionary<string, object>()
			{
				{ "elicitation", Elicitation }
			};
		}

		private static Task<string> Login(string Scope)
		{
			return Login(TestUserName, TestPassword, Scope);
		}

		private static async Task<string> Login(string UserName, string Password,
			string Scope)
		{
			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + OAuthTokenResource.DefaultResourcePath),
				new Dictionary<string, string>()
				{
					{ "grant_type", "password" },
					{ "username", UserName },
					{ "password", Password },
					{ "scope", Scope }
				});

			TokenResponse.AssertOk();
			object Parsed = JSON.Parse(Encoding.UTF8.GetString(TokenResponse.Encoded));
			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));
			string AccessToken = Required<string>(Parsed, "access_token");
			Assert.IsFalse(string.IsNullOrEmpty(AccessToken));
			return AccessToken;
		}

		private static void AssertMcpOperationSucceeded(Dictionary<string, object> Result)
		{
			if (Result.TryGetValue("isError", out object IsError) &&
				IsError is bool Error && Error)
			{
				Assert.Fail(GetSingleTextContent(Result));
			}
		}

		private static string GetSingleTextContent(Dictionary<string, object> Result)
		{
			object[] Content = RequiredArray(Result, "content");
			Assert.HasCount(1, Content);
			return GetTextBlock(AsDictionary(Content[0], "content[0]"));
		}

		private static string GetTextBlock(Dictionary<string, object> Content)
		{
			Assert.AreEqual("text", Required<string>(Content, "type"));
			return Required<string>(Content, "text");
		}

		private static Dictionary<string, object> FindByName(object[] Items, string Name)
		{
			foreach (object Item in Items)
			{
				Dictionary<string, object> Dictionary = AsDictionary(Item, Name);
				if (Dictionary.TryGetValue("name", out object Value) &&
					Value is string s && s == Name)
				{
					return Dictionary;
				}
			}

			Assert.Fail("Item not found: " + Name);
			return null;
		}

		private static void AssertUniqueNames(int ExpectedCount, params object[][] Pages)
		{
			HashSet<string> Names = [];

			foreach (object[] Page in Pages)
			{
				foreach (object Item in Page)
				{
					Dictionary<string, object> Dictionary = AsDictionary(Item, "paged item");
					string Name = Required<string>(Dictionary, "name");
					Assert.IsTrue(Names.Add(Name), "Duplicate item returned across pages: " + Name);
				}
			}

			Assert.HasCount(ExpectedCount, Names);
		}

		private static void AssertArrayContains(object[] Values, string Expected)
		{
			foreach (object Value in Values)
			{
				if (Expected == Value?.ToString())
					return;
			}

			Assert.Fail("Expected array to contain: " + Expected);
		}

		private static void AssertHeaderContains(Dictionary<string, string[]> Headers,
			string HeaderName, string ExpectedText)
		{
			Assert.IsTrue(Headers.TryGetValue(HeaderName, out string[] Values),
				"Missing HTTP header: " + HeaderName);

			foreach (string Value in Values)
			{
				if (Value.Contains(ExpectedText, StringComparison.OrdinalIgnoreCase))
					return;
			}

			Assert.Fail("Expected " + HeaderName + " to contain: " + ExpectedText);
		}

		private static string ExtractHiddenInput(string Html, string Name)
		{
			Match Match = Regex.Match(Html,
				"<input[^>]*name=[\\\"']" + Regex.Escape(Name) +
				"[\\\"'][^>]*value=[\\\"']([^\\\"']*)[\\\"'][^>]*>",
				RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			Assert.IsTrue(Match.Success, "Hidden input not found: " + Name);
			return WebUtility.HtmlDecode(Match.Groups[1].Value);
		}

		private static Dictionary<string, object> RequiredDictionary(object Dictionary,
			string Key)
		{
			return AsDictionary(Required<object>(Dictionary, Key), Key);
		}

		private static Dictionary<string, object> AsDictionary(object Value, string Name)
		{
			if (Value is Dictionary<string, object> Dictionary)
				return Dictionary;

			if (Value is IDictionary<string, object> Interface)
				return new Dictionary<string, object>(Interface);

			throw new Exception("Expected JSON object: " + Name + " (" +
				(Value?.GetType().FullName ?? "null") + ")");
		}

		private static object[] RequiredArray(object Dictionary, string Key)
		{
			object Value = Required<object>(Dictionary, Key);

			if (Value is object[] ObjectArray)
				return ObjectArray;

			if (Value is Array Array)
			{
				object[] Result = new object[Array.Length];
				Array.CopyTo(Result, 0);
				return Result;
			}

			if (Value is IEnumerable Enumerable && Value is not string)
			{
				List<object> Result = [];
				foreach (object Item in Enumerable)
					Result.Add(Item);
				return [.. Result];
			}

			throw new Exception("Expected JSON array: " + Key + " (" +
				Value.GetType().FullName + ")");
		}

		private static T Required<T>(object Dictionary, string Key)
		{
			object Result;

			if (Dictionary is IDictionary<string, object> Typed &&
				Typed.TryGetValue(Key, out object GenericValue))
			{
				Result = GenericValue;
			}
			else if (Dictionary is IDictionary Untyped && Untyped.Contains(Key))
				Result = Untyped[Key];
			else
				throw new Exception("Expected JSON object to contain key: " + Key);

			if (Result is null)
				throw new Exception("Property value is null: " + Key);

			if (Result is T TypedResult)
				return TypedResult;

			if (typeof(T) == typeof(object))
				return (T)Result;

			try
			{
				return (T)Convert.ChangeType(Result, typeof(T));
			}
			catch (Exception ex)
			{
				throw new Exception("Property value not of expected type: " + Key +
					" (" + Result.GetType().FullName + ")", ex);
			}
		}

		private sealed class JsonRpcHttpResponse
		{
			public HttpStatusCode StatusCode;
			public string Body;
			public Dictionary<string, string[]> Headers;
			public Dictionary<string, object> Json;
		}

		private sealed class HtmlFormResponse
		{
			public HttpStatusCode StatusCode;
			public string Body;
			public Dictionary<string, string[]> Headers;
		}

		private sealed class McpConnection : IAsyncDisposable
		{
			private readonly HttpClient client;
			private readonly string endpoint;
			private readonly string accessToken;
			private readonly string protocolVersion;
			private int nextId = 1;
			private bool disposed;

			private McpConnection(string Endpoint, string AccessToken,
				string ProtocolVersion)
			{
				this.endpoint = Endpoint;
				this.accessToken = AccessToken;
				this.protocolVersion = ProtocolVersion;
				this.client = new HttpClient()
				{
					Timeout = Timeout.InfiniteTimeSpan
				};
			}

			public string SessionId { get; private set; }
			public Dictionary<string, object> InitializeResult { get; private set; }

			public static async Task<McpConnection> CreateAsync(string Endpoint,
				string AccessToken, string ProtocolVersion,
				Dictionary<string, object> Capabilities)
			{
				McpConnection Result = new(Endpoint, AccessToken, ProtocolVersion);
				await Result.Initialize(Capabilities);
				return Result;
			}

			private async Task Initialize(Dictionary<string, object> Capabilities)
			{
				int Id = this.NextId();
				JsonRpcHttpResponse Response = await this.SendJsonAsync(
					new Dictionary<string, object>()
					{
						{ "jsonrpc", "2.0" },
						{ "id", Id },
						{ "method", "initialize" },
						{ "params", new Dictionary<string, object>()
							{
								{ "protocolVersion", this.protocolVersion },
								{ "capabilities", Capabilities },
								{ "clientInfo", new Dictionary<string, object>()
									{
										{ "name", "Waher.Networking.HTTP.Test" },
										{ "title", "MCP Unit Tests" },
										{ "version", "1.0" }
									}
								}
							}
						}
					}, false);

				Assert.AreEqual(HttpStatusCode.OK, Response.StatusCode, Response.Body);
				Assert.IsNotNull(Response.Json, Response.Body);
				Assert.AreEqual("2.0", Required<string>(Response.Json, "jsonrpc"));
				Assert.AreEqual(Id, Convert.ToInt32(Required<object>(Response.Json, "id")));
				Assert.IsFalse(Response.Json.ContainsKey("error"), Response.Body);
				this.InitializeResult = RequiredDictionary(Response.Json, "result");

				Assert.IsTrue(Response.Headers.TryGetValue("MCP-Session-Id",
					out string[] SessionHeaders), "Missing MCP-Session-Id response header.");
				Assert.HasCount(1, SessionHeaders);
				this.SessionId = SessionHeaders[0];
				Assert.IsFalse(string.IsNullOrEmpty(this.SessionId));

				JsonRpcHttpResponse Initialized = await this.SendJsonAsync(
					new Dictionary<string, object>()
					{
						{ "jsonrpc", "2.0" },
						{ "method", "notifications/initialized" }
					}, true);

				Assert.AreEqual(HttpStatusCode.Accepted, Initialized.StatusCode,
					Initialized.Body);
			}

			public async Task<Dictionary<string, object>> CallAsync(string Method,
				Dictionary<string, object> Parameters)
			{
				JsonRpcHttpResponse Response = await this.CallRawAsync(Method, Parameters);
				Assert.AreEqual(HttpStatusCode.OK, Response.StatusCode, Response.Body);
				Assert.IsNotNull(Response.Json, Response.Body);
				Assert.AreEqual("2.0", Required<string>(Response.Json, "jsonrpc"));
				Assert.IsFalse(Response.Json.ContainsKey("error"), Response.Body);
				return RequiredDictionary(Response.Json, "result");
			}

			public Task<JsonRpcHttpResponse> CallRawAsync(string Method,
				Dictionary<string, object> Parameters)
			{
				return this.CallRawAsync(Method, Parameters, true);
			}

			public Task<JsonRpcHttpResponse> CallRawAsync(string Method,
				Dictionary<string, object> Parameters, bool IncludeSession)
			{
				return this.SendJsonAsync(new Dictionary<string, object>()
				{
					{ "jsonrpc", "2.0" },
					{ "id", this.NextId() },
					{ "method", Method },
					{ "params", Parameters }
				}, IncludeSession);
			}

			public async Task PostClientResultAsync(object Id,
				Dictionary<string, object> Result)
			{
				JsonRpcHttpResponse Response = await this.SendJsonAsync(
					new Dictionary<string, object>()
					{
						{ "jsonrpc", "2.0" },
						{ "id", Id },
						{ "result", Result }
					}, true);

				Assert.IsTrue(
					Response.StatusCode == HttpStatusCode.OK ||
					Response.StatusCode == HttpStatusCode.Accepted,
					"Expected client response acknowledgement (200 or 202), received " +
					Response.StatusCode + ": " + Response.Body);
			}

			public async Task<SseConnection> OpenSseAsync()
			{
				HttpRequestMessage Request = new(System.Net.Http.HttpMethod.Get, this.endpoint);
				this.SetCommonHeaders(Request, true);
				Request.Headers.Accept.Clear();
				Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

				using CancellationTokenSource Timeout = new(TimeSpan.FromSeconds(15));
				HttpResponseMessage Response = await this.client.SendAsync(Request,
					HttpCompletionOption.ResponseHeadersRead, Timeout.Token);
				if (Response.StatusCode != HttpStatusCode.OK)
				{
					HttpStatusCode StatusCode = Response.StatusCode;
					string Body = await Response.Content.ReadAsStringAsync(Timeout.Token);
					Request.Dispose();
					Response.Dispose();
					Assert.Fail("Unable to open SSE stream: " + StatusCode + ": " + Body);
				}

				Assert.AreEqual("text/event-stream",
					Response.Content.Headers.ContentType?.MediaType);
				Stream Stream = await Response.Content.ReadAsStreamAsync(Timeout.Token);
				SseConnection Result = new(Request, Response, Stream);
				await Result.WaitUntilOpenAsync();

				return Result;
			}

			public async Task<HtmlFormResponse> GetInputFormAsync(string Url)
			{
				using HttpRequestMessage Request = new(System.Net.Http.HttpMethod.Get, Url);
				this.SetCommonHeaders(Request, true);
				Request.Headers.Accept.Clear();
				Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

				using HttpResponseMessage Response = await this.client.SendAsync(Request);
				string Body = await Response.Content.ReadAsStringAsync();
				return new HtmlFormResponse()
				{
					StatusCode = Response.StatusCode,
					Body = Body,
					Headers = GetHeaders(Response)
				};
			}

			public async Task PostInputFormAsync(string Url, string ParametersToken,
				bool Accept, Dictionary<string, string> Fields)
			{
				using MultipartFormDataContent Content = [];
				Content.Add(new StringContent(ParametersToken), "_p_");
				Content.Add(new StringContent(Accept ? "true" : "false"), "_r_");

				foreach (KeyValuePair<string, string> Field in Fields)
					Content.Add(new StringContent(Field.Value ?? string.Empty), Field.Key);

				using HttpRequestMessage Request = new(System.Net.Http.HttpMethod.Post, Url)
				{
					Content = Content
				};
				this.SetCommonHeaders(Request, true);

				using HttpResponseMessage Response = await this.client.SendAsync(Request);
				string Body = await Response.Content.ReadAsStringAsync();
				Assert.AreEqual(HttpStatusCode.NoContent, Response.StatusCode, Body);
			}

			private async Task<JsonRpcHttpResponse> SendJsonAsync(
				Dictionary<string, object> Payload, bool IncludeSession)
			{
				using HttpRequestMessage Request = new(System.Net.Http.HttpMethod.Post,
					this.endpoint)
				{
					Content = new StringContent(JSON.Encode(Payload, false), Encoding.UTF8,
						"application/json")
				};
				this.SetCommonHeaders(Request, IncludeSession);

				using HttpResponseMessage Response = await this.client.SendAsync(Request,
					HttpCompletionOption.ResponseContentRead, CancellationToken.None);
				string Body = await Response.Content.ReadAsStringAsync();
				Dictionary<string, object> Json = null;

				if (!string.IsNullOrWhiteSpace(Body))
				{
					try
					{
						object Parsed = JSON.Parse(Body);
						if (Parsed is Dictionary<string, object> ParsedDictionary)
							Json = ParsedDictionary;
					}
					catch
					{
						// Error responses are not required to contain a JSON body.
					}
				}

				return new JsonRpcHttpResponse()
				{
					StatusCode = Response.StatusCode,
					Body = Body,
					Headers = GetHeaders(Response),
					Json = Json
				};
			}

			private void SetCommonHeaders(HttpRequestMessage Request, bool IncludeSession)
			{
				Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
					this.accessToken);
				Request.Headers.TryAddWithoutValidation("MCP-Protocol-Version",
					this.protocolVersion);

				if (IncludeSession)
				{
					Request.Headers.TryAddWithoutValidation("MCP-Session-Id",
						this.SessionId);
				}

				if (Request.Headers.Accept.Count == 0)
				{
					Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
				}
			}

			private static Dictionary<string, string[]> GetHeaders(HttpResponseMessage Response)
			{
				Dictionary<string, string[]> Result = new(StringComparer.OrdinalIgnoreCase);

				foreach (KeyValuePair<string, IEnumerable<string>> Header in Response.Headers)
					Result[Header.Key] = ToArray(Header.Value);

				foreach (KeyValuePair<string, IEnumerable<string>> Header in Response.Content.Headers)
					Result[Header.Key] = ToArray(Header.Value);

				return Result;
			}

			private static string[] ToArray(IEnumerable<string> Values)
			{
				List<string> Result = [];
				foreach (string Value in Values)
					Result.Add(Value);
				return [.. Result];
			}

			private int NextId()
			{
				return Interlocked.Increment(ref this.nextId);
			}

			public async ValueTask DisposeAsync()
			{
				if (this.disposed)
					return;

				this.disposed = true;

				if (!string.IsNullOrEmpty(this.SessionId))
				{
					try
					{
						using HttpRequestMessage Request = new(System.Net.Http.HttpMethod.Delete,
							this.endpoint);
						this.SetCommonHeaders(Request, true);
						using HttpResponseMessage Response = await this.client.SendAsync(Request);
					}
					catch
					{
						// Cleanup must not mask the test result.
					}
				}

				this.client.Dispose();
			}
		}

		private sealed class SseConnection(HttpRequestMessage Request, 
			HttpResponseMessage Response, Stream Stream) : IAsyncDisposable
		{
			private readonly HttpRequestMessage request = Request;
			private readonly HttpResponseMessage response = Response;
			private readonly StreamReader reader = new(Stream, Encoding.UTF8, true, 4096, false);
			private bool disposed;

			public async Task WaitUntilOpenAsync()
			{
				using CancellationTokenSource Timeout = new(TimeSpan.FromSeconds(15));

				while (true)
				{
					string Line = await this.reader.ReadLineAsync(Timeout.Token)
						?? throw new EndOfStreamException(
							"The MCP SSE stream was closed before initialization.");
					if (Line.Length > 0 && Line[0] == ':')
					{
						// The server registers the SSE subscription immediately after
						// flushing this initial comment. Yield briefly so the request that
						// triggers elicitation cannot overtake that registration.
						await Task.Delay(50, Timeout.Token);
						return;
					}
				}
			}

			public async Task<Dictionary<string, object>> WaitForMethodAsync(string Method)
			{
				using CancellationTokenSource Timeout = new(TimeSpan.FromSeconds(15));

				while (true)
				{
					Dictionary<string, object> Message = await this.ReadJsonMessageAsync(Timeout.Token);
					if (Message.TryGetValue("method", out object Value) &&
						Value is string s && s == Method)
					{
						return Message;
					}
				}
			}

			private async Task<Dictionary<string, object>> ReadJsonMessageAsync(
				CancellationToken CancellationToken)
			{
				StringBuilder Data = new();

				while (true)
				{
					string Line = await this.reader.ReadLineAsync(CancellationToken)
						?? throw new EndOfStreamException("The MCP SSE stream was closed.");

					if (Line.Length == 0)
					{
						if (Data.Length == 0)
							continue;

						object Parsed = JSON.Parse(Data.ToString());
						return AsDictionary(Parsed, "SSE data");
					}

					if (Line[0] == ':')
						continue;

					if (Line.StartsWith("data:", StringComparison.Ordinal))
					{
						if (Data.Length > 0)
							Data.AppendLine();

						string Value = Line[5..];
						if (Value.StartsWith(' '))
							Value = Value[1..];

						Data.Append(Value);
					}
				}
			}

			public ValueTask DisposeAsync()
			{
				if (!this.disposed)
				{
					this.disposed = true;
					this.reader.Dispose();
					this.response.Dispose();
					this.request.Dispose();
				}

				return ValueTask.CompletedTask;
			}
		}

		/// <summary>
		/// Adds enough deterministic tools and prompts to the Event Log server to exercise
		/// the fixed 20-item cursor page size while retaining the real Event Log methods.
		/// </summary>
		private sealed class TestEventLogMcpServer(string ResourceName, 
			ISnifferSet SnifferSet) : EventLogMcpServer(ResourceName, SnifferSet)
		{
			[McpServerTool("Cursor Tool 01", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool01()
			{
			}

			[McpServerTool("Cursor Tool 02", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool02()
			{
			}

			[McpServerTool("Cursor Tool 03", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool03()
			{
			}

			[McpServerTool("Cursor Tool 04", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool04()
			{
			}

			[McpServerTool("Cursor Tool 05", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool05()
			{
			}

			[McpServerTool("Cursor Tool 06", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool06()
			{
			}

			[McpServerTool("Cursor Tool 07", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool07()
			{
			}

			[McpServerTool("Cursor Tool 08", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool08()
			{
			}

			[McpServerTool("Cursor Tool 09", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool09()
			{
			}

			[McpServerTool("Cursor Tool 10", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool10()
			{
			}

			[McpServerTool("Cursor Tool 11", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool11()
			{
			}

			[McpServerTool("Cursor Tool 12", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool12()
			{
			}

			[McpServerTool("Cursor Tool 13", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool13()
			{
			}

			[McpServerTool("Cursor Tool 14", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool14()
			{
			}

			[McpServerTool("Cursor Tool 15", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool15()
			{
			}

			[McpServerTool("Cursor Tool 16", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool16()
			{
			}

			[McpServerTool("Cursor Tool 17", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool17()
			{
			}

			[McpServerTool("Cursor Tool 18", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool18()
			{
			}

			[McpServerTool("Cursor Tool 19", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool19()
			{
			}

			[McpServerTool("Cursor Tool 20", "Synthetic tool used to test MCP cursor pagination.", "", false, false, true, false)]
			public static void ZCursorTool20()
			{
			}

			[McpServerPrompt("Cursor Prompt 01", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt01()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 01.");
			}

			[McpServerPrompt("Cursor Prompt 02", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt02()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 02.");
			}

			[McpServerPrompt("Cursor Prompt 03", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt03()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 03.");
			}

			[McpServerPrompt("Cursor Prompt 04", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt04()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 04.");
			}

			[McpServerPrompt("Cursor Prompt 05", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt05()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 05.");
			}

			[McpServerPrompt("Cursor Prompt 06", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt06()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 06.");
			}

			[McpServerPrompt("Cursor Prompt 07", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt07()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 07.");
			}

			[McpServerPrompt("Cursor Prompt 08", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt08()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 08.");
			}

			[McpServerPrompt("Cursor Prompt 09", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt09()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 09.");
			}

			[McpServerPrompt("Cursor Prompt 10", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt10()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 10.");
			}

			[McpServerPrompt("Cursor Prompt 11", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt11()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 11.");
			}

			[McpServerPrompt("Cursor Prompt 12", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt12()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 12.");
			}

			[McpServerPrompt("Cursor Prompt 13", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt13()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 13.");
			}

			[McpServerPrompt("Cursor Prompt 14", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt14()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 14.");
			}

			[McpServerPrompt("Cursor Prompt 15", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt15()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 15.");
			}

			[McpServerPrompt("Cursor Prompt 16", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt16()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 16.");
			}

			[McpServerPrompt("Cursor Prompt 17", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt17()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 17.");
			}

			[McpServerPrompt("Cursor Prompt 18", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt18()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 18.");
			}

			[McpServerPrompt("Cursor Prompt 19", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt19()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 19.");
			}

			[McpServerPrompt("Cursor Prompt 20", "Synthetic prompt used to test MCP cursor pagination.", "")]
			public static PromptMessage ZCursorPrompt20()
			{
				return new PromptMessage(McpRole.User, "Cursor prompt 20.");
			}

		}
	}
}
