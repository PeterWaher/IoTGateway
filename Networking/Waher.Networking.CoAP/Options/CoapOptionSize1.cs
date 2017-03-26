using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Size1 option
	/// </summary>
	public class CoapOptionSize1 : CoapOptionUInt
	{
		/// <summary>
		/// Size1 option
		/// </summary>
		public CoapOptionSize1()
			: base()
		{
		}

		/// <summary>
		/// Size1 option
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionSize1(ulong Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Size1 option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionSize1(byte[] Value)
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
			get { return 60; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionSize1(Value);
		}

	}
}
