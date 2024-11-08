using System;

namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Event arguments for websocket events.
	/// </summary>
	public class WebSocketEventArgs : EventArgs
	{
		private readonly WebSocket socket;

		internal WebSocketEventArgs(WebSocket Socket)
			: base()
		{
			this.socket = Socket;
		}

		/// <summary>
		/// Web-socket
		/// </summary>
		public WebSocket Socket => this.socket;
	}
}
