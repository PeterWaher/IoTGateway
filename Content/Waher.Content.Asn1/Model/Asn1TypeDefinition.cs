using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model.Macro;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 Type definition.
	/// </summary>
	public class Asn1TypeDefinition : Asn1Type, INamedNode
	{
		private readonly string typeName;
		private readonly Asn1Type definition;
		private readonly int? tag;

		/// <summary>
		/// Represents an ASN.1 Type definition.
		/// </summary>
		/// <param name="TypeName">Type name.</param>
		/// <param name="Tag">Tag</param>
		/// <param name="Definition">Definition</param>
		public Asn1TypeDefinition(string TypeName, int? Tag, Asn1Type Definition)
			: base()
		{
			this.typeName = TypeName;
			this.definition = Definition;
			this.tag = Tag;
		}

		/// <summary>
		/// Type Name
		/// </summary>
		public string Name => this.typeName;

		/// <summary>
		/// Tag
		/// </summary>
		public int? Tag => this.tag;

		/// <summary>
		/// Type definition
		/// </summary>
		public Asn1Type Definition => this.definition;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			return this.definition.Parse(Document, Macro);
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (this.definition.ConstructedType)
				this.definition.ExportCSharp(Output, State, Indent, Pass);
		}
	}
}
