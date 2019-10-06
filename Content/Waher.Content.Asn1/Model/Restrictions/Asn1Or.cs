using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Either restriction applies
	/// </summary>
	public class Asn1Or : Asn1BinaryRestrictionOperator
	{
		/// <summary>
		/// Either restriction applies
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		public Asn1Or(Asn1Restriction Left, Asn1Restriction Right)
			: base(Left, Right)
		{
		}
	}
}
