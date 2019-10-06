using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// INTEGER
	/// </summary>
	public class Asn1Integer : Asn1Type
	{
		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "long";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => false;
	}
}
