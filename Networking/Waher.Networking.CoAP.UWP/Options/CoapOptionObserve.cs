using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Observe option
	/// 
	/// Defined in RFC 7641: 
	/// https://tools.ietf.org/html/rfc7641
	/// </summary>
	public class CoapOptionObserve : CoapOptionUInt
	{
		/// <summary>
		/// Observe option
		/// </summary>
		public CoapOptionObserve()
			: base()
		{
		}

		/// <summary>
		/// Observe option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionObserve(ulong Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Observe option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionObserve(byte[] Value)
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
			get { return 6; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionObserve(Value);
		}

	}
}
