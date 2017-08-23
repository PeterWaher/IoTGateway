using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Proxy-Uri option
	/// 
	/// Defined in RFC 7252 §5.10.2: 
	/// https://tools.ietf.org/html/rfc7252#page-53
	/// </summary>
	public class CoapOptionProxyUri : CoapOptionString
	{
		/// <summary>
		/// Proxy-Uri option
		/// </summary>
		public CoapOptionProxyUri()
			: base()
		{
		}

		/// <summary>
		/// Proxy-Uri option
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionProxyUri(string Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Proxy-Uri option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionProxyUri(byte[] Value)
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
			get { return 35; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionProxyUri(Value);
		}

	}
}
