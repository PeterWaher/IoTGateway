namespace Waher.Networking.HTTP.JsonRpc.Exceptions
{
	/// <summary>
	/// JSON-RPC Parse error response.
	/// </summary>
	public class JsonRpcParseError : JsonRpcError 
	{
		/// <summary>
		/// JSON-RPC Parse error response.
		/// </summary>
		/// <param name="ErrorCode">JSON-RPC Error code.</param>
		/// <param name="Message">Exception message.</param>
		/// <param name="Data">Data associated with the error.</param>
		public JsonRpcParseError(int ErrorCode, string Message, object? Data)
			: base(ErrorCode, Message, Data)
		{
		}
	}
}
