namespace Waher.Networking.HTTP.JsonRpc.Exceptions
{
	/// <summary>
	/// JSON-RPC Invalid Request error response.
	/// </summary>
	public class JsonRpcInvalidRequestError : JsonRpcError 
	{
		/// <summary>
		/// JSON-RPC Invalid Request error response.
		/// </summary>
		/// <param name="ErrorCode">JSON-RPC Error code.</param>
		/// <param name="Message">Exception message.</param>
		/// <param name="Data">Data associated with the error.</param>
		public JsonRpcInvalidRequestError(int ErrorCode, string Message, object? Data)
			: base(ErrorCode, Message, Data)
		{
		}
	}
}
