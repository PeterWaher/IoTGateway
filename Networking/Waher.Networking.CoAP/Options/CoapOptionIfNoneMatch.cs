using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// If-None-Match option
	/// </summary>
	public class CoapOptionIfNoneMatch : CoapOptionEmpty
	{
		/// <summary>
		/// If-NoneMatch option
		/// </summary>
		public CoapOptionIfNoneMatch()
			: base()
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
			get { return 5; }
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionIfNoneMatch();
		}

	}
}
