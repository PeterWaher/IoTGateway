using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Content.Json;
using Waher.Networking.HTTP.JsonRpc.Exceptions;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Networking.HTTP.JsonRpc
{
	// TODO: Custom authentication (e.g. Bearer JWT, WWW-Authenticate)

	/// <summary>
	/// A JSON-RPC client
	/// </summary>
	public class JsonRpcClient : CommunicationLayer, ITlsCertificateEndpoint
	{
		private X509Certificate? certificate;
		private readonly Uri endpoint;
		private long id;

		/// <summary>
		/// A JSON-RPC client
		/// </summary>
		/// <param name="Endpoint">JSON-RPC endpoint</param>
		/// <param name="Sniffers">Sniffers</param>
		public JsonRpcClient(string Endpoint, params ISniffer[] Sniffers)
			: this(new Uri(Endpoint), null, Sniffers)
		{
		}

		/// <summary>
		/// A JSON-RPC client
		/// </summary>
		/// <param name="Endpoint">JSON-RPC endpoint</param>
		/// <param name="Sniffers">Sniffers</param>
		public JsonRpcClient(Uri Endpoint, params ISniffer[] Sniffers)
			: this(Endpoint, null, Sniffers)
		{
			this.endpoint = Endpoint;
		}

		/// <summary>
		/// A JSON-RPC client
		/// </summary>
		/// <param name="Endpoint">JSON-RPC endpoint</param>
		/// <param name="Certificate">Client certificate for mTLS authentication.</param>
		/// <param name="Sniffers">Sniffers</param>
		public JsonRpcClient(string Endpoint, X509Certificate? Certificate, params ISniffer[] Sniffers)
			: this(new Uri(Endpoint), Certificate, Sniffers)
		{
		}

		/// <summary>
		/// A JSON-RPC client
		/// </summary>
		/// <param name="Endpoint">JSON-RPC endpoint</param>
		/// <param name="Certificate">Client certificate for mTLS authentication.</param>
		/// <param name="Sniffers">Sniffers</param>
		public JsonRpcClient(Uri Endpoint, X509Certificate? Certificate, params ISniffer[] Sniffers)
			: base(true, Sniffers)
		{
			this.endpoint = Endpoint;
			this.certificate = Certificate;
		}

		/// <summary>
		/// Updates the certificate used in mTLS negotiation.
		/// </summary>
		/// <param name="Certificate">Updated Certificate</param>
		public void UpdateCertificate(X509Certificate Certificate)
		{
			this.certificate = Certificate;
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(string Method, Dictionary<string, object> Parameters)
		{
			return this.Request(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2,
				Method, Parameters);
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="Method">Method to call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(string Method)
		{
			return this.Request(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, Method);
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(string Method, IEnumerable Parameters)
		{
			return this.Request(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2,
				Method, Parameters);
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(JsonRpcHttpMethod HttpMethod, string Method,
			Dictionary<string, object> Parameters)
		{
			return this.Request(HttpMethod, JsonRpcVersion.JsonRpcV2, Method, Parameters);
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Method">Method to call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(JsonRpcHttpMethod HttpMethod, string Method)
		{
			return this.Request(HttpMethod, JsonRpcVersion.JsonRpcV2, Method);
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(JsonRpcHttpMethod HttpMethod, string Method,
			IEnumerable Parameters)
		{
			return this.Request(HttpMethod, JsonRpcVersion.JsonRpcV2, Method, Parameters);
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(JsonRpcHttpMethod HttpMethod, JsonRpcVersion Version,
			string Method, Dictionary<string, object> Parameters)
		{
			return this.Request(HttpMethod, new JsonRpcRequest(Version, Method, Parameters));
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Method to call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(JsonRpcHttpMethod HttpMethod, JsonRpcVersion Version,
			string Method)
		{
			return this.Request(HttpMethod, new JsonRpcRequest(Version, Method));
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		/// <returns>Result</returns>
		public Task<object?> Request(JsonRpcHttpMethod HttpMethod, JsonRpcVersion Version,
			string Method, IEnumerable Parameters)
		{
			return this.Request(HttpMethod, new JsonRpcRequest(Version, Method, Parameters));
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>Result</returns>
		public Task<object?> Request(JsonRpcRequest Request)
		{
			return this.Request(JsonRpcHttpMethod.POST, Request);
		}

		/// <summary>
		/// Performs a JSON-RPC request.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Request">Request object.</param>
		/// <returns>Result</returns>
		public async Task<object?> Request(JsonRpcHttpMethod HttpMethod, JsonRpcRequest Request)
		{
			Dictionary<string, object> Payload = Request.BuildRequest(this.id++);
			ContentResponse Response = await this.SendNotification(HttpMethod, Payload);
			Response.AssertOk();

			if (!(Response.Decoded is Dictionary<string, object> JsonResponse))
			{
				throw new JsonRpcException("Unexpected response received of type " +
					Response.Decoded?.GetType().FullName);
			}

			JsonRpcResult Result = new JsonRpcResult(JsonResponse);

			if (!Result.HasResult)
				throw new JsonRpcException("Unexpected response received: Result property missing.");

			if (!Result.Id.HasValue)
				throw new JsonRpcException("Unexpected response received: Id property missing.");

			if (Payload.TryGetValue("id", out object PrevIdObj) &&
				PrevIdObj is long PrevId &&
				PrevId != Result.Id.Value)
			{
				throw new JsonRpcException("Response did not match request.");
			}

			return Result.Result;
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		public Task Notify(string Method, Dictionary<string, object> Parameters)
		{
			return this.Notify(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2,
				Method, Parameters);
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="Method">Method to call</param>
		public Task Notify(string Method)
		{
			return this.Notify(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2, Method);
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		public Task Notify(string Method, IEnumerable Parameters)
		{
			return this.Notify(JsonRpcHttpMethod.POST, JsonRpcVersion.JsonRpcV2,
				Method, Parameters);
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		public Task Notify(JsonRpcHttpMethod HttpMethod, string Method,
			Dictionary<string, object> Parameters)
		{
			return this.Notify(HttpMethod, JsonRpcVersion.JsonRpcV2, Method, Parameters);
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Method">Method to call</param>
		public Task Notify(JsonRpcHttpMethod HttpMethod, string Method)
		{
			return this.Notify(HttpMethod, JsonRpcVersion.JsonRpcV2, Method);
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		public Task Notify(JsonRpcHttpMethod HttpMethod, string Method,
			IEnumerable Parameters)
		{
			return this.Notify(HttpMethod, JsonRpcVersion.JsonRpcV2, Method, Parameters);
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		public Task Notify(JsonRpcHttpMethod HttpMethod, JsonRpcVersion Version,
			string Method, Dictionary<string, object> Parameters)
		{
			return this.Notify(HttpMethod, new JsonRpcRequest(Version, Method, Parameters));
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Method to call</param>
		public Task Notify(JsonRpcHttpMethod HttpMethod, JsonRpcVersion Version,
			string Method)
		{
			return this.Notify(HttpMethod, new JsonRpcRequest(Version, Method));
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		public Task Notify(JsonRpcHttpMethod HttpMethod,
			JsonRpcVersion Version, string Method, IEnumerable Parameters)
		{
			return this.Notify(HttpMethod, new JsonRpcRequest(Version, Method, Parameters));
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="Request">Request object.</param>
		public Task Notify(JsonRpcRequest Request)
		{
			return this.Notify(JsonRpcHttpMethod.POST, Request);
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Request">Request object.</param>
		public async Task Notify(JsonRpcHttpMethod HttpMethod, JsonRpcRequest Request)
		{
			ContentResponse Response = await this.SendNotification(HttpMethod, Request.BuildRequest(null));
			Response.AssertOk();
		}

		private async Task<ContentResponse> SendNotification(JsonRpcHttpMethod HttpMethod,
			Dictionary<string, object> Payload)
		{
			if (this.HasSniffers)
				this.TransmitText(JSON.Encode(Payload, true));

			ContentResponse Response;

			if (HttpMethod == JsonRpcHttpMethod.GET)
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				sb.Append(this.endpoint.OriginalString);

				foreach (KeyValuePair<string, object> P in Payload)
				{
					if (First)
					{
						First = false;
						sb.Append('?');
					}
					else
						sb.Append('&');

					sb.Append(P.Key);
					sb.Append('=');

					if (P.Value is string s)
						sb.Append(System.Net.WebUtility.UrlEncode(s));
					else
						sb.Append(System.Net.WebUtility.UrlEncode(JSON.Encode(P.Value, false)));
				}

				Response = await InternetContent.GetAsync(
					new Uri(sb.ToString()), this.certificate,
					new KeyValuePair<string, string>("Accept", JsonCodec.JsonRpcContentType));
			}
			else
			{
				Response = await InternetContent.PostAsync(
					this.endpoint, Payload, this.certificate,
					new KeyValuePair<string, string>("Accept", JsonCodec.JsonRpcContentType));
			}

			if (Response.HasError)
			{
				if (!(Response.Error is WebException ex) || ex.Content is null)
				{
					if (this.HasSniffers)
						this.Error(Response.Error.Message);
				}
				else
				{
					if (this.HasSniffers)
						this.Error(JSON.Encode(ex.Content, true));

					Response = this.CheckJsonRpcError(Response);
				}
			}
			else if (this.HasSniffers)
				this.ReceiveText(JSON.Encode(Response.Decoded, true));

			return Response;
		}

		private ContentResponse CheckJsonRpcError(ContentResponse Response)
		{
			if (Response.HasError &&
				Response.Error is WebException ex &&
				ex.Content is Dictionary<string, object> Result &&
				Result.TryGetValue("error", out object ErrorObj) &&
				ErrorObj is Dictionary<string, object> Error &&
				Error.TryGetValue("code", out object Obj) && Obj is int ErrorCode &&
				Error.TryGetValue("message", out Obj) && Obj is string ErrorMessage)
			{
				return new ContentResponse(GetError(ErrorCode, ErrorMessage));
			}
			else
				return Response;
		}

		internal static JsonRpcError GetError(int ErrorCode, string ErrorMessage)
		{
			switch (ErrorCode)
			{
				case -32700:
					return new JsonRpcParseError(ErrorCode, ErrorMessage, null);

				case -32600:
					return new JsonRpcInvalidRequestError(ErrorCode, ErrorMessage, null);

				case -32601:
					return new JsonRpcMethodNotFoundError(ErrorCode, ErrorMessage, null);

				case -32602:
					return new JsonRpcInvalidParametersError(ErrorCode, ErrorMessage, null);

				case -32603:
					return new JsonRpcInternalError(ErrorCode, ErrorMessage, null);

				default:
					if (ErrorCode <= -32000 && ErrorCode >= -32099)
						return new JsonRpcServerError(ErrorCode, ErrorMessage, null);
					else
						return new JsonRpcError(ErrorCode, ErrorMessage, null);
			}
		}

		/// <summary>
		/// Processes a batch of requests.
		/// </summary>
		/// <param name="Requests">Request objects.</param>
		/// <returns>Responses to requests. Only requests return responses. Notifications
		/// do not return responses.</returns>
		public async Task<JsonRpcResult[]> BatchProcess(params JsonRpcRequest[] Requests)
		{
			int i, c = Requests.Length;
			Dictionary<string, object>[] Payloads = new Dictionary<string, object>[c];

			for (i = 0; i < c; i++)
			{
				JsonRpcRequest Request = Requests[i];
				Payloads[i] = Request.BuildRequest(Request.IsRequest ? this.id++ : (long?)null);
			}

			if (this.HasSniffers)
				this.TransmitText(JSON.Encode(Payloads, true));

			ContentResponse Response = await InternetContent.PostAsync(
				this.endpoint, Payloads, this.certificate,
				new KeyValuePair<string, string>("Accept", JsonCodec.JsonRpcContentType));

			if (this.HasSniffers)
			{
				if (Response.HasError)
				{
					if (!(Response.Error is WebException ex) || ex.Content is null)
						this.Error(Response.Error.Message);
					else
						this.Error(JSON.Encode(ex.Content, true));
				}
				else
					this.ReceiveText(JSON.Encode(Response.Decoded, true));
			}

			Response = this.CheckJsonRpcError(Response);
			Response.AssertOk();

			if (!(Response.Decoded is Array A))
			{
				throw new JsonRpcException("Unexpected response received of type " +
					Response.Decoded?.GetType().FullName);
			}

			c = A.Length;
			JsonRpcResult[] Results = new JsonRpcResult[c];

			for (i = 0; i < c; i++)
			{
				if (!(A.GetValue(i) is Dictionary<string, object> ItemResponse))
				{
					throw new JsonRpcException("Unexpected item response received of type " +
						A.GetValue(i)?.GetType().FullName);
				}

				Results[i] = new JsonRpcResult(ItemResponse);
			}

			return Results;
		}
	}
}
