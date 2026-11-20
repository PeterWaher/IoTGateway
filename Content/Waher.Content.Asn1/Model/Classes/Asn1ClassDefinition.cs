using System.Collections.Generic;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Defines a class.
	/// </summary>
	public class Asn1ClassDefinition : Asn1Node
	{
		private readonly Asn1PropertyDefinition[] properties;
		private readonly string name;
		private Dictionary<string, string> identifiers;
		private Asn1SyntaxNode[] syntax = null;

		/// <summary>
		/// Defines a class.
		/// </summary>
		/// <param name="Name">Class name.</param>
		/// <param name="Properties">Class properties.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassDefinition(string Name, Asn1PropertyDefinition[] Properties,
			Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.properties = Properties;
		}

		/// <summary>
		/// Class name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Property definitions
		/// </summary>
		public Asn1PropertyDefinition[] Properties => this.properties;

		/// <summary>
		/// Optional syntax.
		/// </summary>
		public Asn1SyntaxNode[] Syntax
		{
			get => this.syntax;
			internal set => this.syntax = value;
		}

		/// <summary>
		/// If the class definition has syntax defined.
		/// </summary>
		public bool HasSyntax => !(this.syntax is null);

		/// <inheritdoc/>
		public override string ToString() => this.name;

		/// <summary>
		/// Adds an identification record.
		/// </summary>
		/// <param name="IdentifiedProperty">Property being identified</param>
		/// <param name="IdentifierProperty">Property used to identify property</param>
		public void AddIdentifier(string IdentifiedProperty, string IdentifierProperty)
		{
			if (this.identifiers is null)
				this.identifiers = new Dictionary<string, string>();

			this.identifiers[IdentifiedProperty] = IdentifierProperty;
		}
	}
}
