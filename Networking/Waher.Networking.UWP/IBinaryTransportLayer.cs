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
	/// <param name="Buffer">Binary Data Buffer</param>
	/// <param name="Offset">Start index of first byte written.</param>
	/// <param name="Count">Number of bytes written.</param>
	/// <returns>If the process should be continued.</returns>
	public delegate Task BinaryDataWrittenEventHandler(object Sender, byte[] Buffer, int Offset, int Count);

	/// <summary>
	/// Event handler for binary packet events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Buffer">Binary Data Buffer</param>
	/// <param name="Offset">Start index of first byte read.</param>
	/// <param name="Count">Number of bytes read.</param>
	/// <returns>If the process should be continued.</returns>
	public delegate Task<bool> BinaryDataReadEventHandler(object Sender, byte[] Buffer, int Offset, int Count);

	/// <summary>
	/// Interface for binary transport layers.
	/// </summary>
	public interface IBinaryTransportLayer : IBinaryTransmission
	{
		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		event BinaryDataWrittenEventHandler OnSent;

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		event BinaryDataReadEventHandler OnReceived;

		/// <summary>
		/// If the reading is paused.
		/// </summary>
		bool Paused
		{
			get;
		}

		/// <summary>
		/// Call this method to continue operation. Operation can be paused, by returning false from <see cref="OnReceived"/>.
		/// </summary>
		void Continue();
	}
}
