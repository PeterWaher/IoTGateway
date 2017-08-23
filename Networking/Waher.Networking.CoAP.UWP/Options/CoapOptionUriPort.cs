using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Uri-Port option
	/// 
	/// Defined in RFC 7252 §5.10.1: 
	/// https://tools.ietf.org/html/rfc7252#page-52
	/// </summary>
	public class CoapOptionUriPort : CoapOptionUInt
	{
		/// <summary>
		/// Uri-Port option
		/// </summary>
		public CoapOptionUriPort()
			: base()
		{
		}

		/// <summary>
		/// Uri-Port option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionUriPort(ulong Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Uri-Port option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionUriPort(byte[] Value)
			: base(Value)
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
			get { return 7; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionUriPort(Value);
		}

	}
}
