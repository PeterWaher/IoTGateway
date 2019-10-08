using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// DATE
	/// </summary>
	public class Asn1Date : Asn1Type
	{
		/// <summary>
		/// DATE
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Date(bool Implicit)
			: base(Implicit)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "DateTime";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => false;
	}
}
