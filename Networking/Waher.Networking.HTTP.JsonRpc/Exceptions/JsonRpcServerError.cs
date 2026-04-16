namespace Waher.Networking.HTTP.JsonRpc.Exceptions
{
	/// <summary>
	/// JSON-RPC Server error response.
	/// </summary>
	public class JsonRpcServerError : JsonRpcError 
	{
		/// <summary>
		/// JSON-RPC Server error response.
		/// </summary>
		/// <param name="ErrorCode">JSON-RPC Error code.</param>
		/// <param name="Message">Exception message.</param>
		/// <param name="Data">Data associated with the error.</param>
		public JsonRpcServerError(int ErrorCode, string Message, object? Data)
			: base(ErrorCode, Message, Data)
		{
		}
	}
}
