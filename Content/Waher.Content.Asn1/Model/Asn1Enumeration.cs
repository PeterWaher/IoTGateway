using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Values;

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
			foreach (Asn1Node Node in Nodes)
			{
				if (Node is Asn1NamedValue NamedValue)
					NamedValue.EnumeratedValue = true;
			}
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
		public override async Task ExportCSharp(StringBuilder Output, CSharpExportState State, 
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Implicit)
			{
				State.ClosePending(Output);

				foreach (Asn1Node Node in this.Nodes)
					await Node.ExportCSharp(Output, State, Indent, Pass);

				JSON.Indent(Output, Indent);
				Output.Append("public enum ");
				Output.Append(ToCSharp(this.Name));
				if (!this.TypeDefinition)
					Output.AppendLine("Enum");

				JSON.Indent(Output, Indent);
				Output.AppendLine("{");

				Indent++;

				bool First = true;

				foreach (Asn1Node Node in this.Nodes)
				{
					if (First)
						First = false;
					else
						Output.AppendLine(",");

					JSON.Indent(Output, Indent);
					await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
				}

				Indent--;

				Output.AppendLine();
				JSON.Indent(Output, Indent);
				Output.AppendLine("}");
				Output.AppendLine();
			}
			else if (Pass == CSharpExportPass.Explicit)
			{
				if (this.TypeDefinition)
				{
					State.ClosePending(Output);

					JSON.Indent(Output, Indent);
					Output.Append("public enum ");
					Output.AppendLine(ToCSharp(this.Name));

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					bool First = true;

					foreach (Asn1Node Node in this.Nodes)
					{
						if (First)
							First = false;
						else
							Output.AppendLine(",");

						JSON.Indent(Output, Indent);
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
					}

					if (!First)
						Output.AppendLine();

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");
					Output.AppendLine();
				}
				else
				{
					Output.Append(ToCSharp(this.Name));
					Output.Append("Enum");
				}
			}
		}

	}
}
