using System;

namespace Waher.Networking.HTTP.JsonRpc.Exceptions
{
	/// <summary>
	/// Base class for JSON-RPC exceptions.
	/// </summary>
	public class JsonRpcException : Exception
	{
		/// <summary>
		/// Base class for JSON-RPC exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		public JsonRpcException(string Message)
			: base(Message)
		{
		}
	}
}
