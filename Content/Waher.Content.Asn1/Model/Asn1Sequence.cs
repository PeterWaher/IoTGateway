using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SEQUENCE construct.
	/// </summary>
	public class Asn1Sequence : Asn1List
	{
		/// <summary>
		/// Represents a ASN.1 SEQUENCE construct.
		/// </summary>
		/// <param name="Nodes">Nodes</param>
		public Asn1Sequence(Asn1Node[] Nodes)
			: base(Nodes)
		{
		}
	}
}
