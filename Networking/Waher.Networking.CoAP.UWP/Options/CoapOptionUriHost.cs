using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Uri-Host option
	/// 
	/// Defined in RFC 7252 §5.10.1: 
	/// https://tools.ietf.org/html/rfc7252#page-52
	/// </summary>
	public class CoapOptionUriHost : CoapOptionString
	{
		/// <summary>
		/// Uri-Host option
		/// </summary>
		public CoapOptionUriHost()
			: base()
		{
		}

		/// <summary>
		/// Uri-Host option
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionUriHost(string Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Uri-Host option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionUriHost(byte[] Value)
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
			get { return 3; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionUriHost(Value);
		}

	}
}
