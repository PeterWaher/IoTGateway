using System;
using System.IO;
using System.Text;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Serializes HTTP/2 headers using HPACK, defined in RFC 7541.
	/// https://datatracker.ietf.org/doc/html/rfc7541
	/// </summary>
	public class HeaderWriter : DynamicHeaders
	{
		private readonly uint bufferSize;
		private readonly byte[] buffer;
		private uint pos = 0;
		private byte bitsLeft = 8;
		private byte current = 0;

		/// <summary>
		/// Serializes HTTP/2 headers using HPACK, defined in RFC 7541.
		/// https://datatracker.ietf.org/doc/html/rfc7541
		/// </summary>
		/// <param name="BufferSize">Size of binary buffer.</param>
		/// <param name="MaxDynamicHeaderSize">Maximum dynamic header size.</param>
		public HeaderWriter(uint BufferSize, uint MaxDynamicHeaderSize)
			: this(BufferSize, MaxDynamicHeaderSize, MaxDynamicHeaderSize)
		{
		}

		/// <summary>
		/// Serializes HTTP/2 headers using HPACK, defined in RFC 7541.
		/// https://datatracker.ietf.org/doc/html/rfc7541
		/// </summary>
		/// <param name="BufferSize">Size of binary buffer.</param>
		/// <param name="MaxDynamicHeaderSize">Maximum dynamic header size.</param>
		/// <param name="MaxDynamicHeaderSizeLimit">Upper limit of the maximum dynamic header size.</param>
		public HeaderWriter(uint BufferSize, uint MaxDynamicHeaderSize, uint MaxDynamicHeaderSizeLimit)
			: base(MaxDynamicHeaderSize, MaxDynamicHeaderSizeLimit)
		{
			this.bufferSize = BufferSize;
			this.buffer = new byte[this.bufferSize];
		}

		/// <summary>
		/// Current byte-position.
		/// </summary>
		public uint Position => this.pos;

		/// <summary>
		/// Bits left of current byte.
		/// </summary>
		public int BitsLeft => this.bitsLeft;

		/// <summary>
		/// Current buffer.
		/// </summary>
		public byte[] Buffer => this.buffer;

		/// <summary>
		/// Flushes any remaining bits.
		/// </summary>
		/// <returns>If write was successful (true), or if buffer size did not permit write operation (false).</returns>
		public bool Flush()
		{
			if (this.dynamicHeaderSize > this.maxDynamicHeaderSize)
				this.TrimDynamicHeaders();

			if (this.bitsLeft < 8)
				return this.WriteByteBits(0, this.bitsLeft);
			else
				return true;
		}

		/// <summary>
		/// Resets the writer for a new header, without clearing the dynamic header table.
		/// </summary>
		public void Reset()
		{
			this.pos = 0;
			this.bitsLeft = 8;
			this.current = 0;
		}

		/// <summary>
		/// Clears the table for a new header, clearing the dynamic header table.
		/// </summary>
		public void Clear()
		{
			this.Reset();
			this.dynamicHeaders.Clear();
			this.dynamicRecords.Clear();
			this.nrHeadersCreated = 0;
			this.dynamicHeaderSize = 0;
		}

		/// <summary>
		/// Writes a single bit.
		/// </summary>
		/// <param name="Bit">Bit value.</param>
		/// <returns>If write was successful (true), or if buffer size did not permit write operation (false).</returns>
		public bool WriteBit(bool Bit)
		{
			this.current <<= 1;
			if (Bit)
				this.current |= 1;

			this.bitsLeft--;
			if (this.bitsLeft <= 0)
			{
				if (this.pos >= this.bufferSize)
					return false;

				this.buffer[this.pos++] = this.current;
				this.bitsLeft = 8;
				this.current = 0;
			}

			return true;
		}

		/// <summary>
		/// Writes a set of bits.
		/// </summary>
		/// <param name="Bits">Bits to write.</param>
		/// <param name="NrBits">Number of bits to write.</param>
		/// <returns>If write was successful (true), or if buffer size did not permit write operation (false).</returns>
		public bool WriteByteBits(byte Bits, byte NrBits)
		{
			if (NrBits <= 0)
				throw new ArgumentException("Must be positive.", nameof(NrBits));

			Bits &= bitMasks[NrBits];

			if (NrBits <= this.bitsLeft)
			{
				this.current <<= NrBits;
				this.current |= Bits;

				this.bitsLeft -= NrBits;
				if (this.bitsLeft <= 0)
				{
					if (this.pos >= this.bufferSize)
						return false;

					this.buffer[this.pos++] = this.current;
					this.bitsLeft = 8;
					this.current = 0;
				}
			}
			else
			{
				byte n = (byte)(Bits >> (NrBits - this.bitsLeft));
				NrBits -= this.bitsLeft;
				Bits &= bitMasks[NrBits];

				this.current <<= this.bitsLeft;
				this.current |= n;

				if (this.pos >= this.bufferSize)
					return false;

				this.buffer[this.pos++] = this.current;
				this.bitsLeft = (byte)(8 - NrBits);
				this.current = Bits;
			}

			return true;
		}

		/// <summary>
		/// Writes a set of bits.
		/// </summary>
		/// <param name="Bits">Bits to write.</param>
		/// <param name="NrBits">Number of bits to write.</param>
		/// <returns>If write was successful (true), or if buffer size did not permit write operation (false).</returns>
		public bool WriteUIntBits(uint Bits, byte NrBits)
		{
			if (NrBits <= 0)
				throw new ArgumentException("Must be positive.", nameof(NrBits));

			if (NrBits <= 8)
				return this.WriteByteBits((byte)Bits, NrBits);

			byte b1 = (byte)Bits;

			if (NrBits > 8)
			{
				Bits >>= 8;
				byte b2 = (byte)Bits;

				if (NrBits > 16)
				{
					Bits >>= 8;
					byte b3 = (byte)Bits;

					if (NrBits > 24)
					{
						Bits >>= 8;
						byte b4 = (byte)Bits;

						NrBits &= 7;
						if (NrBits == 0)
							NrBits = 8;

						if (!this.WriteByteBits(b4, NrBits))
							return false;

						NrBits = 8;
					}
					else
					{
						NrBits &= 7;
						if (NrBits == 0)
							NrBits = 8;
					}

					if (!this.WriteByteBits(b3, NrBits))
						return false;

					NrBits = 8;
				}
				else
				{
					NrBits &= 7;
					if (NrBits == 0)
						NrBits = 8;
				}

				if (!this.WriteByteBits(b2, NrBits))
					return false;

				NrBits = 8;
			}
			else
			{
				NrBits &= 7;
				if (NrBits == 0)
					NrBits = 8;
			}

			return this.WriteByteBits(b1, NrBits);
		}

		/// <summary>
		/// Creates a byte array of bytes output.
		/// </summary>
		/// <returns>Byte array.</returns>
		/// <exception cref="InternalBufferOverflowException">If the buffer limit
		/// is reached.</exception>
		public byte[] ToArray()
		{
			if (!this.Flush())
				throw new InternalBufferOverflowException();

			byte[] Result = new byte[this.pos];
			Array.Copy(this.buffer, 0, Result, 0, this.pos);

			return Result;
		}

		/// <summary>
		/// Write a variable-length integer
		/// </summary>
		/// <param name="Value">Value to write.</param>
		/// <returns>If write was successful (true), or if buffer size did not permit write operation (false).</returns>
		public bool WriteInteger(ulong Value)
		{
			byte i = bitMasks[this.bitsLeft];
			if (Value < i)
				return this.WriteByteBits((byte)Value, this.bitsLeft);

			if (!this.WriteByteBits(i, this.bitsLeft))
				return false;

			Value -= i;

			while (true)
			{
				if (Value < 128)
					return this.WriteByteBits((byte)Value, 8);

				if (!this.WriteByteBits((byte)((Value & 127) | 128), 8))
					return false;

				Value >>= 7;
			}
		}

		/// <summary>
		/// Write a string
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="Huffman">If the value should be Huffman encoded.</param>
		/// <param name="Length">Length of string, for use with calculation of dynamic header record size.</param>
		/// <returns>If write was successful (true), or if buffer size did not permit write operation (false).</returns>
		public bool WriteString(string Value, bool Huffman, out uint Length)
		{
			if (!this.WriteBit(Huffman))
			{
				Length = 0;
				return false;
			}

			byte[] Bin = Encoding.UTF8.GetBytes(Value);
			int i;

			Length = (uint)Bin.Length;

			if (Huffman)
			{
				HuffmanEncoding Rec;
				uint HuffmanLen = 0;

				for (i = 0; i < Length; i++)
					HuffmanLen += huffmanEncodings[Bin[i]].NrBits;

				HuffmanLen += 7;
				HuffmanLen >>= 3;

				if (!this.WriteInteger(HuffmanLen))
					return false;

				for (i = 0; i < Length; i++)
				{
					Rec = huffmanEncodings[Bin[i]];
					if (!this.WriteUIntBits(Rec.Value, Rec.NrBits))
						return false;
				}

				if (this.bitsLeft < 8 && !this.WriteByteBits(0xff, this.bitsLeft))  // EOS
					return false;
			}
			else
			{
				if (!this.WriteInteger(Length))
					return false;

				if (this.pos + Length >= this.bufferSize)
					return false;

				Array.Copy(Bin, 0, this.buffer, this.pos, Length);
				this.pos += Length;
			}

			return true;
		}

		/// <summary>
		/// Writes a header and a value pair.
		/// </summary>
		/// <param name="Header">Header</param>
		/// <param name="Value">Value</param>
		/// <param name="Mode">Serialization mode with respect to indexing.</param>
		/// <param name="Huffman">If Huffman encoding should be used.</param>
		/// <returns>If write was successful (true), or if buffer size did not permit write operation (false).</returns>
		public bool WriteHeader(string Header, string Value, IndexMode Mode, bool Huffman)
		{
			DynamicHeader DynamicHeader;
			DynamicRecord DynamicRecord;
			ulong Index;
			uint c;

			if (this.dynamicHeaderSize > this.maxDynamicHeaderSize)
				this.TrimDynamicHeaders();

			Header = Header.ToLower();

			if (staticHeaders.TryGetValue(Header, out StaticRecord[] Records))
			{
				int i = Records.Length;
				StaticRecord Rec;

				while (i > 0)
				{
					Rec = Records[--i];

					if (Value == Rec.Value)
					{
						if (!this.WriteBit(true))
							return false;

						return this.WriteByteBits(Rec.Index, 7);
					}
				}

				Rec = Records[0];
				Index = Rec.Index;

				if (this.dynamicHeaders.TryGetValue(Header, out DynamicHeader) &&
					DynamicHeader.Values.TryGetValue(Value, out DynamicRecord))
				{
					if (!this.WriteBit(true))
						return false;

					Index = lastStaticHeaderIndex + this.nrHeadersCreated - DynamicRecord.Ordinal;
					return this.WriteInteger(Index);
				}

				if (Mode == IndexMode.Indexed)
				{
					if (DynamicHeader is null)
						DynamicRecord = this.AddToDynamicIndex(Header, Value);
					else
						DynamicRecord = this.AddToDynamicIndex(DynamicHeader, Value);

					DynamicRecord.Length += Rec.HeaderLength;
				}
				else
					DynamicRecord = null;
			}
			else
			{
				if (this.dynamicHeaders.TryGetValue(Header, out DynamicHeader))
				{
					if (DynamicHeader.Values.TryGetValue(Value, out DynamicRecord))
					{
						if (!this.WriteBit(true))
							return false;

						Index = lastStaticHeaderIndex + this.nrHeadersCreated - DynamicRecord.Ordinal;
						return this.WriteInteger(Index);
					}
					else
					{
						Index = lastStaticHeaderIndex + this.nrHeadersCreated - DynamicHeader.Ordinal;

						if (Mode == IndexMode.Indexed)
							DynamicRecord = this.AddToDynamicIndex(DynamicHeader, Value);
						else
							DynamicRecord = null;
					}
				}
				else
				{
					if (Mode == IndexMode.Indexed)
						DynamicRecord = this.AddToDynamicIndex(Header, Value);
					else
						DynamicRecord = null;

					Index = 0;
				}
			}

			switch (Mode)
			{
				case IndexMode.Indexed:
					if (!this.WriteByteBits(1, 2) || !this.WriteInteger(Index))
						return false;
					break;

				case IndexMode.NotIndexed:
					if (!this.WriteByteBits(0, 4) || !this.WriteInteger(Index))
						return false;
					break;

				case IndexMode.NeverIndexed:
					if (!this.WriteByteBits(1, 4) || !this.WriteInteger(Index))
						return false;
					break;

				default:
					return false;

			}

			if (Index == 0)
			{
				if (!this.WriteString(Header, Huffman, out c))
					return false;

				if (!(DynamicRecord is null))
					DynamicRecord.Length += c;
			}

			if (!this.WriteString(Value, Huffman, out c))
				return false;

			if (!(DynamicRecord is null))
			{
				DynamicRecord.Length += c;

				this.dynamicHeaderSize += DynamicRecord.Length;
			}

			return true;
		}
	}
}
