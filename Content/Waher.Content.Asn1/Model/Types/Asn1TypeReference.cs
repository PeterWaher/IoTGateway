using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Represents an ASN.1 type reference.
	/// </summary>
	public class Asn1TypeReference : Asn1Type
	{
		private readonly string identifier;

		/// <summary>
		/// Represents an ASN.1 type reference.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1TypeReference(string Identifier, bool Implicit)
			: base(Implicit)
		{
			this.identifier = Identifier;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => identifier;

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => this.identifier;
	}
}
