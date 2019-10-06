using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// WITH COMPONENTS
	/// </summary>
	public class Asn1WithComponents : Asn1ValueRestriction
	{
		/// <summary>
		/// WITH COMPONENTS
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1WithComponents(Asn1Value Value) 
			: base(Value)
		{
		}
	}
}
