using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// TeletexString
	/// CCITT and T.101 character sets
	/// </summary>
	public class Asn1TeletexString : Asn1StringType
	{
		/// <summary>
		/// TeletexString
		/// CCITT and T.101 character sets
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1TeletexString(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
