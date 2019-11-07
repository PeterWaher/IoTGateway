using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 field value definition.
	/// </summary>
	public class Asn1FieldValueDefinition : Asn1Node, INamedNode
	{
		private readonly string fieldName;
		private readonly Asn1Type type;
		private readonly Asn1Value value;
		private readonly Asn1Document document;

		/// <summary>
		/// Represents an ASN.1 field value definition.
		/// </summary>
		/// <param name="FieldName">Field name.</param>
		/// <param name="Type">Type</param>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 Document in where the value is defined.</param>
		public Asn1FieldValueDefinition(string FieldName, Asn1Type Type, Asn1Value Value, 
			Asn1Document Document)
			: base()
		{
			this.fieldName = FieldName;
			this.type= Type;
			this.value = Value;
			this.document = Document;
		}

		/// <summary>
		/// Field Name
		/// </summary>
		public string Name => this.fieldName;

		/// <summary>
		/// Type
		/// </summary>
		public Asn1Type Type => this.type;

		/// <summary>
		/// Type definition
		/// </summary>
		public Asn1Value Value => this.value;

		/// <summary>
		/// ASN.1 Document in where the value is defined.
		/// </summary>
		public Asn1Document Document => this.document;

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
			if (Pass == CSharpExportPass.Variables)
			{
				if (!State.ExportingValues)
				{
					State.ExportingValues = true;

					Output.Append(Tabs(Indent));
					Output.AppendLine("public static partial class Values");

					Output.Append(Tabs(Indent));
					Output.AppendLine("{");

					State.ExportingValuesIndent = Indent + 1;
				}

				Output.Append(Tabs(State.ExportingValuesIndent));
				Output.Append("public static readonly ");

				this.type.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

				Output.Append(' ');
				Output.Append(ToCSharp(this.fieldName));
				Output.Append(" = ");
				this.value.ExportCSharp(Output, State, State.ExportingValuesIndent, CSharpExportPass.Explicit);
				Output.AppendLine(";");
			}
		}
	}
}
