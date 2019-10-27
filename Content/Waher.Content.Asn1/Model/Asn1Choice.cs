using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 CHOICE construct.
	/// </summary>
	public class Asn1Choice : Asn1List
	{
		/// <summary>
		/// Represents a ASN.1 CHOICE construct.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		public Asn1Choice(string Name, bool TypeDef, Asn1Node[] Nodes)
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
			if (Pass == CSharpExportPass.Explicit && !this.TypeDefinition)
			{
				Output.Append(ToCSharp(this.Name));
				Output.Append("Choice");
			}
			else
			{
				if (Pass == CSharpExportPass.Implicit)
				{
					State.ClosePending(Output);

					foreach (Asn1Node Node in this.Nodes)
						Node.ExportCSharp(Output, State, Indent, Pass);

					Output.Append(Tabs(Indent));
					Output.Append("public enum ");
					Output.Append(ToCSharp(this.Name));
					Output.AppendLine("Enum");

					Output.Append(Tabs(Indent));
					Output.Append("{");

					Indent++;

					bool First = true;

					foreach (Asn1Node Node in this.Nodes)
					{
						if (Node is Asn1FieldDefinition Field)
						{
							if (First)
								First = false;
							else
								Output.Append(',');

							Output.AppendLine();
							Output.Append(Tabs(Indent));
							Output.Append(Field.FieldName);
						}
					}

					Indent--;

					Output.AppendLine();
					Output.Append(Tabs(Indent));
					Output.AppendLine("}");
					Output.AppendLine();
				}

				if (Pass == CSharpExportPass.Implicit || Pass == CSharpExportPass.Explicit)
				{
					Output.Append(Tabs(Indent));
					Output.Append("public class ");
					Output.Append(ToCSharp(this.Name));
					if (!this.TypeDefinition)
						Output.Append("Choice");
					Output.AppendLine();

					Output.Append(Tabs(Indent));
					Output.AppendLine("{");

					Indent++;

					Output.Append(Tabs(Indent));
					Output.Append(ToCSharp(this.Name));
					if (!this.TypeDefinition)
						Output.Append("Enum");
					Output.AppendLine(" _choice;");

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
}
