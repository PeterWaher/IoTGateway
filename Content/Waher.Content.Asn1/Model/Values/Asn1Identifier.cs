using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents an ASN.1 identifier.
	/// </summary>
	public class Asn1Identifier : Asn1Value
	{
		private readonly string identifier;

		/// <summary>
		/// Represents an ASN.1 identifier.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		public Asn1Identifier(string Identifier)
		{
			this.identifier = Identifier;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => identifier;
	}
}
