using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SET construct.
	/// </summary>
	public class Asn1Set : Asn1List
	{
		/// <summary>
		/// Represents a ASN.1 SET construct.
		/// </summary>
		/// <param name="Nodes">Nodes</param>
		public Asn1Set(Asn1Node[] Nodes)
			: base(Nodes)
		{
		}
	}
}
