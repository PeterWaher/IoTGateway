using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Base class for Block option
	/// 
	/// Defined in RFC 7959: 
	/// https://tools.ietf.org/html/rfc7959
	/// </summary>
	public abstract class CoapOptionBlock : CoapOptionUInt
	{
		private int size;
		private int number;
		private bool more;

		/// <summary>
		/// Block option
		/// </summary>
		public CoapOptionBlock()
			: base()
		{
		}

		/// <summary>
		/// Block option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionBlock(ulong Value)
			: base(Value)
		{
			this.Parse();
		}

		/// <summary>
		/// Block option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionBlock(byte[] Value)
			: base(Value)
		{
			this.Parse();
		}

		/// <summary>
		/// Block option
		/// </summary>
		/// <param name="Number">Block number.</param>
		/// <param name="More">If more blocks are available.</param>
		/// <param name="Size">Block size. Must be 16, 32, 64, 128, 256, 512, 1024, or 2048.</param>
		public CoapOptionBlock(int Number, bool More, int Size)
			: base(Encode((uint)Number, More, Size))
		{
		}

		private static ulong Encode(uint Number, bool More, int Size)
		{
			ulong Value = 0;
			int NrBits = 0;

			while (Size != 0)
			{
				if ((Size & 1) != 0)
					NrBits++;

				Size >>= 1;
				Value++;
			}

			Value -= 5;
			if (Value < 0 || Value > 7 || NrBits != 1)
				throw new ArgumentException("Invalid block size.", "Size");

			if (More)
				Value |= 8;

			Value |= (Number << 4);

			return Value;
		}

		private void Parse()
		{
			ulong l = this.Value;
			byte b = 0;

			this.number = 0;
			this.more = false;
			this.size = 0;

			do
			{
				this.number <<= 8;
				this.number |= b;

				b = (byte)l;
				l >>= 8;
			}
			while (l != 0);

			this.number <<= 4;
			this.number |= (b >> 4);

			this.more = (b & 8) != 0;
			this.size = 1 << (4 + (b & 7));
		}

		/// <summary>
		/// Block number.
		/// </summary>
		public int Number
		{
			get { return this.number; }
		}

		/// <summary>
		/// If more blocks are available.
		/// </summary>
		public bool More
		{
			get { return this.more; }
		}

		/// <summary>
		/// Block size.
		/// </summary>
		public int Size
		{
			get { return this.size; }
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + ", nr=" + this.number.ToString() + ", more=" + this.more.ToString() + ", size=" + this.size.ToString();
		}

	}
}
