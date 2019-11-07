using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents an ASN.1 value reference.
	/// </summary>
	public class Asn1ValueReference : Asn1Value
	{
		private readonly Asn1Document document;
		private readonly string identifier;

		/// <summary>
		/// Represents an ASN.1 value reference.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Document">ASN.1 Document containing the reference</param>
		public Asn1ValueReference(string Identifier, Asn1Document Document)
		{
			this.identifier = Identifier;
			this.document = Document;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => this.identifier;

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
			if (Pass == CSharpExportPass.Explicit)
			{
				if (this.document.values.TryGetValue(this.identifier, out Asn1FieldValueDefinition ValueDef))
				{
					if (ValueDef.Document.Root.Identifier != this.document.Root.Identifier)
					{
						Output.Append(ToCSharp(ValueDef.Document.Root.Identifier));
						Output.Append(".Values.");
					}
				}
					
				Output.Append(ToCSharp(this.identifier));
			}
		}
	}
}
