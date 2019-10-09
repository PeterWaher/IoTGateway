using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// OCTET STRING
	/// </summary>
	public class Asn1OctetString : Asn1Type
	{
		/// <summary>
		/// OCTET STRING
		/// </summary>
		public Asn1OctetString()
			: base()
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "byte[]";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => true;
	}
}
