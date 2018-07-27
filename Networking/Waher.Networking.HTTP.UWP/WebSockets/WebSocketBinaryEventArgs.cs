using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Delegate for websocket text events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void WebSocketBinaryEventHandler(object Sender, WebSocketBinaryEventArgs e);

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
