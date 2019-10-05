using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 Type definition.
	/// </summary>
	public class Asn1TypeDefinition : Asn1Type
	{
		private readonly string typeName;
		private readonly Asn1Type definition;

		/// <summary>
		/// Represents an ASN.1 Type definition.
		/// </summary>
		/// <param name="TypeName">Type name.</param>
		/// <param name="Definition">Definition</param>
		public Asn1TypeDefinition(string TypeName, Asn1Type Definition)
		{
			this.typeName = TypeName;
			this.definition = Definition;
		}

		/// <summary>
		/// Type Name
		/// </summary>
		public string TypeName => this.typeName;

		/// <summary>
		/// Type definition
		/// </summary>
		public Asn1Type Definition => this.definition;
	}
}
