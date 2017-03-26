using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Proxy-Scheme option
	/// </summary>
	public class CoapOptionProxyScheme : CoapOptionString
	{
		/// <summary>
		/// Proxy-Scheme option
		/// </summary>
		public CoapOptionProxyScheme()
			: base()
		{
		}

		/// <summary>
		/// Proxy-Scheme option
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionProxyScheme(string Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Proxy-Scheme option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionProxyScheme(byte[] Value)
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
			get { return 39; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionProxyScheme(Value);
		}

	}
}
