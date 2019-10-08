using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// REAL
	/// </summary>
	public class Asn1Real : Asn1Type
	{
		/// <summary>
		/// REAL
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Real(bool Implicit)
			: base(Implicit)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "double";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => false;
	}
}
