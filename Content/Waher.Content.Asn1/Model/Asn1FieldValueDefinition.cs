using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 field value definition.
	/// </summary>
	public class Asn1FieldValueDefinition : Asn1Node
	{
		private readonly string fieldName;
		private readonly string typeName;
		private readonly Asn1Node value;

		/// <summary>
		/// Represents an ASN.1 field value definition.
		/// </summary>
		/// <param name="FieldName">Field name.</param>
		/// <param name="TypeName">Type name.</param>
		/// <param name="Value">Value</param>
		public Asn1FieldValueDefinition(string FieldName, string TypeName, Asn1Node Value)
			: base()
		{
			this.fieldName = FieldName;
			this.typeName = TypeName;
			this.value = Value;
		}

		/// <summary>
		/// Field Name
		/// </summary>
		public string FieldName => this.fieldName;

		/// <summary>
		/// Type Name
		/// </summary>
		public string TypeName => this.typeName;

		/// <summary>
		/// Type definition
		/// </summary>
		public Asn1Node Value => this.value;
	}
}
