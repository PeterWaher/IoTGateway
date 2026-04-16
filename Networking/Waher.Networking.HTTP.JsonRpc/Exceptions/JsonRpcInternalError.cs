namespace Waher.Networking.HTTP.JsonRpc.Exceptions
{
	/// <summary>
	/// JSON-RPC Internal error response.
	/// </summary>
	public class JsonRpcInternalError : JsonRpcError 
	{
		/// <summary>
		/// JSON-RPC Internal error response.
		/// </summary>
		/// <param name="ErrorCode">JSON-RPC Error code.</param>
		/// <param name="Message">Exception message.</param>
		/// <param name="Data">Data associated with the error.</param>
		public JsonRpcInternalError(int ErrorCode, string Message, object? Data)
			: base(ErrorCode, Message, Data)
		{
		}
	}
}
