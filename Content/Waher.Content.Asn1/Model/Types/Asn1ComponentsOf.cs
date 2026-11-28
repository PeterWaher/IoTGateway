namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// References the components of a complex type.
	/// </summary>
	public class Asn1ComponentsOf : Asn1Type
	{
		private readonly string identifier;

		/// <summary>
		/// References the components of a complex type.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ComponentsOf(string Identifier, Asn1Document Document)
			: base(Document)
		{
			this.identifier = Identifier;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => this.identifier;

		/// <inheritdoc/>
		public override string ToString()
		{
			return "Components of: " + this.identifier;
		}

		/// <summary>
		/// Gets the components associated with the node.
		/// </summary>
		/// <returns>Array of components.</returns>
		public override Asn1Node[] GetComponents()
		{
			if (!this.Document.TryGetNamedNode(this.identifier, out Asn1ComplexType Type))
				throw this.Document.SyntaxError("Did not find complex type named " + this.identifier);

			return Type.GetComponents();
		}
	}
}
