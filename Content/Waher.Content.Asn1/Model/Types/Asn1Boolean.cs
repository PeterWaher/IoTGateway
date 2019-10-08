using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// BOOLEAN
	/// </summary>
	public class Asn1Boolean : Asn1Type
	{
		/// <summary>
		/// BOOLEAN
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Boolean(bool Implicit)
			: base(Implicit)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "bool";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => false;
	}
}
