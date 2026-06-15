using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Json;
using Waher.Events;
using Waher.Runtime.Collections;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Web Service interface based on JSON-RPC v2.0.
	/// 
	/// Ref:
	/// https://www.jsonrpc.org/specification
	/// https://www.jsonrpc.org/historical/json-rpc-over-http.html
	/// </summary>
	public class JsonRpcWebService : HttpAsynchronousResource, IHttpGetMethod, IHttpPostMethod
	{
		private static readonly JsonCodec jsonCodec = new JsonCodec();

		private readonly Dictionary<string, JsonRpcMethodInfo> methods;
		private readonly bool userSessions;
		private readonly bool caseSensitive;

		/// <summary>
		/// Web Service interface based on JSON-RPC v2.0.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="Methods">Methods to register with the web service interface.</param>
		public JsonRpcWebService(string ResourceName, bool UserSessions,
			params Delegate[] Methods)
			: this(ResourceName, UserSessions, true, Methods)
		{
		}

		/// <summary>
		/// Web Service interface based on JSON-RPC v2.0.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="CaseSensitive">If names are case-sensitive.</param>
		/// <param name="Methods">Methods to register with the web service interface.</param>
		public JsonRpcWebService(string ResourceName, bool UserSessions, bool CaseSensitive,
			params Delegate[] Methods)
			: base(ResourceName)
		{
			this.userSessions = UserSessions;
			this.caseSensitive = CaseSensitive;

			if (CaseSensitive)
				this.methods = new Dictionary<string, JsonRpcMethodInfo>(StringComparer.InvariantCulture);
			else
				this.methods = new Dictionary<string, JsonRpcMethodInfo>(StringComparer.InvariantCultureIgnoreCase);

			if (!(Methods is null))
			{
				foreach (Delegate Method in Methods)
					this.Register(Method);
			}
		}

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
		/// Registers a method to be used in the JSON-RPC interface. 
		/// </summary>
		/// <param name="Method">Method to register.</param>
		public void Register(Delegate Method)
		{
			lock (this.methods)
			{
				string Name = Method.Method.Name;

				if (this.methods.ContainsKey(Name))
					throw new Exception("Method already registered: " + Name);

				this.methods[Name] = new JsonRpcMethodInfo(Method, this.caseSensitive);
			}
		}

		/// <summary>
		/// Unregisters a method from the JSON-RPC interface.
		/// </summary>
		/// <param name="Method">Method to unregister.</param>
		/// <returns>True if the method was successfully unregistered, false otherwise.</returns>
		public bool Unregister(Delegate Method)
		{
			lock (this.methods)
			{
				string Name = Method.Method.Name;

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
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(IDictionary<string, object> Fields)
		{
			return this.SendEvent(null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(params KeyValuePair<string, object>[] Fields)
		{
			return this.SendEvent(null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(IEnumerable<KeyValuePair<string, object>> Fields)
		{
			return this.SendEvent(null, Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(string? Comment, IDictionary<string, object> Fields)
		{
			return this.SendEvent(Comment, (IEnumerable<KeyValuePair<string, object>>)Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(string? Comment, params KeyValuePair<string, object>[] Fields)
		{
			return this.SendEvent(Comment, (IEnumerable<KeyValuePair<string, object>>)Fields);
		}

		/// <summary>
		/// Sends an event to clients with open subscriptions.
		/// </summary>
		/// <param name="Comment">Optional comment.</param>
		/// <param name="Fields">Fields to emit.</param>
		/// <returns>Number of clients the event was forwarded to.</returns>
		public Task<int> SendEvent(string? Comment, IEnumerable<KeyValuePair<string, object>> Fields)
		{
			if (!this.SupportsServerSentEvents)
				throw new InvalidOperationException("Server-Sent Events (SSE) not supported by this resource.");

			StringBuilder sb = new StringBuilder();

			if (!string.IsNullOrEmpty(Comment))
			{
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

			foreach (KeyValuePair<string, object> P in Fields)
			{
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

			sb.Append("\r\n");

			return this.SendEvent(sb.ToString());
		}

		private async Task<int> SendEvent(string Event)
		{ 
			int Count = 0;

			foreach (HttpResponse Response in this.eventSubscriptionsStatic)
			{
				try
				{
					await Response.Write(Event);
					Count++;
				}
				catch (Exception)
				{
					lock (this.eventSubscriptions)
					{
						this.eventSubscriptions.Remove(Response);
						this.eventSubscriptionsStatic = this.eventSubscriptions.ToArray();
					}
				}
			}

			return Count;
		}

		private readonly ChunkedList<HttpResponse> eventSubscriptions = new ChunkedList<HttpResponse>();
		private HttpResponse[] eventSubscriptionsStatic = Array.Empty<HttpResponse>();
		private bool eventSubscriptionsKeepAliveRunning = false;

		private async void KeepEventSubscrptionsAlive()
		{
			try
			{
				do
				{
					await Task.Delay(15000);    // Keep alive every 15 seconds.
				}
				while (await this.SendEvent(":\r\n") > 0);
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

				Response.StatusCode = 200;
				Response.ContentType = "text/event-stream";
				Response.EnableDirectTransfer();
				await Response.WriteLine(":");

				lock (this.eventSubscriptions)
				{
					this.eventSubscriptions.Add(Response);
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
					string s = System.Net.WebUtility.UrlDecode(P.Value);
					object? Value;

					if (P.Key == "params")
					{
						try
						{
							Value = JSON.Parse(s);
						}
						catch (Exception ex)
						{
							JsonRpcRequest.SetError(-32700, "Unable to parse parameter: " + P.Key + ": " + ex.Message, 500);
							continue;
						}
					}
					else
						Value = P.Value;

					this.ProcessQueryParameter(JsonRpcRequest, P.Key, Value);
				}
			}

			await JsonRpcRequest.BuildResponse();
			await this.SendResponse(Request, JsonRpcRequest, Response);
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
				JsonRpcRequest.SetError(-32600, "No payload.", 400);
			else
			{
				ContentResponse RequestData = await Request.DecodeDataAsync();

				if (RequestData.HasError)
					JsonRpcRequest.SetError(-32700, "Unable to parse payload.", 500);
				else if (RequestData.Decoded is Dictionary<string, object> RequestObj)
				{
					foreach (KeyValuePair<string, object> P in RequestObj)
						this.ProcessQueryParameter(JsonRpcRequest, P.Key, P.Value);
				}
				else if (RequestData.Decoded is Array Requests)
				{
					int i, c = Requests.Length;

					if (c == 0)
						JsonRpcRequest.SetError(-32600, "Empty request.", 400);
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
								ItemRequest.SetError(-32600, "Expected JSON object or array of JSON objects in request.", 400);
						}
					}
				}
				else
					JsonRpcRequest.SetError(-32600, "Expected JSON object or array of JSON objects in request.", 400);
			}

			await JsonRpcRequest.BuildResponse();
			await this.SendResponse(Request, JsonRpcRequest, Response);
		}

		private async Task SendResponse(HttpRequest HttpRequest,
			JsonRpcServerRequest JsonRequest, HttpResponse Response)
		{
			if (JsonRequest.StatusCode == 204)
				Response.StatusCode = JsonRequest.StatusCode;
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
							if (!this.methods.TryGetValue(Method, out Request.MethodInfo))
								Request.SetError(-32601, "Method not found: " + Method, 404);
						}
					}
					else
						Request.SetError(-32600, "Invalid method name.", 400);
					break;

				case "params":
					if (Value is Dictionary<string, object> Obj)
						Request.ParametersObj = Obj;
					else if (Value is Array A)
						Request.ParametersArray = A;
					else
						Request.SetError(-32600, "Invalid parameters.", 400);
					break;

				case "id":
					Request.Id = Value;
					break;

				default:
					Request.SetError(-32600, "Unexpected request received: Unknown property: " + Key, 400);
					break;
			}
		}

	}
}
