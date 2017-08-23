using System;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Base class for all empty CoAP options.
	/// </summary>
	public abstract class CoapOptionEmpty : CoapOption
	{
		/// <summary>
		/// Base class for all empty CoAP options.
		/// </summary>
		public CoapOptionEmpty()
			: base()
		{
		}

		/// <summary>
		/// Gets the option value.
		/// </summary>
		/// <returns>Binary value. Can be null, if option does not have a value.</returns>
		public override byte[] GetValue()
		{
			return null;
		}
	}
}
