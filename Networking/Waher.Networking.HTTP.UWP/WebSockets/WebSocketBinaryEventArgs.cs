using System.IO;

namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Event arguments for websocket text events.
	/// </summary>
	public class WebSocketBinaryEventArgs : WebSocketEventArgs
	{
		private readonly Stream payload;

		internal WebSocketBinaryEventArgs(WebSocket Socket, Stream Payload)
			: base(Socket)
		{
			this.payload = Payload;
		}

		/// <summary>
		/// Binary payload.
		/// </summary>
		public Stream Payload => this.payload;

	}
}
