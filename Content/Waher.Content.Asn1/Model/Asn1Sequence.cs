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
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		public Asn1Sequence(string Name, bool TypeDef, Asn1Node[] Nodes)
			: base(Name, TypeDef, Nodes)
		{
		}

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public override bool ConstructedType => true;

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
			if (Pass == CSharpExportPass.Implicit)
			{
				State.ClosePending(Output);

				foreach (Asn1Node Node in this.Nodes)
					Node.ExportCSharp(Output, State, Indent, Pass);

				Output.Append(Tabs(Indent));
				Output.Append("public class ");
				Output.Append(ToCSharp(this.Name));
				if (!this.TypeDefinition)
					Output.AppendLine("Seq");

				Output.Append(Tabs(Indent));
				Output.AppendLine("{");

				Indent++;

				foreach (Asn1Node Node in this.Nodes)
					Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

				Indent--;

				Output.AppendLine();
				Output.Append(Tabs(Indent));
				Output.AppendLine("}");
				Output.AppendLine();
			}
			else if (Pass == CSharpExportPass.Explicit)
			{
				if (this.TypeDefinition)
				{
					State.ClosePending(Output);

					Output.Append(Tabs(Indent));
					Output.Append("public class ");
					Output.AppendLine(ToCSharp(this.Name));

					Output.Append(Tabs(Indent));
					Output.AppendLine("{");

					Indent++;

					foreach (Asn1Node Node in this.Nodes)
						Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					foreach (Asn1Node Node in this.Nodes)
						Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					Indent--;

					Output.Append(Tabs(Indent));
					Output.AppendLine("}");
					Output.AppendLine();
				}
				else
				{
					Output.Append(ToCSharp(this.Name));
					Output.Append("Seq");
				}
			}
		}

	}
}
