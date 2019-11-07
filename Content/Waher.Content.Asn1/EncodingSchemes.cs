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
		BER = 1,

		/// <summary>
		/// Canonical Encoding Rules (CER), as defined in X.690
		/// </summary>
		CER = 2,

		/// <summary>
		/// Distinguished Encoding Rules (DER), as defined in X.690
		/// </summary>
		DER = 4,

		/// <summary>
		/// All supported encoding schemes.
		/// </summary>
		All = BER | CER | DER,

		/// <summary>
		/// None of the supported encoding schemes.
		/// </summary>
		None = 0
	}
}
