using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Restricted to elements in set.
	/// </summary>
	public class Asn1InSet : Asn1SetRestriction
	{
		/// <summary>
		/// Restricted to elements in set.
		/// </summary>
		/// <param name="Set">Set</param>
		public Asn1InSet(Asn1Set Set)
			: base(Set)
		{
		}
	}
}
