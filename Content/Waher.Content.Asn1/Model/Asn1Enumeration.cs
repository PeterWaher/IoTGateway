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
		public Asn1Enumeration(string Name, bool TypeDef, Asn1Node[] Nodes)
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
					Output.Append("Enum");

				if (this.Optional.HasValue && this.Optional.Value)
					Output.Append('?');
			}
			else if (Pass == CSharpExportPass.Implicit)
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
					Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
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
