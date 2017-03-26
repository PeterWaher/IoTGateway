using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Max-Age option
	/// </summary>
	public class CoapOptionMaxAge : CoapOptionUInt
	{
		/// <summary>
		/// Max-Age option
		/// </summary>
		public CoapOptionMaxAge()
			: base()
		{
		}

		/// <summary>
		/// Max-Age option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionMaxAge(ulong Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Max-Age option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionMaxAge(byte[] Value)
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
			get { return 14; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionMaxAge(Value);
		}

	}
}
