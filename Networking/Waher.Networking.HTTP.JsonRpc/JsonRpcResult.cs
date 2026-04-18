using System.Collections.Generic;
using Waher.Networking.HTTP.JsonRpc.Exceptions;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about a JSON-RPC result.
	/// </summary>
	public class JsonRpcResult
	{
		private readonly JsonRpcVersion version;
		private readonly object? result;
		private readonly JsonRpcError? error;
		private readonly long? id;
		private readonly bool hasResult;
		private readonly bool hasError;

		/// <summary>
		/// Information about a JSON-RPC result.
		/// </summary>
		/// <param name="JsonResponse">JSON response object.</param>
		public JsonRpcResult(Dictionary<string,object> JsonResponse)
		{
			foreach (KeyValuePair<string, object> P in JsonResponse)
			{
				switch (P.Key)
				{
					case "jsonrpc":
						if (P.Value is string s)
						{
							if (s == "2.0")
								this.version = JsonRpcVersion.JsonRpcV2;
							else
								throw new JsonRpcException("Unexpected response received: Unsupported JSON-RPC version: " + s);
						}
						else
							throw new JsonRpcException("Unexpected response received: JSON-RPC version property is not a string.");
						break;

					case "result":
						this.result = P.Value;
						this.hasResult = true;
						break;

					case "error":   // Processed in DoNotification

						if (P.Value is Dictionary<string, object> Error &&
							Error.TryGetValue("code", out object Obj) && 
							Obj is int ErrorCode &&
							Error.TryGetValue("message", out Obj) && 
							Obj is string ErrorMessage)
						{
							this.error = JsonRpcClient.GetError(ErrorCode, ErrorMessage);
							this.hasError = true;
						}
						else
							throw new JsonRpcException("Unexpected error response received.");
						break;

					case "id":
						if (P.Value is long l)
							this.id = l;
						else if (P.Value is int i)
							this.id = i;
						else if (P.Value is string s2 && long.TryParse(s2, out l))
							this.id = l;
						else
							throw new JsonRpcException("Unexpected response received: Id property is not a valid integer.");
						break;

					default:
						throw new JsonRpcException("Unexpected response received: Unknown property: " + P.Key);
				}
			}
		}

		/// <summary>
		/// JSON-RPC version.
		/// </summary>
		public JsonRpcVersion Version => this.version;

		/// <summary>
		/// Result of operation, if <see cref="HasResult"/> is true.
		/// </summary>
		public object? Result => this.result;

		/// <summary>
		/// Error of operation, if <see cref="HasError"/> is true.
		/// </summary>
		public JsonRpcError? Error => this.error;

		/// <summary>
		/// ID matching the request, if any.
		/// </summary>
		public long? Id => this.id;

		/// <summary>
		///	If response has a result in <see cref="Result"/>, this property is true.
		/// </summary>
		public bool HasResult => this.hasResult;

		/// <summary>
		/// If response has an error in <see cref="Error"/>, this property is true.
		/// </summary>
		public bool HasError => this.hasError;


	}
}
