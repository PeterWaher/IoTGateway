using System;
using Waher.Security;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Base class for all opaque CoAP options.
	/// </summary>
	public abstract class CoapOptionOpaque : CoapOption
	{
		private byte[] value;

		/// <summary>
		/// Base class for all opaque CoAP options.
		/// </summary>
		public CoapOptionOpaque()
			: base()
		{
		}

		/// <summary>
		/// Base class for all opaque CoAP options.
		/// </summary>
		/// <param name="Value">Opaque option value.</param>
		public CoapOptionOpaque(byte[] Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Gets the option value.
		/// </summary>
		/// <returns>Binary value. Can be null, if option does not have a value.</returns>
		public override byte[] GetValue()
		{
			return this.value;
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + " = " + Hashes.BinaryToString(this.value);
		}
	}
}
