using Waher.Content.Asn1.Model.Classes;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// References an identifier.
	/// </summary>
	public class Asn1IdentifierReference : Asn1Node
	{
		private readonly Asn1ClassReference classReference;
		private readonly string identifier;

		/// <summary>
		/// References an identifier.
		/// </summary>
		/// <param name="Identifier">Required identifier</param>
		/// <param name="ClassReference">Optional class reference.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1IdentifierReference(string Identifier, Asn1ClassReference ClassReference,
			Asn1Document Document)
			: base(Document)
		{
			this.identifier = Identifier;
			this.classReference = ClassReference;
		}

		/// <summary>
		/// Required identifier
		/// </summary>
		public string Identifier => this.identifier;

		/// <summary>
		/// Optional class reference
		/// </summary>
		public Asn1ClassReference ClassReference => this.classReference;
	}
}
