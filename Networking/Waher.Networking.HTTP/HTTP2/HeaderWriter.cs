using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Generates HTTP/2 headers.
	/// </summary>
	public class HeaderWriter
	{
		private readonly static Dictionary<string, StaticRecord[]> staticHeaders = new Dictionary<string, StaticRecord[]>()
		{
			{ ":authority", new StaticRecord[] { new StaticRecord(1) } },
			{
				":method", new StaticRecord[]
				{
					new StaticRecord(2, "GET"),
					new StaticRecord(3, "POST")
				}
			},
			{
				":path", new StaticRecord[]
				{
					new StaticRecord(4, "/"),
					new StaticRecord(5, "/index.html")
				}
			},
			{
				":scheme", new StaticRecord[]
				{
					new StaticRecord(6, "http"),
					new StaticRecord(7, "https")
				}
			},
			{
				":status", new StaticRecord[]
				{
					new StaticRecord(8, "200"),
					new StaticRecord(9, "204"),
					new StaticRecord(10, "206"),
					new StaticRecord(11, "304"),
					new StaticRecord(12, "400"),
					new StaticRecord(13, "404"),
					new StaticRecord(14, "500")
				}
			},
			{ "accept-charset", new StaticRecord[] { new StaticRecord(15) } },
			{ "accept-encoding", new StaticRecord[] { new StaticRecord(16, "gzip, deflate") } },
			{ "accept-language", new StaticRecord[] { new StaticRecord(17) } },
			{ "accept-ranges", new StaticRecord[] { new StaticRecord(18) } },
			{ "accept", new StaticRecord[] { new StaticRecord(19) } },
			{ "access-control-allow-origin", new StaticRecord[] { new StaticRecord(20) } },
			{ "age", new StaticRecord[] { new StaticRecord(21) } },
			{ "allow", new StaticRecord[] { new StaticRecord(22) } },
			{ "authorization", new StaticRecord[] { new StaticRecord(23) } },
			{ "cache-control", new StaticRecord[] { new StaticRecord(24) } },
			{ "content-disposition", new StaticRecord[] { new StaticRecord(25) } },
			{ "content-encoding", new StaticRecord[] { new StaticRecord(26) } },
			{ "content-language", new StaticRecord[] { new StaticRecord(27) } },
			{ "content-length", new StaticRecord[] { new StaticRecord(28) } },
			{ "content-location", new StaticRecord[] { new StaticRecord(29) } },
			{ "content-range", new StaticRecord[] { new StaticRecord(30) } },
			{ "content-type", new StaticRecord[] { new StaticRecord(31) } },
			{ "cookie", new StaticRecord[] { new StaticRecord(32) } },
			{ "date", new StaticRecord[] { new StaticRecord(33) } },
			{ "etag", new StaticRecord[] { new StaticRecord(34) } },
			{ "expect", new StaticRecord[] { new StaticRecord(35) } },
			{ "expires", new StaticRecord[] { new StaticRecord(36) } },
			{ "from", new StaticRecord[] { new StaticRecord(37) } },
			{ "host", new StaticRecord[] { new StaticRecord(38) } },
			{ "if-match", new StaticRecord[] { new StaticRecord(39) } },
			{ "if-modified-since", new StaticRecord[] { new StaticRecord(40) } },
			{ "if-none-match", new StaticRecord[] { new StaticRecord(41) } },
			{ "if-range", new StaticRecord[] { new StaticRecord(42) } },
			{ "if-unmodified-since", new StaticRecord[] { new StaticRecord(43) } },
			{ "last-modified", new StaticRecord[] { new StaticRecord(44) } },
			{ "link", new StaticRecord[] { new StaticRecord(45) } },
			{ "location", new StaticRecord[] { new StaticRecord(46) } },
			{ "max-forwards", new StaticRecord[] { new StaticRecord(47) } },
			{ "proxy-authenticate", new StaticRecord[] { new StaticRecord(48) } },
			{ "proxy-authorization", new StaticRecord[] { new StaticRecord(49) } },
			{ "range", new StaticRecord[] { new StaticRecord(50) } },
			{ "referer", new StaticRecord[] { new StaticRecord(51) } },
			{ "refresh", new StaticRecord[] { new StaticRecord(52) } },
			{ "retry-after", new StaticRecord[] { new StaticRecord(53) } },
			{ "server", new StaticRecord[] { new StaticRecord(54) } },
			{ "set-cookie", new StaticRecord[] { new StaticRecord(55) } },
			{ "strict-transport-security", new StaticRecord[] { new StaticRecord(56) } },
			{ "transfer-encoding", new StaticRecord[] { new StaticRecord(57) } },
			{ "user-agent", new StaticRecord[] { new StaticRecord(58) } },
			{ "vary", new StaticRecord[] { new StaticRecord(59) } },
			{ "via", new StaticRecord[] { new StaticRecord(60) } },
			{ "www-authenticate", new StaticRecord[] { new StaticRecord(61) } }
		};
		private const int lastStaticHeaderIndex = 61;

		private class StaticRecord
		{
			public readonly uint Index;
			public readonly string Value;
			public uint Length = 0;

			public StaticRecord(uint Index)
				: this(Index, null)
			{
			}

			public StaticRecord(uint Index, string Value)
			{
				this.Index = Index;
				this.Value = Value;
			}
		}

		private static readonly HuffmanRecord[] huffmanTable = new HuffmanRecord[257]
		{
			new HuffmanRecord(0x1ff8, 13),
			new HuffmanRecord(0x7fffd8, 23),
			new HuffmanRecord(0xfffffe2, 28),
			new HuffmanRecord(0xfffffe3, 28),
			new HuffmanRecord(0xfffffe4, 28),
			new HuffmanRecord(0xfffffe5, 28),
			new HuffmanRecord(0xfffffe6, 28),
			new HuffmanRecord(0xfffffe7, 28),
			new HuffmanRecord(0xfffffe8, 28),
			new HuffmanRecord(0xffffea, 24),
			new HuffmanRecord(0x3ffffffc, 30),
			new HuffmanRecord(0xfffffe9, 28),
			new HuffmanRecord(0xfffffea, 28),
			new HuffmanRecord(0x3ffffffd, 30),
			new HuffmanRecord(0xfffffeb, 28),
			new HuffmanRecord(0xfffffec, 28),
			new HuffmanRecord(0xfffffed, 28),
			new HuffmanRecord(0xfffffee, 28),
			new HuffmanRecord(0xfffffef, 28),
			new HuffmanRecord(0xffffff0, 28),
			new HuffmanRecord(0xffffff1, 28),
			new HuffmanRecord(0xffffff2, 28),
			new HuffmanRecord(0x3ffffffe, 30),
			new HuffmanRecord(0xffffff3, 28),
			new HuffmanRecord(0xffffff4, 28),
			new HuffmanRecord(0xffffff5, 28),
			new HuffmanRecord(0xffffff6, 28),
			new HuffmanRecord(0xffffff7, 28),
			new HuffmanRecord(0xffffff8, 28),
			new HuffmanRecord(0xffffff9, 28),
			new HuffmanRecord(0xffffffa, 28),
			new HuffmanRecord(0xffffffb, 28),
			new HuffmanRecord(0x14, 6),
			new HuffmanRecord(0x3f8, 10),
			new HuffmanRecord(0x3f9, 10),
			new HuffmanRecord(0xffa, 12),
			new HuffmanRecord(0x1ff9, 13),
			new HuffmanRecord(0x15, 6),
			new HuffmanRecord(0xf8, 8),
			new HuffmanRecord(0x7fa, 11),
			new HuffmanRecord(0x3fa, 10),
			new HuffmanRecord(0x3fb, 10),
			new HuffmanRecord(0xf9, 8),
			new HuffmanRecord(0x7fb, 11),
			new HuffmanRecord(0xfa, 8),
			new HuffmanRecord(0x16, 6),
			new HuffmanRecord(0x17, 6),
			new HuffmanRecord(0x18, 6),
			new HuffmanRecord(0x0, 5),
			new HuffmanRecord(0x1, 5),
			new HuffmanRecord(0x2, 5),
			new HuffmanRecord(0x19, 6),
			new HuffmanRecord(0x1a, 6),
			new HuffmanRecord(0x1b, 6),
			new HuffmanRecord(0x1c, 6),
			new HuffmanRecord(0x1d, 6),
			new HuffmanRecord(0x1e, 6),
			new HuffmanRecord(0x1f, 6),
			new HuffmanRecord(0x5c, 7),
			new HuffmanRecord(0xfb, 8),
			new HuffmanRecord(0x7ffc, 15),
			new HuffmanRecord(0x20, 6),
			new HuffmanRecord(0xffb, 12),
			new HuffmanRecord(0x3fc, 10),
			new HuffmanRecord(0x1ffa, 13),
			new HuffmanRecord(0x21, 6),
			new HuffmanRecord(0x5d, 7),
			new HuffmanRecord(0x5e, 7),
			new HuffmanRecord(0x5f, 7),
			new HuffmanRecord(0x60, 7),
			new HuffmanRecord(0x61, 7),
			new HuffmanRecord(0x62, 7),
			new HuffmanRecord(0x63, 7),
			new HuffmanRecord(0x64, 7),
			new HuffmanRecord(0x65, 7),
			new HuffmanRecord(0x66, 7),
			new HuffmanRecord(0x67, 7),
			new HuffmanRecord(0x68, 7),
			new HuffmanRecord(0x69, 7),
			new HuffmanRecord(0x6a, 7),
			new HuffmanRecord(0x6b, 7),
			new HuffmanRecord(0x6c, 7),
			new HuffmanRecord(0x6d, 7),
			new HuffmanRecord(0x6e, 7),
			new HuffmanRecord(0x6f, 7),
			new HuffmanRecord(0x70, 7),
			new HuffmanRecord(0x71, 7),
			new HuffmanRecord(0x72, 7),
			new HuffmanRecord(0xfc, 8),
			new HuffmanRecord(0x73, 7),
			new HuffmanRecord(0xfd, 8),
			new HuffmanRecord(0x1ffb, 13),
			new HuffmanRecord(0x7fff0, 19),
			new HuffmanRecord(0x1ffc, 13),
			new HuffmanRecord(0x3ffc, 14),
			new HuffmanRecord(0x22, 6),
			new HuffmanRecord(0x7ffd, 15),
			new HuffmanRecord(0x3, 5),
			new HuffmanRecord(0x23, 6),
			new HuffmanRecord(0x4, 5),
			new HuffmanRecord(0x24, 6),
			new HuffmanRecord(0x5, 5),
			new HuffmanRecord(0x25, 6),
			new HuffmanRecord(0x26, 6),
			new HuffmanRecord(0x27, 6),
			new HuffmanRecord(0x6, 5),
			new HuffmanRecord(0x74, 7),
			new HuffmanRecord(0x75, 7),
			new HuffmanRecord(0x28, 6),
			new HuffmanRecord(0x29, 6),
			new HuffmanRecord(0x2a, 6),
			new HuffmanRecord(0x7, 5),
			new HuffmanRecord(0x2b, 6),
			new HuffmanRecord(0x76, 7),
			new HuffmanRecord(0x2c, 6),
			new HuffmanRecord(0x8, 5),
			new HuffmanRecord(0x9, 5),
			new HuffmanRecord(0x2d, 6),
			new HuffmanRecord(0x77, 7),
			new HuffmanRecord(0x78, 7),
			new HuffmanRecord(0x79, 7),
			new HuffmanRecord(0x7a, 7),
			new HuffmanRecord(0x7b, 7),
			new HuffmanRecord(0x7ffe, 15),
			new HuffmanRecord(0x7fc, 11),
			new HuffmanRecord(0x3ffd, 14),
			new HuffmanRecord(0x1ffd, 13),
			new HuffmanRecord(0xffffffc, 28),
			new HuffmanRecord(0xfffe6, 20),
			new HuffmanRecord(0x3fffd2, 22),
			new HuffmanRecord(0xfffe7, 20),
			new HuffmanRecord(0xfffe8, 20),
			new HuffmanRecord(0x3fffd3, 22),
			new HuffmanRecord(0x3fffd4, 22),
			new HuffmanRecord(0x3fffd5, 22),
			new HuffmanRecord(0x7fffd9, 23),
			new HuffmanRecord(0x3fffd6, 22),
			new HuffmanRecord(0x7fffda, 23),
			new HuffmanRecord(0x7fffdb, 23),
			new HuffmanRecord(0x7fffdc, 23),
			new HuffmanRecord(0x7fffdd, 23),
			new HuffmanRecord(0x7fffde, 23),
			new HuffmanRecord(0xffffeb, 24),
			new HuffmanRecord(0x7fffdf, 23),
			new HuffmanRecord(0xffffec, 24),
			new HuffmanRecord(0xffffed, 24),
			new HuffmanRecord(0x3fffd7, 22),
			new HuffmanRecord(0x7fffe0, 23),
			new HuffmanRecord(0xffffee, 24),
			new HuffmanRecord(0x7fffe1, 23),
			new HuffmanRecord(0x7fffe2, 23),
			new HuffmanRecord(0x7fffe3, 23),
			new HuffmanRecord(0x7fffe4, 23),
			new HuffmanRecord(0x1fffdc, 21),
			new HuffmanRecord(0x3fffd8, 22),
			new HuffmanRecord(0x7fffe5, 23),
			new HuffmanRecord(0x3fffd9, 22),
			new HuffmanRecord(0x7fffe6, 23),
			new HuffmanRecord(0x7fffe7, 23),
			new HuffmanRecord(0xffffef, 24),
			new HuffmanRecord(0x3fffda, 22),
			new HuffmanRecord(0x1fffdd, 21),
			new HuffmanRecord(0xfffe9, 20),
			new HuffmanRecord(0x3fffdb, 22),
			new HuffmanRecord(0x3fffdc, 22),
			new HuffmanRecord(0x7fffe8, 23),
			new HuffmanRecord(0x7fffe9, 23),
			new HuffmanRecord(0x1fffde, 21),
			new HuffmanRecord(0x7fffea, 23),
			new HuffmanRecord(0x3fffdd, 22),
			new HuffmanRecord(0x3fffde, 22),
			new HuffmanRecord(0xfffff0, 24),
			new HuffmanRecord(0x1fffdf, 21),
			new HuffmanRecord(0x3fffdf, 22),
			new HuffmanRecord(0x7fffeb, 23),
			new HuffmanRecord(0x7fffec, 23),
			new HuffmanRecord(0x1fffe0, 21),
			new HuffmanRecord(0x1fffe1, 21),
			new HuffmanRecord(0x3fffe0, 22),
			new HuffmanRecord(0x1fffe2, 21),
			new HuffmanRecord(0x7fffed, 23),
			new HuffmanRecord(0x3fffe1, 22),
			new HuffmanRecord(0x7fffee, 23),
			new HuffmanRecord(0x7fffef, 23),
			new HuffmanRecord(0xfffea, 20),
			new HuffmanRecord(0x3fffe2, 22),
			new HuffmanRecord(0x3fffe3, 22),
			new HuffmanRecord(0x3fffe4, 22),
			new HuffmanRecord(0x7ffff0, 23),
			new HuffmanRecord(0x3fffe5, 22),
			new HuffmanRecord(0x3fffe6, 22),
			new HuffmanRecord(0x7ffff1, 23),
			new HuffmanRecord(0x3ffffe0, 26),
			new HuffmanRecord(0x3ffffe1, 26),
			new HuffmanRecord(0xfffeb, 20),
			new HuffmanRecord(0x7fff1, 19),
			new HuffmanRecord(0x3fffe7, 22),
			new HuffmanRecord(0x7ffff2, 23),
			new HuffmanRecord(0x3fffe8, 22),
			new HuffmanRecord(0x1ffffec, 25),
			new HuffmanRecord(0x3ffffe2, 26),
			new HuffmanRecord(0x3ffffe3, 26),
			new HuffmanRecord(0x3ffffe4, 26),
			new HuffmanRecord(0x7ffffde, 27),
			new HuffmanRecord(0x7ffffdf, 27),
			new HuffmanRecord(0x3ffffe5, 26),
			new HuffmanRecord(0xfffff1, 24),
			new HuffmanRecord(0x1ffffed, 25),
			new HuffmanRecord(0x7fff2, 19),
			new HuffmanRecord(0x1fffe3, 21),
			new HuffmanRecord(0x3ffffe6, 26),
			new HuffmanRecord(0x7ffffe0, 27),
			new HuffmanRecord(0x7ffffe1, 27),
			new HuffmanRecord(0x3ffffe7, 26),
			new HuffmanRecord(0x7ffffe2, 27),
			new HuffmanRecord(0xfffff2, 24),
			new HuffmanRecord(0x1fffe4, 21),
			new HuffmanRecord(0x1fffe5, 21),
			new HuffmanRecord(0x3ffffe8, 26),
			new HuffmanRecord(0x3ffffe9, 26),
			new HuffmanRecord(0xffffffd, 28),
			new HuffmanRecord(0x7ffffe3, 27),
			new HuffmanRecord(0x7ffffe4, 27),
			new HuffmanRecord(0x7ffffe5, 27),
			new HuffmanRecord(0xfffec, 20),
			new HuffmanRecord(0xfffff3, 24),
			new HuffmanRecord(0xfffed, 20),
			new HuffmanRecord(0x1fffe6, 21),
			new HuffmanRecord(0x3fffe9, 22),
			new HuffmanRecord(0x1fffe7, 21),
			new HuffmanRecord(0x1fffe8, 21),
			new HuffmanRecord(0x7ffff3, 23),
			new HuffmanRecord(0x3fffea, 22),
			new HuffmanRecord(0x3fffeb, 22),
			new HuffmanRecord(0x1ffffee, 25),
			new HuffmanRecord(0x1ffffef, 25),
			new HuffmanRecord(0xfffff4, 24),
			new HuffmanRecord(0xfffff5, 24),
			new HuffmanRecord(0x3ffffea, 26),
			new HuffmanRecord(0x7ffff4, 23),
			new HuffmanRecord(0x3ffffeb, 26),
			new HuffmanRecord(0x7ffffe6, 27),
			new HuffmanRecord(0x3ffffec, 26),
			new HuffmanRecord(0x3ffffed, 26),
			new HuffmanRecord(0x7ffffe7, 27),
			new HuffmanRecord(0x7ffffe8, 27),
			new HuffmanRecord(0x7ffffe9, 27),
			new HuffmanRecord(0x7ffffea, 27),
			new HuffmanRecord(0x7ffffeb, 27),
			new HuffmanRecord(0xffffffe, 28),
			new HuffmanRecord(0x7ffffec, 27),
			new HuffmanRecord(0x7ffffed, 27),
			new HuffmanRecord(0x7ffffee, 27),
			new HuffmanRecord(0x7ffffef, 27),
			new HuffmanRecord(0x7fffff0, 27),
			new HuffmanRecord(0x3ffffee, 26),
			new HuffmanRecord(0x3fffffff, 30)
		};

		/// <summary>
		/// End of string code.
		/// </summary>
		private const int EOS = 256;

		private class HuffmanRecord
		{
			public readonly uint Value;
			public readonly byte NrBits;

			public HuffmanRecord(uint Value, byte NrBits)
			{
				this.Value = Value;
				this.NrBits = NrBits;
			}
		}

		private readonly Dictionary<string, DynamicHeader> dynamicHeaders = new Dictionary<string, DynamicHeader>();
		private readonly LinkedList<DynamicRecord> dynamicRecords = new LinkedList<DynamicRecord>();
		private readonly uint bufferSize;
		private readonly byte[] buffer;
		private uint maxDynamicHeaderSize;
		private uint dynamicHeaderSize = 0;
		private uint nrDynamicRecords = 0;
		private ulong nrHeadersCreated = 0;
		private uint pos = 0;
		private byte bitsLeft = 8;
		private byte current = 0;

		/// <summary>
		/// Generates HTTP/2 headers.
		/// </summary>
		/// <param name="BufferSize">Size of binary buffer.</param>
		/// <param name="MaxDynamicHeaderSize">Maximum dynamic header size.</param>
		public HeaderWriter(uint BufferSize, uint MaxDynamicHeaderSize)
		{
			this.bufferSize = BufferSize;
			this.buffer = new byte[this.bufferSize];
			this.maxDynamicHeaderSize = MaxDynamicHeaderSize;
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
		/// Number of headers available in the dynamic headers table.
		/// </summary>
		public uint NrDynamicHeaderRecords => this.nrDynamicRecords;

		/// <summary>
		/// Size of dynamic header.
		/// </summary>
		public uint DynamicHeaderSize => this.dynamicHeaderSize;

		/// <summary>
		/// Maximum size of dynamic header.
		/// </summary>
		public uint MaxDynamicHeaderSize => this.maxDynamicHeaderSize;

		/// <summary>
		/// Gets a copy of available dynamic records
		/// </summary>
		public DynamicRecord[] GetDynamicRecords()
		{
			DynamicRecord[] Records = new DynamicRecord[this.nrDynamicRecords];
			this.dynamicRecords.CopyTo(Records, 0);
			return Records;
		}

		/// <summary>
		/// Flushes any remaining bits.
		/// </summary>
		/// <returns>If write was successful (true), or if buffer size did not permit write operation (false).</returns>
		public bool Flush()
		{
			if (this.bitsLeft < 8)
				return this.WriteBits(0, this.bitsLeft);
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
		/// Clears the writer for a new header, clearing the dynamic header table.
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
		public bool WriteBits(ulong Bits, byte NrBits)
		{
			if (NrBits <= 0)
				throw new ArgumentException("Must be positive.", nameof(NrBits));

			byte n;

			while (NrBits > 0)
			{
				if (NrBits > this.bitsLeft)
					n = this.bitsLeft;
				else
					n = NrBits;

				this.current <<= n;
				this.current |= (byte)(Bits & bitMasks[n]);
				Bits >>= n;
				NrBits -= n;
				this.bitsLeft -= n;
				if (this.bitsLeft <= 0)
				{
					if (this.pos >= this.bufferSize)
						return false;

					this.buffer[this.pos++] = this.current;
					this.bitsLeft = 8;
					this.current = 0;
				}
			}

			return true;
		}

		private static readonly byte[] bitMasks = new[]
		{
			(byte)0b00000000,
			(byte)0b00000001,
			(byte)0b00000011,
			(byte)0b00000111,
			(byte)0b00001111,
			(byte)0b00011111,
			(byte)0b00111111,
			(byte)0b01111111,
			(byte)0b11111111
		};

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
				return this.WriteBits(Value, this.bitsLeft);

			if (!this.WriteBits(i, this.bitsLeft))
				return false;

			Value -= i;

			while (true)
			{
				if (Value < 128)
					return this.WriteBits(Value, 8);

				if (!this.WriteBits((Value & 127) | 128, 8))
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

			if (!this.WriteInteger(Length))
				return false;

			if (Huffman)
			{
				HuffmanRecord Rec;

				for (i = 0; i < Length; i++)
				{
					Rec = huffmanTable[Bin[i]];
					if (!this.WriteBits(Rec.Value, Rec.NrBits))
						return false;
				}
			}
			else
			{
				for (i = 0; i < Length; i++)
				{
					if (!this.WriteBits(Bin[i], 8))
						return false;
				}
			}

			return true;
		}

		private void TrimDynamicHeaders()
		{
			DynamicRecord DynamicRecord;

			while (this.dynamicHeaderSize > this.maxDynamicHeaderSize)
			{
				DynamicRecord = this.dynamicRecords.Last?.Value;
				if (DynamicRecord is null)
					return;

				this.dynamicRecords.RemoveLast();
				this.nrDynamicRecords--;

				if (DynamicRecord.Header.Values.Remove(DynamicRecord.Value) &&
					DynamicRecord.Header.Values.Count == 0)
				{
					this.dynamicHeaders.Remove(DynamicRecord.Header.Header);
				}

				this.dynamicHeaderSize -= DynamicRecord.Length;
			}
		}

		private DynamicRecord AddToDynamicIndex(DynamicHeader Header, string Value)
		{
			ulong Index = this.nrHeadersCreated++;

			DynamicRecord DynamicRecord = new DynamicRecord(Header, Value, 32, Index);
			this.dynamicRecords.AddFirst(DynamicRecord);
			this.nrDynamicRecords++;
			Header.Values[Value] = DynamicRecord;

			return DynamicRecord;
		}

		private DynamicRecord AddToDynamicIndex(string Header, string Value)
		{
			ulong Index = this.nrHeadersCreated++;

			DynamicHeader DynamicHeader = new DynamicHeader(Header, Index);
			this.dynamicHeaders[Header] = DynamicHeader;

			DynamicRecord DynamicRecord = new DynamicRecord(DynamicHeader, Value, 32, Index);
			this.dynamicRecords.AddFirst(DynamicRecord);
			this.nrDynamicRecords++;
			DynamicHeader.Values[Value] = DynamicRecord;

			return DynamicRecord;
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

						return this.WriteBits(Rec.Index, 7);
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
					return this.WriteBits(Index, 7);
				}

				if (Mode == IndexMode.Indexed)
				{
					if (Rec.Length == 0)
						Rec.Length = (uint)Encoding.UTF8.GetBytes(Header).Length;

					if (DynamicHeader is null)
						DynamicRecord = this.AddToDynamicIndex(Header, Value);
					else
						DynamicRecord = this.AddToDynamicIndex(DynamicHeader, Value);

					DynamicRecord.Length += Rec.Length;
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
						return this.WriteBits(Index, 7);
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
					if (!this.WriteBits(1, 2) || !this.WriteInteger(Index))
						return false;
					break;

				case IndexMode.NotIndexed:
					if (!this.WriteBits(0, 4) || !this.WriteInteger(Index))
						return false;
					break;

				case IndexMode.NeverIndexed:
					if (!this.WriteBits(1, 4) || !this.WriteInteger(Index))
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
