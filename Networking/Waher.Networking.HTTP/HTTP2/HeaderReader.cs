using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Deserializes HTTP/2 headers using HPACK, defined in RFC 7541.
	/// https://datatracker.ietf.org/doc/html/rfc7541
	/// </summary>
	public class HeaderReader : DynamicHeaders
	{
		private readonly uint bufferSize;
		private readonly byte[] buffer;
		private byte[] stringBuffer = new byte[128];
		private int stringBufferLen = 128;
		private uint pos;
		private byte bitsLeft = 0;
		private byte current = 0;

		/// <summary>
		/// Deserializes HTTP/2 headers using HPACK, defined in RFC 7541.
		/// https://datatracker.ietf.org/doc/html/rfc7541
		/// </summary>
		/// <param name="Buffer">Input buffer.</param>
		/// <param name="MaxDynamicHeaderSize">Maximum dynamic header size.</param>
		public HeaderReader(byte[] Buffer, uint MaxDynamicHeaderSize)
			: this(Buffer, 0, (uint)Buffer.Length, MaxDynamicHeaderSize, MaxDynamicHeaderSize)
		{
		}

		/// <summary>
		/// Deserializes HTTP/2 headers using HPACK, defined in RFC 7541.
		/// https://datatracker.ietf.org/doc/html/rfc7541
		/// </summary>
		/// <param name="Buffer">Input buffer.</param>
		/// <param name="Offset">Start reading at this position.</param>
		/// <param name="Count">Number of bytes to read.</param>
		/// <param name="MaxDynamicHeaderSize">Maximum dynamic header size.</param>
		/// <param name="MaxDynamicHeaderSizeLimit">Upper limit of the maximum dynamic header size.</param>
		public HeaderReader(byte[] Buffer, uint Offset, uint Count, uint MaxDynamicHeaderSize, uint MaxDynamicHeaderSizeLimit)
			: base(MaxDynamicHeaderSize, MaxDynamicHeaderSizeLimit)
		{
			this.bufferSize = Offset + Count;
			this.buffer = Buffer;
			this.pos = Offset;
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
		/// Resets the reader for a new header, without clearing the dynamic header table.
		/// </summary>
		public override void Reset()
		{
			this.pos = 0;
			this.bitsLeft = 8;
			this.current = 0;
		}

		/// <summary>
		/// Reads a single bit.
		/// </summary>
		/// <param name="Bit">Bit value read.</param>
		/// <returns>If read was successful (true), or if buffer size did not permit read operation (false).</returns>
		public bool ReadBit(out bool Bit)
		{
			if (this.bitsLeft == 0)
			{
				if (this.pos >= this.bufferSize)
				{
					Bit = false;
					return false;
				}

				this.current = this.buffer[this.pos++];
				this.bitsLeft = 8;
			}

			Bit = (this.current & 0x80) != 0;
			this.current <<= 1;
			this.bitsLeft--;

			return true;
		}

		/// <summary>
		/// Reads a set of bits.
		/// </summary>
		/// <param name="Bits">Bits read.</param>
		/// <param name="NrBits">Number of bits to read.</param>
		/// <returns>If read was successful (true), or if buffer size did not permit read operation (false).</returns>
		public bool ReadByteBits(out byte Bits, byte NrBits)
		{
			if (NrBits <= 0)
				throw new ArgumentException("Must be positive.", nameof(NrBits));

			if (this.bitsLeft == 0)
			{
				if (this.pos >= this.bufferSize)
				{
					Bits = 0;
					return false;
				}

				this.current = this.buffer[this.pos++];
				this.bitsLeft = 8;
			}

			if (NrBits <= this.bitsLeft)
			{
				Bits = this.current;
				this.current <<= NrBits;
				Bits >>= 8 - NrBits;

				this.bitsLeft -= NrBits;
			}
			else
			{
				byte n = (byte)(NrBits - this.bitsLeft);

				Bits = this.current;
				Bits >>= 8 - n;
				NrBits -= n;

				if (this.pos >= this.bufferSize)
					return false;

				this.current = this.buffer[this.pos++];
				this.bitsLeft = 8;

				Bits <<= NrBits;
				Bits |= (byte)(this.current >> 8 - NrBits);
				this.current <<= NrBits;
				this.bitsLeft -= NrBits;
			}

			return true;
		}

		/// <summary>
		/// Reads a variable-length integer
		/// </summary>
		/// <param name="Value">Value read.</param>
		/// <returns>If read was successful (true), or if buffer size did not permit read operation (false).</returns>
		public bool ReadInteger(out ulong Value)
		{
			if (this.bitsLeft == 0)
			{
				if (this.pos >= this.bufferSize)
				{
					Value = 0;
					return false;
				}

				this.current = this.buffer[this.pos++];
				this.bitsLeft = 8;
			}

			byte Mask = bitMasks[this.bitsLeft];
			Value = (byte)(this.current & Mask);
			this.bitsLeft = 0;

			if (Value < Mask)
				return true;

			do
			{
				if (this.pos >= this.bufferSize)
					return false;

				Mask = this.buffer[this.pos++];

				Value <<= 7;
				Value |= (byte)(Mask & 127);
			}
			while ((Mask & 128) != 0);

			return true;
		}

		/// <summary>
		/// Reads a string
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="Length">Length of string, for use with calculation of dynamic header record size.</param>
		/// <returns>If read was successful (true), or if buffer size did not permit read operation (false).</returns>
		public bool ReadString(out string Value, out uint Length)
		{
			if (this.ReadInteger(out ulong l) && l <= uint.MaxValue)
				Length = (uint)l;
			else
			{
				Value = null;
				Length = 0;
				return false;
			}

			if (!this.ReadBit(out bool Huffman))
			{
				Value = null;
				return false;
			}

			if (Huffman)
			{
				uint EndPos = this.pos + Length;
				HuffmanDecoding Loop = huffmanDecodingRoot;
				int StringPos = 0;

				while (this.pos < EndPos || this.bitsLeft > 0)
				{
					if (!this.ReadBit(out bool Bit))
					{
						Value = null;
						return false;
					}

					if (Bit)
						Loop = Loop.One;
					else
						Loop = Loop.Zero;

					if (Loop.LeafNode)
					{
						if (StringPos >= this.stringBufferLen)
						{
							Array.Resize(ref this.stringBuffer, this.stringBufferLen + 128);
							this.stringBufferLen += 128;
						}

						this.stringBuffer[StringPos++] = Loop.Value.Value;
						Loop = huffmanDecodingRoot;
					}
				}

				if (!Loop.PartOfEoS)
				{
					Value = null;
					return false;
				}

				Value = Encoding.UTF8.GetString(this.stringBuffer, 0, StringPos);
			}
			else
			{
				byte[] Bin = new byte[Length];
				int i;

				for (i = 0; i < Length; i++)
				{
					if (!this.ReadByteBits(out Bin[i], 8))
					{
						Value = null;
						return false;
					}
				}

				Value = Encoding.UTF8.GetString(Bin);
			}

			return true;
		}

		/// <summary>
		/// Reads a header and a value pair.
		/// </summary>
		/// <param name="Header">Header</param>
		/// <param name="Value">Value</param>
		/// <param name="Mode">Serialization mode with respect to indexing.</param>
		/// <returns>If read was successful (true), or if buffer size did not permit read operation (false).</returns>
		public bool ReadHeader(out string Header, out string Value, out IndexMode Mode)
		{
			DynamicHeader DynamicHeader = null;
			DynamicRecord DynamicRecord;
			ulong Index;
			uint HeaderLen;

			if (this.dynamicHeaderSize > this.maxDynamicHeaderSize)
				this.TrimDynamicHeaders();

			if (!this.ReadBit(out bool Bit))
			{
				Header = Value = null;
				Mode = IndexMode.NotIndexed;
				return false;
			}

			if (Bit)    // Header & Value in index.
			{
				if (!this.ReadInteger(out Index))
				{
					Header = Value = null;
					Mode = IndexMode.NotIndexed;
					return false;
				}

				if (Index <= lastStaticHeaderIndex)
				{
					StaticRecord Rec = staticTable[Index - 1];
					Header = Rec.Header;
					Value = Rec.Value;
					Mode = IndexMode.Indexed;
					return true;
				}
				else
				{
					Index -= lastStaticHeaderIndex + 1;
					if (Index >= this.nrDynamicRecords)
					{
						Header = Value = null;
						Mode = IndexMode.NotIndexed;
						return false;
					}

					DynamicRecord Rec = this.dynamicRecords[(int)Index];
					Header = Rec.Header.Header;
					Value = Rec.Value;
					Mode = IndexMode.Indexed;
					return true;
				}
			}
			else
			{
				if (!this.ReadBit(out Bit))
				{
					Header = Value = null;
					Mode = IndexMode.NotIndexed;
					return false;
				}

				if (Bit)
					Mode = IndexMode.Indexed;
				else
				{
					if (!this.ReadByteBits(out byte b, 2))
					{
						Header = Value = null;
						Mode = IndexMode.NotIndexed;
						return false;
					}

					if (b == 0)
						Mode = IndexMode.NotIndexed;
					else if (b == 1)
						Mode = IndexMode.NeverIndexed;
					else
					{
						Header = Value = null;
						Mode = IndexMode.NotIndexed;
						return false;
					}
				}

				if (!this.ReadInteger(out Index))
				{
					Header = Value = null;
					return false;
				}

				if (Index == 0)
				{
					if (!this.ReadString(out Header, out HeaderLen))
					{
						Value = null;
						return false;
					}
				}
				else if (Index <= lastStaticHeaderIndex)
				{
					StaticRecord Rec = staticTable[(int)Index];
					Header = Rec.Header;
					HeaderLen = Rec.HeaderLength;
				}
				else
				{
					Index -= lastStaticHeaderIndex + 1;
					if (Index >= this.nrDynamicRecords)
					{
						Header = Value = null;
						return false;
					}

					DynamicRecord Rec = this.dynamicRecords[(int)Index];
					DynamicHeader = Rec.Header;
					Header = DynamicHeader.Header;
					HeaderLen = DynamicHeader.HeaderLength;
				}

				if (!this.ReadString(out Value, out uint ValueLen))
					return false;

				if (Mode == IndexMode.Indexed)
				{
					if (DynamicHeader is null)
						DynamicRecord = this.AddToDynamicIndex(Header, Value);
					else
						DynamicRecord = this.AddToDynamicIndex(DynamicHeader, Value);

					DynamicRecord.Length += ValueLen + HeaderLen;
					this.dynamicHeaderSize += DynamicRecord.Length;
				}
			}

			return true;
		}

		/// <summary>
		/// If there's more data to read.
		/// </summary>
		public bool HasMore
		{
			get
			{
				if (this.pos < this.bufferSize)
					return true;

				if (this.pos > this.bufferSize)
					return false;

				return this.bitsLeft > 0;
			}
		}

		/// <summary>
		/// Reads available fields.
		/// </summary>
		/// <returns></returns>
		public bool ReadFields(out IEnumerable<HttpField> Fields)
		{
			LinkedList<HttpField> Result = new LinkedList<HttpField>();
			Fields = Result;

			while (this.HasMore)
			{
				if (!this.ReadHeader(out string Header, out string Value, out _))
					return false;

				Result.AddLast(new HttpField(Header, Value));
			}

			return true;
		}
	}
}
