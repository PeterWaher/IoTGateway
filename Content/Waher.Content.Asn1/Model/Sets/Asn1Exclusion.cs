using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Exclusion of sets.
	/// </summary>
	public class Asn1Exclusion : Asn1BinaryOperator
	{
		/// <summary>
		/// Union of sets.
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		public Asn1Exclusion(Asn1Set Left, Asn1Set Right)
			: base(Left, Right)
		{
		}
	}
}
