using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// OBJECT IDENTIFIER, or RELATIVE-OID
	/// </summary>
	public class Asn1ObjectIdentifier : Asn1Type
	{
		private readonly bool relative;

		/// <summary>
		/// OBJECT IDENTIFIER, or RELATIVE-OID
		/// </summary>
		/// <param name="Relative">If it is a relative object ID.</param>
		public Asn1ObjectIdentifier(bool Relative)
		{
			this.relative = Relative;
		}

		/// <summary>
		/// If it is a relative object ID.
		/// </summary>
		public bool Relative => this.relative;

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "int[]";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => true;
	}
}
