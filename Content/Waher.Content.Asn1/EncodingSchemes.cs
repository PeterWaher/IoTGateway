using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Available ASN.1 encoding schemes.
	/// </summary>
	[Flags]
	public enum EncodingSchemes
	{
		/// <summary>
		/// Basic Encoding Rules (BER)
		/// </summary>
		BER,

		/// <summary>
		/// All supported encoding schemes.
		/// </summary>
		All = BER
	}
}
