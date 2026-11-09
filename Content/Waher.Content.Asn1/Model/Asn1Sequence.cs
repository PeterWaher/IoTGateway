using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SEQUENCE construct.
	/// </summary>
	public class Asn1Sequence : Asn1List, INamedNode
	{
		/// <summary>
		/// Represents a ASN.1 SEQUENCE construct.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Sequence(string Name, bool TypeDef, Asn1Node[] Nodes, 
			Asn1Document Document)
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
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				string s = ToCSharp(this.Name);
				if (!this.TypeDefinition)
					s += "Seq";
				return s;
			}
		}

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
			if (Pass == CSharpExportPass.Explicit && !this.TypeDefinition)
			{
				Output.Append(ToCSharp(State, this, this.Name));
				Output.Append("Seq");
			}
			else
			{
				if (Pass == CSharpExportPass.Implicit)
				{
					State.ClosePending(Output);

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, Pass);
				}

				if (Pass == CSharpExportPass.Implicit || Pass == CSharpExportPass.Explicit)
				{
					JSON.Indent(Output, Indent);
					Output.Append("public partial class ");
					Output.Append(ToCSharp(this.Name));
					if (!this.TypeDefinition)
						Output.Append("Seq");
					Output.AppendLine();

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public static ");
					Output.Append(ToCSharp(State, this, this.Name));
					if (!this.TypeDefinition)
						Output.Append("Seq");
					Output.AppendLine(" Create(Asn1F.AsnReader Reader)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.Append(ToCSharp(State, this, this.Name));
					if (!this.TypeDefinition)
						Output.Append("Seq");
					Output.AppendLine(" _Result = new();");

					JSON.Indent(Output, Indent);
					Output.AppendLine("_Result.Read(Reader);");

					JSON.Indent(Output, Indent);
					Output.AppendLine("return _Result;");

					Indent--;
					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public void ");
					Output.AppendLine(" Read(Asn1F.AsnReader Reader0)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("Asn1F.AsnReader Reader = Reader0.ReadSequence();");

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportReadCSharp(Output, State, Indent, "this");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public static bool TryCreate(Asn1F.AsnReader Reader0, out ");
					Output.Append(ToCSharp(State, this, this.Name));
					if (!this.TypeDefinition)
						Output.Append("Seq");
					Output.AppendLine(" _Result)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("Asn1F.AsnReader Reader = Asn1.Common.TryReadInnerVector(Reader0);");
					JSON.Indent(Output, Indent);
					Output.AppendLine("if (Reader is null)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");
					JSON.Indent(Output, Indent + 1);
					Output.AppendLine("_Result = null;");
					JSON.Indent(Output, Indent + 1);
					Output.AppendLine("return false;");
					JSON.Indent(Output, Indent);
					Output.AppendLine("}");
					Output.AppendLine();

					JSON.Indent(Output, Indent);
					Output.Append("_Result = new ");
					Output.Append(ToCSharp(State, this, this.Name));
					if (!this.TypeDefinition)
						Output.Append("Seq");
					Output.AppendLine("();");

					JSON.Indent(Output, Indent);
					Output.AppendLine("_Result.Read(Reader);");

					JSON.Indent(Output, Indent);
					Output.AppendLine("return true;");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.AppendLine("public void Write(Asn1F.AsnWriter Writer)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					// TODO: ASN.1 Write method

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");
					Output.AppendLine();
				}
			}
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		public override Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf)
		{
			Output.Append(ToCSharp(State, this, this.Name));
			if (!this.TypeDefinition)
				Output.Append("Seq");
			Output.Append(".Create(Reader)");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Exports optional read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="ItemName">Name of item variable.</param>
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string ItemName)
		{
			Output.Append(ToCSharp(State, this, this.Name));
			if (!this.TypeDefinition)
				Output.Append("Seq");

			if (string.IsNullOrEmpty(ItemName))
				Output.Append(".TryCreate(Reader, out var _Item) ? _Item : default");
			else
			{
				Output.Append(".TryCreate(Reader, out ");
				Output.Append(ItemName);
				Output.Append(')');
			}

			return Task.CompletedTask;
		}

	}
}
