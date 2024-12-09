using System;
using System.IO;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Deserializes binary data
	/// </summary>
	public class BinaryReader
	{
		private byte[] buffer;
		private int bufferSize;
		private int pos;

		/// <summary>
		/// Deserializes binary data
		/// </summary>
		/// <param name="Buffer">Input buffer.</param>
		public BinaryReader(byte[] Buffer)
			: this(Buffer, 0, Buffer.Length)
		{
		}

		/// <summary>
		/// Deserializes binary data
		/// </summary>
		/// <param name="Buffer">Input buffer.</param>
		/// <param name="Offset">Start reading at this position.</param>
		/// <param name="Count">Number of bytes to read.</param>
		public BinaryReader(byte[] Buffer, int Offset, int Count)
		{
			this.bufferSize = Offset + Count;
			this.buffer = Buffer;
			this.pos = Offset;
		}

		/// <summary>
		/// Current byte-position.
		/// </summary>
		public int Position => this.pos;

		/// <summary>
		/// Current buffer.
		/// </summary>
		public byte[] Buffer => this.buffer;

		/// <summary>
		/// Resets the writer for a new header, without clearing the dynamic header table.
		/// </summary>
		/// <param name="Buffer">Input buffer.</param>
		public void Reset(byte[] Buffer)
		{
			this.Reset(Buffer, 0, Buffer.Length);
		}

		/// <summary>
		/// Resets the writer for a new header, without clearing the dynamic header table.
		/// </summary>
		/// <param name="Buffer">Input buffer.</param>
		/// <param name="Offset">Start reading at this position.</param>
		/// <param name="Count">Number of bytes to read.</param>
		public void Reset(byte[] Buffer, int Offset, int Count)
		{
			this.bufferSize = Offset + Count;
			this.buffer = Buffer;
			this.pos = Offset;
		}

		/// <summary>
		/// Returns the next <see cref="byte"/>.
		/// </summary>
		/// <returns>Byte</returns>
		public byte NextByte()
		{
			if (this.pos >= this.bufferSize)
				throw new IOException("Unexpected end of data.");

			return this.buffer[this.pos++];
		}

		/// <summary>
		/// Returns the next <see cref="ushort"/>.
		/// </summary>
		/// <returns>Next 16-bit unsigned integer.</returns>
		public ushort NextUInt16()
		{
			if (this.pos + 1 >= this.bufferSize)
				throw new IOException("Unexpected end of data.");

			ushort Result = this.buffer[this.pos++];
			Result <<= 8;
			Result |= this.buffer[this.pos++];

			return Result;
		}

		/// <summary>
		/// Returns the next <see cref="int"/>.
		/// </summary>
		/// <returns>Next 32-bit unsigned integer.</returns>
		public uint NextUInt32()
		{
			if (this.pos + 3 >= this.bufferSize)
				throw new IOException("Unexpected end of data.");

			uint Result = this.buffer[this.pos++];
			Result <<= 8;
			Result |= this.buffer[this.pos++];
			Result <<= 8;
			Result |= this.buffer[this.pos++];
			Result <<= 8;
			Result |= this.buffer[this.pos++];

			return Result;
		}

		/// <summary>
		/// Reads a specified number of bytes.
		/// </summary>
		/// <param name="NrBytes">Number of bytes to read.</param>
		/// <returns>Bytes read.</returns>
		public byte[] NextBytes(int NrBytes)
		{
			if (this.pos + NrBytes > this.bufferSize)
				throw new IOException("Unexpected end of data.");

			byte[] Result = new byte[NrBytes];
			Array.Copy(this.buffer, this.pos, Result, 0, NrBytes);
			this.pos += NrBytes;

			return Result;
		}

		/// <summary>
		/// If there's more data to read.
		/// </summary>
		public bool HasMore => this.pos < this.bufferSize;

		/// <summary>
		/// Number of bytes left to read.
		/// </summary>
		public int BytesLeft => this.bufferSize - this.pos;

		/// <summary>
		/// Checks if a byte-array is equal to a portion of the read buffer.
		/// </summary>
		/// <param name="Buffer">Buffer</param>
		/// <param name="ThisPosition">Start position in this object's buffer.</param>
		/// <returns>If bytes are the same.</returns>
		public bool AreSame(byte[] Buffer, int ThisPosition)
		{
			return this.AreSame(Buffer, 0, Buffer.Length, ThisPosition);
		}

		/// <summary>
		/// Checks if a byte-array is equal to a portion of the read buffer.
		/// </summary>
		/// <param name="Buffer">Buffer</param>
		/// <param name="Offset">Start offset in buffer.</param>
		/// <param name="Count">Number of bytes to compare.</param>
		/// <param name="ThisPosition">Start position in this object's buffer.</param>
		/// <returns>If bytes are the same.</returns>
		public bool AreSame(byte[] Buffer, int Offset, int Count, int ThisPosition)
		{
			if (ThisPosition + Count > this.bufferSize)
				return false;

			if (Offset + Count > Buffer.Length)
				return false;

			while (Count-- > 0)
			{
				if (Buffer[Offset++] != this.buffer[ThisPosition++])
					return false;
			}

			return true;
		}
	}
}
