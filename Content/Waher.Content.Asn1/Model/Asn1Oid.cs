using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 Object ID
	/// </summary>
	public class Asn1Oid : Asn1Node
	{
		private readonly Asn1Node[] values;

		/// <summary>
		/// Represents an ASN.1 Object ID
		/// </summary>
		/// <param name="Values">OID values.</param>
		public Asn1Oid(Asn1Node[] Values)
		{
			this.values = Values;
		}

		/// <summary>
		/// OID values.
		/// </summary>
		public Asn1Node[] Values => this.values;
	}
}
