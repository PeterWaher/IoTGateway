using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Manages a table of dynamic headers.
	/// </summary>
	public abstract class DynamicHeaders
	{
		/// <summary>
		/// Information on how to encode bytes using the HPACK Huffman encoding.
		/// </summary>
		protected static readonly HuffmanEncoding[] huffmanEncodings = new HuffmanEncoding[257]
		{
			new HuffmanEncoding(0x1ff8, 13),
			new HuffmanEncoding(0x7fffd8, 23),
			new HuffmanEncoding(0xfffffe2, 28),
			new HuffmanEncoding(0xfffffe3, 28),
			new HuffmanEncoding(0xfffffe4, 28),
			new HuffmanEncoding(0xfffffe5, 28),
			new HuffmanEncoding(0xfffffe6, 28),
			new HuffmanEncoding(0xfffffe7, 28),
			new HuffmanEncoding(0xfffffe8, 28),
			new HuffmanEncoding(0xffffea, 24),
			new HuffmanEncoding(0x3ffffffc, 30),
			new HuffmanEncoding(0xfffffe9, 28),
			new HuffmanEncoding(0xfffffea, 28),
			new HuffmanEncoding(0x3ffffffd, 30),
			new HuffmanEncoding(0xfffffeb, 28),
			new HuffmanEncoding(0xfffffec, 28),
			new HuffmanEncoding(0xfffffed, 28),
			new HuffmanEncoding(0xfffffee, 28),
			new HuffmanEncoding(0xfffffef, 28),
			new HuffmanEncoding(0xffffff0, 28),
			new HuffmanEncoding(0xffffff1, 28),
			new HuffmanEncoding(0xffffff2, 28),
			new HuffmanEncoding(0x3ffffffe, 30),
			new HuffmanEncoding(0xffffff3, 28),
			new HuffmanEncoding(0xffffff4, 28),
			new HuffmanEncoding(0xffffff5, 28),
			new HuffmanEncoding(0xffffff6, 28),
			new HuffmanEncoding(0xffffff7, 28),
			new HuffmanEncoding(0xffffff8, 28),
			new HuffmanEncoding(0xffffff9, 28),
			new HuffmanEncoding(0xffffffa, 28),
			new HuffmanEncoding(0xffffffb, 28),
			new HuffmanEncoding(0x14, 6),
			new HuffmanEncoding(0x3f8, 10),
			new HuffmanEncoding(0x3f9, 10),
			new HuffmanEncoding(0xffa, 12),
			new HuffmanEncoding(0x1ff9, 13),
			new HuffmanEncoding(0x15, 6),
			new HuffmanEncoding(0xf8, 8),
			new HuffmanEncoding(0x7fa, 11),
			new HuffmanEncoding(0x3fa, 10),
			new HuffmanEncoding(0x3fb, 10),
			new HuffmanEncoding(0xf9, 8),
			new HuffmanEncoding(0x7fb, 11),
			new HuffmanEncoding(0xfa, 8),
			new HuffmanEncoding(0x16, 6),
			new HuffmanEncoding(0x17, 6),
			new HuffmanEncoding(0x18, 6),
			new HuffmanEncoding(0x0, 5),
			new HuffmanEncoding(0x1, 5),
			new HuffmanEncoding(0x2, 5),
			new HuffmanEncoding(0x19, 6),
			new HuffmanEncoding(0x1a, 6),
			new HuffmanEncoding(0x1b, 6),
			new HuffmanEncoding(0x1c, 6),
			new HuffmanEncoding(0x1d, 6),
			new HuffmanEncoding(0x1e, 6),
			new HuffmanEncoding(0x1f, 6),
			new HuffmanEncoding(0x5c, 7),
			new HuffmanEncoding(0xfb, 8),
			new HuffmanEncoding(0x7ffc, 15),
			new HuffmanEncoding(0x20, 6),
			new HuffmanEncoding(0xffb, 12),
			new HuffmanEncoding(0x3fc, 10),
			new HuffmanEncoding(0x1ffa, 13),
			new HuffmanEncoding(0x21, 6),
			new HuffmanEncoding(0x5d, 7),
			new HuffmanEncoding(0x5e, 7),
			new HuffmanEncoding(0x5f, 7),
			new HuffmanEncoding(0x60, 7),
			new HuffmanEncoding(0x61, 7),
			new HuffmanEncoding(0x62, 7),
			new HuffmanEncoding(0x63, 7),
			new HuffmanEncoding(0x64, 7),
			new HuffmanEncoding(0x65, 7),
			new HuffmanEncoding(0x66, 7),
			new HuffmanEncoding(0x67, 7),
			new HuffmanEncoding(0x68, 7),
			new HuffmanEncoding(0x69, 7),
			new HuffmanEncoding(0x6a, 7),
			new HuffmanEncoding(0x6b, 7),
			new HuffmanEncoding(0x6c, 7),
			new HuffmanEncoding(0x6d, 7),
			new HuffmanEncoding(0x6e, 7),
			new HuffmanEncoding(0x6f, 7),
			new HuffmanEncoding(0x70, 7),
			new HuffmanEncoding(0x71, 7),
			new HuffmanEncoding(0x72, 7),
			new HuffmanEncoding(0xfc, 8),
			new HuffmanEncoding(0x73, 7),
			new HuffmanEncoding(0xfd, 8),
			new HuffmanEncoding(0x1ffb, 13),
			new HuffmanEncoding(0x7fff0, 19),
			new HuffmanEncoding(0x1ffc, 13),
			new HuffmanEncoding(0x3ffc, 14),
			new HuffmanEncoding(0x22, 6),
			new HuffmanEncoding(0x7ffd, 15),
			new HuffmanEncoding(0x3, 5),
			new HuffmanEncoding(0x23, 6),
			new HuffmanEncoding(0x4, 5),
			new HuffmanEncoding(0x24, 6),
			new HuffmanEncoding(0x5, 5),
			new HuffmanEncoding(0x25, 6),
			new HuffmanEncoding(0x26, 6),
			new HuffmanEncoding(0x27, 6),
			new HuffmanEncoding(0x6, 5),
			new HuffmanEncoding(0x74, 7),
			new HuffmanEncoding(0x75, 7),
			new HuffmanEncoding(0x28, 6),
			new HuffmanEncoding(0x29, 6),
			new HuffmanEncoding(0x2a, 6),
			new HuffmanEncoding(0x7, 5),
			new HuffmanEncoding(0x2b, 6),
			new HuffmanEncoding(0x76, 7),
			new HuffmanEncoding(0x2c, 6),
			new HuffmanEncoding(0x8, 5),
			new HuffmanEncoding(0x9, 5),
			new HuffmanEncoding(0x2d, 6),
			new HuffmanEncoding(0x77, 7),
			new HuffmanEncoding(0x78, 7),
			new HuffmanEncoding(0x79, 7),
			new HuffmanEncoding(0x7a, 7),
			new HuffmanEncoding(0x7b, 7),
			new HuffmanEncoding(0x7ffe, 15),
			new HuffmanEncoding(0x7fc, 11),
			new HuffmanEncoding(0x3ffd, 14),
			new HuffmanEncoding(0x1ffd, 13),
			new HuffmanEncoding(0xffffffc, 28),
			new HuffmanEncoding(0xfffe6, 20),
			new HuffmanEncoding(0x3fffd2, 22),
			new HuffmanEncoding(0xfffe7, 20),
			new HuffmanEncoding(0xfffe8, 20),
			new HuffmanEncoding(0x3fffd3, 22),
			new HuffmanEncoding(0x3fffd4, 22),
			new HuffmanEncoding(0x3fffd5, 22),
			new HuffmanEncoding(0x7fffd9, 23),
			new HuffmanEncoding(0x3fffd6, 22),
			new HuffmanEncoding(0x7fffda, 23),
			new HuffmanEncoding(0x7fffdb, 23),
			new HuffmanEncoding(0x7fffdc, 23),
			new HuffmanEncoding(0x7fffdd, 23),
			new HuffmanEncoding(0x7fffde, 23),
			new HuffmanEncoding(0xffffeb, 24),
			new HuffmanEncoding(0x7fffdf, 23),
			new HuffmanEncoding(0xffffec, 24),
			new HuffmanEncoding(0xffffed, 24),
			new HuffmanEncoding(0x3fffd7, 22),
			new HuffmanEncoding(0x7fffe0, 23),
			new HuffmanEncoding(0xffffee, 24),
			new HuffmanEncoding(0x7fffe1, 23),
			new HuffmanEncoding(0x7fffe2, 23),
			new HuffmanEncoding(0x7fffe3, 23),
			new HuffmanEncoding(0x7fffe4, 23),
			new HuffmanEncoding(0x1fffdc, 21),
			new HuffmanEncoding(0x3fffd8, 22),
			new HuffmanEncoding(0x7fffe5, 23),
			new HuffmanEncoding(0x3fffd9, 22),
			new HuffmanEncoding(0x7fffe6, 23),
			new HuffmanEncoding(0x7fffe7, 23),
			new HuffmanEncoding(0xffffef, 24),
			new HuffmanEncoding(0x3fffda, 22),
			new HuffmanEncoding(0x1fffdd, 21),
			new HuffmanEncoding(0xfffe9, 20),
			new HuffmanEncoding(0x3fffdb, 22),
			new HuffmanEncoding(0x3fffdc, 22),
			new HuffmanEncoding(0x7fffe8, 23),
			new HuffmanEncoding(0x7fffe9, 23),
			new HuffmanEncoding(0x1fffde, 21),
			new HuffmanEncoding(0x7fffea, 23),
			new HuffmanEncoding(0x3fffdd, 22),
			new HuffmanEncoding(0x3fffde, 22),
			new HuffmanEncoding(0xfffff0, 24),
			new HuffmanEncoding(0x1fffdf, 21),
			new HuffmanEncoding(0x3fffdf, 22),
			new HuffmanEncoding(0x7fffeb, 23),
			new HuffmanEncoding(0x7fffec, 23),
			new HuffmanEncoding(0x1fffe0, 21),
			new HuffmanEncoding(0x1fffe1, 21),
			new HuffmanEncoding(0x3fffe0, 22),
			new HuffmanEncoding(0x1fffe2, 21),
			new HuffmanEncoding(0x7fffed, 23),
			new HuffmanEncoding(0x3fffe1, 22),
			new HuffmanEncoding(0x7fffee, 23),
			new HuffmanEncoding(0x7fffef, 23),
			new HuffmanEncoding(0xfffea, 20),
			new HuffmanEncoding(0x3fffe2, 22),
			new HuffmanEncoding(0x3fffe3, 22),
			new HuffmanEncoding(0x3fffe4, 22),
			new HuffmanEncoding(0x7ffff0, 23),
			new HuffmanEncoding(0x3fffe5, 22),
			new HuffmanEncoding(0x3fffe6, 22),
			new HuffmanEncoding(0x7ffff1, 23),
			new HuffmanEncoding(0x3ffffe0, 26),
			new HuffmanEncoding(0x3ffffe1, 26),
			new HuffmanEncoding(0xfffeb, 20),
			new HuffmanEncoding(0x7fff1, 19),
			new HuffmanEncoding(0x3fffe7, 22),
			new HuffmanEncoding(0x7ffff2, 23),
			new HuffmanEncoding(0x3fffe8, 22),
			new HuffmanEncoding(0x1ffffec, 25),
			new HuffmanEncoding(0x3ffffe2, 26),
			new HuffmanEncoding(0x3ffffe3, 26),
			new HuffmanEncoding(0x3ffffe4, 26),
			new HuffmanEncoding(0x7ffffde, 27),
			new HuffmanEncoding(0x7ffffdf, 27),
			new HuffmanEncoding(0x3ffffe5, 26),
			new HuffmanEncoding(0xfffff1, 24),
			new HuffmanEncoding(0x1ffffed, 25),
			new HuffmanEncoding(0x7fff2, 19),
			new HuffmanEncoding(0x1fffe3, 21),
			new HuffmanEncoding(0x3ffffe6, 26),
			new HuffmanEncoding(0x7ffffe0, 27),
			new HuffmanEncoding(0x7ffffe1, 27),
			new HuffmanEncoding(0x3ffffe7, 26),
			new HuffmanEncoding(0x7ffffe2, 27),
			new HuffmanEncoding(0xfffff2, 24),
			new HuffmanEncoding(0x1fffe4, 21),
			new HuffmanEncoding(0x1fffe5, 21),
			new HuffmanEncoding(0x3ffffe8, 26),
			new HuffmanEncoding(0x3ffffe9, 26),
			new HuffmanEncoding(0xffffffd, 28),
			new HuffmanEncoding(0x7ffffe3, 27),
			new HuffmanEncoding(0x7ffffe4, 27),
			new HuffmanEncoding(0x7ffffe5, 27),
			new HuffmanEncoding(0xfffec, 20),
			new HuffmanEncoding(0xfffff3, 24),
			new HuffmanEncoding(0xfffed, 20),
			new HuffmanEncoding(0x1fffe6, 21),
			new HuffmanEncoding(0x3fffe9, 22),
			new HuffmanEncoding(0x1fffe7, 21),
			new HuffmanEncoding(0x1fffe8, 21),
			new HuffmanEncoding(0x7ffff3, 23),
			new HuffmanEncoding(0x3fffea, 22),
			new HuffmanEncoding(0x3fffeb, 22),
			new HuffmanEncoding(0x1ffffee, 25),
			new HuffmanEncoding(0x1ffffef, 25),
			new HuffmanEncoding(0xfffff4, 24),
			new HuffmanEncoding(0xfffff5, 24),
			new HuffmanEncoding(0x3ffffea, 26),
			new HuffmanEncoding(0x7ffff4, 23),
			new HuffmanEncoding(0x3ffffeb, 26),
			new HuffmanEncoding(0x7ffffe6, 27),
			new HuffmanEncoding(0x3ffffec, 26),
			new HuffmanEncoding(0x3ffffed, 26),
			new HuffmanEncoding(0x7ffffe7, 27),
			new HuffmanEncoding(0x7ffffe8, 27),
			new HuffmanEncoding(0x7ffffe9, 27),
			new HuffmanEncoding(0x7ffffea, 27),
			new HuffmanEncoding(0x7ffffeb, 27),
			new HuffmanEncoding(0xffffffe, 28),
			new HuffmanEncoding(0x7ffffec, 27),
			new HuffmanEncoding(0x7ffffed, 27),
			new HuffmanEncoding(0x7ffffee, 27),
			new HuffmanEncoding(0x7ffffef, 27),
			new HuffmanEncoding(0x7fffff0, 27),
			new HuffmanEncoding(0x3ffffee, 26),
			new HuffmanEncoding(0x3fffffff, 30)
		};

		/// <summary>
		/// Huffman decoding root node.
		/// </summary>
		protected static readonly HuffmanDecoding huffmanDecodingRoot = GenerateHuffmanDecodingRoot();

		private static HuffmanDecoding GenerateHuffmanDecodingRoot()
		{
			HuffmanDecoding Root = new HuffmanDecoding();
			HuffmanDecoding Loop;
			HuffmanEncoding Encoding;
			uint Code;
			int i, c = huffmanEncodings.Length;
			bool PartOfEoS;
			byte n;

			for (i = 0; i < c; i++)
			{
				Encoding = huffmanEncodings[i];
				PartOfEoS = i >= 256;

				n = Encoding.NrBits;
				Code = Encoding.Value;

				Code <<= 32 - n;
				Loop = Root;
				Loop.PartOfEoS |= PartOfEoS;

				while (n > 1)
				{
					if ((Code & 0x80000000) == 0)
					{
						Loop.Zero ??= new HuffmanDecoding();
						Loop = Loop.Zero;
					}
					else
					{
						Loop.One ??= new HuffmanDecoding();
						Loop = Loop.One;
					}

					n--;
					Code <<= 1;
					Loop.PartOfEoS |= PartOfEoS;
				}

				if ((Code & 0x80000000) == 0)
					Loop.Zero ??= new HuffmanDecoding((byte)i, PartOfEoS);
				else
					Loop.One ??= new HuffmanDecoding((byte)i, PartOfEoS);
			}

			return Root;
		}

		/// <summary>
		/// Static header table.
		/// </summary>
		protected static readonly StaticRecord[] staticTable = new StaticRecord[]
		{
			new StaticRecord(1, ":authority"),
			new StaticRecord(2, ":method", "GET"),
			new StaticRecord(3, ":method", "POST"),
			new StaticRecord(4, ":path", "/"),
			new StaticRecord(5, ":path", "/index.html"),
			new StaticRecord(6, ":scheme", "http"),
			new StaticRecord(7, ":scheme", "https"),
			new StaticRecord(8, ":status", "200"),
			new StaticRecord(9, ":status", "204"),
			new StaticRecord(10, ":status", "206"),
			new StaticRecord(11, ":status", "304"),
			new StaticRecord(12, ":status", "400"),
			new StaticRecord(13, ":status", "404"),
			new StaticRecord(14, ":status", "500"),
			new StaticRecord(15, "accept-charset"),
			new StaticRecord(16, "accept-encoding", "gzip, deflate"),
			new StaticRecord(17, "accept-language"),
			new StaticRecord(18, "accept-ranges"),
			new StaticRecord(19, "accept"),
			new StaticRecord(20, "access-control-allow-origin"),
			new StaticRecord(21, "age"),
			new StaticRecord(22, "allow"),
			new StaticRecord(23, "authorization"),
			new StaticRecord(24, "cache-control"),
			new StaticRecord(25, "content-disposition"),
			new StaticRecord(26, "content-encoding"),
			new StaticRecord(27, "content-language"),
			new StaticRecord(28, "content-length"),
			new StaticRecord(29, "content-location"),
			new StaticRecord(30, "content-range"),
			new StaticRecord(31, "content-type"),
			new StaticRecord(32, "cookie"),
			new StaticRecord(33, "date"),
			new StaticRecord(34, "etag"),
			new StaticRecord(35, "expect"),
			new StaticRecord(36, "expires"),
			new StaticRecord(37, "from"),
			new StaticRecord(38, "host"),
			new StaticRecord(39, "if-match"),
			new StaticRecord(40, "if-modified-since"),
			new StaticRecord(41, "if-none-match"),
			new StaticRecord(42, "if-range"),
			new StaticRecord(43, "if-unmodified-since"),
			new StaticRecord(44, "last-modified"),
			new StaticRecord(45, "link"),
			new StaticRecord(46, "location"),
			new StaticRecord(47, "max-forwards"),
			new StaticRecord(48, "proxy-authenticate"),
			new StaticRecord(49, "proxy-authorization"),
			new StaticRecord(50, "range"),
			new StaticRecord(51, "referer"),
			new StaticRecord(52, "refresh"),
			new StaticRecord(53, "retry-after"),
			new StaticRecord(54, "server"),
			new StaticRecord(55, "set-cookie"),
			new StaticRecord(56, "strict-transport-security"),
			new StaticRecord(57, "transfer-encoding"),
			new StaticRecord(58, "user-agent"),
			new StaticRecord(59, "vary"),
			new StaticRecord(60, "via"),
			new StaticRecord(61, "www-authenticate")
		};

		/// <summary>
		/// Static headers table.
		/// </summary>
		protected readonly static Dictionary<string, StaticRecord[]> staticHeaders = SortStaticTable();

		/// <summary>
		/// Index of last record in static table.
		/// </summary>
		protected const int lastStaticHeaderIndex = 61;

		private static Dictionary<string, StaticRecord[]> SortStaticTable()
		{
			Dictionary<string, List<StaticRecord>> Ordered = new Dictionary<string, List<StaticRecord>>();

			foreach (StaticRecord Rec in staticTable)
			{
				if (!Ordered.TryGetValue(Rec.Header, out List<StaticRecord> List))
				{
					List = new List<StaticRecord>();
					Ordered[Rec.Header] = List;
				}

				List.Add(Rec);
			}

			Dictionary<string, StaticRecord[]> Result = new Dictionary<string, StaticRecord[]>();

			foreach (KeyValuePair<string, List<StaticRecord>> P in Ordered)
				Result[P.Key] = P.Value.ToArray();

			return Result;
		}

		/// <summary>
		/// Bit mask.
		/// </summary>
		protected static readonly byte[] bitMasks = new[]
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
		/// Dynamic headers, ordered by header
		/// </summary>
		protected readonly Dictionary<string, DynamicHeader> dynamicHeaders = new Dictionary<string, DynamicHeader>();

		/// <summary>
		/// Dynamic records in table.
		/// </summary>
		protected readonly List<DynamicRecord> dynamicRecords = new List<DynamicRecord>();

		/// <summary>
		/// Upper limit of maximum dynamic header table size.
		/// </summary>
		protected readonly int maxDynamicHeaderSizeLimit;

		/// <summary>
		/// Maximum size of the table of dynamic headers.
		/// </summary>
		protected int maxDynamicHeaderSize;

		/// <summary>
		/// Size of the dynamic headers table.
		/// </summary>
		protected int dynamicHeaderSize = 0;

		/// <summary>
		/// Number of records in the dynamic headers table.
		/// </summary>
		protected uint nrDynamicRecords = 0;

		/// <summary>
		/// Number of dynamic header records created.
		/// </summary>
		protected ulong nrHeadersCreated = 0;

		private readonly SemaphoreSlim syncObject = new SemaphoreSlim(1);
#if DEBUG
		private string lockedFrom = null;
#endif

		/// <summary>
		/// Serializes HTTP/2 headers using HPACK, defined in RFC 7541.
		/// https://datatracker.ietf.org/doc/html/rfc7541
		/// </summary>
		/// <param name="MaxDynamicHeaderSize">Maximum dynamic header size.</param>
		public DynamicHeaders(int MaxDynamicHeaderSize)
			: this(MaxDynamicHeaderSize, MaxDynamicHeaderSize)
		{
		}

		/// <summary>
		/// Serializes HTTP/2 headers using HPACK, defined in RFC 7541.
		/// https://datatracker.ietf.org/doc/html/rfc7541
		/// </summary>
		/// <param name="MaxDynamicHeaderSize">Maximum dynamic header size.</param>
		/// <param name="MaxDynamicHeaderSizeLimit">Upper limit of the maximum dynamic header size.</param>
		public DynamicHeaders(int MaxDynamicHeaderSize, int MaxDynamicHeaderSizeLimit)
		{
			this.maxDynamicHeaderSize = MaxDynamicHeaderSize;
			this.maxDynamicHeaderSizeLimit = MaxDynamicHeaderSizeLimit;
		}

		/// <summary>
		/// Number of headers available in the dynamic headers table.
		/// </summary>
		public uint NrDynamicHeaderRecords => this.nrDynamicRecords;

		/// <summary>
		/// Size of dynamic header.
		/// </summary>
		public int DynamicHeaderSize => this.dynamicHeaderSize;

		/// <summary>
		/// Maximum size of dynamic header.
		/// </summary>
		public int MaxDynamicHeaderSize => this.maxDynamicHeaderSize;

		/// <summary>
		/// Upper limit of the Maximum size of dynamic header (SETTINGS_HEADER_TABLE_SIZE).
		/// </summary>
		public int MaxDynamicHeaderSizeLimit => this.maxDynamicHeaderSizeLimit;

#if DEBUG
		/// <summary>
		/// Stack trace of current lock.
		/// </summary>
		public string LockedFrom => this.lockedFrom;
#endif

		/// <summary>
		/// Waits for the reader to be free.
		/// </summary>
#if DEBUG
		public async Task Lock()
		{
			await this.syncObject.WaitAsync();
			this.lockedFrom = Environment.StackTrace;
		}
#else
		public Task Lock()
		{
			return this.syncObject.WaitAsync();
		}
#endif
		/// <summary>
		/// Waits for the reader to be free.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>If lock was granted.</returns>
#if DEBUG
		public async Task<bool> TryLock(int TimeoutMilliseconds)
		{
			if (!await this.syncObject.WaitAsync(TimeoutMilliseconds))
				return false;

			this.lockedFrom = Environment.StackTrace;

			return true;
		}
#else
		public Task<bool> TryLock(int TimeoutMilliseconds)
		{
			return this.syncObject.WaitAsync(TimeoutMilliseconds);
		}
#endif

		/// <summary>
		/// Releases the object for use by another.
		/// </summary>
		public void Release()
		{
#if DEBUG
			this.lockedFrom = null;
#endif
			this.syncObject.Release();
		}

		/// <summary>
		/// Updates the maximum dynamic header size, and trims the dynamic header table.
		/// </summary>
		/// <param name="MaxDynamicHeaderSize">New Maximum size of the dynamic header.</param>
		/// <returns>If change was applied (true), or if the request was rejected because
		/// it surpassed the upper limit (false).</returns>
		public bool UpdateMaxDynamicHeaderSize(int MaxDynamicHeaderSize)
		{
			if (MaxDynamicHeaderSize > this.maxDynamicHeaderSizeLimit)
				return false;

			this.maxDynamicHeaderSize = MaxDynamicHeaderSize;
			this.TrimDynamicHeaders();

			return true;
		}

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
		/// Trims the dynamic headers table, to assue it is not larger than the
		/// maximum table size.
		/// </summary>
		protected void TrimDynamicHeaders()
		{
			DynamicRecord DynamicRecord;

			while (this.dynamicHeaderSize > this.maxDynamicHeaderSize)
			{
				int c = (int)this.nrDynamicRecords - 1;
				if (c < 0)
					return;
				
				DynamicRecord = this.dynamicRecords[c];

				this.dynamicRecords.RemoveAt(c);
				this.nrDynamicRecords--;

				if (DynamicRecord.Header.Values.Remove(DynamicRecord.Value) &&
					DynamicRecord.Header.Values.Count == 0)
				{
					this.dynamicHeaders.Remove(DynamicRecord.Header.Header);
				}

				this.dynamicHeaderSize -= DynamicRecord.Length;
			}
		}

		/// <summary>
		/// Adds a record to the dynamic headers table.
		/// </summary>
		/// <param name="Header">Header</param>
		/// <param name="Value">Value</param>
		/// <returns>Created record</returns>
		protected DynamicRecord AddToDynamicIndex(DynamicHeader Header, string Value)
		{
			ulong Index = this.nrHeadersCreated++;

			DynamicRecord DynamicRecord = new DynamicRecord(Header, Value, 32, Index);
			this.dynamicRecords.Insert(0, DynamicRecord);
			this.nrDynamicRecords++;
			Header.Values[Value] = DynamicRecord;

			return DynamicRecord;
		}

		/// <summary>
		/// Adds a record to the dynamic headers table.
		/// </summary>
		/// <param name="Header">Header</param>
		/// <param name="Value">Value</param>
		/// <returns>Created record</returns>
		protected DynamicRecord AddToDynamicIndex(string Header, string Value)
		{
			ulong Index = this.nrHeadersCreated++;

			DynamicHeader DynamicHeader = new DynamicHeader(Header, Index);
			this.dynamicHeaders[Header] = DynamicHeader;

			DynamicRecord DynamicRecord = new DynamicRecord(DynamicHeader, Value, 32, Index);
			this.dynamicRecords.Insert(0, DynamicRecord);
			this.nrDynamicRecords++;
			DynamicHeader.Values[Value] = DynamicRecord;

			return DynamicRecord;
		}

	}
}
