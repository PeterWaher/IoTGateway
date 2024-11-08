namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Event arguments for websocket closed events.
	/// </summary>
	public class WebSocketClosedEventArgs : WebSocketEventArgs
	{
		private readonly ushort? code;
		private readonly string reason;

		internal WebSocketClosedEventArgs(WebSocket Socket, ushort? Code, string Reason)
			: base(Socket)
		{
			this.code = Code;
			this.reason = Reason;
		}

		/// <summary>
		/// Optional code the remote party reported when closing the connection.
		/// </summary>
		public ushort? Code => this.code;

		/// <summary>
		/// Optional reasong the remote party reported when closing the connection.
		/// </summary>
		public string Reason => this.reason;

	}
}
