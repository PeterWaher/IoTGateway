using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Accept option
	/// </summary>
	public class CoapOptionAccept : CoapOptionUInt
	{
		/// <summary>
		/// Accept option
		/// </summary>
		public CoapOptionAccept()
			: base()
		{
		}

		/// <summary>
		/// Accept option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionAccept(ulong Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Accept option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionAccept(byte[] Value)
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
			get { return 17; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionAccept(Value);
		}

	}
}
