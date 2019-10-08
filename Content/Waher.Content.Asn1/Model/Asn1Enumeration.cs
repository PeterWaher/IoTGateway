using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// ENUMERATED
	/// </summary>
	public class Asn1Enumeration : Asn1List
	{
		/// <summary>
		/// ENUMERATED
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Enumeration(string Name, bool TypeDef, Asn1Node[] Nodes, bool Implicit)
			: base(Name, TypeDef, Nodes, Implicit)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => this.TypeDefinition ? this.Name : this.Name + "Enum";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => false;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings, 
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Implicit)
			{
				Output.Append(Tabs(Indent));
				Output.Append("public enum ");
				Output.Append(this.Name);
				if (!this.TypeDefinition)
					Output.AppendLine("Enum");

				Output.Append(Tabs(Indent));
				Output.Append("{");

				Indent++;

				bool First = true;

				foreach (Asn1Node Node in this.Nodes)
				{
					if (First)
						First = false;
					else
						Output.Append(',');

					Output.AppendLine();
					Output.Append(Tabs(Indent));
					Node.ExportCSharp(Output, Settings, Indent, Pass);
				}

				Indent--;

				Output.AppendLine();
				Output.Append(Tabs(Indent));
				Output.AppendLine("}");
				Output.AppendLine();
			}
		}

	}
}
