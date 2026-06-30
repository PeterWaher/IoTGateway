using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Images;
using Waher.Events;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.HTTP.Mcp.Model.Client;
using Waher.Networking.HTTP.Mcp.Model.ContentBlocks;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script.Model;
using Waher.Security;
using Waher.Things.Http;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Abstract base class for HTTP-based Model Context Protocol (MCP) server resource.
	/// </summary>
	[OAuthScopesSupported(true, "McpScopesSupported")]
	public abstract class HttpMcpServerResource : JsonRpcWebService
	{
		private static readonly ObjectContent defaultObjectEncoder = new ObjectContent();
		private static Dictionary<Type, IContentBlock> contentBlocks = GetContentBlocksFirstTime();
		private const int PageSize = 20;
		private readonly Dictionary<string, Tool> tools = new Dictionary<string, Tool>();
		private readonly Dictionary<string, Prompt> prompts = new Dictionary<string, Prompt>();
		private bool requiresAuthentication = false;
		private bool supportsTools = false;
		private bool supportsPrompts = false;
		private bool supportsResources = false;
		private HttpAuthenticationScheme[]? authenticationSchemes = null;

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

				if (Method.GetCustomAttribute<McpServerPromptAttribute>() is
					McpServerPromptAttribute McpServerPromptAttribute)
				{
					this.RegisterPrompt(Method, McpServerPromptAttribute);
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
			ChunkedList<string> Scopes = new ChunkedList<string>();

			if (this.supportsTools)
				Scopes.Add("mcp:tools");

			if (this.supportsPrompts)
				Scopes.Add("mcp:prompts");

			if (this.supportsResources)
				Scopes.Add("mcp:resources");

			return Scopes.ToArray();
		}

		/// <summary>
		/// Registers a MCP Server tool.
		/// </summary>
		/// <param name="Method">Method to call when tool is invoked.</param>
		/// <param name="Attributes">Attributes associated with tool</param>
		public void RegisterTool(MethodInfo Method, McpServerToolAttribute Attributes)
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
				this.supportsTools = true;
			}

			// TODO: Send notification to clients about new tool.
			// TODO: Declare tool notification in capabilities.
		}

		/// <summary>
		/// Registers a MCP Server prompt.
		/// </summary>
		/// <param name="Method">Method to call when prompt is invoked.</param>
		/// <param name="Attributes">Attributes associated with prompt</param>
		public void RegisterPrompt(MethodInfo Method, McpServerPromptAttribute Attributes)
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
				this.supportsPrompts = true;
			}

			// TODO: Send notification to clients about new prompt.
			// TODO: Declare prompt notification in capabilities.
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

			bool HasMetaDataResource = this.TryGetResourceMetaDataResource(Server,
				out ResourceMetaDataResource? MetaDataResource);
			bool HasDomain = Types.TryGetModuleParameter("Domain", out string Domain);

			lock (this.tools)
			{
				foreach (Tool Tool in this.tools.Values)
				{
					if (Tool.RequiresAuthentication)
					{
						if (HasMetaDataResource && HasDomain)
						{
							Tool.AuthenticationMechanisms = HttpModule.GetAuthenticationSchemes(
								new Uri(MetaDataResource!.GetResourceMetaDataUri(true, Domain, this.ResourceName)),
								Tool.RequiredPrivileges);
						}
						else
						{
							Tool.AuthenticationMechanisms = HttpModule.GetAuthenticationSchemes(
								Tool.RequiredPrivileges);
						}
					}
				}
			}

			lock (this.prompts)
			{
				foreach (Prompt Prompt in this.prompts.Values)
				{
					if (Prompt.RequiresAuthentication)
					{
						if (HasMetaDataResource && HasDomain)
						{
							Prompt.AuthenticationMechanisms = HttpModule.GetAuthenticationSchemes(
								new Uri(MetaDataResource!.GetResourceMetaDataUri(true, Domain, this.ResourceName)),
								Prompt.RequiredPrivileges);
						}
						else
						{
							Prompt.AuthenticationMechanisms = HttpModule.GetAuthenticationSchemes(
								Prompt.RequiredPrivileges);
						}
					}
				}
			}
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

		/// <summary>
		/// Notification that the client has completed its initialization.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		[JsonRpcMethod]
		protected void Notifications_Initialized(HttpRequest Request)
		{
			Log.Informational("MCP client initialized: " + Request.RemoteEndPoint,
				this.ResourceName, Request.RemoteEndPoint, "McpInitialized");
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
			IUser? User = await this.GetAuthenticatedUser(Request, Response);
			if (Response.ResponseSent)
				return null;

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
					if (!Tool.IsAuthorized(User))
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

			ToolsJson = new Dictionary<string, object>[c];

			foreach (Tool Tool in Tools)
				ToolsJson[i++] = await Tool.ToJson(this);

			Result["tools"] = ToolsJson;

			return Result;
		}

		private async Task<IUser?> GetAuthenticatedUser(HttpRequest Request, HttpResponse Response)
		{
			IUser User = Request.User;
			bool Encrypted = Request.Encrypted;
			int Strength = Request.CipherStrength;

			if (this.requiresAuthentication && User is null)
			{
				this.authenticationSchemes ??= HttpModule.GetAuthenticationSchemes();

				foreach (HttpAuthenticationScheme Scheme in this.authenticationSchemes)
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

					foreach (HttpAuthenticationScheme Scheme in this.authenticationSchemes
						?? Array.Empty<HttpAuthenticationScheme>())
					{
						if (Scheme.RequireEncryption &&
							(!Encrypted || Strength < Scheme.MinStrength))
						{
							continue;
						}

						foreach (string Challenge in Scheme.GetChallenges())
							Challenges.Add(Challenge);
					}

					await Response.SendResponse(new UnauthorizedException(
						Challenges.ToArray()));
					return null;
				}
			}

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
			IUser? User = await this.GetAuthenticatedUser(Request, Response);
			if (Response.ResponseSent)
				return null;

			Dictionary<string, object?> Result = new Dictionary<string, object?>();
			object? ToolResult;

			try
			{
				if (!this.tools.TryGetValue(Name, out Tool? Tool))
					throw new NotFoundException("Tool not found: " + Name);

				Tool.AssertAuthorized(this.ResourceName, User);

				Dictionary<string, object?>? MetaData = _Meta as Dictionary<string, object?>;

				if (Tool.TryBuildRequest(Arguments, Request, Response, MetaData,
					out string? Reason, out object?[]? Arguments2))
				{
					ToolResult = await ScriptNode.WaitPossibleTask(
						Tool.Method.Invoke(this, Arguments2));
				}
				else
				{
					ToolResult = Reason;
					Result["isError"] = true;
				}
			}
			catch (Exception ex)
			{
				ToolResult = ex.Message;
				Result["isError"] = true;
			}

			if (ToolResult is null)
				Result["content"] = Array.Empty<object>();
			else if (ToolResult is Dictionary<string, object?> StructuredContent)
			{
				Result["content"] = new object[]
				{
					new Dictionary<string, object?>()
					{
						{ "type", "text" },
						{ "text", JSON.Encode(StructuredContent, false) }
					}
				};
				Result["structuredContent"] = new Dictionary<string, object?>()
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
							new Dictionary<string, object?>()
							{
								{ "type", "text" },
								{ "text", JSON.Encode(StructuredContent, false) }
							}
						};
						Result["structuredContent"] = new Dictionary<string, object?>()
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
						new Dictionary<string, object?>()
						{
							{ "type", "text" },
							{ "text", JSON.Encode(StructuredContent, false) }
						}
					};
					Result["structuredContent"] = new Dictionary<string, object?>()
					{
						{ "result", StructuredContent }
					};
				}
			}

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
			IUser? User = await this.GetAuthenticatedUser(Request, Response);
			if (Response.ResponseSent)
				return null;

			int Offset = 0;
			int MaxCount = PageSize;

			if (!string.IsNullOrEmpty(Cursor))
			{
				if (!int.TryParse(Cursor, out Offset) || Offset < 0)
					throw new Exception("Invalid cursor.");
			}

			ChunkedList<Prompt> Prompts = new ChunkedList<Prompt>();
			Dictionary<string, object>[] PromptsJson;
			int Next = Offset + MaxCount;

			Dictionary<string, object> Result = new Dictionary<string, object>();

			lock (this.prompts)
			{
				foreach (Prompt Prompt in this.prompts.Values)
				{
					if (!Prompt.IsAuthorized(User))
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

			PromptsJson = new Dictionary<string, object>[c];

			foreach (Prompt Prompt in Prompts)
				PromptsJson[i++] = await Prompt.ToJson(this);

			Result["prompts"] = PromptsJson;

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
			IUser? User = await this.GetAuthenticatedUser(Request, Response);
			if (Response.ResponseSent)
				return null;

			Dictionary<string, object?> Result = new Dictionary<string, object?>();
			object? PromptResult;

			try
			{
				if (!this.prompts.TryGetValue(Name, out Prompt? Prompt))
					throw new NotFoundException("Prompt not found: " + Name);

				Prompt.AssertAuthorized(this.ResourceName, User);

				Dictionary<string, object?>? MetaData = _Meta as Dictionary<string, object?>;

				if (Prompt.TryBuildRequest(Arguments, Request, Response, MetaData,
					out string? Reason, out object?[]? Arguments2))
				{
					PromptResult = await ScriptNode.WaitPossibleTask(
						Prompt.Method.Invoke(this, Arguments2));
				}
				else
				{
					PromptResult = Reason;
					Result["isError"] = true;
				}

				Prompt.ReturnAttributes?.Annotate(Result);
			}
			catch (Exception ex)
			{
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
			Dictionary<string, object?>[] EncodedMessages = new Dictionary<string, object?>[c];

			foreach (PromptMessage Message in Messages)
			{
				Dictionary<string, object?> Content;

				if (Message.IsEncoded)
					Content = Message.Encoded!;
				else
				{
					Type T = Message.Content.GetType();
					if (!contentBlocks.TryGetValue(T, out IContentBlock Encoder))
						Encoder = defaultObjectEncoder;

					Content = await Encoder.Encode(Message.Content);
				}

				EncodedMessages[i++] = new Dictionary<string, object?>()
				{
					{ "role", Message.Role.ToString().ToLower() },
					{ "content", Content }
				};
			}

			Result["messages"] = EncodedMessages;

			return Result;
		}

		[JsonRpcMethod]
		protected async Task<Dictionary<string, object>?> Resources_List(HttpRequest Request,
			HttpResponse Response, string? Cursor = null)
		{
			IUser? User = await this.GetAuthenticatedUser(Request, Response);
			if (Response.ResponseSent)
				return null;

			// TODO: resources/list

			return new Dictionary<string, object>()
			{
				{ "resources", Array.Empty<Dictionary<string, object>>() }
			};
		}
	}
}
