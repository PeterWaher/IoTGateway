using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Size2 option
	/// 
	/// Defined in RFC 7959: 
	/// https://tools.ietf.org/html/rfc7959
	/// </summary>
	public class CoapOptionSize2 : CoapOptionUInt
	{
		/// <summary>
		/// Size2 option
		/// </summary>
		public CoapOptionSize2()
			: base()
		{
		}

		/// <summary>
		/// Size2 option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionSize2(ulong Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Size2 option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionSize2(byte[] Value)
			: base(Value)
		{
		}

		/// <summary>
		/// If the option is critical or not. Messages containing critical options that are not processed, must be discarded.
		/// </summary>
		public override bool Critical
		{
			get { return false; }
		}

		/// <summary>
		/// Option number.
		/// </summary>
		public override int OptionNumber
		{
			get { return 28; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionSize2(Value);
		}

	}
}
