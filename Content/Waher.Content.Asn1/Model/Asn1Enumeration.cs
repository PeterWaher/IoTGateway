using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// ENUMERATED
	/// </summary>
	public class Asn1Enumeration : Asn1List 
	{
		/// <summary>
		/// ENUMERATED
		/// </summary>
		/// <param name="Nodes">Nodes</param>
		public Asn1Enumeration(Asn1Node[] Nodes)
			: base(Nodes)
		{
		}
	}
}
