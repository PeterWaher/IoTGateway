using System.Collections;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about a JSON-RPC request.
	/// </summary>
	public class JsonRpcRequest
	{
		private readonly JsonRpcVersion version;
		private readonly string method;
		private readonly object? parameters;


		/// <summary>
		/// Information about a JSON-RPC request.
		/// </summary>
		/// <param name="Method">Name of method.</param>
		public JsonRpcRequest(string Method)
			: this(JsonRpcVersion.JsonRpcV2, Method)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC request.
		/// </summary>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Array of parameters.</param>
		public JsonRpcRequest(string Method, params object?[] Parameters)
			: this(JsonRpcVersion.JsonRpcV2, Method, Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC request.
		/// </summary>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Array of parameters.</param>
		public JsonRpcRequest(string Method, IEnumerable Parameters)
			: this(JsonRpcVersion.JsonRpcV2, Method, Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC request.
		/// </summary>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Named parameters.</param>
		public JsonRpcRequest(string Method, Dictionary<string, object> Parameters)
			: this(JsonRpcVersion.JsonRpcV2, Method, Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC request.
		/// </summary>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Name of method.</param>
		public JsonRpcRequest(JsonRpcVersion Version, string Method)
		{
			this.version = Version;
			this.method = Method;
			this.parameters = null;
		}

		/// <summary>
		/// Information about a JSON-RPC request.
		/// </summary>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Array of parameters.</param>
		public JsonRpcRequest(JsonRpcVersion Version, string Method, params object?[] Parameters)
			: this(Version, Method, (IEnumerable)Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC request.
		/// </summary>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Array of parameters.</param>
		public JsonRpcRequest(JsonRpcVersion Version, string Method, IEnumerable Parameters)
		{
			this.version = Version;
			this.method = Method;
			this.parameters = Parameters;
		}

		/// <summary>
		/// Information about a JSON-RPC request.
		/// </summary>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Named parameters.</param>
		public JsonRpcRequest(JsonRpcVersion Version, string Method, Dictionary<string, object> Parameters)
		{
			this.version = Version;
			this.method = Method;
			this.parameters = Parameters;
		}

		/// <summary>
		/// Builds a request object.
		/// </summary>
		/// <param name="Id">Request identity</param>
		/// <returns>JSON object representing the request.</returns>
		internal Dictionary<string, object> BuildRequest(long? Id)
		{
			Dictionary<string, object> Request = new Dictionary<string, object>();

			if (this.version == JsonRpcVersion.JsonRpcV2)
				Request["jsonrpc"] = "2.0";

			Request["method"] = this.method;

			if (!(this.parameters is null))
				Request["params"] = this.parameters;

			if (Id.HasValue)
				Request["id"] = Id.Value;

			return Request;
		}

		/// <summary>
		/// If object represents a request.
		/// </summary>
		public virtual bool IsRequest => true;

	}
}
