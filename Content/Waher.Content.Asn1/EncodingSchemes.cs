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
		/// Basic Encoding Rules (BER), as defined in X.690
		/// </summary>
		BER,

		/// <summary>
		/// Canonical Encoding Rules (CER), as defined in X.690
		/// </summary>
		CER,

		/// <summary>
		/// Distinguished Encoding Rules (DER), as defined in X.690
		/// </summary>
		DER,

		/// <summary>
		/// All supported encoding schemes.
		/// </summary>
		All = BER
	}
}
