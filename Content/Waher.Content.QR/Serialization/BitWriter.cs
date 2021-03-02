using System;
using System.Collections.Generic;
using System.IO;

namespace Waher.Content.QR.Serialization
{
	/// <summary>
	/// Writes a sequence of bits (Most significant bits first).
	/// </summary>
	public class BitWriter : IDisposable
	{
		private MemoryStream output;
		private byte current;
		private int bits;
		private int length;

		/// <summary>
		/// Writes a sequence of bits (Most significant bits first).
		/// </summary>
		public BitWriter()
		{
			this.output = new MemoryStream();
			this.current = 0;
			this.bits = 0;
			this.length = 0;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.output?.Dispose();
			this.output = null;
		}

		/// <summary>
		/// Writes a bit to the output.
		/// </summary>
		/// <param name="Bit">Bit.</param>
		public void WriteBit(bool Bit)
		{
			if (Bit)
				this.current |= (byte)(1 << (7 - this.bits));

			if (++this.bits >= 8)
			{
				this.output.WriteByte(this.current);
				this.length++;
				this.bits = 0;
				this.current = 0;
			}
		}

		/// <summary>
		/// Writes several bits to the output stream.
		/// </summary>
		/// <param name="Value">Binary value.</param>
		/// <param name="NrBits">Number of bits to output.</param>
		public void WriteBits(uint Value, int NrBits)
		{
			if (NrBits < 32)
				Value <<= (32 - NrBits);

			int c;

			while (NrBits > 0)
			{
				c = 8 - this.bits;
				if (c > NrBits)
					c = NrBits;

				this.current |= (byte)(Value >> (24 + this.bits));
				Value <<= c;
				NrBits -= c;
				this.bits += c;

				if (this.bits >= 8)
				{
					this.output.WriteByte(this.current);
					this.length++;
					this.bits = 0;
					this.current = 0;
				}
			}
		}

		/// <summary>
		/// Flushes any remaining bits to the output.
		/// </summary>
		public void Flush()
		{
			if (this.bits > 0)
			{
				this.output.WriteByte(this.current);
				this.length++;
				this.bits = 0;
				this.current = 0;
			}
		}

		/// <summary>
		/// Pads the output with padding bytes.
		/// </summary>
		/// <param name="MaxLength">Maximum length of message.</param>
		/// <param name="PaddingBytes">Padding bytes.</param>
		public void Pad(int MaxLength, params byte[] PaddingBytes)
		{
			int i = 0;
			int c = PaddingBytes.Length;

			if (c == 0)
				throw new ArgumentException("Missing padding bytes.", nameof(PaddingBytes));

			this.Flush();
			while (this.length < MaxLength)
			{
				this.output.WriteByte(PaddingBytes[i++]);
				this.length++;
				if (i >= c)
					i = 0;
			}
		}

		/// <summary>
		/// Returns a byte-array of serialized bits.
		/// </summary>
		/// <returns>Byte array.</returns>
		public byte[] ToArray()
		{
			this.Flush();
			return this.output.ToArray();
		}

		/// <summary>
		/// Total number of bits written.
		/// </summary>
		public int TotalBits => (this.length << 3) + this.bits;
	}
}
