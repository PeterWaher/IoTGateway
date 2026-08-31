using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Content.Html;
using Waher.Content.Html.Elements;
using Waher.Content.Html.JavaScript;
using Waher.Content.Images;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.JsonRpc.MetaData;
using Waher.Networking.HTTP.JsonRpc.Transports;
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
using Waher.Script;
using Waher.Script.Model;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Abstract base class for HTTP-based Model Context Protocol (MCP) server resource,
	/// as defined in:
	/// 
	/// https://modelcontextprotocol.io/
	/// https://github.com/modelcontextprotocol/modelcontextprotocol/blob/main/schema/2025-11-25/schema.ts
	/// </summary>
	[OAuthScopesSupported(true, "McpScopesSupported")]
	public abstract class HttpMcpServerResource : JsonRpcWebService, IHttpDeleteMethod
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
		private readonly SortedDictionary<string, Tool> tools = new SortedDictionary<string, Tool>();
		private readonly SortedDictionary<string, Prompt> prompts = new SortedDictionary<string, Prompt>();
		private readonly ISnifferSet? snifferSet;
		private readonly string[] rootScopes;
		private readonly string[] toolScopes;
		private readonly string[] promptScopes;
		private readonly string[] resourceScopes;
		private readonly string[] scopesSupported;
		private readonly string title;
		private readonly string description;
		private readonly string markdownDescription;
		private readonly bool hasScopes;
		private readonly bool hasSnifferSet;
		private bool hasPrompts;
		private bool hasTools;
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
			string Version, string Description, Icon[] Icons, Uri? WebSiteUri,
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
			string Version, string Description, Icon[] Icons, Uri? WebSiteUri,
			string Instructions, ISnifferSet? SnifferSet)
			: this(ResourceName, Name, Title, Version, Description,
				  MarkdownDocument.Encode(Description), Icons,
				  WebSiteUri, Instructions, SnifferSet)
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
		/// <param name="MarkdownDescription">Markdown version of description.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="Instructions">Instructions for server.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public HttpMcpServerResource(string ResourceName, string Name, string Title,
			string Version, string Description, string MarkdownDescription, Icon[] Icons,
			Uri? WebSiteUri, string Instructions, ISnifferSet? SnifferSet)
			: base(ResourceName, false, false)
		{
			this.Name = Name;
			this.title = Title;
			this.description = Description;
			this.markdownDescription = MarkdownDescription;
			this.Version = Version;
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
		public override bool SendSseWelcomeMessage => false;

		/// <summary>
		/// Name of server.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Title of MCP server.
		/// </summary>
		public override string Title => this.title;

		/// <summary>
		/// Version of server.
		/// </summary>
		public string Version { get; }

		/// <summary>
		/// Icons of server.
		/// </summary>
		public Icons Icons { get; }

		/// <summary>
		/// Website URI of server.
		/// </summary>
		public Uri? WebSiteUri { get; }

		/// <summary>
		/// Instructions for server.
		/// </summary>
		public string Instructions { get; }

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// OAUTH scopes supported by resource.
		/// </summary>
		/// <returns>Array of scopes supported.</returns>
		public string[] McpScopesSupported()
		{
			return this.scopesSupported;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public override async Task GET(HttpRequest Request, HttpResponse Response)
		{
			if (string.IsNullOrEmpty(Request.SubPath))
			{
				await base.GET(Request, Response);
				return;
			}

			string FormId = Request.SubPath[1..];

			if (FormId == "UserInput.js")
			{
				StringBuilder Javascript = new StringBuilder();

				Javascript.AppendLine("function Ok() { PostForm('true'); }");
				Javascript.AppendLine("function Cancel() { PostForm('false'); }");
				Javascript.AppendLine("function PostForm(Response)");
				Javascript.AppendLine("{");
				Javascript.AppendLine("\tdocument.getElementById('_r_').value=Response;");
				Javascript.AppendLine("\tdocument.getElementById('InputForm').submit();");
				Javascript.AppendLine("}");
				Javascript.AppendLine("function Loaded()");
				Javascript.AppendLine("{");
				Javascript.AppendLine("\tdocument.getElementById('OkButton').addEventListener('click', Ok);");
				Javascript.AppendLine("\tdocument.getElementById('CancelButton').addEventListener('click', Cancel);");
				Javascript.AppendLine("}");
				Javascript.AppendLine("window.addEventListener('load', Loaded);");

				Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
				Response.SetHeader("Pragma", "no-cache");

				await Response.Return(new JavaScriptDocument(Javascript.ToString()));
			}
			else if (FormId == "CloseInput.js")
			{
				StringBuilder Javascript = new StringBuilder();

				Javascript.AppendLine("window.close();");

				Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
				Response.SetHeader("Pragma", "no-cache");

				await Response.Return(new JavaScriptDocument(Javascript.ToString()));
			}
			else
			{
				if (!this.TryGetRequest(FormId, out IJsonRpcClientRequest? ClientRequest) ||
					ClientRequest.Tag is null)
				{
					await Response.Return(await CloseForm(Response));
				}
				else
				{
					await Response.Return(await this.GenerateInputForm(Request, Response,
						ClientRequest));
				}
			}
		}

		private async Task<HtmlDocument> GenerateInputForm(HttpRequest Request,
			HttpResponse Response, IJsonRpcClientRequest ClientRequest)
		{
			StringBuilder Markdown = new StringBuilder();

			Markdown.AppendLine("Title: User Input");
			Markdown.AppendLine("Description: Form allowing a user to input elicited information.");
			Markdown.AppendLine("Javascript: UserInput.js");

			if (Types.TryGetModuleParameter<OAuth2Environment>("OAUTH2", out OAuth2Environment? Environment) &&
				Environment.HasLoginMasterFileName)
			{
				Markdown.Append("Master: ");
				Markdown.AppendLine(Environment.LoginMasterFileName);
			}

			Markdown.Append("Date: ");
			Markdown.AppendLine(CommonTypes.EncodeRfc822(DateTime.UtcNow));
			Markdown.AppendLine();
			Markdown.AppendLine(new string('=', 40));
			Markdown.AppendLine();

			Markdown.AppendLine("Requested Information");
			Markdown.AppendLine("========================");
			Markdown.AppendLine();

			Markdown.AppendLine(MarkdownDocument.Encode(ClientRequest.Message));
			Markdown.AppendLine();

			string ParametersToken = this.JwtFactory?.Create(
				new KeyValuePair<string, object>(JwtClaims.JwtId, ClientRequest.Id?.ToString() ?? string.Empty),
				new KeyValuePair<string, object>(JwtClaims.Subject, Request.RemoteEndPoint.RemovePortNumber()))
				?? string.Empty;

			Markdown.Append("<form id='InputForm' action='");
			Markdown.Append(Request.Header.GetURL(false, false));
			Markdown.AppendLine("' method='post' enctype='multipart/form-data'>");
			Markdown.Append("<input type='hidden' name='_p_' value='");
			Markdown.Append(XML.HtmlAttributeEncode(ParametersToken));
			Markdown.AppendLine("'/>");
			Markdown.AppendLine("<input type='hidden' id='_r_' name='_r_' value=''/>");
			Markdown.AppendLine();

			Type T = ClientRequest.Tag!.GetType();
			McpParameterAttribute? ParameterInfo;
			Dictionary<string, string> InputAttributes = new Dictionary<string, string>();
			Type ValueType;
			string s;
			object Value;
			bool Password;
			bool Required;
			StringBuilder Label = new StringBuilder();
			StringBuilder Input = new StringBuilder();
			bool LabelFirst;

			foreach (MemberInfo MI in T.GetMembers(BindingFlags.Instance | BindingFlags.Public))
			{
				if (MI is FieldInfo FI)
				{
					Value = FI.GetValue(ClientRequest.Tag);
					ValueType = FI.FieldType;
				}
				else if (MI is PropertyInfo PI)
				{
					Value = PI.GetValue(ClientRequest.Tag);
					ValueType = PI.PropertyType;
				}
				else
					continue;

				ParameterInfo = MI.GetCustomAttribute<McpParameterAttribute>(true);
				Password = ParameterInfo is McpPasswordParameterAttribute;
				InputAttributes.Clear();
				LabelFirst = true;
				Required = ParameterInfo?.IsRequired(ValueType) ?? !Expression.CanBeSetToNull(ValueType);

				if (MI.Name == "_p_" ||
					MI.Name == "_r_" ||
					MI.Name.EndsWith("_Binary") ||
					MI.Name.EndsWith("_ContentType"))
				{
					throw new Exception("Reserved name: " + MI.Name);
				}

				InputAttributes["id"] = MI.Name;
				InputAttributes["name"] = MI.Name;
				InputAttributes["value"] = ParameterInfo?.GetHtmlAttributeValue(Value)
					?? Value?.ToString() ?? string.Empty;

				if (Value is double || Value is float || Value is decimal ||
					Value is int || Value is long || Value is short || Value is sbyte ||
					Value is uint || Value is ulong || Value is ushort || Value is byte)
				{
					InputAttributes["type"] = "number";
					InputAttributes["step"] = "any";
				}
				else if (Value is bool)
				{
					InputAttributes.Remove("value");
					InputAttributes["type"] = "checkbox";

					if (Value is bool b && b)
						InputAttributes["checked"] = "checked";

					LabelFirst = false;
				}
				else if (Value is string)
					InputAttributes["type"] = "text";
				else if (Value is TimeSpan)
					InputAttributes["type"] = "time";
				else if (Value is DateTime)
					InputAttributes["type"] = "datetime-local";
				else if (Value is Uri)
					InputAttributes["type"] = "url";
				else if (Value is SKColor)
					InputAttributes["type"] = "color";
				else
					InputAttributes["type"] = "text";

				ParameterInfo?.GetHtmlInputAttributes(InputAttributes);

				if (Required)
					InputAttributes["required"] = "required";

				Label.Clear();
				Input.Clear();

				Label.Append("<label for=\"");
				Label.Append(MI.Name);
				Label.Append("\">");
				Label.Append(s = ParameterInfo?.Title ?? MI.Name);
				if (!s.EndsWith(':'))
					Label.Append(':');

				if (Required)
					Label.Append(" (Required)");

				Label.Append("</label>");

				if (Value is Enum EnumValue)
				{
					Type EnumType = EnumValue.GetType();
					IEnumerable<McpEnumValueAttribute> Options = MI.GetCustomAttributes<McpEnumValueAttribute>(true);

					if (EnumType.IsDefined(typeof(FlagsAttribute)))
						throw new NotImplementedException("Flagged enumerations are not yet supported in input forms."); // TODO:
					else
					{
						InputAttributes.Remove("value");
						Input.Append("<select");

						foreach (KeyValuePair<string, string> P in InputAttributes)
						{
							Input.Append(' ');
							Input.Append(P.Key);
							Input.Append("=\"");
							Input.Append(XML.HtmlAttributeEncode(P.Value));
							Input.Append('"');
						}

						Input.AppendLine(">");

						foreach (McpEnumValueAttribute Option in Options)
						{
							Input.Append("<option value=\"");
							Input.Append(XML.HtmlAttributeEncode(Option.Value.ToString()));

							if (Option.Value.Equals(EnumValue))
								Input.Append("\" selected=\"selected");

							Input.Append("\">");
							Input.Append(XML.HtmlValueEncode(Option.Title));
							Input.AppendLine("</option>");
						}

						Input.Append("</select>");
					}
				}
				else if (Value is string[] ||
					(Value is string s2 && s2.IndexOfAny(CommonTypes.CRLF) >= 0))
				{
					InputAttributes.Remove("value");
					Input.Append("<textarea");

					foreach (KeyValuePair<string, string> P in InputAttributes)
					{
						Input.Append(' ');
						Input.Append(P.Key);
						Input.Append("=\"");
						Input.Append(XML.HtmlAttributeEncode(P.Value));
						Input.Append('"');
					}

					Input.Append(">");

					if (Value is string[] Rows)
					{
						bool First = true;

						foreach (string Row in Rows)
						{
							if (First)
								First = false;
							else
								Input.AppendLine();

							Input.Append(Row);
						}
					}
					else
						Input.Append(Value.ToString());

					Input.AppendLine("</textarea>");
				}
				else
				{
					Input.Append("<input");

					foreach (KeyValuePair<string, string> P in InputAttributes)
					{
						Input.Append(' ');
						Input.Append(P.Key);
						Input.Append("=\"");
						Input.Append(XML.HtmlAttributeEncode(P.Value));
						Input.Append('"');
					}

					Input.Append("/>");
				}

				Markdown.Append("<p>");

				if (LabelFirst)
				{
					Markdown.Append(Label.ToString());
					Markdown.AppendLine("  ");
					Markdown.AppendLine(Input.ToString());
				}
				else
				{
					Markdown.AppendLine(Input.ToString());
					Markdown.AppendLine(Label.ToString());
				}

				Markdown.AppendLine("</p>");
				Markdown.AppendLine();
			}

			Markdown.AppendLine("<button id='OkButton' type='button'>OK</button>");
			Markdown.AppendLine("<button id='CancelButton' type='button'>Cancel</button>");
			Markdown.AppendLine("</form>");
			Markdown.AppendLine();

			return await ReturnHtml(Response, Markdown.ToString());
		}

		private static async Task<HtmlDocument> CloseForm(HttpResponse Response)
		{
			StringBuilder Markdown = new StringBuilder();

			Markdown.AppendLine("Title: User Input");
			Markdown.AppendLine("Description: Form allowing a user to input elicited information.");
			Markdown.AppendLine("Javascript: CloseInput.js");

			if (Types.TryGetModuleParameter<OAuth2Environment>("OAUTH2", out OAuth2Environment? Environment) &&
				Environment.HasLoginMasterFileName)
			{
				Markdown.Append("Master: ");
				Markdown.AppendLine(Environment.LoginMasterFileName);
			}

			Markdown.Append("Date: ");
			Markdown.AppendLine(CommonTypes.EncodeRfc822(DateTime.UtcNow));
			Markdown.AppendLine();
			Markdown.AppendLine(new string('=', 40));
			Markdown.AppendLine();

			Markdown.AppendLine("Close Form");
			Markdown.AppendLine("=============");
			Markdown.AppendLine();

			Markdown.Append("You can now safely close the form, if it does not close ");
			Markdown.AppendLine("automatically by itself.");

			return await ReturnHtml(Response, Markdown.ToString());
		}

		private static async Task<HtmlDocument> ReturnHtml(HttpResponse Response, string Markdown)
		{
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown,
				new MarkdownSettings()
				{
					Variables = new Variables()
				});

			string Html = await Doc.GenerateHTML();

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");
			Response.SetHeader("X-Frame-Options", "DENY");
			Response.SetHeader("Content-Security-Policy", "frame-ancestors 'none'; " +
				"default-src 'self'; script-src 'self'; object-src 'none'; " +
				"base-uri 'none'; form-action 'self'");

			return new HtmlDocument(Html);
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public override async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (string.IsNullOrEmpty(Request.SubPath))
			{
				await base.POST(Request, Response);
				return;
			}

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("Missing data."));
				return;
			}

			string FormId = Request.SubPath[1..];
			if (!this.TryGetRequest(FormId, out IJsonRpcClientRequest? ClientRequest))
			{
				await Response.SendResponse(new NotFoundException("Request not found: " + FormId));
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError)
			{
				await Response.SendResponse(Content.Error);
				return;
			}

			if (!(Content.Decoded is Dictionary<string, object> Form))
			{
				await Response.SendResponse(new BadRequestException("Expected form data."));
				return;
			}

			if (!Form.TryGetValue("_p_", out object Obj) ||
				!(Obj is string ParametersToken) ||
				!JwtToken.TryParse(ParametersToken, out JwtToken ParsedToken) ||
				!this.JwtFactory!.IsValid(ParsedToken) ||
				ParsedToken.Id != ClientRequest.Id?.ToString() ||
				ParsedToken.Subject != Request.RemoteEndPoint.RemovePortNumber())
			{
				await Response.SendResponse(new BadRequestException("Invalid parameters token."));
				return;
			}

			if (!Form.TryGetValue("_r_", out Obj) ||
				!(Obj is string ResponseString) ||
				!CommonTypes.TryParse(ResponseString, out bool ResponseValue))
			{
				await Response.SendResponse(new BadRequestException("Invalid response."));
				return;
			}

			Type T = ClientRequest.Tag!.GetType();
			FieldInfo FI;
			PropertyInfo PI;

			string[] Keys = new string[Form.Count];
			Form.Keys.CopyTo(Keys, 0);

			foreach (string Key in Keys)
			{
				FI = T.GetField(Key);
				if (!(FI is null) &&
					FI.FieldType == typeof(CustomEncoding) &&
					Form.TryGetValue(Key + "_Binary", out Obj) &&
					Obj is byte[] Bin &&
					Form.TryGetValue(Key + "_ContentType", out Obj) &&
					Obj is string ContentType)
				{
					Form[Key] = new CustomEncoding(ContentType, Bin);
				}
				else
				{
					PI = T.GetProperty(Key);
					if (!(PI is null) &&
						PI.PropertyType == typeof(CustomEncoding) &&
						Form.TryGetValue(Key + "_Binary", out Obj) &&
						Obj is byte[] Bin2 &&
						Form.TryGetValue(Key + "_ContentType", out Obj) &&
						Obj is string ContentType2)
					{
						Form[Key] = new CustomEncoding(ContentType2, Bin2);
					}
				}

				Form.Remove(Key + "_Binary");
				Form.Remove(Key + "_ContentType");
				Form.Remove(Key + "_FileName");
			}

			Form.Remove("_p_");
			Form.Remove("_r_");

			if (ResponseValue)
				await SetProperties(ClientRequest.Tag, Form);

			try
			{
				await Response.Return(await CloseForm(Response));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			finally
			{
				await ClientRequest.ReportResult(ResponseValue);
			}
		}

		/// <summary>
		/// Gets an array of available tools.
		/// </summary>
		/// <returns>Array of tools.</returns>
		public Tool[] GetTools()
		{
			lock (this.tools)
			{
				Tool[] Result = new Tool[this.tools.Count];
				this.tools.Values.CopyTo(Result, 0);
				return Result;
			}
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
				Tool? First = null;
				string Name = Method.Name;
				string Suffix = string.Empty;
				int i = 1;

				while (this.tools.TryGetValue(Name + Suffix, out Tool Prev))
				{
					First ??= Prev;
					i++;
					Suffix = "_" + i.ToString();
				}

				Tool Tool = new Tool(Method, Attributes);
				this.tools[Name + Suffix] = Tool;

				if (!string.IsNullOrEmpty(Suffix))
				{
					Log.Warning("Duplicate tool name: " + Name + ". Renamed to: " + Name + Suffix,
						new KeyValuePair<string, object>("First", First?.FullName ?? string.Empty),
						new KeyValuePair<string, object>("Duplicate", Tool.FullName));
				}

				this.requiresAuthentication |= Tool.RequiresAuthentication;
				this.hasTools = true;
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

			Dictionary<string, object> Notification = new Dictionary<string, object>()
			{
				{ "jsonrpc", "2.0" },
				{ "method", "notifications/tools/list_changed" }
			};

			return this.SendNotification(
				Session =>
				{
					if (!(Session is Session McpSession))
						return false;

					if (this.hasSnifferSet)
						McpSession.TransmitText(JSON.Encode(Notification, false));

					return true;
				},
				Notification);
		}

		/// <summary>
		/// Gets an array of available prompts.
		/// </summary>
		/// <returns>Array of prompts.</returns>
		public Prompt[] GetPrompts()
		{
			lock (this.prompts)
			{
				Prompt[] Result = new Prompt[this.prompts.Count];
				this.prompts.Values.CopyTo(Result, 0);
				return Result;
			}
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
				Prompt? First = null;
				string Name = Method.Name;
				string Suffix = string.Empty;
				int i = 1;

				while (this.prompts.TryGetValue(Name + Suffix, out Prompt Prev))
				{
					First ??= Prev;
					i++;
					Suffix = "_" + i.ToString();
				}

				Prompt Prompt = new Prompt(Method, Attributes);
				this.prompts[Name + Suffix] = Prompt;

				if (!string.IsNullOrEmpty(Suffix))
				{
					Log.Warning("Duplicate prompt name: " + Name + ". Renamed to: " + Name + Suffix,
						new KeyValuePair<string, object>("First", First?.FullName ?? string.Empty),
						new KeyValuePair<string, object>("Duplicate", Prompt.FullName));
				}

				this.requiresAuthentication |= Prompt.RequiresAuthentication;
				this.hasPrompts = true;
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

			Dictionary<string, object> Notification = new Dictionary<string, object>()
			{
				{ "jsonrpc", "2.0" },
				{ "method", "notifications/prompts/list_changed" }
			};

			return this.SendNotification(
				Session =>
				{
					if (!(Session is Session McpSession))
						return false;

					if (this.hasSnifferSet)
						McpSession.TransmitText(JSON.Encode(Notification, false));

					return true;
				},
				Notification);
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
		/// Description of MCP server.
		/// </summary>
		public override string ShortDescription
		{
			get
			{
				OAuthResourceNameAttribute? Attribute = this.GetType().GetCustomAttribute<OAuthResourceNameAttribute>();
				return Attribute?.ResourceName ?? "MCP Server: " + this.Name;
			}
		}

		/// <summary>
		/// Description
		/// </summary>
		public string Description => this.description;

		/// <summary>
		/// Markdown description of web service.
		/// </summary>
		public override string MarkdownDescription => this.markdownDescription;

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
		/// MCP initialize method. Called by client to initialize connection and exchange 
		/// information about capabilities.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="ProtocolVersion">Protocol Version</param>
		/// <param name="Capabilities">Client capabilities</param>
		/// <param name="ClientInfo">Client information</param>
		/// <returns>Server capabilities and information.</returns>
		[JsonRpcMethod]
		[JsonRpcDocumentation("MCP `initialize` method. Called by client to initialize " +
			"connection and exchange information about capabilities.", true)]
		[JsonRpcDocName("initialize")]
		[return: JsonRpcDocumentation("Server capabilities and information.")]
		protected Dictionary<string, object> Initialize(IJsonRpcCall Call,

			[JsonRpcDocumentation("Protocol Version")]
			string ProtocolVersion,

			[JsonRpcDocumentation("Client capabilities")]
			Dictionary<string, object> Capabilities,

			[JsonRpcDocumentation("Client information")]
			Dictionary<string, object> ClientInfo)
		{
			if (!ClientCapabilities.TryParse(Capabilities, out ClientCapabilities? CapabilitiesParsed))
				CapabilitiesParsed = null;

			if (!Implementation.TryParse(ClientInfo, out Implementation? ClientInfoParsed))
				ClientInfoParsed = null;

			string RemoteEndpoint = Call.RemoteEndPoint.RemovePortNumber();
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
			Call.SetSessionId(SessionId);

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".Initialize(");
				sb.Append(ProtocolVersion);
				sb.Append(',');
				sb.Append(JSON.Encode(Capabilities, false));
				sb.Append(',');
				sb.Append(JSON.Encode(ClientInfo, false));
				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			Dictionary<string, object> ServerCapabilities = new Dictionary<string, object>();

			// TODO:
			//{
			//	{ "logging", new Dictionary<string, object>() },
			//	{ "completions", new Dictionary<string, object>() },
			//	{ "tasks", new Dictionary<string, object>()
			//		{
			//			{ "list", new Dictionary<string, object>() },
			//			{ "cancel", new Dictionary<string, object>() },
			//			{ "requests", new Dictionary<string, object>()
			//				{
			//					{ "tools", new Dictionary<string, object>()
			//						{
			//							{ "call", new Dictionary<string, object>() }
			//						}
			//					}
			//				}
			//			}
			//		}
			//	},
			//	{ "experimental", new Dictionary<string, object>() }
			//};

			if (this.hasPrompts)
			{
				ServerCapabilities["prompts"] = new Dictionary<string, object>()
				{
					{ "listChanged", true }
				};
			}

			if (this.hasTools)
			{
				ServerCapabilities["tools"] = new Dictionary<string, object>()
				{
					{ "listChanged", true }
				};
			}

			if (this.HasResources)
			{
				ServerCapabilities["resources"] = new Dictionary<string, object>()
				{
					{ "subscribe", true },
					{ "listChanged", true }
				};
			}

			string? WebSite = this.WebSiteUri?.ToString();
			if (string.IsNullOrEmpty(WebSite))
				WebSite = Call.GetBaseUrl();

			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "protocolVersion", "2025-11-25" },
				{ "capabilities", ServerCapabilities },
				{ "serverInfo", new Dictionary<string,object>()
					{
						{ "name", this.Name },
						{ "title", this.title },
						{ "version", this.Version },
						{ "description", this.description },
						{ "icons", this.Icons.ToJson() },
						{ "websiteUrl", WebSite }
					}
				},
				{ "instructions", this.Instructions }
			};

			if (this.hasSnifferSet)
				Session.TransmitText(JSON.Encode(Result, false));

			return Result;
		}

		/// <summary>
		/// Notification that the client has completed its initialization.
		/// </summary>
		/// <param name="Call">JSON-RPC request object.</param>
		[JsonRpcMethod]
		[JsonRpcDocumentation("Notification that the client has completed its " +
			"initialization.")]
		[JsonRpcDocName("notifications/initialized")]
		protected async Task Notifications_Initialized(IJsonRpcCall Call)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return;

			if (this.hasSnifferSet)
				Session.ReceiveText(this.Name + ".Initialized()");

			Log.Informational("MCP client initialized: " + Call.RemoteEndPoint,
				this.ResourceName, Call.RemoteEndPoint, "McpInitialized");

			await Call.SendResponse(202, "Accepted");
		}

		/// <summary>
		/// Tries to get an MCP session object for the resource, if any.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object</param>
		/// <returns>Session object, if any.</returns>
		public async Task<Session?> TryGetMcpSession(IJsonRpcCall Call)
		{
			return await this.TryGetSession(Call, true) as Session;
		}

		/// <summary>
		/// Tries to get a session object for the resource, if any.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object</param>
		/// <param name="ErrorIfNone">If an error should be returned if no session is found.</param>
		/// <returns>Session object, if any.</returns>
		public override async Task<IJsonRpcSession?> TryGetSession(IJsonRpcCall Call,
			bool ErrorIfNone)
		{
			if (!Call.TryGetSessionId(out string? SessionId))
			{
				if (ErrorIfNone)
					await Call.SendResponse(new BadRequestException("Missing MCP-Session-Id header."));

				return null;
			}

			if (this.HasJwtFactory)
			{
				if (!JwtToken.TryParse(SessionId, out JwtToken Token))
				{
					if (ErrorIfNone)
						await Call.SendResponse(new NotFoundException("Invalid MCP-Session-Id."));

					return null;
				}

				if (!this.JwtFactory!.IsValid(Token))
				{
					if (ErrorIfNone)
						await Call.SendResponse(new NotFoundException("MCP-Session-Id invalid or expired."));

					return null;
				}
			}

			if (!sessions.TryGetValue(SessionId, out Session? Session))
			{
				if (ErrorIfNone)
					await Call.SendResponse(new NotFoundException("MCP-Session-Id expired or not found."));

				return null;
			}

			if (Session.RemoteEndpoint != Call.RemoteEndPoint.RemovePortNumber())
			{
				if (ErrorIfNone)
					await Call.SendResponse(new NotFoundException("MCP-Session-Id not found for this endpoint."));

				return null;
			}

			return Session;
		}

		/// <summary>
		/// Tries to get an MCP session object for the resource, if any.
		/// </summary>
		/// <param name="SessionId">Session ID of the MCP session.</param>
		/// <param name="Session">Session object, if any.</param>
		/// <returns>If a session with the given session identity was found.</returns>
		protected bool TryGetMcpSession(string SessionId,
			[NotNullWhen(true)] out Session? Session)
		{
			return sessions.TryGetValue(SessionId, out Session);
		}

		/// <summary>
		/// Lists available MCP server tools.
		/// </summary>
		/// <param name="Call">JSON-RPC request object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of tools.</returns>
		[JsonRpcMethod]
		[JsonRpcDocumentation("Lists available MCP server tools.")]
		[JsonRpcDocName("tools/list")]
		[return: JsonRpcDocumentation("Dictionary containing the list of tools.")]
		protected async Task<Dictionary<string, object>?> Tools_List(IJsonRpcCall Call,

			[JsonRpcDocumentation("Cursor for pagination.")]
			string? Cursor = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent)
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

					await Call.SendResponse(new BadRequestException("Invalid cursor."));
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
				Session.TransmitText(JSON.Encode(Result, false));

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

		/// <summary>
		/// Gets the authenticated user in the MCP session.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Session">MCP session object.</param>
		/// <returns>Authenticated user, if any.</returns>
		protected async Task<IUser?> GetAuthenticatedUser(IJsonRpcCall Call,
			Session Session)
		{
			if (!await Call.CheckAuthentication(Session, this.requiresAuthentication,
				this.AuthenticationSchemes, null))
			{
				return null;
			}

			IUser User = Call.User;
			if (!Session.IsAuthenticated && !(User is null))
				await Session.SetUser(User);

			return User;
		}

		/// <summary>
		/// Calls an MCP server tool.
		/// </summary>
		/// <param name="Id">ID of request.</param>
		/// <param name="Call">JSON-RPC call object.</param>
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
		[JsonRpcDocumentation("Calls an MCP server tool.")]
		[JsonRpcDocName("tools/call")]
		[return: JsonRpcDocumentation("Dictionary containing the result of the tool call.")]
		protected async Task<Dictionary<string, object?>?> Tools_Call(
			[JsonRpcId] object? Id, IJsonRpcCall Call,

			[JsonRpcDocumentation("Name of the tool to call.")]
			string Name,

			[JsonRpcDocumentation("Arguments for the tool.")]
			Dictionary<string, object?> Arguments,

			[JsonRpcDocumentation("If specified, the caller is requesting task-augmented " +
				"execution for this request. The request will return a `CreateTaskResult` " +
				"immediately, and the actual result can be retrieved later via tasks/result.\r\n\r\n" +
				"Task augmentation is subject to capability negotiation - receivers MUST declare " +
				"support for task augmentation of specific request types in their capabilities.", true)]
			object? Task = null,

			[JsonRpcMetaDataArgument]
			[JsonRpcDocumentation("Associated meta-data, if available.")]
			object? _Meta = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".tools/call(");
				sb.Append(Name);
				sb.Append(',');
				sb.Append(JSON.Encode(Arguments, false));

				if (!(Task is null))
				{
					sb.Append(',');
					JSON.Encode(Task, false, sb);
				}

				if (!(_Meta is null))
				{
					sb.Append(',');
					JSON.Encode(_Meta, false, sb);
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

					await Call.SendResponse(new NotFoundException("Tool not found."));
					return null;
				}

				if (!Tool.IsAuthorized(User, out string? MissingPrivilege) ||
					!this.CheckScopes(User, this.toolScopes, out MissingPrivilege))
				{
					if (this.hasSnifferSet)
						Session.Error("Access denied. Missing privilege: " + MissingPrivilege);

					await Call.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
						User?.UserName ?? string.Empty, MissingPrivilege ?? string.Empty));
					return null;
				}

				await RuntimeCounters.IncrementCounter("MCP.Tool." + Name);
				await RuntimeCounters.IncrementCounter("MCP.User.Tool." + Session.UserName);

				Dictionary<string, object?>? MetaData = _Meta as Dictionary<string, object?>;

				if (Tool.TryBuildRequest(Id, Arguments, Call, MetaData,
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

				ToolResult = Log.UnnestException(ex).Message;
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
				Session.TransmitText(JSON.Encode(Result, false));

			return Result;
		}

		/// <summary>
		/// Lists available MCP server prompts.
		/// </summary>
		/// <param name="Call">JSON-RPC request object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of prompts.</returns>
		[JsonRpcMethod]
		[JsonRpcDocumentation("Lists available MCP server prompts.")]
		[JsonRpcDocName("prompts/list")]
		[return: JsonRpcDocumentation("Dictionary containing the list of prompts.")]
		protected async Task<Dictionary<string, object>?> Prompts_List(IJsonRpcCall Call,

			[JsonRpcDocumentation("Cursor for pagination.")]
			string? Cursor = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent)
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

					await Call.SendResponse(new BadRequestException("Invalid cursor."));
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
				Session.TransmitText(JSON.Encode(Result, false));

			return Result;
		}

		/// <summary>
		/// Gets an MCP server prompt.
		/// </summary>
		/// <param name="Id">ID of request.</param>
		/// <param name="Call">JSON-RPC request object.</param>
		/// <param name="Name">Name of the prompt to call.</param>
		/// <param name="Arguments">Arguments for the prompt.</param>
		/// <param name="_Meta">Associated meta-data, if available.</param>
		/// <returns>Dictionary containing the prompt.</returns>
		[JsonRpcMethod]
		[JsonRpcDocumentation("Gets an MCP server prompt.")]
		[JsonRpcDocName("prompts/get")]
		[return: JsonRpcDocumentation("Dictionary containing the prompt.")]
		protected async Task<Dictionary<string, object?>?> Prompts_Get(
			[JsonRpcId] object? Id, IJsonRpcCall Call,

			[JsonRpcDocumentation("Name of the prompt to call.")]
			string Name,

			[JsonRpcDocumentation("Arguments for the prompt.")]
			Dictionary<string, object?> Arguments,

			[JsonRpcMetaDataArgument]
			[JsonRpcDocumentation("Associated meta-data, if available.")]
			object? _Meta = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".prompts/get(");
				sb.Append(Name);
				sb.Append(',');
				sb.Append(JSON.Encode(Arguments, false));

				if (!(_Meta is null))
				{
					sb.Append(',');
					JSON.Encode(_Meta, false, sb);
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

					await Call.SendResponse(new NotFoundException("Prompt not found."));
					return null;
				}

				if (!Prompt.IsAuthorized(User, out string? MissingPrivilege) ||
					!this.CheckScopes(User, this.promptScopes, out MissingPrivilege))
				{
					if (this.hasSnifferSet)
						Session.Error("Access denied. Missing privilege: " + MissingPrivilege);

					await Call.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
						User?.UserName ?? string.Empty, MissingPrivilege ?? string.Empty));
					return null;
				}

				await RuntimeCounters.IncrementCounter("MCP.Prompt." + Name);
				await RuntimeCounters.IncrementCounter("MCP.User.Prompt." + Session.UserName);

				Dictionary<string, object?>? MetaData = _Meta as Dictionary<string, object?>;

				if (Prompt.TryBuildRequest(Id, Arguments, Call, MetaData,
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

				PromptResult = Log.UnnestException(ex).Message;
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
				Session.TransmitText(JSON.Encode(Result, false));

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
		/// <param name="Call">JSON-RPC request object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of resources.</returns>
		[JsonRpcMethod]
		[JsonRpcDocumentation("Lists available MCP server resources.")]
		[JsonRpcDocName("resources/list")]
		[return: JsonRpcDocumentation("Dictionary containing the list of resources.")]
		protected virtual async Task<Dictionary<string, object>?> Resources_List(
			IJsonRpcCall Call,

			[JsonRpcDocumentation("Cursor for pagination.")]
			string? Cursor = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent)
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

					await Call.SendResponse(new BadRequestException("Invalid cursor."));
					return null;
				}
			}

			Resource[] AllResources = await this.GetResources(Call, User, Session);
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
				Session.TransmitText(JSON.Encode(Result, false));

			return Result;
		}

		/// <summary>
		/// Reads an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC request object.</param>
		/// <param name="Uri">URI of the resource to read.</param>
		/// <param name="_Meta">Associated meta-data, if available.</param>
		/// <returns>Dictionary containing the contents of the resource.</returns>
		[JsonRpcMethod]
		[JsonRpcDocumentation("Reads an MCP server resource.")]
		[JsonRpcDocName("resources/read")]
		[return: JsonRpcDocumentation("Dictionary containing the contents of the resource.")]
		protected virtual async Task<Dictionary<string, object>?> Resources_Read(
			IJsonRpcCall Call,

			[JsonRpcDocumentation("URI of the resource to read.")]
			Uri Uri,

			[JsonRpcMetaDataArgument]
			[JsonRpcDocumentation("Associated meta-data, if available.")]
			object? _Meta = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return null;

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent)
				return null;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".resources/read(");
				sb.Append(Uri);

				if (!(_Meta is null))
				{
					sb.Append(',');
					JSON.Encode(_Meta, false, sb);
				}

				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			Resource? Resource = await this.TryGetResource(Call, User, Uri, Session);

			if (Resource is null)
			{
				if (this.hasSnifferSet)
					Session.Error("Resource not found: " + Uri);

				await Call.SendResponse(new NotFoundException("Resource not found."));
				return null;
			}

			if (!Resource.IsAuthorized(User, out string? MissingPrivilege) ||
				!this.CheckScopes(User, this.resourceScopes, out MissingPrivilege))
			{
				if (this.hasSnifferSet)
					Session.Error("Access denied. Missing privilege: " + MissingPrivilege);

				await Call.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
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
				Session.TransmitText(JSON.Encode(Result, false));

			return Result;
		}

		/// <summary>
		/// Subscribes to an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC request object.</param>
		/// <param name="Uri">URI of the resource to subscribe to.</param>
		[JsonRpcMethod]
		[JsonRpcDocumentation("Subscribes to an MCP server resource.")]
		[JsonRpcDocName("resources/subscribe")]
		protected virtual async Task Resources_Subscribe(IJsonRpcCall Call,

			[JsonRpcDocumentation("URI of the resource to subscribe to.")]
			Uri Uri)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return;

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent)
				return;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".resources/subscribe(");
				sb.Append(Uri);
				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			Resource? Resource = await this.TryGetResource(Call, User, Uri, Session);

			if (Resource is null)
			{
				if (this.hasSnifferSet)
					Session.Error("Resource not found: " + Uri);

				await Call.SendResponse(new NotFoundException("Resource not found."));
				return;
			}

			if (!Resource.IsAuthorized(User, out string? MissingPrivilege) ||
				!this.CheckScopes(User, this.resourceScopes, out MissingPrivilege))
			{
				if (this.hasSnifferSet)
					Session.Error("Access denied. Missing privilege: " + MissingPrivilege);

				await Call.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
					User?.UserName ?? string.Empty, MissingPrivilege ?? string.Empty));
				return;
			}

			bool Result = Session.Subscribe(Uri.ToString());

			if (this.hasSnifferSet)
			{
				if (Result)
					Session.Information("Subscription to resource successful: " + Uri);
				else
					Session.Information("Subscription already exists for: " + Uri);
			}
		}

		/// <summary>
		/// Unsubscribes from an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC call.</param>
		/// <param name="Uri">URI of the resource to unsubscribe from.</param>
		[JsonRpcMethod]
		[JsonRpcDocumentation("Unsubscribes from an MCP server resource.")]
		[JsonRpcDocName("resources/unsubscribe")]
		protected virtual async Task Resources_Unsubscribe(IJsonRpcCall Call,

			[JsonRpcDocumentation("URI of the resource to unsubscribe from.")]
			Uri Uri)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return;

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent)
				return;

			if (this.hasSnifferSet)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.Name);
				sb.Append(".resources/unsubscribe(");
				sb.Append(Uri);
				sb.Append(')');

				Session.ReceiveText(sb.ToString());
			}

			Resource? Resource = await this.TryGetResource(Call, User, Uri, Session);

			if (Resource is null)
			{
				if (this.hasSnifferSet)
					Session.Error("Resource not found: " + Uri);

				await Call.SendResponse(new NotFoundException("Resource not found."));
				return;
			}

			if (!Resource.IsAuthorized(User, out string? MissingPrivilege) ||
				!this.CheckScopes(User, this.resourceScopes, out MissingPrivilege))
			{
				if (this.hasSnifferSet)
					Session.Error("Access denied. Missing privilege: " + MissingPrivilege);

				await Call.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
					User?.UserName ?? string.Empty, MissingPrivilege ?? string.Empty));
				return;
			}

			bool Result = Session.Unsubscribe(Uri.ToString());

			if (this.hasSnifferSet)
			{
				if (Result)
					Session.Information("Unsubscription from resource successful: " + Uri);
				else
					Session.Information("No subscription found for: " + Uri);
			}
		}

		/// <summary>
		/// Gets available resources.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Array of resources.</returns>
		public virtual Task<Resource[]> GetResources(IJsonRpcCall Call, IUser? User,
			Session? Session)
		{
			return Task.FromResult(Array.Empty<Resource>());
		}

		/// <summary>
		/// If the MCP server has resource capabilities.
		/// </summary>
		public virtual bool HasResources => false;

		/// <summary>
		/// MCP server resource documentation, as an array of key-value pairs.
		/// The Key represents Markdown (true) or plain text (false), and the Value
		/// represents the documentation text. Each entry in the array represents a
		/// paragraph.
		/// </summary>
		public virtual KeyValuePair<bool, string>[] ResourceDocumentation =>
			Array.Empty<KeyValuePair<bool, string>>();

		/// <summary>
		/// Tries to get a resource, given its URI.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Resource, if found (and user has access rights to it), null otherwise.</returns>
		public virtual Task<Resource?> TryGetResource(IJsonRpcCall Call, IUser? User,
			Uri Uri, Session? Session)
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
				Dictionary<string, object> Notification = new Dictionary<string, object>()
				{
					{ "jsonrpc", "2.0" },
					{ "method", "notifications/resources/list_changed" }
				};

				await this.SendNotification(
					Session =>
					{
						if (!(Session is Session McpSession))
							return false;

						if (this.hasSnifferSet)
							McpSession.TransmitText(JSON.Encode(Notification, false));

						return true;
					},
					Notification);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Sends a notification to all sessions that match the given filter.
		/// </summary>
		/// <param name="Filter">Filter to select sessions.</param>
		/// <param name="Notification">Notification to send.</param>
		/// <returns>Task representing the asynchronous operation.</returns>
		protected Task SendNotification(Predicate<IJsonRpcSession?> Filter,
			Dictionary<string, object> Notification)
		{
			return this.SendEvent(
				Filter,
				new KeyValuePair<string, object>("event", "message"),
				new KeyValuePair<string, object>("data", JSON.Encode(Notification, false)));
		}

		/// <summary>
		/// Sends a notification to all sessions that match the given filter.
		/// </summary>
		/// <param name="Filter">Filter to select sessions.</param>
		/// <param name="Notification">Notification to send.</param>
		/// <returns>Task representing the asynchronous operation.</returns>
		protected Task SendNotification(Predicate<IJsonRpcSession?> Filter,
			string Notification)
		{
			return this.SendEvent(
				Filter,
				new KeyValuePair<string, object>("event", "message"),
				new KeyValuePair<string, object>("data", Notification));
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
				Dictionary<string, object> Notification = new Dictionary<string, object>()
				{
					{ "jsonrpc", "2.0" },
					{ "method", "notifications/resources/updated" },
					{ "params", new Dictionary<string, object>()
						{
							{ "uri", Uri.OriginalString }
						}
					}
				};

				string s = Uri.ToString();

				await this.SendNotification(
					Session =>
					{
						if (!(Session is Session McpSession))
							return false;

						if (!McpSession.IsSubscribed(s))
							return false;

						if (this.hasSnifferSet)
							McpSession.TransmitText(JSON.Encode(Notification, false));

						return true;
					},
					Notification);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			HttpJsonRpcCall JsonRpcCall = new HttpJsonRpcCall(Request, Response);
			Session? Session = await this.TryGetMcpSession(JsonRpcCall);
			if (Session is null)
				return;

			sessions.Remove(Session.SessionId);

			try
			{
				Response.StatusCode = 204;
				Response.StatusMessage = "No Content";

				await Response.SendResponse();
			}
			finally
			{
				await this.SendEvent(
					Loop => Session == Loop,
					"Terminating session.");
				this.Unregister(Session);
			}
		}

		/// <summary>
		/// Gets parameter documentation for a method parameter.
		/// </summary>
		/// <param name="Parameter">The method parameter.</param>
		/// <returns>The documentation for the parameter.</returns>
		protected override KeyValuePair<bool, string>[] GetParameterDocumentation(
			ProtectedMethodArgumentInfo Parameter)
		{
			Parameter.AdditionalDocumentation ??= GetAdditionalParameterDocumentation(Parameter);
			return base.GetParameterDocumentation(Parameter);
		}

		/// <summary>
		/// Gets documentation for a member.
		/// </summary>
		/// <returns>The documentation for the member.</returns>
		protected override KeyValuePair<bool, string>[] GetMemberDocumentation(
			ICustomAttributeProvider Member)
		{
			ChunkedList<KeyValuePair<bool, string>>? PropertyDoc = null;

			foreach (object Attribute in
				Member.GetCustomAttributes(typeof(McpParameterAttribute), true))
			{
				if (Attribute is McpParameterAttribute TypedAttribute)
				{
					PropertyDoc ??= new ChunkedList<KeyValuePair<bool, string>>();
					PropertyDoc.Add(new KeyValuePair<bool, string>(
						true, TypedAttribute.AnnotatedDescription));
				}
			}

			StringBuilder? Values = null;

			foreach (object Attribute in
				Member.GetCustomAttributes(typeof(McpEnumValueAttribute), true))
			{
				if (Attribute is McpEnumValueAttribute TypedAttribute)
				{
					if (Values is null)
					{
						Values = new StringBuilder();
						Values.AppendLine("Possible values:");
						Values.AppendLine();
					}

					Values.Append("* `\"");
					Values.Append(TypedAttribute.Value.ToString());
					Values.Append("\"` - ");
					Values.AppendLine(TypedAttribute.Title);
				}
			}

			if (!(Values is null))
			{
				PropertyDoc ??= new ChunkedList<KeyValuePair<bool, string>>();
				PropertyDoc.Add(new KeyValuePair<bool, string>(true, Values.ToString()));
			}

			return (PropertyDoc?.ToArray() ?? Array.Empty<KeyValuePair<bool, string>>()).Join(
				base.GetMemberDocumentation(Member));
		}

		/// <summary>
		/// Gets parameter documentation for a method parameter.
		/// </summary>
		/// <param name="Parameter">The method parameter.</param>
		/// <returns>The documentation for the parameter.</returns>
		private static KeyValuePair<bool, string>[] GetAdditionalParameterDocumentation(
			ProtectedMethodArgumentInfo Parameter)
		{
			ChunkedList<KeyValuePair<bool, string>>? Result = null;

			foreach (object Attribute in Parameter.Parameter.
				GetCustomAttributes(typeof(McpParameterAttribute), true))
			{
				if (Attribute is McpParameterAttribute McpParameterAttribute)
				{
					Result ??= new ChunkedList<KeyValuePair<bool, string>>();
					Result.Add(new KeyValuePair<bool, string>(true,
						McpParameterAttribute.AnnotatedDescription));
				}
			}

			StringBuilder? Values = null;

			foreach (object Attribute in Parameter.Parameter.
				GetCustomAttributes(typeof(McpEnumValueAttribute), true))
			{
				if (Attribute is McpEnumValueAttribute McpEnumValueAttribute)
				{
					if (Values is null)
					{
						Values = new StringBuilder();
						Values.AppendLine("Possible values:");
						Values.AppendLine();
					}

					Values.Append("* `\"");
					Values.Append(McpEnumValueAttribute.Value.ToString());
					Values.Append("\"` - ");
					Values.AppendLine(McpEnumValueAttribute.Title);
				}
			}

			if (!(Values is null))
			{
				Result ??= new ChunkedList<KeyValuePair<bool, string>>();
				Result.Add(new KeyValuePair<bool, string>(true, Values.ToString()));
			}

			return Result?.ToArray() ?? Array.Empty<KeyValuePair<bool, string>>();
		}

		/// <summary>
		/// Generates the Markdown ApiDescription for the documentation page.
		/// </summary>
		/// <param name="Notes">List of notes for the Markdown table.</param>
		/// <param name="TypesToDocument">Types that need documenting.</param>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Markdown">Markdown output.</param>
		protected override async Task GenerateDocumentationApiDescription(
			ChunkedList<string> Notes, HashSet<Type> TypesToDocument,
			HttpRequest Request, StringBuilder Markdown)
		{
			Markdown.AppendLine(new string('=', 80));
			Markdown.AppendLine();
			Markdown.AppendLine("MCP Server Interface");
			Markdown.AppendLine("-----------------------");
			Markdown.AppendLine();
			Markdown.Append("This [MCP Server](https://modelcontextprotocol.io/specification/2025-11-25) ");
			Markdown.AppendLine("is accessible on this endpoint: `");
			Markdown.Append(Request.Header.GetURL(false, false));
			Markdown.AppendLine("`");
			Markdown.AppendLine();

			if (this.hasScopes)
			{
				Markdown.AppendLine("Scopes supported:");
				Markdown.AppendLine();

				foreach (string Scope in this.scopesSupported)
				{
					Markdown.Append("* `");
					Markdown.Append(Scope);
					Markdown.AppendLine("`");
				}

				Markdown.AppendLine();
			}

			Markdown.Append("The following subsections list MCP Server interfaces that are ");
			Markdown.Append("available on this resource. The MCP protocol is built on ");
			Markdown.Append("top of the [JSON-RPC protocol](#jsonRpcInterface). You find ");
			Markdown.AppendLine("JSON-RPC interface below.");
			Markdown.AppendLine();

			if (this.hasTools)
			{
				await this.GenerateToolDocumentation(Notes, TypesToDocument,
					Request, Markdown);
			}

			if (this.hasPrompts)
			{
				await this.GeneratePromptDocumentation(Notes, TypesToDocument,
					Request, Markdown);
			}

			if (this.HasResources)
				await this.GenerateResourceDocumentation(Notes, Request, Markdown);

			await base.GenerateDocumentationApiDescription(Notes, TypesToDocument,
				Request, Markdown);
		}

		/// <summary>
		/// Generates documentation for MCP Server tools.
		/// </summary>
		/// <param name="Notes">List of notes for the Markdown table.</param>
		/// <param name="TypesToDocument">Types that need documenting.</param>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Markdown">Markdown output.</param>
		protected virtual Task GenerateToolDocumentation(ChunkedList<string> Notes,
			HashSet<Type> TypesToDocument, HttpRequest Request, StringBuilder Markdown)
		{
			Markdown.AppendLine(new string('=', 80));
			Markdown.AppendLine();
			Markdown.AppendLine("MCP Server Tools");
			Markdown.AppendLine("-------------------");
			Markdown.AppendLine();
			Markdown.Append("Following subsections list ");
			Markdown.Append("[MCP Server Tools](https://modelcontextprotocol.io/specification/2025-11-25/server/tools) ");
			Markdown.AppendLine("that can be used to interact with the MCP Server.");
			Markdown.AppendLine();

			Tool[] Tools;

			lock (this.tools)
			{
				Tools = new Tool[this.tools.Count];
				this.tools.Values.CopyTo(Tools, 0);
			}

			foreach (Tool Tool in Tools)
			{
				Markdown.AppendLine("<section>");
				Markdown.AppendLine();
				Markdown.Append("### ");
				Markdown.AppendLine(Tool.Title);
				Markdown.AppendLine();

				Markdown.AppendLine(MarkdownDocument.Encode(Tool.Description));
				Markdown.AppendLine();

				Markdown.AppendLine("| Properties ||");
				Markdown.AppendLine("|:-------|:------:|");
				Markdown.Append("| Can Modify:  | ");
				Markdown.Append(YesNo(Tool.CanModifyEnvironment));
				Markdown.AppendLine(" |");
				Markdown.Append("| Can Destroy:  | ");
				Markdown.Append(YesNo(Tool.CanDestroyEnvironment));
				Markdown.AppendLine(" |");
				Markdown.Append("| Is Idempotent:  | ");
				Markdown.Append(YesNo(Tool.Idempotent));
				Markdown.AppendLine(" |");
				Markdown.Append("| Open World Access:  | ");
				Markdown.Append(YesNo(Tool.OpenWorldAccess));
				Markdown.AppendLine(" |");
				Markdown.AppendLine();

				this.AppendDocumentation(Notes, TypesToDocument, Tool, Markdown);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates documentation for MCP Server prompts.
		/// </summary>
		/// <param name="Notes">List of notes for the Markdown table.</param>
		/// <param name="TypesToDocument">Types that need documenting.</param>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Markdown">Markdown output.</param>
		protected virtual Task GeneratePromptDocumentation(ChunkedList<string> Notes,
			HashSet<Type> TypesToDocument, HttpRequest Request, StringBuilder Markdown)
		{
			Markdown.AppendLine(new string('=', 80));
			Markdown.AppendLine();
			Markdown.AppendLine("MCP Server Prompts");
			Markdown.AppendLine("---------------------");
			Markdown.AppendLine();
			Markdown.Append("Following subsections list ");
			Markdown.Append("[MCP Server Prompts](https://modelcontextprotocol.io/specification/2025-11-25/server/prompts) ");
			Markdown.AppendLine("that can be used to interact with the MCP Server.");
			Markdown.AppendLine();

			Prompt[] Prompts;

			lock (this.prompts)
			{
				Prompts = new Prompt[this.prompts.Count];
				this.prompts.Values.CopyTo(Prompts, 0);
			}

			foreach (Prompt Prompt in Prompts)
			{
				Markdown.AppendLine("<section>");
				Markdown.AppendLine();
				Markdown.Append("### ");
				Markdown.AppendLine(Prompt.Title);
				Markdown.AppendLine();

				Markdown.AppendLine(MarkdownDocument.Encode(Prompt.Description));
				Markdown.AppendLine();

				this.AppendDocumentation(Notes, TypesToDocument, Prompt, Markdown);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates documentation for MCP Server resources.
		/// </summary>
		/// <param name="Notes">List of notes for the Markdown table.</param>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Markdown">Markdown output.</param>
		protected virtual Task GenerateResourceDocumentation(ChunkedList<string> Notes,
			HttpRequest Request, StringBuilder Markdown)
		{
			Markdown.AppendLine(new string('=', 80));
			Markdown.AppendLine();
			Markdown.AppendLine("MCP Server Resources");
			Markdown.AppendLine("-----------------------");
			Markdown.AppendLine();

			Markdown.Append("This MCP Server supports [MCP Server Resources]");
			Markdown.Append("(https://modelcontextprotocol.io/specification/2025-11-25/server/resources).");
			Markdown.AppendLine();

			AppendDocumentation(this.ResourceDocumentation, Markdown);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Elicits input from the user, if the client supports elicitation.
		/// </summary>
		/// <param name="Call">JSON-RPC Request object.</param>
		/// <param name="Message">Message to display to the user.</param>
		/// <param name="InputRequest">Input request object.</param>
		/// <param name="Sensitive">If information is sensitive.</param>
		/// <param name="Session">MCP session object.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <returns>Returns true if user provided input (which will be stored in
		/// <paramref name="InputRequest"/>, false if user declined to provide user
		/// input, or null if request was cancelled or timed out.</returns>
		public async Task<bool?> ElicitUserInput<T>(IJsonRpcCall Call, string Message,
			T InputRequest, bool Sensitive, Session Session, int Timeout)
			where T : class
		{
			if (Session.ClientCapabilities?.Elicitation is null)
				throw new ServiceUnavailableException("MCP Client does not support elication of user input.");

			Type InputType = InputRequest.GetType();
			McpParameterAttribute? ParameterInfo = InputType.GetCustomAttribute<McpParameterAttribute>();
			IEnumerable<McpEnumValueAttribute> EnumValues = InputType.GetCustomAttributes<McpEnumValueAttribute>();
			Dictionary<string, object?> ElicitationRequest;
			bool UrlMode;

			if (Session.ClientCapabilities.Elicitation.Form &&
				(!Sensitive || !Session.ClientCapabilities.Elicitation.Url))
			{
				object InputSchema = await Tool.GenerateSchema(InputType, true, InputRequest,
					ParameterInfo, EnumValues);

				if (Sensitive)
				{
					Message += "\n\nNOTE: The information you provide is marked as " +
						"sensitive, and should be input using a separate form. Your " +
						"MCP client does not support opening such separate forms. " +
						"DO NOT enter sensitive information unless you know your " +
						"agent manages sensitive information correctly. Consider using " +
						"an MCP client that supports opening input forms in separate " +
						"windows or tabs.";
				}

				ElicitationRequest = new Dictionary<string, object?>()
				{
					{ "mode", "form" },
					{ "message", Message },
					{ "requestedSchema", InputSchema }
				};

				UrlMode = false;
			}
			else if (Session.ClientCapabilities.Elicitation.Url)
			{
				ElicitationRequest = new Dictionary<string, object?>()
				{
					{ "mode", "url" },
					{ "message", Message }
				};

				UrlMode = true;
			}
			else
				throw new ServiceUnavailableException("Unable to elicit user input via URL.");

			using JsonRpcClientRequest<bool?> Request = this.CreateRequest<bool?>(
				Message, "elicitation/create", ElicitationRequest, Session,
				async Result =>
				{
					if (!(Result is Dictionary<string, object> ResultObj))
						throw new BadRequestException("Invalid response.");

					if (!ResultObj.TryGetValue("action", out object Obj) ||
						!(Obj is string Action))
					{
						throw new BadRequestException("Expected action.");
					}

					switch (Action)
					{
						case "decline": return false;
						case "cancel": return null;

						case "accept":
							if (!ResultObj.TryGetValue("content", out Obj))
								throw new BadRequestException("Missing content.");

							if (!(Obj is Dictionary<string, object> Properties))
								throw new BadRequestException("Invalid content.");

							await SetProperties(InputRequest, Properties);

							return true;

						default:
							throw new Exception("Unexpected action: " + Action);
					}
				},
				Call);

			Request.Tag = InputRequest;

			if (UrlMode)
			{
				string Url = Call.GetBaseUrl() + "/" + Request.Id;

				ElicitationRequest["elicitationId"] = Request.Id;
				ElicitationRequest["url"] = Url;

				async Task Completed(object _, EventArgs e)
				{
					Dictionary<string, object> Notification = new Dictionary<string, object>()
					{
						{ "jsonrpc", "2.0" },
						{ "method", "notifications/elicitation/complete" },
						{ "params", new Dictionary<string, object?>()
							{
								{ "elicitationId", Request.Id }
							}
						}
					};

					await this.SendNotification(
						Session2 =>
						{
							if (Session.SessionId != Session2?.SessionId)
								return false;

							Session2.TransmitText(JSON.Encode(Notification, false));

							return true;
						},
						Notification);
				}

				Request.ResultReturned += Completed;
				Request.ErrorReturned += Completed;
				Request.Cancelled += Completed;
			}

			await Request.SendRequest();
			return await Request.WaitForResultAsync(Timeout);
		}

		/// <summary>
		/// Elicits input from the user, if the client supports elicitation.
		/// </summary>
		/// <param name="Call">JSON-RPC Request object.</param>
		/// <param name="Message">Message to display to the user.</param>
		/// <param name="Url">URL to open.</param>
		/// <param name="Session">MCP session object.</param>
		/// <returns>Returns true if URL propagated to user.</returns>
		public async Task<bool> ElicitOpenUrl(IJsonRpcCall Call, string Message,
			string Url, Session Session)
		{
			if (Session.ClientCapabilities?.Elicitation is null)
				return false;

			if (!Session.ClientCapabilities.Elicitation.Url)
				return false;

			Dictionary<string, object?> ElicitationRequest = new Dictionary<string, object?>()
			{
				{ "mode", "url" },
				{ "message", Message },
				{ "url", Url }
			};

			using JsonRpcClientRequest<bool?> Request = this.CreateRequest(
				Message, "elicitation/create", ElicitationRequest, Session,
				Result =>
				{
					if (!(Result is Dictionary<string, object> ResultObj))
						return Task.FromResult<bool?>(false);

					if (!ResultObj.TryGetValue("action", out object Obj) ||
						!(Obj is string Action))
					{
						return Task.FromResult<bool?>(false);
					}

					return Action switch
					{
						"decline" => Task.FromResult<bool?>(false),
						"cancel" => Task.FromResult<bool?>(null),
						"accept" => Task.FromResult<bool?>(true),
						_ => Task.FromResult<bool?>(false),
					};
				},
				Call);

			async Task Completed(object _, EventArgs e)
			{
				Dictionary<string, object> Notification = new Dictionary<string, object>()
				{
					{ "jsonrpc", "2.0" },
					{ "method", "notifications/elicitation/complete" },
					{ "params", new Dictionary<string, object?>()
						{
							{ "elicitationId", Request.Id }
						}
					}
				};

				await this.SendNotification(
					Session2 =>
					{
						if (Session.SessionId != Session2?.SessionId)
							return false;

						Session2.TransmitText(JSON.Encode(Notification, false));

						return true;
					},
					Notification);
			}

			Request.ResultReturned += Completed;
			Request.ErrorReturned += Completed;
			Request.Cancelled += Completed;

			await Request.SendRequest();

			return true;
		}

		internal static async Task SetProperties(object Object, Dictionary<string, object> Properties)
		{
			Type T = Object.GetType();

			foreach (KeyValuePair<string, object> P in Properties)
			{
				object Value = P.Value;
				Dictionary<string, object>? SubProperties = Value as Dictionary<string, object>;
				bool IsSubProperties = !(SubProperties is null);
				McpFileUploadParameterAttribute? FileParameter;

				FieldInfo? FI = T.GetField(P.Key, BindingFlags.Public | BindingFlags.Instance);
				if (!(FI is null))
				{
					if (IsSubProperties)
					{
						object? Item = FI.GetValue(Object);

						if (Item is null)
						{
							Item = Types.Create(false, FI.FieldType);
							FI.SetValue(Object, Item);
						}

						await SetProperties(Item, SubProperties!);
					}
					else if (Value is null || FI.FieldType.IsAssignableFrom(Value.GetType()))
						FI.SetValue(Object, Value);
					else if (Expression.TryConvert(Value, FI.FieldType, true, out object? Value2))
						FI.SetValue(Object, Value2);
					else if (Value is string s && string.IsNullOrEmpty(s) && Expression.CanBeSetToNull(FI.FieldType))
						FI.SetValue(Object, null);
					else if (!((FileParameter = FI.GetCustomAttribute<McpFileUploadParameterAttribute>()) is null) &&
						!((Value2 = await TryConvertFileInput(Value, FI.FieldType, FileParameter)) is null))
					{
						FI.SetValue(Object, Value2);
					}
					else
					{
						throw new InvalidCastException("Unable to convert value of type " +
							Value.GetType().FullName + " to " +
							FI.FieldType.FullName + ".");
					}

					continue;
				}

				PropertyInfo? PI = T.GetProperty(P.Key, BindingFlags.Public | BindingFlags.Instance);
				if (!(PI is null))
				{
					if (IsSubProperties)
					{
						object? Item = PI.GetValue(Object);

						if (Item is null)
						{
							Item = Types.Create(false, PI.PropertyType);
							PI.SetValue(Object, Item);
						}

						await SetProperties(Item, SubProperties!);
					}
					else if (Value is null || PI.PropertyType.IsAssignableFrom(Value.GetType()))
						PI.SetValue(Object, Value);
					else if (Expression.TryConvert(Value, PI.PropertyType, true, out object? Value2))
						PI.SetValue(Object, Value2);
					else if (Value is string s && string.IsNullOrEmpty(s) && Expression.CanBeSetToNull(PI.PropertyType))
						PI.SetValue(Object, null);
					else if (!((FileParameter = PI.GetCustomAttribute<McpFileUploadParameterAttribute>()) is null) &&
						!((Value2 = await TryConvertFileInput(Value, PI.PropertyType, FileParameter)) is null))
					{
						PI.SetValue(Object, Value2);
					}
					else
					{
						throw new InvalidCastException("Unable to convert value of type " +
							Value.GetType().FullName + " to " +
							PI.PropertyType.FullName + ".");
					}

					continue;
				}

				throw new InvalidOperationException("Unrecognized field ro property name: " + P.Key);
			}
		}

		private static async Task<object?> TryConvertFileInput(object Value, Type DesiredType,
			McpFileUploadParameterAttribute FileParameter)
		{
			string? Accept = FileParameter?.Accept;

			if (!(Value is byte[] Bin))
			{
				if (Value is string s)
				{
					try
					{
						Bin = Convert.FromBase64String(s);
					}
					catch (Exception)
					{
						return null;
					}
				}
				else
					return null;
			}

			IContentDecoder[] Decoders;

			if (!string.IsNullOrEmpty(Accept))
			{
				ChunkedList<IContentDecoder>? AcceptableDecoders = null;
				HttpFieldAccept Field = new HttpFieldAccept("Accept", Accept);

				foreach (AcceptRecord Record in Field.Records)
				{
					foreach (IContentDecoder Decoder in InternetContent.Decoders)
					{
						foreach (string ContentType in Decoder.ContentTypes)
						{
							if (Record.IsAcceptable(ContentType, out _, out _))
							{
								AcceptableDecoders ??= new ChunkedList<IContentDecoder>();
								AcceptableDecoders.Add(Decoder);
								break;
							}
						}
					}
				}

				Decoders = AcceptableDecoders?.ToArray() ?? Array.Empty<IContentDecoder>();
			}
			else
				Decoders = InternetContent.Decoders;

			if ((Decoders?.Length ?? 0) == 0)
				return false;

			foreach (IContentDecoder Decoder in Decoders!)
			{
				try
				{
					ContentResponse Decoded = await Decoder.DecodeAsync(Accept, Bin,
						Encoding.UTF8, Array.Empty<KeyValuePair<string, string>>(),
						null, null);

					if (Decoded.HasError)
						continue;

					if (DesiredType.IsAssignableFrom(Decoded.Decoded.GetType()))
						return Decoded.Decoded;

					if (DesiredType == typeof(CustomEncoding))
						return new CustomEncoding(Decoded.ContentType, Decoded.Encoded);
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			return null;
		}
	}
}
