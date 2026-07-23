using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Images;
using Waher.Events;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.HTTP.Mcp.Model.Client;
using Waher.Networking.HTTP.Mcp.Model.ContentBlocks;
using Waher.Networking.HTTP.Mcp.Model.Resources;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Runtime.Collections;
using Waher.Runtime.Counters;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script.Model;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Abstract base class for HTTP-based Model Context Protocol (MCP) server resource.
	/// </summary>
	[OAuthScopesSupported(true, "McpScopesSupported")]
	public abstract class HttpMcpServerResource : JsonRpcWebService
	{
		private static readonly Cache<string, Session> sessions = GetCache();

		/// <summary>
		/// Scope suffix for MCP server tools is ":tools".
		/// </summary>
		public const string ToolsScopeSuffix = ":Tools";

		/// <summary>
		/// Scope suffix for MCP server prompts is ":prompts".
		/// </summary>
		public const string PromptsScopeSuffix = ":Prompts";

		/// <summary>
		/// Scope suffix for MCP server resources is ":resources".
		/// </summary>
		public const string ResourcesScopeSuffix = ":Resources";

		private static readonly ObjectContent defaultObjectEncoder = new ObjectContent();
		private static Dictionary<Type, IContentBlock> contentBlocks = GetContentBlocksFirstTime();
		private const int PageSize = 20;
		private readonly Dictionary<string, Tool> tools = new Dictionary<string, Tool>();
		private readonly Dictionary<string, Prompt> prompts = new Dictionary<string, Prompt>();
		private readonly ISnifferSet? snifferSet;
		private readonly string[] rootScopes;
		private readonly string[] toolScopes;
		private readonly string[] promptScopes;
		private readonly string[] resourceScopes;
		private readonly string[] scopesSupported;
		private readonly bool hasScopes;
		private readonly bool hasSnifferSet;
		private bool requiresAuthentication;

		private static Cache<string, Session> GetCache()
		{
			Cache<string, Session> Result = new Cache<string, Session>(int.MaxValue,
				TimeSpan.MaxValue, TimeSpan.FromHours(1));

			Result.Removed += (sender, e) => e.Value.DisposeAsync();

			return Result;
		}

		private static Dictionary<Type, IContentBlock> GetContentBlocksFirstTime()
		{
			Types.OnInvalidated += (_, e) => contentBlocks = GetContentBlocks();
			return GetContentBlocks();
		}

		private static Dictionary<Type, IContentBlock> GetContentBlocks()
		{
			Dictionary<Type, IContentBlock> Result = new Dictionary<Type, IContentBlock>();
			Type[] ContentBlockTypes = Types.GetTypesImplementingInterface(typeof(IContentBlock));

			foreach (Type T in ContentBlockTypes)
			{
				if (T.IsAbstract)
					continue;

				ConstructorInfo? CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					IContentBlock Encoder = (IContentBlock)CI.Invoke(Array.Empty<object>());

					foreach (Type T2 in Encoder.Encodes)
						Result[T2] = Encoder;
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return Result;
		}

		internal static bool TryGetEncodingContentBlock(Type Type, out IContentBlock ContentBlock)
		{
			return contentBlocks.TryGetValue(Type, out ContentBlock);
		}

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
			: this(ResourceName, Name, Title, Version, Description, Icons, WebSiteUri,
				  Instructions, null)
		{
		}

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
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public HttpMcpServerResource(string ResourceName, string Name, string Title,
			string Version, string Description, Icon[] Icons, Uri WebSiteUri,
			string Instructions, ISnifferSet? SnifferSet)
			: base(ResourceName, true, false)
		{
			this.Name = Name;
			this.Title = Title;
			this.Version = Version;
			this.Description = Description;
			this.Icons = new Icons(Icons);
			this.WebSiteUri = WebSiteUri;
			this.Instructions = Instructions;
			this.snifferSet = SnifferSet;
			this.hasSnifferSet = !(SnifferSet is null);

			if (this.Icons.Empty)
			{
				Icon[] DefaultIcons = GetDefaultIcons();
				if (DefaultIcons.Length > 0)
					this.Icons = new Icons(DefaultIcons);
			}

			ChunkedList<string> ScopeRoots = new ChunkedList<string>();

			foreach (McpScopeRootAttribute ScopeRoot in this.GetType().GetCustomAttributes<McpScopeRootAttribute>())
				ScopeRoots.Add(ScopeRoot.ScopeRoot);

			this.hasScopes = ScopeRoots.Count > 0;
			this.rootScopes = ScopeRoots.ToArray();

			int i, j, c = this.rootScopes.Length;

			this.toolScopes = new string[c];
			this.promptScopes = new string[c];
			this.resourceScopes = new string[c];
			this.scopesSupported = new string[c * 3];
			this.requiresAuthentication = this.ResourcesRequireAuthentication;

			for (i = j = 0; i < c; i++)
			{
				this.toolScopes[i] = this.scopesSupported[j++] = this.rootScopes[i] + ToolsScopeSuffix;
				this.promptScopes[i] = this.scopesSupported[j++] = this.rootScopes[i] + PromptsScopeSuffix;
				this.resourceScopes[i] = this.scopesSupported[j++] = this.rootScopes[i] + ResourcesScopeSuffix;
			}

			foreach (MethodInfo Method in this.GetType().GetMethods(BindingFlags.Instance |
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (Method.GetCustomAttribute<McpServerToolAttribute>() is
					McpServerToolAttribute McpServerToolAttribute)
				{
					this.RegisterToolNoNotification(Method, McpServerToolAttribute);
				}

				if (Method.GetCustomAttribute<McpServerPromptAttribute>() is
					McpServerPromptAttribute McpServerPromptAttribute)
				{
					this.RegisterPromptNoNotification(Method, McpServerPromptAttribute);
				}
			}
		}

		/// <summary>
		/// If Server-Sent Events (SSE) are supported by the resource.
		/// </summary>
		public override bool SupportsServerSentEvents => true;

		/// <summary>
		/// If a Server-Sent Events (SSE) welcome message should be sent to clients 
		/// with open subscriptions.
		/// </summary>
		public override bool SendSseWelcomeMessage => true;

		/// <summary>
		/// Server-Sent Events (SSE) welcome message, if one should be sent.
		/// </summary>
		public override string SseWelcomeMessage => "Connected to MCP Server at " +
			this.ResourceName + ". This connection should be made using an MCP Client.";

		/// <summary>
		/// Name of server.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Title of server.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// Version of server.
		/// </summary>
		public string Version { get; }

		/// <summary>
		/// Description of server.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Icons of server.
		/// </summary>
		public Icons Icons { get; }

		/// <summary>
		/// Website URI of server.
		/// </summary>
		public Uri WebSiteUri { get; }

		/// <summary>
		/// Instructions for server.
		/// </summary>
		public string Instructions { get; }

		/// <summary>
		/// OAUTH scopes supported by resource.
		/// </summary>
		/// <returns>Array of scopes supported.</returns>
		public string[] McpScopesSupported()
		{
			return this.scopesSupported;
		}

		/// <summary>
		/// Registers a MCP Server tool.
		/// </summary>
		/// <param name="Method">Method to call when tool is invoked.</param>
		/// <param name="Attributes">Attributes associated with tool</param>
		private void RegisterToolNoNotification(MethodInfo Method, McpServerToolAttribute Attributes)
		{
			lock (this.tools)
			{
				string Name = Method.Name;

				if (this.tools.ContainsKey(Name))
					throw new Exception("Tool already registered: " + Name);

				Tool Tool = new Tool(Method, Attributes.Title,
					Attributes.Description, Attributes.IconsMethod,
					Attributes.CanModifyEnvironment, Attributes.CanDestroyEnvironment,
					Attributes.Idempotent, Attributes.OpenWorldAccess);

				this.tools[Name] = Tool;

				this.requiresAuthentication |= Tool.RequiresAuthentication;
			}
		}

		/// <summary>
		/// Registers a MCP Server tool.
		/// </summary>
		/// <param name="Method">Method to call when tool is invoked.</param>
		/// <param name="Attributes">Attributes associated with tool</param>
		public Task RegisterTool(MethodInfo Method, McpServerToolAttribute Attributes)
		{
			this.RegisterToolNoNotification(Method, Attributes);

			return this.SendNotification(new Dictionary<string, object>()
			{
				{ "jsonrpc", "2.0" },
				{ "method", "notifications/tools/list_changed" }
			});
		}

		/// <summary>
		/// Registers a MCP Server prompt.
		/// </summary>
		/// <param name="Method">Method to call when prompt is invoked.</param>
		/// <param name="Attributes">Attributes associated with prompt</param>
		private void RegisterPromptNoNotification(MethodInfo Method, McpServerPromptAttribute Attributes)
		{
			lock (this.prompts)
			{
				string Name = Method.Name;

				if (this.prompts.ContainsKey(Name))
					throw new Exception("Prompt already registered: " + Name);

				Prompt Prompt = new Prompt(Method, Attributes.Title,
					Attributes.Description, Attributes.IconsMethod);

				this.prompts[Name] = Prompt;

				this.requiresAuthentication |= Prompt.RequiresAuthentication;
			}
		}

		/// <summary>
		/// Registers a MCP Server prompt.
		/// </summary>
		/// <param name="Method">Method to call when prompt is invoked.</param>
		/// <param name="Attributes">Attributes associated with prompt</param>
		public Task RegisterPrompt(MethodInfo Method, McpServerPromptAttribute Attributes)
		{
			this.RegisterPromptNoNotification(Method, Attributes);

			return this.SendNotification(new Dictionary<string, object>()
			{
				{ "jsonrpc", "2.0" },
				{ "method", "notifications/prompts/list_changed" }
			});
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
		/// Method called when a resource has been registered on a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public override void AddReference(HttpServer Server)
		{
			base.AddReference(Server);

			Tool[] Tools;
			Prompt[] Prompts;
			int c, d;

			lock (this.tools)
			{
				c = this.tools.Count;
				Tools = new Tool[c];
				this.tools.Values.CopyTo(Tools, 0);
			}

			lock (this.prompts)
			{
				d = this.prompts.Count;
				Prompts = new Prompt[d];
				this.prompts.Values.CopyTo(Prompts, 0);
			}

			ProtectedMethod[] Methods = new ProtectedMethod[c + d];
			Array.Copy(Tools, 0, Methods, 0, c);
			Array.Copy(Prompts, 0, Methods, c, d);

			foreach (ProtectedMethod Method in Methods)
				this.AddAuthenticationMechanisms(Method);
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
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="Response">HTTP Response object.</param>
		/// <param name="ProtocolVersion">Protocol Version</param>
		/// <param name="Capabilities">Client capabilities</param>
		/// <param name="ClientInfo">Client information</param>
		/// <returns>Server capabilities and information.</returns>
		[JsonRpcMethod]
		protected Dictionary<string, object> Initialize(HttpRequest Request,
			HttpResponse Response, string ProtocolVersion,
			Dictionary<string, object> Capabilities,
			Dictionary<string, object> ClientInfo)
		{
			if (!ClientCapabilities.TryParse(Capabilities, out ClientCapabilities? CapabilitiesParsed))
				CapabilitiesParsed = null;

			if (!Implementation.TryParse(ClientInfo, out Implementation? ClientInfoParsed))
				ClientInfoParsed = null;

			string RemoteEndpoint = Request.RemoteEndPoint.RemovePortNumber();
			string SessionId;

			do
			{
				SessionId = OAuth2Environment.GenerateRandomCode(32);

				if (this.HasJwtFactory)
				{
					SessionId = this.JwtFactory!.Create(
						new KeyValuePair<string, object>(JwtClaims.JwtId, SessionId),
						new KeyValuePair<string, object>(JwtClaims.ClientId, RemoteEndpoint));
				}
			}
			while (sessions.ContainsKey(SessionId));

			Session Session = new Session(SessionId, ProtocolVersion,
				CapabilitiesParsed, ClientInfoParsed, RemoteEndpoint, this.snifferSet);

			sessions[SessionId] = Session;
			Response.SetHeader("MCP-Session-Id", SessionId);

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".Initialize(");
				sb.Append(ProtocolVersion);
				sb.AppendLine(",");
				sb.Append(JSON.Encode(Capabilities, 1));
				sb.AppendLine(",");
				sb.Append(JSON.Encode(ClientInfo, 1));
				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "protocolVersion", "2025-11-25" },
				{ "capabilities", new Dictionary<string, object>()
					{
						{ "prompts", new Dictionary<string, object>()
							{
								{ "listChanged", true }
							}
						},
						{ "resources", new Dictionary<string, object>()
							{
								{ "subscribe", false },		// TODO (for instance, when available resources change)
								{ "listChanged", true }
							}
						},
						{ "tools", new Dictionary<string, object>()
							{
								{ "listChanged", true }
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

			if (this.hasSnifferSet)
				Session.TransmitText(JSON.Encode(Result, true));

			return Result;
		}

		/// <summary>
		/// Notification that the client has completed its initialization.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		[JsonRpcMethod]
		protected async Task Notifications_Initialized(HttpRequest Request,
			HttpResponse Response)
		{
			Session? Session = await this.TryGetSession(Request, Response);
			if (Session is null)
				return;

			if (this.hasSnifferSet)
				Session.ReceiveText(this.Name + ".Initialized()");

			Log.Informational("MCP client initialized: " + Request.RemoteEndPoint,
				this.ResourceName, Request.RemoteEndPoint, "McpInitialized");
		}

		private async Task<Session?> TryGetSession(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.Header.TryGetHeaderField("MCP-Session-Id", out HttpField SessionHeader))
			{
				await Response.SendResponse(new BadRequestException("Missing MCP-Session-Id header."));
				return null;
			}

			string SessionId = SessionHeader.Value;

			if (this.HasJwtFactory)
			{
				if (!JwtToken.TryParse(SessionId, out JwtToken Token))
				{
					await Response.SendResponse(new NotFoundException("Invalid MCP-Session-Id."));
					return null;
				}

				if (!this.JwtFactory!.IsValid(Token))
				{
					await Response.SendResponse(new NotFoundException("MCP-Session-Id invalid or expired."));
					return null;
				}
			}

			if (!sessions.TryGetValue(SessionId, out Session? Session))
			{
				await Response.SendResponse(new NotFoundException("MCP-Session-Id expired or not found."));
				return null;
			}

			if (Session.RemoteEndpoint != Request.RemoteEndPoint.RemovePortNumber())
			{
				await Response.SendResponse(new NotFoundException("MCP-Session-Id not found for this endpoint."));
				return null;
			}

			return Session;
		}

		/// <summary>
		/// Lists available MCP server tools.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of tools.</returns>
		[JsonRpcMethod]
		protected async Task<Dictionary<string, object>?> Tools_List(HttpRequest Request,
			HttpResponse Response, string? Cursor = null)
		{
			Session? Session = await this.TryGetSession(Request, Response);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Request, Response, Session);
			if (Response.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".tools/list(");
				sb.Append(Cursor);
				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			int Offset = 0;
			int MaxCount = PageSize;

			if (!string.IsNullOrEmpty(Cursor))
			{
				if (!int.TryParse(Cursor, out Offset) || Offset < 0)
				{
					if (!this.hasSnifferSet)
						Session.Error("Invalid cursor: " + Cursor);

					await Response.SendResponse(new BadRequestException("Invalid cursor."));
					return null;
				}
			}

			ChunkedList<Tool> Tools = new ChunkedList<Tool>();
			int Next = Offset + MaxCount;

			Dictionary<string, object> Result = new Dictionary<string, object>();

			lock (this.tools)
			{
				foreach (Tool Tool in this.tools.Values)
				{
					if (!Tool.IsAuthorized(User))
						continue;

					if (!this.CheckScopes(User, this.toolScopes, out _))
						continue;

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

			Dictionary<string, object>[] ToolsJson = new Dictionary<string, object>[c];

			foreach (Tool Tool in Tools)
				ToolsJson[i++] = await Tool.ToJson(this);

			Result["tools"] = ToolsJson;

			if (this.hasSnifferSet)
				Session.TransmitText(JSON.Encode(Result, true));

			return Result;
		}

		private bool CheckScopes(IUser? User, string[] Scopes, out string? MissingPrivilege)
		{
			if (!this.hasScopes)
			{
				MissingPrivilege = null;
				return true;
			}

			if (User is null)
			{
				MissingPrivilege = null;
				return false;
			}

			return OAuthResource.HasScopePrivileges(Scopes, User, out MissingPrivilege);
		}

		private async Task<IUser?> GetAuthenticatedUser(HttpRequest Request,
			HttpResponse Response, Session Session)
		{
			IUser User = Request.User;
			bool Encrypted = Request.Encrypted;
			int Strength = Request.CipherStrength;

			if ((this.requiresAuthentication || !(Request.Header.Authorization is null)) &&
				User is null)
			{
				if (this.AuthenticationSchemes is null)
				{
					await Response.SendResponse(new ForbiddenException());

					if (this.hasSnifferSet)
						Session.Error("Access denied. No authentication schemes available.");

					return null;
				}

				foreach (HttpAuthenticationScheme Scheme in this.AuthenticationSchemes)
				{
					if (Scheme.RequireEncryption &&
						(!Encrypted || Strength < Scheme.MinStrength))
					{
						continue;
					}

					if (Scheme.UserSessions && Request.Session is null)
						Request.GetSessionFromCookie();

					User = await Scheme.IsAuthenticated(Request);
					if (!(User is null))
					{
						Request.User = User;
						break;
					}
				}

				if (User is null)
				{
					List<string> Challenges = new List<string>();

					foreach (HttpAuthenticationScheme Scheme in this.AuthenticationSchemes
						?? Array.Empty<HttpAuthenticationScheme>())
					{
						if (Scheme.RequireEncryption &&
							(!Encrypted || Strength < Scheme.MinStrength))
						{
							continue;
						}

						foreach (string Challenge in Scheme.GetChallenges(Request))
							Challenges.Add(Challenge);
					}

					await Response.SendResponse(new UnauthorizedException(
						Challenges.ToArray()));

					if (this.hasSnifferSet)
						Session.Error("Access denied. Unauthorized.");

					return null;
				}
			}

			if (!Session.IsAuthenticated && !(User is null))
				await Session.SetUserName(User.UserName);

			return User;
		}

		/// <summary>
		/// Calls an MCP server tool.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Name">Name of the tool to call.</param>
		/// <param name="Arguments">Arguments for the tool.</param>
		/// <param name="Task">If specified, the caller is requesting task-augmented 
		/// execution for this request. The request will return a CreateTaskResult 
		/// immediately, and the actual result can be retrieved later via tasks/result.
		/// 
		/// Task augmentation is subject to capability negotiation - receivers MUST declare 
		/// support for task augmentation of specific request types in their capabilities.</param>
		/// <param name="_Meta">Associated meta-data, if available.</param>
		/// <returns>Dictionary containing the result of the tool call.</returns>
		[JsonRpcMethod]
		protected async Task<Dictionary<string, object?>?> Tools_Call(HttpRequest Request,
			HttpResponse Response, string Name, Dictionary<string, object?> Arguments,
			object? Task = null, [JsonRpcMetaDataArgument] object? _Meta = null)
		{
			Session? Session = await this.TryGetSession(Request, Response);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Request, Response, Session);
			if (Response.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".tools/call(");
				sb.Append(Name);
				sb.AppendLine(",");
				sb.Append(JSON.Encode(Arguments, 1));

				if (!(Task is null))
				{
					sb.AppendLine(",");
					JSON.Encode(Task, 1, sb);
				}

				if (!(_Meta is null))
				{
					sb.AppendLine(",");
					JSON.Encode(_Meta, 1, sb);
				}

				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			Dictionary<string, object?> Result = new Dictionary<string, object?>();
			object? ToolResult;

			try
			{
				if (!this.tools.TryGetValue(Name, out Tool? Tool))
				{
					if (this.hasSnifferSet)
						Session.Error("Tool not found: " + Name);

					await Response.SendResponse(new NotFoundException("Tool not found."));
					return null;
				}

				if (!Tool.IsAuthorized(User, out string? MissingPrivilege) ||
					!this.CheckScopes(User, this.toolScopes, out MissingPrivilege))
				{
					if (this.hasSnifferSet)
						Session.Error("Access denied. Missing privilege: " + MissingPrivilege);

					await Response.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
						User?.UserName ?? string.Empty, MissingPrivilege ?? string.Empty));
					return null;
				}

				await RuntimeCounters.IncrementCounter("MCP.Tool." + Name);
				await RuntimeCounters.IncrementCounter("MCP.User.Tool." + Session.UserName);

				Dictionary<string, object?>? MetaData = _Meta as Dictionary<string, object?>;

				if (Tool.TryBuildRequest(Arguments, Request, Response, MetaData,
					out string? Reason, out object?[]? Arguments2))
				{
					ToolResult = await ScriptNode.WaitPossibleTask(
						Tool.Method.Invoke(this, Arguments2));
				}
				else
				{
					if (this.hasSnifferSet)
						Session.Error(Reason);

					ToolResult = Reason;
					Result["isError"] = true;
				}
			}
			catch (Exception ex)
			{
				if (this.hasSnifferSet)
					Session.Exception(ex);

				ToolResult = ex.Message;
				Result["isError"] = true;
			}

			if (ToolResult is null)
				Result["content"] = Array.Empty<object>();
			else if (ToolResult is Dictionary<string, object> StructuredContent)
			{
				Result["content"] = new object[]
				{
					new Dictionary<string, object>()
					{
						{ "type", "text" },
						{ "text", JSON.Encode(StructuredContent, false) }
					}
				};
				Result["structuredContent"] = new Dictionary<string, object>()
				{
					{ "result", StructuredContent }
				};
			}
			else
			{
				Type T = ToolResult.GetType();

				if (contentBlocks.TryGetValue(T, out IContentBlock Encoder))
				{
					if (Encoder.IsStructuredContent)
					{
						StructuredContent = await Encoder.Encode(ToolResult);

						Result["content"] = new object[]
						{
							new Dictionary<string, object>()
							{
								{ "type", "text" },
								{ "text", JSON.Encode(StructuredContent, false) }
							}
						};
						Result["structuredContent"] = new Dictionary<string, object>()
						{
							{ "result", StructuredContent }
						};
					}
					else
						Result["content"] = new object[] { await Encoder.Encode(ToolResult) };
				}
				else if (T.IsArray && ToolResult is IEnumerable Enumerable)
				{
					ChunkedList<object> Content = new ChunkedList<object>();
					IEnumerator e = Enumerable.GetEnumerator();

					while (e.MoveNext())
					{
						object? Item = e.Current;
						if (Item is null)
							continue;

						Type T2 = Item.GetType();
						if (!contentBlocks.TryGetValue(T2, out IContentBlock Encoder2))
							Encoder2 = defaultObjectEncoder;

						Content.Add(await Encoder2.Encode(Item));
					}

					Result["content"] = Content.ToArray();
				}
				else
				{
					StructuredContent = await defaultObjectEncoder.Encode(ToolResult);

					Result["content"] = new object[]
					{
						new Dictionary<string, object>()
						{
							{ "type", "text" },
							{ "text", JSON.Encode(StructuredContent, false) }
						}
					};
					Result["structuredContent"] = new Dictionary<string, object>()
					{
						{ "result", StructuredContent }
					};
				}
			}

			if (this.hasSnifferSet)
				Session.TransmitText(JSON.Encode(Result, true));

			return Result;
		}

		/// <summary>
		/// Lists available MCP server prompts.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of prompts.</returns>
		[JsonRpcMethod]
		protected async Task<Dictionary<string, object>?> Prompts_List(HttpRequest Request,
			HttpResponse Response, string? Cursor = null)
		{
			Session? Session = await this.TryGetSession(Request, Response);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Request, Response, Session);
			if (Response.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".prompts/list(");
				sb.Append(Cursor);
				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			int Offset = 0;
			int MaxCount = PageSize;

			if (!string.IsNullOrEmpty(Cursor))
			{
				if (!int.TryParse(Cursor, out Offset) || Offset < 0)
				{
					if (!this.hasSnifferSet)
						Session.Error("Invalid cursor: " + Cursor);

					await Response.SendResponse(new BadRequestException("Invalid cursor."));
					return null;
				}
			}

			ChunkedList<Prompt> Prompts = new ChunkedList<Prompt>();
			int Next = Offset + MaxCount;

			Dictionary<string, object> Result = new Dictionary<string, object>();

			lock (this.prompts)
			{
				foreach (Prompt Prompt in this.prompts.Values)
				{
					if (!Prompt.IsAuthorized(User))
						continue;

					if (!this.CheckScopes(User, this.promptScopes, out _))
						continue;

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

					Prompts.Add(Prompt);
					MaxCount--;
				}
			}

			int i = 0;
			int c = Prompts.Count;

			Dictionary<string, object>[] PromptsJson = new Dictionary<string, object>[c];

			foreach (Prompt Prompt in Prompts)
				PromptsJson[i++] = await Prompt.ToJson(this);

			Result["prompts"] = PromptsJson;

			if (this.hasSnifferSet)
				Session.TransmitText(JSON.Encode(Result, true));

			return Result;
		}

		/// <summary>
		/// Gets an MCP server prompt.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Name">Name of the prompt to call.</param>
		/// <param name="Arguments">Arguments for the prompt.</param>
		/// <param name="_Meta">Associated meta-data, if available.</param>
		/// <returns>Dictionary containing the result of the tool call.</returns>
		[JsonRpcMethod]
		protected async Task<Dictionary<string, object?>?> Prompts_Get(HttpRequest Request,
			HttpResponse Response, string Name, Dictionary<string, object?> Arguments,
			[JsonRpcMetaDataArgument] object? _Meta = null)
		{
			Session? Session = await this.TryGetSession(Request, Response);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Request, Response, Session);
			if (Response.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".prompts/get(");
				sb.Append(Name);
				sb.AppendLine(",");
				sb.Append(JSON.Encode(Arguments, 1));

				if (!(_Meta is null))
				{
					sb.AppendLine(",");
					JSON.Encode(_Meta, 1, sb);
				}

				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			Dictionary<string, object?> Result = new Dictionary<string, object?>();
			object? PromptResult;

			try
			{
				if (!this.prompts.TryGetValue(Name, out Prompt? Prompt))
				{
					if (this.hasSnifferSet)
						Session.Error("Prompt not found: " + Name);

					await Response.SendResponse(new NotFoundException("Prompt not found."));
					return null;
				}

				if (!Prompt.IsAuthorized(User, out string? MissingPrivilege) ||
					!this.CheckScopes(User, this.promptScopes, out MissingPrivilege))
				{
					if (this.hasSnifferSet)
						Session.Error("Access denied. Missing privilege: " + MissingPrivilege);

					await Response.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
						User?.UserName ?? string.Empty, MissingPrivilege ?? string.Empty));
					return null;
				}

				await RuntimeCounters.IncrementCounter("MCP.Prompt." + Name);
				await RuntimeCounters.IncrementCounter("MCP.User.Prompt." + Session.UserName);

				Dictionary<string, object?>? MetaData = _Meta as Dictionary<string, object?>;

				if (Prompt.TryBuildRequest(Arguments, Request, Response, MetaData,
					out string? Reason, out object?[]? Arguments2))
				{
					PromptResult = await ScriptNode.WaitPossibleTask(
						Prompt.Method.Invoke(this, Arguments2));
				}
				else
				{
					if (this.hasSnifferSet)
						Session.Error(Reason);

					PromptResult = Reason;
					Result["isError"] = true;
				}

				Prompt.ReturnAttributes?.Annotate(Result);
			}
			catch (Exception ex)
			{
				if (this.hasSnifferSet)
					Session.Exception(ex);

				PromptResult = ex.Message;
				Result["isError"] = true;
			}

			ChunkedList<PromptMessage> Messages = new ChunkedList<PromptMessage>();

			if (!(PromptResult is null))
			{
				if (PromptResult is PromptMessage PromptMessage)
					Messages.Add(PromptMessage);
				else if (PromptResult is IEnumerable<PromptMessage> PromptMessages)
					Messages.AddRange(PromptMessages);
				else
				{
					Type T = PromptResult.GetType();

					if (contentBlocks.TryGetValue(T, out IContentBlock Encoder))
					{
						Messages.Add(new PromptMessage(McpRole.Assistant,
							await Encoder.Encode(PromptResult)));
					}
					else if (T.IsArray && PromptResult is IEnumerable Enumerable)
					{
						IEnumerator e = Enumerable.GetEnumerator();

						while (e.MoveNext())
						{
							object? Item = e.Current;
							if (Item is null)
								continue;

							if (e.Current is PromptMessage PromptMessage2)
								Messages.Add(PromptMessage2);
							else if (e.Current is IEnumerable<PromptMessage> PromptMessages2)
								Messages.AddRange(PromptMessages2);
							else
								Messages.Add(new PromptMessage(McpRole.Assistant, e.Current));
						}
					}
					else
						Messages.Add(new PromptMessage(McpRole.Assistant, PromptResult));
				}
			}

			int i = 0;
			int c = Messages.Count;
			Dictionary<string, object>[] EncodedMessages = new Dictionary<string, object>[c];

			foreach (PromptMessage Message in Messages)
			{
				Dictionary<string, object> Content;

				if (Message.IsEncoded)
					Content = Message.Encoded!;
				else
				{
					Type T = Message.Content.GetType();
					if (!contentBlocks.TryGetValue(T, out IContentBlock Encoder))
						Encoder = defaultObjectEncoder;

					Content = await Encoder.Encode(Message.Content);
				}

				EncodedMessages[i++] = new Dictionary<string, object>()
				{
					{ "role", Message.Role.ToString().ToLower() },
					{ "content", Content }
				};
			}

			Result["messages"] = EncodedMessages;

			if (this.hasSnifferSet)
				Session.TransmitText(JSON.Encode(Result, true));

			return Result;
		}

		/// <summary>
		/// If resources published by the MCP Server require authentication. If true, 
		/// the client must authenticate before resources can be listed or read.
		/// </summary>
		public virtual bool ResourcesRequireAuthentication => false;

		/// <summary>
		/// Lists available MCP server resources.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of resources.</returns>
		[JsonRpcMethod]
		protected async Task<Dictionary<string, object>?> Resources_List(HttpRequest Request,
			HttpResponse Response, string? Cursor = null)
		{
			Session? Session = await this.TryGetSession(Request, Response);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Request, Response, Session);
			if (Response.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".resources/list(");
				sb.Append(Cursor);
				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			int Offset = 0;
			int MaxCount = PageSize;

			if (!string.IsNullOrEmpty(Cursor))
			{
				if (!int.TryParse(Cursor, out Offset) || Offset < 0)
				{
					if (!this.hasSnifferSet)
						Session.Error("Invalid cursor: " + Cursor);

					await Response.SendResponse(new BadRequestException("Invalid cursor."));
					return null;
				}
			}

			Resource[] AllResources = await this.GetResources(Request, User);
			ChunkedList<Resource> Resources = new ChunkedList<Resource>();
			Dictionary<string, object> Result = new Dictionary<string, object>();
			int Next = Offset + MaxCount;

			foreach (Resource Resource in AllResources)
			{
				if (!Resource.IsAuthorized(User, out _))
					continue;

				if (!this.CheckScopes(User, this.resourceScopes, out _))
					continue;

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

				Resources.Add(Resource);
				MaxCount--;
			}

			int i = 0;
			int c = Resources.Count;

			Dictionary<string, object>[] ResourcesJson = new Dictionary<string, object>[c];

			foreach (Resource Resource in Resources)
				ResourcesJson[i++] = Resource.ToJson();

			Result["resources"] = ResourcesJson;

			if (this.hasSnifferSet)
				Session.TransmitText(JSON.Encode(Result, true));

			return Result;
		}

		/// <summary>
		/// Reads an MCP server resource.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Uri">URI of the resource to read.</param>
		/// <param name="_Meta">Associated meta-data, if available.</param>
		/// <returns>Dictionary containing the result of the tool call.</returns>
		[JsonRpcMethod]
		protected async Task<Dictionary<string, object>?> Resources_Read(HttpRequest Request,
			HttpResponse Response, Uri Uri, [JsonRpcMetaDataArgument] object? _Meta = null)
		{
			Session? Session = await this.TryGetSession(Request, Response);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Request, Response, Session);
			if (Response.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".resources/read(");
				sb.Append(Uri);

				if (!(_Meta is null))
				{
					sb.AppendLine(",");
					JSON.Encode(_Meta, 1, sb);
				}

				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			Resource? Resource = await this.TryGetResource(Request, User, Uri);

			if (Resource is null)
			{
				if (this.hasSnifferSet)
					Session.Error("Resource not found: " + Uri);

				await Response.SendResponse(new NotFoundException("Resource not found."));
				return null;
			}

			if (!Resource.IsAuthorized(User, out string? MissingPrivilege) ||
				!this.CheckScopes(User, this.resourceScopes, out MissingPrivilege))
			{
				if (this.hasSnifferSet)
					Session.Error("Access denied. Missing privilege: " + MissingPrivilege);

				await Response.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
					User?.UserName ?? string.Empty, MissingPrivilege ?? string.Empty));
				return null;
			}

			await RuntimeCounters.IncrementCounter("MCP.Resource." + Resource.Name);
			await RuntimeCounters.IncrementCounter("MCP.User.Resource." + Session.UserName);

			Dictionary<string, object>? MetaData = _Meta as Dictionary<string, object>;

			IResourceContent[] Content = await Resource.Read(MetaData);
			int i, c = Content.Length;

			Dictionary<string, object>[] Contents = new Dictionary<string, object>[c];

			for (i = 0; i < c; i++)
				Contents[i] = Content[i].Encode();

			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "contents",Contents }
			};

			if (this.hasSnifferSet)
				Session.TransmitText(JSON.Encode(Result, true));

			return Result;
		}

		/// <summary>
		/// Gets available resources.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <returns>Array of resources.</returns>
		public virtual Task<Resource[]> GetResources(HttpRequest Request, IUser? User)
		{
			return Task.FromResult(Array.Empty<Resource>());
		}

		/// <summary>
		/// Tries to get a resource, given its URI.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Uri">URI of resource.</param>
		/// <returns>Resource, if found (and user has access rights to it), null otherwise.</returns>
		public virtual Task<Resource?> TryGetResource(HttpRequest Request, IUser? User,
			Uri Uri)
		{
			return Task.FromResult<Resource?>(null);
		}

		/// <summary>
		/// Called when the resources have been updated (new resources added,
		/// existing resources updated or removed.)
		/// </summary>
		/// <param name="User">MCP Client user whose resources have been updated.</param>
		/// <remarks>If multiple updates are done simultaneously, only call this
		/// method once at the end, not for each update.</remarks>
		public virtual async void ResourcesUpdated(IUser User)
		{
			try
			{
				// TODO: Only to clients who has resources that have been updated.

				await this.SendNotification(new Dictionary<string, object>()
				{
					{ "jsonrpc", "2.0" },
					{ "method", "notifications/resources/list_changed" }
				});
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private Task SendNotification(Dictionary<string, object> Notification)
		{
			// TODO: Sniffers

			return this.SendEvent(
				new KeyValuePair<string, object>("event", "message"),
				new KeyValuePair<string, object>("data", JSON.Encode(Notification, false)));
		}

		/// <summary>
		/// Called when a single resource has been updated.
		/// </summary>
		/// <param name="User">MCP Client user whose resource has been updated.</param>
		/// <param name="Uri">The URI of the updated resource.</param>
		public virtual async void ResourceUpdated(IUser User, Uri Uri)
		{
			try
			{
				// TODO: Only to subscribers
				// TODO: Sniffers

				await this.SendNotification(new Dictionary<string, object>()
				{
					{ "jsonrpc", "2.0" },
					{ "method", "notifications/resources/updated" },
					{ "params", new Dictionary<string, object>()
						{
							{ "uri", Uri.OriginalString }
						}
					},
				});
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
	}
}
