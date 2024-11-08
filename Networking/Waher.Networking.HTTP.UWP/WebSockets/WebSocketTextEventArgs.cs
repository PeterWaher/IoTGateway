namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Event arguments for websocket text events.
	/// </summary>
	public class WebSocketTextEventArgs : WebSocketEventArgs
	{
		private readonly string payload;

		internal WebSocketTextEventArgs(WebSocket Socket, string Payload)
			: base(Socket)
		{
			this.payload = Payload;
		}

		/// <summary>
		/// Text payload.
		/// </summary>
		public string Payload => this.payload;

	}
}
