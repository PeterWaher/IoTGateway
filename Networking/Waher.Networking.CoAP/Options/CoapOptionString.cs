using System;
using System.Text;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Base class for all string-valued CoAP options.
	/// </summary>
	public abstract class CoapOptionString : CoapOption
	{
		private readonly string value;

		/// <summary>
		/// Base class for all string-valued CoAP options.
		/// </summary>
		public CoapOptionString()
			: base()
		{
		}

		/// <summary>
		/// Base class for all string-valued CoAP options.
		/// </summary>
		/// <param name="Value">String value.</param>
		public CoapOptionString(string Value)
			: base()
		{
			this.value = Value;
		}

		/// <summary>
		/// Base class for all string-valued CoAP options.
		/// </summary>
		/// <param name="Value">Binary value.</param>
		public CoapOptionString(byte[] Value)
		{
			this.value = Encoding.UTF8.GetString(Value);
		}

		/// <summary>
		/// Gets the option value.
		/// </summary>
		/// <returns>Binary value. Can be null, if option does not have a value.</returns>
		public override byte[] GetValue()
		{
			return Encoding.UTF8.GetBytes(this.value);
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + " = " + this.value;
		}

		/// <summary>
		/// String value.
		/// </summary>
		public string Value
		{
			get { return this.value; }
		}
	}
}
