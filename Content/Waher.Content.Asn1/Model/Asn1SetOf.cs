using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SET OF construct.
	/// </summary>
	public class Asn1SetOf : Asn1Type
	{
		private readonly Asn1Values size;
		private readonly string typeName;

		/// <summary>
		/// Represents a ASN.1 SET OF construct.
		/// </summary>
		/// <param name="Size">SIZE</param>
		/// <param name="TypeName">Type name</param>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1SetOf(Asn1Values Size, string TypeName, bool Implicit)
			: base(Implicit)
		{
			this.size = Size;
			this.typeName = TypeName;
		}

		/// <summary>
		/// SIZE
		/// </summary>
		public Asn1Values Size => this.size;

		/// <summary>
		/// Type name.
		/// </summary>
		public string TypeName => this.typeName;
	}
}
