using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// If-Match option
	/// </summary>
	public class CoapOptionIfMatch : CoapOptionOpaque
	{
		/// <summary>
		/// If-Match option
		/// </summary>
		public CoapOptionIfMatch()
			: base()
		{
		}

		/// <summary>
		/// If-Match option
		/// </summary>
		/// <param name="Value">Opaque value.</param>
		public CoapOptionIfMatch(byte[] Value)
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
			get { return 1; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionIfMatch(Value);
		}

	}
}
