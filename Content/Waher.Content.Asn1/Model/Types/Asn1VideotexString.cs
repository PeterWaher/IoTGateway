using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// VideotexString
	/// CCITT's T.100 and T.101 character sets 
	/// </summary>
	public class Asn1VideotexString : Asn1StringType
	{
		/// <summary>
		/// VideotexString
		/// CCITT's T.100 and T.101 character sets 
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1VideotexString(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
