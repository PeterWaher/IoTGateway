using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 field definition.
	/// </summary>
	public class Asn1FieldDefinition : Asn1Node
	{
		private readonly string fieldName;
		private readonly string typeName;
		private readonly Asn1Restriction[] restrictions;
		private readonly Asn1NamedValue[] namedOptions;
		private readonly bool optional;
		private readonly bool unique;
		private readonly Asn1Node _default;

		/// <summary>
		/// Represents an ASN.1 field definition.
		/// </summary>
		/// <param name="FieldName">Field name.</param>
		/// <param name="TypeName">Type name.</param>
		/// <param name="Restrictions">Optional restrictions.</param>
		/// <param name="Optional">If field is optional.</param>
		/// <param name="Unique">If field value is unique.</param>
		/// <param name="Default">Default value if field not provided.</param>
		/// <param name="NamedOptions">Named options.</param>
		public Asn1FieldDefinition(string FieldName, string TypeName, Asn1Restriction[] Restrictions, 
			bool Optional, bool Unique, Asn1Node Default, Asn1NamedValue[] NamedOptions)
			: base()
		{
			this.fieldName = FieldName;
			this.typeName = TypeName;
			this.restrictions = Restrictions;
			this.optional = Optional;
			this.unique = Unique;
			this._default = Default;
			this.namedOptions = NamedOptions;
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
		/// Optional restrictions.
		/// </summary>
		public Asn1Restriction[] Restrictions => this.restrictions;

		/// <summary>
		/// If field is optional.
		/// </summary>
		public bool Optional => this.optional;

		/// <summary>
		/// If field value is unique.
		/// </summary>
		public bool Unique => this.unique;

		/// <summary>
		/// Default value if field not provided.
		/// </summary>
		public Asn1Node Default => this._default;

		/// <summary>
		/// Named options.
		/// </summary>
		public Asn1NamedValue[] NamedOptions => this.namedOptions;
	}
}
