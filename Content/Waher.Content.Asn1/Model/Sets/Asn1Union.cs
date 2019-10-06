using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Union of sets.
	/// </summary>
	public class Asn1Union : Asn1BinarySetOperator
	{
		/// <summary>
		/// Union of sets.
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		public Asn1Union(Asn1Values Left, Asn1Values Right)
			: base(Left, Right)
		{
		}
	}
}
