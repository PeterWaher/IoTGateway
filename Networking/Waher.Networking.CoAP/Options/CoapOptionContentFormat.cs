using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Content-Format option
	/// </summary>
	public class CoapOptionContentFormat : CoapOptionUInt
	{
		/// <summary>
		/// Content-Format option
		/// </summary>
		public CoapOptionContentFormat()
			: base()
		{
		}

		/// <summary>
		/// Content-Format option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionContentFormat(ulong Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Content-Format option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionContentFormat(byte[] Value)
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
			get { return 12; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionContentFormat(Value);
		}

	}
}
