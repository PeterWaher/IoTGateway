using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Event handler for text packet events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Packet">Text packet.</param>
	/// <returns>If the process should be continued.</returns>
	public delegate bool TextEventHandler(object Sender, string Packet);

	/// <summary>
	/// Interface for text transport layers.
	/// </summary>
	public interface ITextTransportLayer : IDisposable
	{
		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		void Send(string Packet);

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		event TextEventHandler OnSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		event TextEventHandler OnReceived;
	}
}
