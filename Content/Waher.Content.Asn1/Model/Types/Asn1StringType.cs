using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Abstract base class for string types
	/// </summary>
	public abstract class Asn1StringType : Asn1Type
	{
		/// <summary>
		/// Abstract base class for string types
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1StringType(bool Implicit)
			: base(Implicit)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "string";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => true;
	}
}
