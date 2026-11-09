using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;

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
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1TypeReference(string Identifier, Asn1Document Document)
			: base(Document)
		{
			this.identifier = Identifier;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => this.identifier;

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
					return TypeDef.CSharpType;
				else
					return ToCSharp(this.identifier);
			}
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override async Task ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
					await TypeDef.Definition.ExportCSharp(Output, State, Indent, Pass);
				else
					Output.Append(ToCSharp(State, this, this.identifier));
			}
		}

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			if (!Macro.Document.namedNodes.TryGetValue(this.identifier, out Asn1Node Node))
				throw Document.SyntaxError("Type named " + this.identifier + " not found.");

			if (Node is Asn1Type Type)
				return Type.Parse(Document, Macro);
			else if (Node is Asn1FieldDefinition FieldDef)
				return FieldDef.Type.Parse(Document, Macro);

			throw Document.SyntaxError("Type expected.");
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		public override Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf)
		{
			if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
				return TypeDef.ExportReadCSharp(Output, State, Indent, MemberOf);
			else if (this.Document.namedNodes.TryGetValue(this.identifier, out Asn1Node Node))
				return Node.ExportReadCSharp(Output, State, Indent, MemberOf);
			else
			{
				Output.Append(ToCSharp(State, this, this.identifier));
				Output.Append(".Create(Reader)");

				return Task.CompletedTask;
			}
		}

		/// <summary>
		/// Exports optional read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="ItemName">Name of item variable.</param>
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string ItemName)
		{
			if (this.Document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
				return TypeDef.ExportTryReadCSharp(Output, State, Indent, MemberOf, ItemName);
			else if (this.Document.namedNodes.TryGetValue(this.identifier, out Asn1Node Node))
				return Node.ExportTryReadCSharp(Output, State, Indent, MemberOf, ItemName);
			else
			{
				if (string.IsNullOrEmpty(ItemName))
				{
					Output.Append(ToCSharp(State, this, this.identifier));
					Output.Append(".TryCreate(Reader, out var _Item) ? _Item : default");
				}
				else
				{
					Output.Append(ToCSharp(State, this, this.identifier));
					Output.Append(".TryCreate(Reader, out ");
					Output.Append(ItemName);
					Output.Append(')');
				}

				return Task.CompletedTask;
			}
		}
	}
}
