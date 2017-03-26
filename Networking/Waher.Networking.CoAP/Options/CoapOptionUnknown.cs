using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP.Options
{
	/// <summary>
	/// Unknown CoAP option.
	/// </summary>
	public class CoapOptionUnknown : CoapOption
	{
		private int nr;
		private byte[] value;
		private bool critical;

		/// <summary>
		/// Unknown CoAP option.
		/// </summary>
		public CoapOptionUnknown()
			: base()
		{
			this.nr = 0;
			this.value = null;
			this.critical = false;
		}

		/// <summary>
		/// Unknown CoAP option.
		/// </summary>
		/// <param name="Nr">CoAP option number.</param>
		/// <param name="Value">CoAP option value.</param>
		/// <param name="Critical">If the option is critical or not.</param>
		public CoapOptionUnknown(int Nr, byte[] Value, bool Critical)
			: base()
		{
			this.nr = Nr;
			this.value = Value;
			this.critical = Critical;
		}

		/// <summary>
		/// Option number.
		/// </summary>
		public override int OptionNumber
		{
			get
			{
				return this.nr;
			}
		}

		/// <summary>
		/// If the option is critical or not. Messages containing critical options that are not processed, must be discarded.
		/// </summary>
		public override bool Critical
		{
			get
			{
				return this.critical;
			}
		}

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		public override CoapOption Create(byte[] Value)
		{
			return new CoapOptionUnknown(this.nr, Value, this.critical);
		}

		/// <summary>
		/// Gets the option value.
		/// </summary>
		/// <returns>Binary value. Can be null, if option does not have a value.</returns>
		public override byte[] GetValue()
		{
			return this.value;
		}
	}
}
