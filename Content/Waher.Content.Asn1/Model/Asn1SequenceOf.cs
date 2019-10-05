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
		private readonly Asn1Set size;
		private readonly string typeName;

		/// <summary>
		/// Represents a ASN.1 SEQUENCE OF construct.
		/// </summary>
		/// <param name="Size">SIZE</param>
		/// <param name="TypeName">Type name</param>
		public Asn1SequenceOf(Asn1Set Size, string TypeName)
		{
			this.size = Size;
			this.typeName = TypeName;
		}

		/// <summary>
		/// SIZE
		/// </summary>
		public Asn1Set Size => this.size;

		/// <summary>
		/// Type name.
		/// </summary>
		public string TypeName => this.typeName;
	}
}
