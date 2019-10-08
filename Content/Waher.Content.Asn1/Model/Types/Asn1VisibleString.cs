using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// VisibleString
	/// International ASCII printing character sets 
	/// </summary>
	public class Asn1VisibleString : Asn1StringType
	{
		/// <summary>
		/// VisibleString
		/// International ASCII printing character sets 
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1VisibleString(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
