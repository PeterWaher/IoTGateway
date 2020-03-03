using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Event handler for binary packet events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Packet">Binary packet.</param>
	public delegate Task BinaryEventHandler(object Sender, byte[] Packet);

	/// <summary>
	/// Interface for binary transport layers.
	/// </summary>
	public interface IBinaryTransportLayer : IDisposable
	{
		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		void Send(byte[] Packet);

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		event BinaryEventHandler OnSent;

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		event BinaryEventHandler OnReceived;
	}
}
