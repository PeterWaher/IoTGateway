
using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// FROM()
	/// </summary>
	public class Asn1From : Asn1SetRestriction
	{
		/// <summary>
		/// FROM()
		/// </summary>
		/// <param name="Set">Set</param>
		public Asn1From(Asn1Set Set)
			: base(Set)
		{
		}
	}
}
