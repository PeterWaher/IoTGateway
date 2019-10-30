using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// Un-typed user-defined part.
	/// </summary>
	public class UserDefinedPart : UserDefinedItem
	{
		private readonly string identifier;

		/// <summary>
		/// Un-typed user-defined part.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		public UserDefinedPart(string Identifier)
		{
			this.identifier = Identifier;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => this.identifier;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the
		/// instructions available in the macro.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro being executed.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			if (Document.namedNodes.TryGetValue(this.identifier, out Asn1Node Node))
			{
				if (Node is Asn1TypeDefinition TypeDef)
					return TypeDef.Definition.Parse(Document, Macro);
				else if (Node is Asn1Type Type)
					return Type.Parse(Document, Macro);
				else
					throw Document.SyntaxError("Type reference expected: " + this.identifier);
			}

			foreach (SupportingSyntax Syntax in Macro.SupportingSyntax)
			{
				if (Syntax.Name == this.identifier)
					return Syntax.Parse(Document, Macro);
			}

			throw Document.SyntaxError("Supporting syntax for " + this.identifier + " not found.");
		}
	}
}
