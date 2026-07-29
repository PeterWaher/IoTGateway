using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Content.Html;
using Waher.Content.Json;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Networking.HTTP.JsonRpc.MetaData;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Security.JWT;
using Waher.Things.Http;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Abstract base class for Web Services based on JSON-RPC v2.0.
	/// 
	/// Ref:
	/// https://www.jsonrpc.org/specification
	/// https://www.jsonrpc.org/historical/json-rpc-over-http.html
	/// </summary>
	public abstract class JsonRpcWebService : HttpProtectedResource, IHttpGetMethod, IHttpPostMethod
	{
		private static readonly JsonCodec jsonCodec = new JsonCodec();

		private readonly Dictionary<string, IJsonRpcClientRequest> requests = new Dictionary<string, IJsonRpcClientRequest>();
		private readonly SortedDictionary<string, JsonRpcMethodInfo> methods;
		private readonly bool userSessions;
		private readonly bool caseSensitive;
		private HttpAuthenticationScheme[]? authenticationSchemes = null;
		private ProtectedResourceMetaData? resourceMetaData = null;
		private ProtectedResourceMetaData? metaDataResource = null;
		private JwtFactory? jwtFactory = null;
		private string? domain = null;
		private bool hasMetaDataResource = false;
		private bool hasDomain = false;
		private bool hasJwtFactory = false;

		/// <summary>
		/// Abstract base class for Web Services based on JSON-RPC v2.0.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		public JsonRpcWebService(string ResourceName, bool UserSessions)
			: this(ResourceName, UserSessions, true)
		{
		}

		/// <summary>
		/// Abstract base class for Web Services based on JSON-RPC v2.0.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="CaseSensitive">If names are case-sensitive.</param>
		public JsonRpcWebService(string ResourceName, bool UserSessions, bool CaseSensitive)
			: base(ResourceName)
		{
			this.userSessions = UserSessions;
			this.caseSensitive = CaseSensitive;

			if (CaseSensitive)
				this.methods = new SortedDictionary<string, JsonRpcMethodInfo>(StringComparer.InvariantCulture);
			else
				this.methods = new SortedDictionary<string, JsonRpcMethodInfo>(StringComparer.InvariantCultureIgnoreCase);

			foreach (MethodInfo Method in this.GetType().GetMethods(BindingFlags.Instance |
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (!Method.IsDefined(typeof(JsonRpcMethodAttribute), true))
					continue;
					
				this.RegisterMethod(Method, GetRequiredPrivileges(Method));
			}
		}

		/// <summary>
		/// Gets required privileges for the user calling the method, if any.
		/// If no privilege requirements are found, null is returned.
		/// </summary>
		/// <param name="Method">Method information.</param>
		/// <returns>Array of required privileges, or null if none.</returns>
		public static string[]? GetRequiredPrivileges(MethodInfo Method)
		{
			ChunkedList<string>? RequiredPrivileges = null;

			foreach (RequiredPrivilegeAttribute Attribute in
				Method.GetCustomAttributes<RequiredPrivilegeAttribute>(true))
			{
				RequiredPrivileges ??= new ChunkedList<string>();
				RequiredPrivileges.Add(Attribute.Privilege);
			}

			return RequiredPrivileges?.ToArray();
		}

		/// <summary>
		/// If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).
		/// </summary>
		public override bool Synchronous => false;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => this.userSessions;

		/// <summary>
		/// If Server-Sent Events (SSE) are supported by the resource.
		/// </summary>
		public virtual bool SupportsServerSentEvents => false;

		/// <summary>
		/// If a Server-Sent Events (SSE) welcome message should be sent to clients 
		/// with open subscriptions.
		/// </summary>
		public virtual bool SendSseWelcomeMessage => false;

		/// <summary>
		/// Server-Sent Events (SSE) welcome message, if one should be sent.
		/// </summary>
		public virtual string SseWelcomeMessage => string.Empty;

		/// <summary>
		/// OAUTH resource meta-data resource, if any registered.
		/// </summary>
		public ProtectedResourceMetaData? MetaDataResource => this.metaDataResource;

		/// <summary>
		/// Domain name of the server, if any registered.
		/// </summary>
		public string? Domain => this.domain;

		/// <summary>
		/// If an OAUTH resource meta-data resource is registered on the server.
		/// </summary>
		public bool HasMetaDataResource => this.hasMetaDataResource;

		/// <summary>
		/// If a domain name is registered on the server.
		/// </summary>
		public bool HasDomain => this.hasDomain;

		/// <summary>
		/// JWT Factory, if available.
		/// </summary>
		protected JwtFactory? JwtFactory => this.jwtFactory;

		/// <summary>
		/// If a JWT Factory is available.
		/// </summary>
		protected bool HasJwtFactory => this.hasJwtFactory;

		/// <summary>
		/// Generic authentication schemes for the resource.
		/// </summary>
		public HttpAuthenticationScheme[]? AuthenticationSchemes => this.authenticationSchemes;

		/// <summary>
		/// Registers a method to be used in the JSON-RPC interface. 
		/// </summary>
		/// <param name="Method">Method to register.</param>
		/// <param name="RequiredPrivileges">Required privileges for accessing the method.</param>
		public void RegisterMethod(MethodInfo Method, params string[]? RequiredPrivileges)
		{
			JsonRpcMethodInfo MethodInfo;

			lock (this.methods)
			{
				string Name = Method.Name;

				if (this.methods.ContainsKey(Name))
					throw new Exception("Method already registered: " + Name);

				this.methods[Name] = MethodInfo = new JsonRpcMethodInfo(Method,
					this.caseSensitive, RequiredPrivileges);
			}

			if (!(this.FirstServer is null))
				this.AddAuthenticationMechanisms(MethodInfo);
		}

		/// <summary>
		/// Unregisters a method from the JSON-RPC interface.
		/// </summary>
		/// <param name="Method">Method to unregister.</param>
		/// <returns>True if the method was successfully unregistered, false otherwise.</returns>
		public bool Unregister(MethodInfo Method)
		{
			lock (this.methods)
			{
				string Name = Method.Name;

				if (this.methods.TryGetValue(Name, out JsonRpcMethodInfo Prev) &&
					Prev.Method == Method)
				{
					return this.methods.Remove(Name);
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Tries to get the resource meta-data resource, if any registered.
		/// </summary>
		/// <param name="Server">HTTP Server whose resources are being queried.</param>
		/// <param name="Resource">The resource meta-data resource, if found.</param>
		/// <returns>True if a resource meta-data resource is found, false otherwise.</returns>
		public bool TryGetResourceMetaDataResource(HttpServer Server,
			[NotNullWhen(true)] out ProtectedResourceMetaData? Resource)
		{
			if (this.resourceMetaData is null)
			{
				string s = ProtectedResourceMetaData.WellKnowResourcePath;

				if (Server.TryGetResource(ref s, out HttpResource HttpResource, out _) &&
					HttpResource is ProtectedResourceMetaData MetaDataResource)
				{
					this.resourceMetaData = MetaDataResource;
				}
			}

			Resource = this.resourceMetaData;
			return !(Resource is null);
		}

		/// <summary>
		/// Method called when a resource has been registered on a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public override void AddReference(HttpServer Server)
		{
			base.AddReference(Server);

			this.hasMetaDataResource = this.TryGetResourceMetaDataResource(Server,
				out this.metaDataResource);
			this.hasDomain = Types.TryGetModuleParameter("Domain", out this.domain);

			if (Types.TryGetModuleParameter("JWT", out JwtFactory JwtFactory) &&
				!JwtFactory.Disposed)
			{
				this.jwtFactory = JwtFactory;
				this.hasJwtFactory = true;
			}
			else
			{
				this.jwtFactory = null;
				this.hasJwtFactory = false;
			}

			JsonRpcMethodInfo[] Methods;
			int c;

			lock (this.methods)
			{
				c = this.methods.Count;
				Methods = new JsonRpcMethodInfo[c];
				this.methods.Values.CopyTo(Methods, 0);
			}

			foreach (JsonRpcMethodInfo MethodInfo in Methods)
				this.AddAuthenticationMechanisms(MethodInfo);

			if (this.HasMetaDataResource)
			{
				string ResourceMetaData = this.metaDataResource!.GetResourceMetaDataUri(
					this.hasDomain, this.domain, this.ResourceName);

				this.authenticationSchemes = HttpModule.GetAuthenticationSchemes(
					new Uri(ResourceMetaData));
			}
			else
				this.authenticationSchemes = HttpModule.GetAuthenticationSchemes();
		}

		/// <summary>
		/// Adds authentication mechanisms to a method, if required.
		/// </summary>
		/// <param name="Method">Method to add authentication mechanisms to.</param>
		public void AddAuthenticationMechanisms(ProtectedMethod Method)
		{
			if (Method.RequiresAuthentication)
			{
				if (this.hasMetaDataResource && this.hasDomain)
				{
					Method.UpdateAuthenticationMechanisms(
						this.metaDataResource!.GetResourceMetaDataUri(true, this.domain, this.ResourceName));
				}
				else
					Method.UpdateAuthenticationMechanisms();
			}
		}


		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(IDictionary<string, object> Fields)
		{
			return this.SendEvent(All, null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(params KeyValuePair<string, object>[] Fields)
		{
			return this.SendEvent(All, null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(IEnumerable<KeyValuePair<string, object>> Fields)
		{
			return this.SendEvent(All, null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(string? Comment, IDictionary<string, object> Fields)
		{
			return this.SendEvent(All, Comment,
				(IEnumerable<KeyValuePair<string, object>>)Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(string? Comment, params KeyValuePair<string, object>[] Fields)
		{
			return this.SendEvent(All, Comment, (IEnumerable<KeyValuePair<string, object>>)Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(string? Comment, IEnumerable<KeyValuePair<string, object>> Fields)
		{
			return this.SendEvent(All, Comment, Fields);
		}

		/// <summary>
		/// Filter function that selects all sessions.
		/// </summary>
		/// <param name="Session">Session</param>
		/// <returns>Always returns true.</returns>
		public static bool All(IJsonRpcSession? Session)
		{
			return true;
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Filter">Session filter, selecting the subscriptions that will
		/// receive the event.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(Predicate<IJsonRpcSession?> Filter,
			IDictionary<string, object> Fields)
		{
			return this.SendEvent(Filter, null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Filter">Session filter, selecting the subscriptions that will
		/// receive the event.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(Predicate<IJsonRpcSession?> Filter,
			params KeyValuePair<string, object>[] Fields)
		{
			return this.SendEvent(Filter, null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Filter">Session filter, selecting the subscriptions that will
		/// receive the event.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(Predicate<IJsonRpcSession?> Filter,
			IEnumerable<KeyValuePair<string, object>> Fields)
		{
			return this.SendEvent(Filter, null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Filter">Session filter, selecting the subscriptions that will
		/// receive the event.</param>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(Predicate<IJsonRpcSession?> Filter, string? Comment,
			IDictionary<string, object> Fields)
		{
			return this.SendEvent(Filter, Comment,
				(IEnumerable<KeyValuePair<string, object>>)Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Filter">Session filter, selecting the subscriptions that will
		/// receive the event.</param>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(Predicate<IJsonRpcSession?> Filter, string? Comment,
			params KeyValuePair<string, object>[] Fields)
		{
			return this.SendEvent(Filter, Comment, (IEnumerable<KeyValuePair<string, object>>)Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Filter">Session filter, selecting the subscriptions that will
		/// receive the event.</param>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(Predicate<IJsonRpcSession?> Filter, string? Comment,
			IEnumerable<KeyValuePair<string, object>> Fields)
		{
			if (!this.SupportsServerSentEvents)
				throw new InvalidOperationException("Server-Sent Events (SSE) not supported by this resource.");

			StringBuilder sb = new StringBuilder();
			bool Empty = true;

			if (!string.IsNullOrEmpty(Comment))
			{
				Empty = false;
				sb.Append(Comment);
				if (Comment.IndexOfAny(CommonTypes.CRLF) >= 0)
				{
					foreach (string Line in Comment.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
					{
						sb.Append(": ");
						sb.Append(Line);
						sb.Append("\r\n");
					}
				}
				else
				{
					sb.Append(": ");
					sb.Append(Comment);
					sb.Append("\r\n");
				}
			}

			if (!(Fields is null))
			{
				foreach (KeyValuePair<string, object> P in Fields)
				{
					Empty = false;

					if (!(P.Value is string s))
						s = JSON.Encode(P.Value, false);

					if (s.IndexOfAny(CommonTypes.CRLF) >= 0)
					{
						foreach (string Line in s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
						{
							sb.Append(P.Key);
							sb.Append(": ");
							sb.Append(Line);
							sb.Append("\r\n");
						}
					}
					else
					{
						sb.Append(P.Key);
						sb.Append(": ");
						sb.Append(s);
						sb.Append("\r\n");
					}
				}
			}

			if (Empty)
				sb.Append(":\r\n");

			sb.Append("\r\n");

			return this.SendEvent(Filter, sb.ToString());
		}

		private async Task<int> SendEvent(Predicate<IJsonRpcSession?> Filter, string Event)
		{
			int Count = 0;

			foreach (Subscription Subscription in this.eventSubscriptionsStatic)
			{
				try
				{
					if (Filter(Subscription.Session))
					{
						await Subscription.Response.Write(Event);
						await Subscription.Response.Flush(false);
						Count++;
					}
				}
				catch (Exception)
				{
					lock (this.eventSubscriptions)
					{
						this.eventSubscriptions.Remove(Subscription);
						this.eventSubscriptionsStatic = this.eventSubscriptions.ToArray();
					}
				}
			}

			return Count;
		}

		private readonly ChunkedList<Subscription> eventSubscriptions = new ChunkedList<Subscription>();
		private Subscription[] eventSubscriptionsStatic = Array.Empty<Subscription>();
		private bool eventSubscriptionsKeepAliveRunning = false;

		private class Subscription
		{
			public readonly HttpResponse Response;
			public readonly IJsonRpcSession? Session;

			public Subscription(HttpResponse Response, IJsonRpcSession? Session)
			{
				this.Response = Response;
				this.Session = Session;
			}
		}

		private async void KeepEventSubscrptionsAlive()
		{
			try
			{
				do
				{
					await Task.Delay(15000);    // Keep alive every 15 seconds.
				}
				while (await this.SendEvent(string.Empty) > 0);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			finally
			{
				this.eventSubscriptionsKeepAliveRunning = false;
			}
		}

		/// <summary>
		/// Sends a request to the client, and waits for a response.
		/// </summary>
		/// <param name="Message">Message to user.</param>
		/// <param name="Method">Method to call.</param>
		/// <param name="Parameters">Parameters for the method.</param>
		/// <param name="Session">Session to receive the request.</param>
		/// <param name="ParseResult">Function to parse incoming results.</param>
		/// <param name="HttpRequest">HTTP Request object.</param>
		/// <returns>Request object.</returns>
		public JsonRpcClientRequest<T> CreateRequest<T>(string Message, string Method, 
			object? Parameters, IJsonRpcSession Session, Func<object?, Task<T>> ParseResult,
			HttpRequest HttpRequest)
		{
			JsonRpcClientRequest<T> Request;
			string Id;

			lock (this.requests)
			{
				do
				{
					Id = OAuth2Environment.GenerateRandomCode(32);
				}
				while (this.requests.ContainsKey(Id));

				Request = new JsonRpcClientRequest<T>(Message, Id, Method, Parameters, 
					Session, ParseResult, this, HttpRequest);

				this.requests[Id] = Request;
			}

			return Request;
		}

		/// <summary>
		/// Tries to get a pending client request, given its ID.
		/// </summary>
		/// <param name="Id">ID of the request.</param>
		/// <param name="Request">The request object, if found.</param>
		/// <returns>True if the request was found; otherwise, false.</returns>
		public bool TryGetRequest(string Id, 
			[NotNullWhen(true)] out IJsonRpcClientRequest? Request)
		{
			lock (this.requests)
			{
				return this.requests.TryGetValue(Id, out Request);
			}
		}

		/// <summary>
		/// Removes a client request.
		/// </summary>
		/// <param name="Id">Request ID</param>
		/// <returns>True if the request was successfully removed; otherwise, false.</returns>
		internal bool RemoveClientRequest(string Id)
		{
			lock (this.requests)
			{
				return this.requests.Remove(Id);
			}
		}

		/// <summary>
		/// Gets and removes a client request from the list of pending requests.
		/// </summary>
		/// <param name="Id">ID of request.</param>
		/// <returns>If a request object matching the ID was found.</returns>
		internal IJsonRpcClientRequest? PopClientRequest(string Id)
		{
			lock (this.requests)
			{
				if (this.requests.TryGetValue(Id, out IJsonRpcClientRequest? Request))
				{
					this.requests.Remove(Id);
					return Request;
				}
				else
					return null;
			}
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public virtual async Task GET(HttpRequest Request, HttpResponse Response)
		{
			if (Request.Header.IsAcceptable(HtmlCodec.DefaultContentType))
			{
				await this.GenerateDocumentation(Request, Response);
				return;
			}
			else if (Request.Header.IsAcceptable("text/event-stream"))
			{
				if (!this.SupportsServerSentEvents)
				{
					await Response.SendResponse(new NotAcceptableException("Server-Sent Events (SSE) not supported by this resource."));
					return;
				}

				IJsonRpcSession? Session = await this.TryGetSession(Request, Response);
				if (Response.ResponseSent)
					return;

				Response.StatusCode = 200;
				Response.StatusMessage = "OK";
				Response.ContentType = "text/event-stream";
				Response.EnableDirectTransfer();

				if (this.SendSseWelcomeMessage)
				{
					string s = this.SseWelcomeMessage;

					if (string.IsNullOrEmpty(s))
						await Response.Write(":\r\n");
					else
					{
						StringBuilder sb = new StringBuilder();

						foreach (string Line in s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
						{
							sb.Append(": ");
							sb.Append(Line);
							sb.Append("\r\n");
						}

						sb.Append("\r\n");

						await Response.Write(sb.ToString());
					}
				}
				else
					await Response.Write(":\r\n");

				await Response.Flush(false);

				lock (this.eventSubscriptions)
				{
					foreach (Subscription Subscription in this.eventSubscriptions)
					{
						if (!(Subscription.Session is null) &&
							!(Session is null) &&
							Subscription.Session.SessionId == Session.SessionId)
						{
							this.eventSubscriptions.Remove(Subscription);
							break;
						}
					}

					this.eventSubscriptions.Add(new Subscription(Response, Session));
					this.eventSubscriptionsStatic = this.eventSubscriptions.ToArray();

					if (!this.eventSubscriptionsKeepAliveRunning)
					{
						this.eventSubscriptionsKeepAliveRunning = true;
						this.KeepEventSubscrptionsAlive();
					}
				}

				return;
			}

			using JsonRpcServerRequest JsonRpcRequest = new JsonRpcServerRequest();

			if (!(Request.Header.QueryParameters is null))
			{
				foreach (KeyValuePair<string, string> P in Request.Header.QueryParameters)
				{
					string s = P.Value;
					object? Value;

					if (P.Key == "params")
					{
						try
						{
							Value = JSON.Parse(s);
						}
						catch (Exception ex)
						{
							JsonRpcRequest.SetError(-32700, "Unable to parse parameter: " +
								P.Key + ": " + Log.UnnestException(ex).Message,
								InternalServerErrorException.Code, InternalServerErrorException.StatusMessage);
							continue;
						}
					}
					else
						Value = P.Value;

					this.ProcessQueryParameter(JsonRpcRequest, P.Key, Value);
				}
			}

			if (!await JsonRpcRequest.BuildResponse(this, Request, Response))
				await this.SendResponse(Request, JsonRpcRequest, Response);
		}

		/// <summary>
		/// Generates a documentation page for the resource, if supported.
		/// </summary>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Response">HTTP Response object</param>	
		protected virtual async Task GenerateDocumentation(HttpRequest Request, HttpResponse Response)
		{
			StringBuilder Markdown = new StringBuilder();
			ChunkedList<string> Notes = new ChunkedList<string>();
			HashSet<Type> TypesToDocument = new HashSet<Type>();

			await this.GenerateDocumentationHeader(Request, Markdown);
			await this.GenerateDocumentationIntroduction(Request, Markdown);
			await this.GenerateDocumentationApiDescription(Notes, TypesToDocument, Request, Markdown);
			await this.GenerateTypeDocumentation(Notes, TypesToDocument, Request, Markdown);

			int i = 0;

			foreach (string Note in Notes)
			{
				Markdown.Append("[^n");
				Markdown.Append(i++);
				Markdown.Append("]: ");
				Markdown.AppendLine(Note);
				Markdown.AppendLine();
			}

			MarkdownSettings Settings = new MarkdownSettings(null, true, new Variables());
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown.ToString(), Settings);
			string Html = await Doc.GenerateHTML();

			await Response.Return(new HtmlDocument(Html));
		}

		/// <summary>
		/// Generates the Markdown header for the documentation page.
		/// </summary>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Markdown">Markdown output.</param>
		protected virtual Task GenerateDocumentationHeader(HttpRequest Request, StringBuilder Markdown)
		{
			Markdown.Append("Title: ");
			Markdown.AppendLine(this.Title);
			Markdown.Append("Description: ");
			Markdown.AppendLine(this.ShortDescription);

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

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates the Markdown introduction for the documentation page.
		/// </summary>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Markdown">Markdown output.</param>
		protected virtual Task GenerateDocumentationIntroduction(HttpRequest Request, StringBuilder Markdown)
		{
			Markdown.AppendLine(new string('=', 80));
			Markdown.AppendLine();
			Markdown.AppendLine(this.Title);
			Markdown.AppendLine("========");
			Markdown.AppendLine();
			Markdown.AppendLine(this.MarkdownDescription);
			Markdown.AppendLine();
			Markdown.AppendLine("![Table of Contents](ToC)");
			Markdown.AppendLine();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates the Markdown ApiDescription for the documentation page.
		/// </summary>
		/// <param name="Notes">List of notes for the Markdown table.</param>
		/// <param name="TypesToDocument">Types that need documentation.</param>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Markdown">Markdown output.</param>
		protected virtual Task GenerateDocumentationApiDescription(
			ChunkedList<string> Notes, HashSet<Type> TypesToDocument,
			HttpRequest Request, StringBuilder Markdown)
		{
			Markdown.AppendLine(new string('=', 80));
			Markdown.AppendLine();
			Markdown.AppendLine("JSON-RPC Interface");
			Markdown.AppendLine("---------------------");
			Markdown.AppendLine();
			Markdown.Append("This [JSON-RPC](https://www.jsonrpc.org/specification) ");
			Markdown.AppendLine("Web Service is accessible on this endpoint: `");
			Markdown.Append(Request.Header.GetURL(false, false));
			Markdown.AppendLine("`");
			Markdown.AppendLine();

			Markdown.Append("The following subsections list JSON-RPC methods that are ");
			Markdown.AppendLine("available on this resource.");
			Markdown.AppendLine();

			JsonRpcMethodInfo[] Methods;

			lock (this.methods)
			{
				Methods = new JsonRpcMethodInfo[this.methods.Count];
				this.methods.Values.CopyTo(Methods, 0);
			}

			foreach (JsonRpcMethodInfo Method in Methods)
			{
				Markdown.AppendLine("<section>");
				Markdown.AppendLine();

				Markdown.Append("### ");
				this.AppendDocumentation(Notes, TypesToDocument, Method, Markdown);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Documents types used in the JSON-RPC interface, if any.
		/// </summary>
		/// <param name="Notes">List of notes for the Markdown table.</param>
		/// <param name="TypesToDocument">Types that need documentation.</param>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Markdown">Markdown output.</param>
		/// <returns></returns>
		protected virtual Task GenerateTypeDocumentation(ChunkedList<string> Notes,
			HashSet<Type> TypesToDocument, HttpRequest Request, StringBuilder Markdown)
		{
			if (TypesToDocument.Count == 0)
				return Task.CompletedTask;

			Markdown.AppendLine(new string('=', 80));
			Markdown.AppendLine();
			Markdown.AppendLine("Types");
			Markdown.AppendLine("--------");
			Markdown.AppendLine();
			Markdown.Append("This Web Service encodes named types as JSON dictionary ");
			Markdown.Append("objects. The following subsections list the named typed ");
			Markdown.AppendLine("and their corresponding properties.");
			Markdown.AppendLine();

			HashSet<Type> TypesToDocument2 = TypesToDocument;
			TypesToDocument = new HashSet<Type>();

			while (TypesToDocument2.Count > 0)
			{
				foreach (Type T in TypesToDocument2)
				{
					Markdown.AppendLine("<section>");
					Markdown.AppendLine();
					Markdown.Append("### `");
					Markdown.Append(T.Name);
					Markdown.AppendLine("`");
					Markdown.AppendLine();

					Markdown.AppendLine("| >>Properties<< |||");
					Markdown.AppendLine("| Name | Type | Description |");
					Markdown.AppendLine("|:-----|:-----|:------------|");

					foreach (MemberInfo Member in T.GetMembers(BindingFlags.Instance | BindingFlags.Public))
					{
						Type MemberType;

						if (Member is PropertyInfo Property)
							MemberType = Property.PropertyType;
						else if (Member is FieldInfo Field)
							MemberType = Field.FieldType;
						else
							continue;

						Markdown.Append("| `");
						Markdown.Append(Member.Name);
						Markdown.Append("` | ");
						AppendType(MemberType, Markdown, TypesToDocument);
						Markdown.Append(" | ");
						AppendCell(Notes, this.GetMemberDocumentation(Member), Markdown);
						Markdown.AppendLine(" |");
					}

					Markdown.AppendLine();
					Markdown.AppendLine("</section>");
					Markdown.AppendLine();
				}

				TypesToDocument2 = TypesToDocument;
				TypesToDocument = new HashSet<Type>();
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Appends Documentation to a Markdown document.
		/// </summary>
		/// <param name="Notes">List of notes for the Markdown table.</param>
		/// <param name="TypesToDocument">Types that need documentation.</param>
		/// <param name="Method">Protected method containing documentation.</param>
		/// <param name="Markdown">Markdown builder.</param>
		protected virtual void AppendDocumentation(ChunkedList<string> Notes,
			HashSet<Type> TypesToDocument, ProtectedMethod Method, StringBuilder Markdown)
		{
			Markdown.Append("`");
			Markdown.Append(Method.Name);
			Markdown.Append('(');

			bool First = true;
			int NrArguments = 0;

			foreach (ProtectedMethodArgumentInfo Parameter in Method.Arguments)
			{
				if (Parameter.IsSpecialArgument)
					continue;

				if (First)
					First = false;
				else
					Markdown.Append(", ");

				Markdown.Append(Parameter.Parameter.Name);
				NrArguments++;
			}

			Markdown.AppendLine(")`");
			Markdown.AppendLine();

			AppendDocumentation(Method.Documentation, Markdown);

			Markdown.AppendLine("| >>Authentication<< ||");
			Markdown.AppendLine("|:-------|:-------|");
			Markdown.Append("| Required:  | ");
			Markdown.Append(YesNo(Method.RequiresAuthentication));
			Markdown.AppendLine(" |");

			if (Method.RequiresAuthentication && Method.RequiredPrivileges.Length > 0)
			{
				Markdown.Append("| Privileges Required:  | ");
				First = true;

				foreach (string Privilege in Method.RequiredPrivileges)
				{
					if (First)
						First = false;
					else
						Markdown.Append(", ");

					Markdown.Append('`');
					Markdown.Append(Privilege);
					Markdown.Append('`');
				}

				Markdown.AppendLine(" |");
			}

			HttpAuthenticationScheme[]? Schemes = Method.AuthenticationMechanisms;
			if ((Schemes?.Length ?? 0) > 0)
			{
				Markdown.Append("| Authentication Mechanisms:  | ");
				First = true;

				foreach (HttpAuthenticationScheme Mechanism in Schemes!)
				{
					if (First)
						First = false;
					else
						Markdown.Append(", ");

					Markdown.Append(Mechanism.DisplayName);
				}

				Markdown.AppendLine(" |");
			}

			Markdown.AppendLine();

			if (NrArguments > 0)
			{
				Markdown.AppendLine("| >>Arguments<< |||||");
				Markdown.AppendLine("| Name | Type | Use | Default Value | Description |");
				Markdown.AppendLine("|:-----|:-----|:---:|:-------------:|:------------|");

				foreach (ProtectedMethodArgumentInfo Parameter in Method.Arguments)
				{
					if (Parameter.IsSpecialArgument)
						continue;

					Markdown.Append("| `");
					Markdown.Append(Parameter.Parameter.Name);
					Markdown.Append("` | ");
					AppendType(Parameter.Parameter.ParameterType, Markdown, TypesToDocument);
					Markdown.Append(" | ");

					if (Parameter.HasDefaultValue)
					{
						Markdown.Append("Optional | ");
						AppendValue(Parameter.DefaultValue, Markdown);
					}
					else
						Markdown.Append("Required | -");

					Markdown.Append(" | ");
					AppendCell(Notes, this.GetParameterDocumentation(Parameter), Markdown);
					Markdown.AppendLine(" |");
				}

				Markdown.AppendLine();
			}

			if (Method.HasReturnValue)
			{
				Markdown.AppendLine("| >>Return Value<< ||");
				Markdown.AppendLine("| Type | Description |");
				Markdown.AppendLine("|:-----|:------------|");

				Markdown.Append("| ");
				AppendType(Method.Method.ReturnType, Markdown, TypesToDocument);
				Markdown.Append(" | ");
				AppendCell(Notes, this.GetMemberDocumentation(Method.Method.ReturnParameter), Markdown);
				Markdown.AppendLine(" |");
				Markdown.AppendLine();
			}

			Markdown.AppendLine("</section>");
			Markdown.AppendLine();
		}

		/// <summary>
		/// Returns "Yes" or "No" based on the boolean value provided.
		/// </summary>
		/// <param name="Value">Boolean value</param>
		/// <returns>"Yes" if the value is true, "No" if the value is false.</returns>
		protected static string YesNo(bool Value)
		{
			return Value ? "Yes" : "No";
		}

		/// <summary>
		/// Gets parameter documentation for a method parameter.
		/// </summary>
		/// <param name="Parameter">The method parameter.</param>
		/// <returns>The documentation for the parameter.</returns>
		protected virtual KeyValuePair<bool, string>[] GetParameterDocumentation(
			ProtectedMethodArgumentInfo Parameter)
		{
			return Parameter.Documentation.Join(Parameter.AdditionalDocumentation);
		}

		/// <summary>
		/// Gets documentation for a member.
		/// </summary>
		/// <returns>The documentation for the member.</returns>
		protected virtual KeyValuePair<bool, string>[] GetMemberDocumentation(
			ICustomAttributeProvider Member)
		{
			ChunkedList<KeyValuePair<bool, string>>? PropertyDoc = null;

			foreach (object Attribute in
				Member.GetCustomAttributes(typeof(JsonRpcDocumentationAttribute), true))
			{
				if (Attribute is JsonRpcDocumentationAttribute TypedAttribute)
				{
					PropertyDoc ??= new ChunkedList<KeyValuePair<bool, string>>();
					PropertyDoc.Add(new KeyValuePair<bool, string>(
						TypedAttribute.IsMarkdown, TypedAttribute.Documentation));
				}
			}

			return PropertyDoc?.ToArray() ?? Array.Empty<KeyValuePair<bool, string>>();
		}

		/// <summary>
		/// Appends Documentation to a Markdown document.
		/// </summary>
		/// <param name="Documentation">Documentation</param>
		/// <param name="Markdown">Markdown builder.</param>
		protected static void AppendDocumentation(KeyValuePair<bool, string>[] Documentation,
			StringBuilder Markdown)
		{
			foreach (KeyValuePair<bool, string> P in Documentation)
			{
				Markdown.AppendLine();

				if (P.Key)
					Markdown.AppendLine(P.Value);
				else
					Markdown.AppendLine(MarkdownDocument.Encode(P.Value));

				Markdown.AppendLine();
			}
		}

		/// <summary>
		/// Appends a cell to the Markdown table.
		/// </summary>
		/// <param name="Notes">List of notes for the Markdown table.</param>
		/// <param name="Documentation">Documentation array for the cell.</param>
		/// <param name="Markdown">Markdown builder.</param>
		protected static void AppendCell(ChunkedList<string> Notes,
			KeyValuePair<bool, string>[] Documentation, StringBuilder Markdown)
		{
			if (IsOneRow(Documentation))
			{
				foreach (KeyValuePair<bool, string> P in Documentation)
				{
					if (P.Key)
						Markdown.Append(P.Value);
					else
						Markdown.Append(MarkdownDocument.Encode(P.Value));
				}
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				foreach (KeyValuePair<bool, string> P in Documentation)
				{
					foreach (string Row in P.Value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
					{
						if (First)
							First = false;
						else
						{
							sb.AppendLine();
							sb.Append('\t');
						}

						if (P.Key)
							sb.Append(Row);
						else
							sb.Append(MarkdownDocument.Encode(Row));
					}
				}

				Markdown.Append("[^n");
				Markdown.Append(Notes.Count);
				Markdown.Append(']');

				Notes.Add(sb.ToString());
			}
		}

		/// <summary>
		/// Outputs a value in Markdown format.
		/// </summary>
		/// <param name="Value">Value to output.</param>
		/// <param name="Markdown">Markdown builder.</param>
		protected static void AppendValue(object? Value, StringBuilder Markdown)
		{
			Markdown.Append('`');

			if (Value is null)
				Markdown.Append("null");
			else if (Value.GetType().IsEnum)
			{
				Markdown.Append('"');
				Markdown.Append(Value.ToString());
				Markdown.Append('"');
			}
			else if (Value is bool b)
				Markdown.Append(CommonTypes.Encode(b));
			else
				Markdown.Append(Expression.ToExpressionString(Value));

			Markdown.Append('`');
		}

		/// <summary>
		/// Outputs a type in Markdown format.
		/// </summary>
		/// <param name="Type">Type to output.</param>
		/// <param name="Markdown">Markdown builder.</param>
		/// <param name="TypesToDocument">Types that need documenting.</param>
		protected static void AppendType(Type Type, StringBuilder Markdown,
			HashSet<Type> TypesToDocument)
		{
			if (Type.IsGenericType)
			{
				Type GenericType = Type.GetGenericTypeDefinition();
				Type[] TypeArguments;

				if (GenericType == typeof(Task<>))
				{
					TypeArguments = Type.GetGenericArguments();
					if (TypeArguments.Length == 1)
						Type = TypeArguments[0];
				}
				else if (GenericType == typeof(Nullable<>))
				{
					TypeArguments = Type.GetGenericArguments();
					if (TypeArguments.Length == 1)
					{
						Markdown.Append("Nullable ");
						Type = TypeArguments[0];
					}
				}
			}

			while (Type.IsArray && Type != typeof(byte[]))
			{
				Markdown.Append("Array of ");
				Type = Type.GetElementType();
			}

			if (Type == typeof(byte[]))
				Markdown.Append("BASE64-encoded binary");
			if (Type == typeof(Dictionary<string, object>))
				Markdown.Append("Dictionary");
			else if (Type == typeof(string))
				Markdown.Append("String");
			else if (Type == typeof(object))
				Markdown.Append("Object");
			else if (Type == typeof(Uri))
				Markdown.Append("URI");
			else if (Type == typeof(bool))
				Markdown.Append("Boolean");
			else if (Type == typeof(int))
				Markdown.Append("32-bit signed integer");
			else if (Type == typeof(long))
				Markdown.Append("64-bit signed integer");
			else if (Type == typeof(short))
				Markdown.Append("16-bit signed integer");
			else if (Type == typeof(sbyte))
				Markdown.Append("8-bit signed integer");
			else if (Type == typeof(uint))
				Markdown.Append("32-bit unsigned integer");
			else if (Type == typeof(ulong))
				Markdown.Append("64-bit unsigned integer");
			else if (Type == typeof(ushort))
				Markdown.Append("16-bit unsigned integer");
			else if (Type == typeof(byte))
				Markdown.Append("Byte");
			else if (Type == typeof(char))
				Markdown.Append("Character");
			else if (Type == typeof(double))
				Markdown.Append("Double-precision floating-point");
			else if (Type == typeof(float))
				Markdown.Append("Single-precision floating-point");
			else if (Type == typeof(decimal))
				Markdown.Append("Decimal-precision floating-point");
			else if (Type == typeof(BigInteger))
				Markdown.Append("Big Integer");
			else if (Type == typeof(DateTime))
				Markdown.Append("Date & Time");
			else if (Type == typeof(DateTimeOffset))
				Markdown.Append("Date & Time & Time Zone");
			else if (Type == typeof(TimeSpan))
				Markdown.Append("Time span");
			else if (Type == typeof(CustomEncoding))
				Markdown.Append("Custom Encoding");
			else if (Expression.IsVoid(Type))
				Markdown.Append("`void`");
			else if (Type.IsEnum)
			{
				Markdown.Append("`");
				Markdown.Append(Type.Name);
				Markdown.Append('`');
			}
			else
			{
				Markdown.Append("[`");
				Markdown.Append(Type.Name);
				Markdown.Append("`](#");
				Markdown.Append(Type.Name[..1].ToLowerInvariant());
				Markdown.Append(Type.Name[1..]);
				Markdown.Append(')');

				TypesToDocument.Add(Type);
			}
		}

		/// <summary>
		/// Checks if a documentation array is one row or multiple rows.
		/// </summary>
		/// <param name="Documentation">Documentation array to check.</param>
		/// <returns>True if the documentation array is one row, false otherwise.</returns>
		protected static bool IsOneRow(KeyValuePair<bool, string>[] Documentation)
		{
			if (Documentation.Length > 1)
				return false;

			if (Documentation.Length == 0)
				return true;

			if (Documentation[0].Value.IndexOfAny(CommonTypes.CRLF) >= 0)
				return false;

			return true;
		}

		/// <summary>
		/// Title of JSON-RPC web service.
		/// </summary>
		public abstract string Title { get; }

		/// <summary>
		/// Short Description of JSON-RPC web service.
		/// </summary>
		public virtual string ShortDescription
		{
			get
			{
				OAuthResourceNameAttribute? Attribute = this.GetType().GetCustomAttribute<OAuthResourceNameAttribute>();
				return Attribute?.ResourceName ?? "JSON-RPC Web Service: " + this.ResourceName;
			}
		}

		/// <summary>
		/// Markdown description of web service.
		/// </summary>
		public abstract string MarkdownDescription { get; }

		/// <summary>
		/// Unregisters an existing SSE subscription for a session, if any.
		/// </summary>
		/// <param name="Session">Session object to unregister.</param>
		protected bool Unregister(IJsonRpcSession? Session)
		{
			if (Session is null)
				return false;

			lock (this.eventSubscriptions)
			{
				foreach (Subscription Subscription in this.eventSubscriptions)
				{
					if (Subscription.Session == Session)
					{
						this.eventSubscriptions.Remove(Subscription);
						this.eventSubscriptionsStatic = this.eventSubscriptions.ToArray();
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Tries to get a session object for the resource, if any.
		/// </summary>
		/// <param name="Request">HTTP Request object</param>
		/// <param name="Response">HTTP Response object</param>
		/// <returns>Session object, if any.</returns>
		protected virtual Task<IJsonRpcSession?> TryGetSession(HttpRequest Request, HttpResponse Response)
		{
			return Task.FromResult<IJsonRpcSession?>(null);
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public virtual async Task POST(HttpRequest Request, HttpResponse Response)
		{
			using JsonRpcServerRequest JsonRpcRequest = new JsonRpcServerRequest();

			if (!Request.HasData)
			{
				JsonRpcRequest.SetError(-32600, "No payload.",
					BadRequestException.Code, BadRequestException.StatusMessage);
			}
			else
			{
				ContentResponse RequestData = await Request.DecodeDataAsync();

				if (RequestData.HasError)
				{
					JsonRpcRequest.SetError(-32700, "Unable to parse payload.",
						InternalServerErrorException.Code, InternalServerErrorException.StatusMessage);
				}
				else if (RequestData.Decoded is Dictionary<string, object> RequestObj)
				{
					foreach (KeyValuePair<string, object> P in RequestObj)
						this.ProcessQueryParameter(JsonRpcRequest, P.Key, P.Value);
				}
				else if (RequestData.Decoded is Array Requests)
				{
					int i, c = Requests.Length;

					if (c == 0)
					{
						JsonRpcRequest.SetError(-32600, "Empty request.",
							BadRequestException.Code, BadRequestException.StatusMessage);
					}
					else
					{
						JsonRpcRequest.BatchRequests = new JsonRpcServerRequest[c];

						for (i = 0; i < c; i++)
						{
							JsonRpcServerRequest ItemRequest = new JsonRpcServerRequest();
							JsonRpcRequest.BatchRequests[i] = ItemRequest;

							if (Requests.GetValue(i) is Dictionary<string, object> ItemRequestObj)
							{
								foreach (KeyValuePair<string, object> P in ItemRequestObj)
									this.ProcessQueryParameter(ItemRequest, P.Key, P.Value);
							}
							else
							{
								ItemRequest.SetError(-32600, "Expected JSON object or array of JSON objects in request.",
									BadRequestException.Code, BadRequestException.StatusMessage);
							}
						}
					}
				}
				else
				{
					JsonRpcRequest.SetError(-32600, "Expected JSON object or array of JSON objects in request.",
						BadRequestException.Code, BadRequestException.StatusMessage);
				}
			}

			if (!await JsonRpcRequest.BuildResponse(this, Request, Response))
				await this.SendResponse(Request, JsonRpcRequest, Response);
		}

		private async Task SendResponse(HttpRequest HttpRequest,
			JsonRpcServerRequest JsonRequest, HttpResponse Response)
		{
			if (JsonRequest.StatusCode == 204)
			{
				Response.StatusCode = JsonRequest.StatusCode;
				Response.StatusMessage = JsonRequest.StatusMessage;
			}
			else if (JsonRequest.IsResult)
			{
				Response.StatusCode = 200;
				Response.StatusMessage = "OK";
			}
			else
			{
				ContentResponse Encoded;

				if (HttpRequest.Header.Accept.IsAcceptable(JsonCodec.JsonRpcContentType))
				{
					Encoded = await jsonCodec.EncodeAsync(JsonRequest.Response,
						Encoding.UTF8, null, JsonCodec.JsonRpcContentType);
				}
				else
				{
					string ContentType = HttpRequest.Header.Accept.GetBestAlternative(JsonCodec.JsonContentTypes);

					Encoded = await jsonCodec.EncodeAsync(JsonRequest.Response,
						Encoding.UTF8, null, ContentType);
				}

				if (Encoded.HasError)
				{
					await Response.SendResponse(Encoded.Error);
					return;
				}
				else
				{
					Response.StatusCode = JsonRequest.StatusCode;
					Response.StatusMessage = JsonRequest.StatusMessage;

					if (JsonRequest.StatusCode != 204)
					{
						Response.ContentType = Encoded.ContentType;

						await Response.Write(true, Encoded.Encoded, 0, Encoded.Encoded.Length);
					}
				}
			}

			await Response.SendResponse();
		}

		private void ProcessQueryParameter(JsonRpcServerRequest Request, string Key, object Value)
		{
			switch (Key)
			{
				case "jsonrpc":
					Request.JsonVersion = Value?.ToString() ?? string.Empty;
					break;

				case "method":
					if (Value is string Method)
					{
						lock (this.methods)
						{
							if (!this.methods.TryGetValue(Method.Replace('/', '_'), out Request.MethodInfo))
								Request.SetError(-32601, "Method not found: " + Method,
									NotFoundException.Code, NotFoundException.StatusMessage);
						}
					}
					else
					{
						Request.SetError(-32600, "Invalid method name.",
							BadRequestException.Code, BadRequestException.StatusMessage);
					}
					break;

				case "result":
					Request.IsResult = true;
					Request.Result = Value;
					break;

				case "params":
					if (Value is Dictionary<string, object?> Obj)
						Request.ParametersObj = Obj;
					else if (Value is Array A)
						Request.ParametersArray = A;
					else
					{
						Request.SetError(-32600, "Invalid parameters.",
							BadRequestException.Code, BadRequestException.StatusMessage);
					}
					break;

				case "id":
					Request.Id = Value;
					break;

				case "error":
					Request.IsError = true;

					if (Value is Dictionary<string, object> Error &&
						Error.TryGetValue("code", out object Obj2) &&
						Obj2 is int ErrorCode &&
						Error.TryGetValue("message", out Obj2) &&
						Obj2 is string ErrorMessage)
					{
						Request.SetError(ErrorCode, ErrorMessage,
							ServiceUnavailableException.Code,
							ServiceUnavailableException.StatusMessage);
					}
					else
					{
						Request.SetError(-32600, Value.ToString(),
							ServiceUnavailableException.Code,
							ServiceUnavailableException.StatusMessage);
					}
					break;

				default:
					Request.SetError(-32600, "Unexpected request received: Unknown property: " + Key,
						BadRequestException.Code, BadRequestException.StatusMessage);
					break;
			}
		}

	}
}
