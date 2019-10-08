using System;
using System.Collections.Generic;
using System.IO;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Distinguished Encoding Rules (DER), as defined in X.690
	/// (Subset of BER)
	/// </summary>
	public class Asn1DecoderDer : Asn1DecoderBer 
	{
		/// <summary>
		/// Distinguished Encoding Rules (DER), as defined in X.690
		/// (Subset of BER)
		/// </summary>
		/// <param name="Input">Input stream.</param>
		public Asn1DecoderDer(Stream Input)
			: base(Input)
		{
		}
	}
}
