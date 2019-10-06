using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Both restrictions apply
	/// </summary>
	public class Asn1And : Asn1BinaryRestrictionOperator
	{
		/// <summary>
		/// Both restrictions apply
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		public Asn1And(Asn1Restriction Left, Asn1Restriction Right)
			: base(Left, Right)
		{
		}
	}
}
