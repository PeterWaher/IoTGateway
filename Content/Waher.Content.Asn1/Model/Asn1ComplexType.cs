namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for complex types.
	/// </summary>
	public abstract class Asn1ComplexType : Asn1Type
	{
		private readonly bool typeDef;
		private string name;

		/// <summary>
		/// Abstract base class for complex types.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ComplexType(string Name, bool TypeDef, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.typeDef = TypeDef;
		}

		/// <summary>
		/// Optional field or type name.
		/// </summary>
		public string Name
		{
			get => this.name;
			internal set => this.name = value;
		}

		/// <summary>
		/// If construct is part of a type definition.
		/// </summary>
		public bool TypeDefinition => this.typeDef;

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.name;
		}
	}
}
