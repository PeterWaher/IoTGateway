using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// CONTAINING
	/// </summary>
	public class Asn1Containing : Asn1ValueRestriction
	{
		/// <summary>
		/// CONTAINING
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1Containing(Asn1Value Value)
			: base(Value)
		{
		}
	}
}
