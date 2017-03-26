using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Uri-Query option
	/// </summary>
	public class CoapOptionUriQuery : CoapOptionString
	{
		/// <summary>
		/// Uri-Query option
		/// </summary>
		public CoapOptionUriQuery()
			: base()
		{
		}

		/// <summary>
		/// Uri-Query option
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionUriQuery(string Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Uri-Query option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionUriQuery(byte[] Value)
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
			get { return 15; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionUriQuery(Value);
		}

	}
}
