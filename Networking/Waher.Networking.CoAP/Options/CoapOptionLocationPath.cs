using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Location-Path option
	/// 
	/// Defined in RFC 7252 §5.10.7: 
	/// https://tools.ietf.org/html/rfc7252#page-57
	/// </summary>
	public class CoapOptionLocationPath : CoapOptionString
	{
		/// <summary>
		/// Location-Path option
		/// </summary>
		public CoapOptionLocationPath()
			: base()
		{
		}

		/// <summary>
		/// Location-Path option
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionLocationPath(string Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Location-Path option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionLocationPath(byte[] Value)
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
			get { return 8; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionLocationPath(Value);
		}

	}
}
