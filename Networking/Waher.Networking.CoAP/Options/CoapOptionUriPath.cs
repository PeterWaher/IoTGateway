using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Uri-Path option
	/// </summary>
	public class CoapOptionUriPath : CoapOptionString
	{
		/// <summary>
		/// Uri-Path option
		/// </summary>
		public CoapOptionUriPath()
			: base()
		{
		}

		/// <summary>
		/// Uri-Path option
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionUriPath(string Value)
			: base(Value)
		{
		}

		/// <summary>
		/// Uri-Path option
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionUriPath(byte[] Value)
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
			get { return 11; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionUriPath(Value);
		}

	}
}
