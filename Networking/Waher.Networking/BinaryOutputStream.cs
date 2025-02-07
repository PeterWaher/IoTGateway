using System.IO;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking
{
	/// <summary>
	/// Encapsulates a binary output stream.
	/// </summary>
	public class BinaryOutputStream : IBinaryTransmission
	{
		private readonly Stream output;

		/// <summary>
		/// Encapsulates a binary output stream.
		/// </summary>
		/// <param name="Output">Output stream.</param>
		public BinaryOutputStream(Stream Output)
		{
			this.output = Output;
		}

		/// <summary>
		/// Disposes of the object (but not the underlying stream).
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Flushes any pending or intermediate data.
		/// </summary>
		/// <returns>If output has been flushed.</returns>
		public async Task<bool> FlushAsync()
		{
			await this.output.FlushAsync();
			return true;
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet)
		{
			return this.SendAsync(ConstantBuffer, Packet, 0, Packet.Length, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Packet, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendAsync(ConstantBuffer, Packet, 0, Packet.Length, Callback, State);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte written.</param>
		/// <param name="Count">Number of bytes written.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			return this.SendAsync(ConstantBuffer, Buffer, Offset, Count, null, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public async Task<bool> SendAsync(bool ConstantBuffer, byte[] Buffer, int Offset, int Count, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			await this.output.WriteAsync(Buffer, Offset, Count);

			if (!(Callback is null))
				await Callback.Raise(this, new DeliveryEventArgs(State, true));

			return true;
		}
	}
}
