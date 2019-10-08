using System;
using System.Collections.Generic;
using System.IO;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Constrained Encoding Rules (CER), as defined in X.690
	/// (Subset of BER)
	/// </summary>
	public class Asn1DecoderCer : Asn1DecoderBer 
	{
		/// <summary>
		/// Constrained Encoding Rules (CER), as defined in X.690
		/// (Subset of BER)
		/// </summary>
		/// <param name="Input">Input stream.</param>
		public Asn1DecoderCer(Stream Input)
			: base(Input)
		{
		}
	}
}
