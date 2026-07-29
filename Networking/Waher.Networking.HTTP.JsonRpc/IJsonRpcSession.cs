namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Interface for JSON-RPC session objects.
	/// </summary>
	public interface IJsonRpcSession
	{
		/// <summary>
		/// Session ID
		/// </summary>
		string SessionId { get; }

		/// <summary>
		/// Is called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Transmitted text.</param>
		void TransmitText(string Text);
	}
}
