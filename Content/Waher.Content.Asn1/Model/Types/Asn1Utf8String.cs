using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// UTF8String
	/// any character from a recognized alphabet (including ASCII control characters)
	/// </summary>
	public class Asn1Utf8String : Asn1StringType
	{
		/// <summary>
		/// UTF8String
		/// any character from a recognized alphabet (including ASCII control characters)
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Utf8String(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
