using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Interface for all CoAP options.
	/// </summary>
	public interface ICoapOption
	{
		/// <summary>
		/// Option number.
		/// </summary>
		int OptionNumber
		{
			get;
		}

		/// <summary>
		/// If the option is critical or not. Messages containing critical options that are not processed, must be discarded.
		/// </summary>
		bool Critical
		{
			get;
		}

		/// <summary>
		/// Gets the option value.
		/// </summary>
		/// <returns>Binary value. Can be null, if option does not have a value.</returns>
		byte[] GetValue();

		/// <summary>
		/// Creates a new CoAP option.
		/// </summary>
		/// <param name="Value">Option value.</param>
		/// <returns>Newly created CoAP option.</returns>
		CoapOption Create(byte[] Value);
	}
}
