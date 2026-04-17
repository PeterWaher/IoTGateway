using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Json;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Web Service interface based on JSON-RPC v2.0.
	/// 
	/// Ref:
	/// https://www.jsonrpc.org/specification
	/// https://www.jsonrpc.org/historical/json-rpc-over-http.html
	/// </summary>
	public class JsonRpcWebService : HttpSynchronousResource, IHttpGetMethod, IHttpPostMethod
	{
		private static readonly JsonCodec jsonCodec = new JsonCodec();

		private readonly Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
		private readonly bool userSessions;

		/// <summary>
		/// Web Service interface based on JSON-RPC v2.0.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="UserSessions">If the resource uses user sessions.</param>
		/// <param name="Methods">Methods to register with the web service interface.</param>
		public JsonRpcWebService(string ResourceName, bool UserSessions,
			params Delegate[] Methods)
			: base(ResourceName)
		{
			this.userSessions = UserSessions;

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

				this.methods[Name] = new MethodInfo(Method);
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

				if (this.methods.TryGetValue(Name, out MethodInfo Prev) &&
					Prev.Method == Method)
				{
					return this.methods.Remove(Name);
				}
				else
					return false;
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
			using JsonRpcRequest JsonRpcRequest = new JsonRpcRequest();

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

			await JsonRpcRequest.BuildResponse();
			await this.SendResponse(JsonRpcRequest, Response);
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			using JsonRpcRequest JsonRpcRequest = new JsonRpcRequest();

			if (!Request.HasData)
				JsonRpcRequest.SetError(-32600, "No payload.", 400);
			else
			{
				ContentResponse RequestData = await Request.DecodeDataAsync();
				if (RequestData.HasError)
					JsonRpcRequest.SetError(-32700, "Unable to parse payload.", 500);
				else if (!(RequestData.Decoded is Dictionary<string, object> RequestObj))
					JsonRpcRequest.SetError(-32600, "Expected JSON object in request.", 400);
				else
				{
					foreach (KeyValuePair<string, object> P in RequestObj)
						this.ProcessQueryParameter(JsonRpcRequest, P.Key, P.Value);
				}
			}

			await JsonRpcRequest.BuildResponse();
			await this.SendResponse(JsonRpcRequest, Response);
		}

		private async Task SendResponse(JsonRpcRequest Request, HttpResponse Response)
		{
			if (Request.StatusCode == 204)
				Response.StatusCode = Request.StatusCode;
			else
			{
				ContentResponse Encoded = await jsonCodec.EncodeAsync(Request.ResponseObj,
					Encoding.UTF8, null, JsonCodec.JsonRpcContentType);

				if (Encoded.HasError)
				{
					await Response.SendResponse(Encoded.Error);
					return;
				}
				else
				{
					Response.StatusCode = Request.StatusCode;

					if (Request.StatusCode != 204)
					{
						Response.ContentType = JsonCodec.JsonRpcContentType;

						await Response.Write(true, Encoded.Encoded, 0, Encoded.Encoded.Length);
					}
				}
			}

			await Response.SendResponse();
		}

		private void ProcessQueryParameter(JsonRpcRequest Request, string Key, object Value)
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
								Request.SetError(-32601, "Method not found.", 404);
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
					if (Request.StatusCode == 204)
						Request.StatusCode = 200;
					break;

				default:
					Request.SetError(-32600, "Unexpected request received: Unknown property: " + Key, 400);
					break;
			}
		}

	}
}
