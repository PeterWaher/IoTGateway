using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Implements a HTTP Binding mechanism based on BOSH.
	/// 
	/// XEP-0124: Bidirectional-streams Over Synchronous HTTP (BOSH):
	///	https://xmpp.org/extensions/xep-0124.html
	///	
	/// XEP-0206: XMPP Over BOSH:
	/// https://xmpp.org/extensions/xep-0206.html
	/// </summary>
	public class HttpBinding : ITextTransportLayer
    {
		private string url;

		/// <summary>
		/// Implements a HTTP Binding mechanism based on BOSH.
		/// </summary>
		/// <param name="Url">URL to remote HTTP endpoint.</param>
		public HttpBinding(string Url)
		{
			this.url = Url;
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event TextEventHandler OnSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		public event TextEventHandler OnReceived;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		public void Send(string Packet)
		{
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		public void Send(string Packet, EventHandler DeliveryCallback)
		{
		}
	}
}
