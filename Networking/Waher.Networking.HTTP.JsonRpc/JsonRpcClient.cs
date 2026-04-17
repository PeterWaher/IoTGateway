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
	// TODO: Batch calls

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
			return this.DoRequest(HttpMethod, this.BuildRequest(Version, Method, Parameters, this.id++));
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
			return this.DoRequest(HttpMethod, this.BuildRequest(Version, Method, null, this.id++));
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
			return this.DoRequest(HttpMethod, this.BuildRequest(Version, Method, Parameters, this.id++));
		}

		private async Task<object?> DoRequest(JsonRpcHttpMethod HttpMethod,
			Dictionary<string, object> Payload)
		{
			ContentResponse Response = await this.DoNotification(HttpMethod, Payload);
			Response.AssertOk();

			if (!(Response.Decoded is Dictionary<string, object> JsonResponse))
			{
				throw new JsonRpcException("Unexpected response received of type " +
					Response.Decoded?.GetType().FullName);
			}

			object? Result = null;
			long? Id = null;
			bool HasResult = false;

			foreach (KeyValuePair<string, object> P in JsonResponse)
			{
				switch (P.Key)
				{
					case "jsonrpc":
						break;

					case "result":
						Result = P.Value;
						HasResult = true;
						break;

					case "error":	// Processed in DoNotification
						break;

					case "id":
						if (P.Value is long l)
							Id = l;
						else if (P.Value is int i)
							Id = i;
						else if (P.Value is string s && long.TryParse(s, out l))
							Id = l;
						else
							throw new JsonRpcException("Unexpected response received: Id property is not a valid integer.");
						break;

					default:
						throw new JsonRpcException("Unexpected response received: Unknown property: " + P.Key);
				}
			}

			if (!HasResult)
				throw new JsonRpcException("Unexpected response received: Result property missing.");

			if (!Id.HasValue)
				throw new JsonRpcException("Unexpected response received: Id property missing.");

			if (Payload.TryGetValue("id", out object PrevIdObj) &&
				PrevIdObj is long PrevId &&
				PrevId != Id.Value)
			{
				throw new JsonRpcException("Response did not match request.");
			}

			return Result;
		}

		private Dictionary<string, object> BuildRequest(JsonRpcVersion Version, string Method,
			object? Parameters, long? Id)
		{
			Dictionary<string, object> Request = new Dictionary<string, object>();

			if (Version == JsonRpcVersion.JsonRpcV2)
				Request["jsonrpc"] = "2.0";

			Request["method"] = Method;

			if (!(Parameters is null))
				Request["params"] = Parameters;

			if (Id.HasValue)
				Request["id"] = Id.Value;

			return Request;
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
		public async Task Notify(JsonRpcHttpMethod HttpMethod, JsonRpcVersion Version,
			string Method, Dictionary<string, object> Parameters)
		{
			ContentResponse Response = await this.DoNotification(HttpMethod, this.BuildRequest(Version, Method, Parameters, null));
			Response.AssertOk();
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Method to call</param>
		public async Task Notify(JsonRpcHttpMethod HttpMethod, JsonRpcVersion Version,
			string Method)
		{
			ContentResponse Response = await this.DoNotification(HttpMethod, this.BuildRequest(Version, Method, null, null));
			Response.AssertOk();
		}

		/// <summary>
		/// Sends a JSON-RPC notification.
		/// </summary>
		/// <param name="HttpMethod">HTTP Method</param>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Method to call</param>
		/// <param name="Parameters">Parameters of method call</param>
		public async Task Notify(JsonRpcHttpMethod HttpMethod,
			JsonRpcVersion Version, string Method, IEnumerable Parameters)
		{
			ContentResponse Response = await this.DoNotification(HttpMethod, this.BuildRequest(Version, Method, Parameters, null));
			Response.AssertOk();
		}

		private async Task<ContentResponse> DoNotification(JsonRpcHttpMethod HttpMethod,
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

					if (ex.Content is Dictionary<string, object> Result &&
						Result.TryGetValue("error", out object ErrorObj) &&
						ErrorObj is Dictionary<string, object> Error &&
						Error.TryGetValue("code", out object Obj) && Obj is int ErrorCode &&
						Error.TryGetValue("message", out Obj) && Obj is string ErrorMessage)
					{
						switch (ErrorCode)
						{
							case -32700:
								Response = new ContentResponse(new JsonRpcParseError(ErrorCode, ErrorMessage, null));
								break;

							case -32600:
								Response = new ContentResponse(new JsonRpcInvalidRequestError(ErrorCode, ErrorMessage, null));
								break;

							case -32601:
								Response = new ContentResponse(new JsonRpcMethodNotFoundError(ErrorCode, ErrorMessage, null));
								break;

							case -32602:
								Response = new ContentResponse(new JsonRpcInvalidParametersError(ErrorCode, ErrorMessage, null));
								break;

							case -32603:
								Response = new ContentResponse(new JsonRpcInternalError(ErrorCode, ErrorMessage, null));
								break;

							default:
								if (ErrorCode <= -32000 && ErrorCode >= -32099)
									Response = new ContentResponse(new JsonRpcServerError(ErrorCode, ErrorMessage, null));
								else
									Response = new ContentResponse(new JsonRpcError(ErrorCode, ErrorMessage, null));
								break;
						}
					}
				}
			}
			else if (this.HasSniffers)
				this.ReceiveText(JSON.Encode(Response.Decoded, true));

			return Response;
		}
	}
}
