using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SEQUENCE OF construct.
	/// </summary>
	public class Asn1SequenceOf : Asn1Type
	{
		private readonly Asn1Values size;
		private readonly string typeName;

		/// <summary>
		/// Represents a ASN.1 SEQUENCE OF construct.
		/// </summary>
		/// <param name="Size">Optional SIZE</param>
		/// <param name="TypeName">Type name</param>
		public Asn1SequenceOf(Asn1Values Size, string TypeName)
		{
			this.size = Size;
			this.typeName = TypeName;
		}

		/// <summary>
		/// Optional SIZE
		/// </summary>
		public Asn1Values Size => this.size;

		/// <summary>
		/// Type name.
		/// </summary>
		public string TypeName => this.typeName;

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => this.typeName + "[]";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => base.CSharpTypeNullable;
	}
}
