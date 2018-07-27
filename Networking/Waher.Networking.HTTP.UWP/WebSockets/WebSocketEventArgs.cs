using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.WebSockets
{
	/// <summary>
	/// Delegate for websocket events.
	/// </summary>
	/// <param name="Sender"></param>
	/// <param name="e"></param>
	public delegate void WebSocketEventHandler(object Sender, WebSocketEventArgs e);

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
