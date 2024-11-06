using System;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Interface for binary transmission.
	/// </summary>
	public interface IBinaryTransmission : IDisposable
	{
		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <returns>If data was sent.</returns>
		Task<bool> SendAsync(byte[] Packet);

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <returns>If data was sent.</returns>
		Task<bool> SendAsync(byte[] Packet, EventHandlerAsync Callback);

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte written.</param>
		/// <param name="Count">Number of bytes written.</param>
		/// <returns>If data was sent.</returns>
		Task<bool> SendAsync(byte[] Buffer, int Offset, int Count);

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <returns>If data was sent.</returns>
		Task<bool> SendAsync(byte[] Buffer, int Offset, int Count, EventHandlerAsync Callback);

		/// <summary>
		/// Flushes any pending or intermediate data.
		/// </summary>
		/// <returns>If output has been flushed.</returns>
		Task<bool> FlushAsync();
	}
}
