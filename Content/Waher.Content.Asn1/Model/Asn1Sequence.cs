using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SEQUENCE construct.
	/// </summary>
	public class Asn1Sequence : Asn1List
	{
		/// <summary>
		/// Represents a ASN.1 SEQUENCE construct.
		/// </summary>
		/// <param name="FieldName">Optional field name.</param>
		/// <param name="Nodes">Nodes</param>
		public Asn1Sequence(string FieldName, Asn1Node[] Nodes)
			: base(FieldName, Nodes)
		{
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="TypeName">Type name.</param>
		public override void ExportCSharpTypeDefinition(StringBuilder Output, CSharpExportSettings Settings,
			int Indent, string TypeName)
		{
			Output.Append(Tabs(Indent));
			Output.Append("public class ");
			Output.AppendLine(TypeName);

			Output.Append(Tabs(Indent));
			Output.AppendLine("{");

			Indent++;

			foreach (Asn1Node Node in this.Nodes)
				Node.ExportImplicitCSharp(Output, Settings, Indent);

			foreach (Asn1Node Node in this.Nodes)
				Node.ExportCSharp(Output, Settings, Indent);

			Indent--;

			Output.Append(Tabs(Indent));
			Output.AppendLine("}");
			Output.AppendLine();
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => this.FieldName + "Seq";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => true;

		/// <summary>
		/// Exports implicit definitions to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public override void ExportImplicitCSharp(StringBuilder Output, CSharpExportSettings Settings, int Indent)
		{
			foreach (Asn1Node Node in this.Nodes)
				Node.ExportImplicitCSharp(Output, Settings, Indent);

			Output.Append(Tabs(Indent));
			Output.Append("public class ");
			Output.Append(this.FieldName);
			Output.AppendLine("Seq");

			Output.Append(Tabs(Indent));
			Output.AppendLine("{");

			Indent++;

			foreach (Asn1Node Node in this.Nodes)
				Node.ExportCSharp(Output, Settings, Indent);

			Indent--;

			Output.AppendLine();
			Output.Append(Tabs(Indent));
			Output.AppendLine("}");
			Output.AppendLine();
		}

	}
}
