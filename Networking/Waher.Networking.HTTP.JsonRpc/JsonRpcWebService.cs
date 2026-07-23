using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Json;
using Waher.Events;
using Waher.Networking.HTTP.OAuth;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
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

		private readonly Dictionary<string, JsonRpcMethodInfo> methods;
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
				this.methods = new Dictionary<string, JsonRpcMethodInfo>(StringComparer.InvariantCulture);
			else
				this.methods = new Dictionary<string, JsonRpcMethodInfo>(StringComparer.InvariantCultureIgnoreCase);

			foreach (MethodInfo Method in this.GetType().GetMethods(BindingFlags.Instance |
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (Method.GetCustomAttribute<JsonRpcMethodAttribute>() is null)
					continue;

				this.Register(Method, GetRequiredPrivileges(Method));
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
		public void Register(MethodInfo Method, params string[]? RequiredPrivileges)
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
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			if (Request.Header.IsAcceptable("text/event-stream"))
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

				lock (this.eventSubscriptions)
				{
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
								P.Key + ": " + ex.Message,
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
		public async Task POST(HttpRequest Request, HttpResponse Response)
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

				default:
					Request.SetError(-32600, "Unexpected request received: Unknown property: " + Key,
						BadRequestException.Code, BadRequestException.StatusMessage);
					break;
			}
		}

	}
}
