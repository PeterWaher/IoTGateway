using System;
using System.Text;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Base class for all uint-valued CoAP options.
	/// </summary>
	public abstract class CoapOptionUInt : CoapOption
	{
		private ulong value;

		/// <summary>
		/// Base class for all uint-valued CoAP options.
		/// </summary>
		public CoapOptionUInt()
			: base()
		{
		}

		/// <summary>
		/// Base class for all uint-valued CoAP options.
		/// </summary>
		/// <param name="Value">Integer value.</param>
		public CoapOptionUInt(ulong Value)
			: base()
		{
			this.value = Value;
		}

		/// <summary>
		/// Base class for all uint-valued CoAP options.
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionUInt(byte[] Value)
		{
			this.value = 0;

			if (Value != null)
			{
				int i = 0;
				int c = Value.Length;

				while (i < c)
				{
					this.value <<= 8;
					this.value |= Value[i++];
				}
			}
		}

		/// <summary>
		/// Gets the option value.
		/// </summary>
		/// <returns>Binary value. Can be null, if option does not have a value.</returns>
		public override byte[] GetValue()
		{
			return ToBinary(this.value);
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + " = " + this.value.ToString();
		}

		/// <summary>
		/// Integer value.
		/// </summary>
		public ulong Value
		{
			get { return this.value; }
		}
	}
}
