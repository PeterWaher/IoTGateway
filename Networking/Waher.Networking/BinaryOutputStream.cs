using System;
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
		/// <param name="Packet">Binary packet.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(byte[] Packet)
		{
			return this.SendAsync(Packet, 0, Packet.Length, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(byte[] Packet, EventHandler Callback)
		{
			return this.SendAsync(Packet, 0, Packet.Length, Callback);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte written.</param>
		/// <param name="Count">Number of bytes written.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(byte[] Buffer, int Offset, int Count)
		{
			return this.SendAsync(Buffer, Offset, Count, null);
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte to write.</param>
		/// <param name="Count">Number of bytes to write.</param>
		/// <param name="Callback">Method to call when packet has been sent.</param>
		/// <returns>If data was sent.</returns>
		public async Task<bool> SendAsync(byte[] Buffer, int Offset, int Count, EventHandler Callback)
		{
			await this.output.WriteAsync(Buffer, Offset, Count);

			if (!(Callback is null))
			{
				try
				{
					Callback(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			return true;
		}
	}
}
