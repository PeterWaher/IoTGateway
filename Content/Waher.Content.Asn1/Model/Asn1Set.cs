using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SET construct.
	/// </summary>
	public class Asn1Set : Asn1List
	{
		/// <summary>
		/// Represents a ASN.1 SET construct.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		public Asn1Set(string Name, bool TypeDef, Asn1Node[] Nodes)
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
			if (Pass == CSharpExportPass.Explicit)
			{
				Output.Append(this.Name);

				if (!this.TypeDefinition)
					Output.Append("Set");
			}
			else if (Pass == CSharpExportPass.Implicit)
			{
				State.ClosePending(Output);

				foreach (Asn1Node Node in this.Nodes)
					Node.ExportCSharp(Output, State, Indent, Pass);

				Output.Append(Tabs(Indent));
				Output.Append("public class ");
				Output.Append(this.Name);
				if (!this.TypeDefinition)
					Output.AppendLine("Set");

				Output.Append(Tabs(Indent));
				Output.AppendLine("{");

				Indent++;

				foreach (Asn1Node Node in this.Nodes)
					Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

				Indent--;

				Output.Append(Tabs(Indent));
				Output.AppendLine("}");
				Output.AppendLine();
			}
		}
	}
}
