using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// ENCODED BY
	/// </summary>
	public class Asn1EncodedBy : Asn1ValueRestriction
	{
		/// <summary>
		/// ENCODED BY
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1EncodedBy(Asn1Value Value) 
			: base(Value)
		{
		}
	}
}
