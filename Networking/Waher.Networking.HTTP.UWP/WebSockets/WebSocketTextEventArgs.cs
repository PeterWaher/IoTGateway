using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Delegate for websocket text events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void WebSocketTextEventHandler(object Sender, WebSocketTextEventArgs e);

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
