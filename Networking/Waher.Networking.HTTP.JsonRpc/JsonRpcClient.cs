using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
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

			if (!(Response.Decoded is Dictionary<string, object> JsonResponse))
			{
				Response.AssertOk();

				throw new JsonRpcException("Unexpected response received of type " +
					Response.Decoded?.GetType().FullName);
			}

			object? Result = null;
			object? Error = null;
			long? Id = null;
			bool HasResult = false;
			bool HasError = false;

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

					case "error":
						Error = P.Value;
						HasError = !(Error is null);
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

			if (HasError)
			{
				if (Error is Dictionary<string, object> ErrorObj)
				{
					object? Code = null;
					object? Message = null;
					object? Data = null;

					foreach (KeyValuePair<string, object> P in ErrorObj)
					{
						switch (P.Key)
						{
							case "code":
								Code = P.Value;
								break;

							case "message":
								Message = P.Value;
								break;

							case "data":
								Data = P.Value;
								break;

							default:
								throw new JsonRpcException("Unexpected response received: Unknown property in error object: " + P.Key);
						}
					}

					if (Code is null && Message is null)
						throw new JsonRpcException("Unknown error");

					if (!(Message is string Msg))
						throw new JsonRpcException("Unexpected error message returned.");

					if (Code is null)
						throw new JsonRpcException(Msg);

					if (!(Code is int ErrorCode))
						throw new JsonRpcException("Unexpected error code returned.");

					switch (ErrorCode)
					{
						case -32700:
							throw new JsonRpcParseError(ErrorCode, Msg, Data);

						case -32600:
							throw new JsonRpcInvalidRequestError(ErrorCode, Msg, Data);

						case -32601:
							throw new JsonRpcMethodNotFoundError(ErrorCode, Msg, Data);

						case -32602:
							throw new JsonRpcInvalidParametersError(ErrorCode, Msg, Data);

						case -32603:
							throw new JsonRpcInternalError(ErrorCode, Msg, Data);

						default:
							if (ErrorCode <= -32000 && ErrorCode >= -32099)
								throw new JsonRpcServerError(ErrorCode, Msg, Data);
							else
								throw new JsonRpcError(ErrorCode, Msg, Data);
					}
				}
				else
					throw new JsonRpcException("Unexpected response received: Error property is not an object.");
			}

			Response.AssertOk();

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
					sb.Append(WebUtility.UrlEncode(P.Value.ToString()));
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

			if (this.HasSniffers)
			{
				if (Response.HasError)
				{
					if (Response.Encoded is null)
						this.Error(Response.Error.Message);
					else
						this.Error(JSON.Encode(Response.Decoded, true));
				}
				else
					this.ReceiveText(JSON.Encode(Response.Decoded, true));
			}

			return Response;
		}
	}
}
