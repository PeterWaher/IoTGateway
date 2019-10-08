using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// NumericString
	/// 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, and SPACE 
	/// </summary>
	public class Asn1NumericString : Asn1StringType
	{
		/// <summary>
		/// NumericString
		/// 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, and SPACE 
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1NumericString(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
