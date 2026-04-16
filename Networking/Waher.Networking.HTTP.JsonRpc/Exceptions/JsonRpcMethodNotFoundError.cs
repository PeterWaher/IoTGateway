namespace Waher.Networking.HTTP.JsonRpc.Exceptions
{
	/// <summary>
	/// JSON-RPC Method Not Found error response.
	/// </summary>
	public class JsonRpcMethodNotFoundError : JsonRpcError 
	{
		/// <summary>
		/// JSON-RPC Method Not Found error response.
		/// </summary>
		/// <param name="ErrorCode">JSON-RPC Error code.</param>
		/// <param name="Message">Exception message.</param>
		/// <param name="Data">Data associated with the error.</param>
		public JsonRpcMethodNotFoundError(int ErrorCode, string Message, object? Data)
			: base(ErrorCode, Message, Data)
		{
		}
	}
}
