using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Location-Query option
	/// </summary>
	public class CoapOptionLocationQuery : CoapOptionString
	{
		/// <summary>
		/// Location-Query option
		/// </summary>
		public CoapOptionLocationQuery()
			: base()
		{
		}

		/// <summary>
		/// Location-Query option
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionLocationQuery(string Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Location-Query option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionLocationQuery(byte[] Value)
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
			get { return 20; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionLocationQuery(Value);
		}

	}
}
