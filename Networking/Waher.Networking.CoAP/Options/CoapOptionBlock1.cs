using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Block1 option
	/// 
	/// Defined in RFC 7959: 
	/// https://tools.ietf.org/html/rfc7959
	/// </summary>
	public class CoapOptionBlock1 : CoapOptionBlock
	{
		/// <summary>
		/// Block1 option
		/// </summary>
		public CoapOptionBlock1()
			: base()
		{
		}

		/// <summary>
		/// Block1 option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionBlock1(ulong Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Block1 option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionBlock1(byte[] Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Block1 option
		/// </summary>
		/// <param name="Number">Block number.</param>
		/// <param name="More">If more blocks are available.</param>
		/// <param name="Size">Block size. Must be 16, 32, 64, 128, 256, 512, 1024, or 2048.</param>
		public CoapOptionBlock1(int Number, bool More, int Size)
			: base(Number, More, Size)
		{
		}

		/// <summary>
		/// If the option is critical or not. Messages containing critical options that are not processed, must be discarded.
		/// </summary>
		public override bool Critical
		{
			get { return true; }
		}

		/// <summary>
		/// Option number.
		/// </summary>
		public override int OptionNumber
		{
			get { return 27; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionBlock1(Value);
		}

	}
}
