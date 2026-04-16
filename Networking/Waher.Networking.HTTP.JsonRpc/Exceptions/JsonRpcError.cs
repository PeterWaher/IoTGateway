namespace Waher.Networking.HTTP.JsonRpc.Exceptions
{
	/// <summary>
	/// Base class for JSON-RPC error responses.
	/// </summary>
	public class JsonRpcError : JsonRpcException
	{
		/// <summary>
		/// Base class for JSON-RPC error responses.
		/// </summary>
		/// <param name="ErrorCode">JSON-RPC Error code.</param>
		/// <param name="Message">Exception message.</param>
		/// <param name="Data">Data associated with the error.</param>
		public JsonRpcError(int ErrorCode, string Message, object? Data)
			: base(Message)
		{
			this.ErrorCode = ErrorCode;
			this.ErrorData = Data;
		}

		/// <summary>
		/// JSON-RPC Error code.
		/// </summary>
		public int ErrorCode { get; }

		/// <summary>
		/// Data associated with the error.
		/// </summary>
		public object? ErrorData { get; }
	}
}
