using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents a restricted ASN.1 value reference.
	/// </summary>
	public class Asn1RestrictedValueReference : Asn1ValueReference
	{
		private readonly Asn1Restriction restriction;

		/// <summary>
		/// Represents an ASN.1 value reference.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Restriction">Restriction</param>
		public Asn1RestrictedValueReference(string Identifier, Asn1Restriction Restriction)
			: base(Identifier)
		{
			this.restriction = Restriction;
		}

		/// <summary>
		/// Restriction
		/// </summary>
		public Asn1Restriction Restriction => this.restriction;
	}
}
