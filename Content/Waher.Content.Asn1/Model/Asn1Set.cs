using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SET construct.
	/// </summary>
	public class Asn1Set : Asn1List, INamedNode
	{
		/// <summary>
		/// Represents a ASN.1 SET construct.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Set(string Name, bool TypeDef, Asn1Node[] Nodes, Asn1Document Document)
			: base(Name, TypeDef, Nodes, Document)
		{
		}

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public override bool ConstructedType => true;

		/// <summary>
		/// If the named node represents a type.
		/// </summary>
		public bool IsType => true;

		/// <summary>
		/// Gets the fully qualified name, in reverse order.
		/// </summary>
		public void GetFullyQualifiedName(ChunkedList<string> Fqn)
		{
			Fqn.Add(this.Name);

			if (this.TryGetAncestor(out INamedNode Container))
				Container.GetFullyQualifiedName(Fqn);
		}

		/// <summary>
		/// If the named element must be qualified.
		/// </summary>
		public bool MustBeQualified => false;

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
			/*if (Pass == CSharpExportPass.Implicit)
			{
				State.ClosePending(Output);

				foreach (Asn1Node Node in this.Nodes)
					await Node.ExportCSharp(Output, State, Indent, Pass);

				JSON.Indent(Output, Indent);
				Output.Append("public partial class ");
				Output.Append(ToCSharp(this.Name));
				if (!this.TypeDefinition)
					Output.Append("Set");
				Output.AppendLine();

				JSON.Indent(Output, Indent);
				Output.AppendLine("{");

				Indent++;

				foreach (Asn1Node Node in this.Nodes)
					await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

				Indent--;

				Output.AppendLine();
				JSON.Indent(Output, Indent);
				Output.AppendLine("}");
				Output.AppendLine();
			}
			else*/ if (Pass == CSharpExportPass.Explicit)
			{
				if (this.TypeDefinition)
				{
					State.ClosePending(Output);

					JSON.Indent(Output, Indent);
					Output.Append("public partial class ");
					Output.AppendLine(ToCSharp(this.Name));

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");
					Output.AppendLine();
				}
				else
				{
					Output.Append(ToCSharp(State, this, this.Name));
					Output.Append("Set");
				}
			}
		}
	}
}
