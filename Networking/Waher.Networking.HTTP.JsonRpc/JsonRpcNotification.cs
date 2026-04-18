using System.Collections;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about a JSON-RPC notification.
	/// </summary>
	public class JsonRpcNotification : JsonRpcRequest
	{
		/// <summary>
		/// Information about a JSON-RPC notification.
		/// </summary>
		/// <param name="Method">Name of method.</param>
		public JsonRpcNotification(string Method)
			: base(Method)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC notification.
		/// </summary>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Array of parameters.</param>
		public JsonRpcNotification(string Method, params object?[] Parameters)
			: base(Method, Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC notification.
		/// </summary>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Array of parameters.</param>
		public JsonRpcNotification(string Method, IEnumerable Parameters)
			: base(Method, Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC notification.
		/// </summary>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Named parameters.</param>
		public JsonRpcNotification(string Method, Dictionary<string, object> Parameters)
			: base(Method, Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC notification.
		/// </summary>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Name of method.</param>
		public JsonRpcNotification(JsonRpcVersion Version, string Method)
			: base(Version, Method)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC notification.
		/// </summary>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Array of parameters.</param>
		public JsonRpcNotification(JsonRpcVersion Version, string Method, params object?[] Parameters)
			: base(Version, Method, Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC notification.
		/// </summary>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Array of parameters.</param>
		public JsonRpcNotification(JsonRpcVersion Version, string Method, IEnumerable Parameters)
			: base(Version, Method, Parameters)
		{
		}

		/// <summary>
		/// Information about a JSON-RPC notification.
		/// </summary>
		/// <param name="Version">JSON-RPC version</param>
		/// <param name="Method">Name of method.</param>
		/// <param name="Parameters">Named parameters.</param>
		public JsonRpcNotification(JsonRpcVersion Version, string Method, Dictionary<string, object> Parameters)
			: base(Version, Method, Parameters)
		{
		}

		/// <summary>
		/// If object represents a request.
		/// </summary>
		public override bool IsRequest => false;
	}
}
