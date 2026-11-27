using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Defines a class.
	/// </summary>
	public class Asn1ClassDefinition : Asn1Type, INamedNode
	{
		private readonly Asn1TypeDeclaration[] typeDeclarations;
		private readonly Asn1PropertyDefinition[] properties;
		private readonly string name;
		private Dictionary<string, string> identifiers;
		private Asn1SyntaxNode[] syntax = null;

		/// <summary>
		/// Defines a class.
		/// </summary>
		/// <param name="Name">Class name.</param>
		/// <param name="TypeDeclarations">Type declarations.</param>
		/// <param name="Tags">How tags are encoded</param>
		/// <param name="Properties">Class properties.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassDefinition(string Name, Asn1TypeDeclaration[] TypeDeclarations,
			Asn1Tags? Tags, Asn1PropertyDefinition[] Properties, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.typeDeclarations = TypeDeclarations;
			this.properties = Properties;
			this.Tags = Tags;
		}

		/// <summary>
		/// Class name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Type declarations.
		/// </summary>
		public Asn1TypeDeclaration[] TypeDeclarations => this.typeDeclarations;

		/// <summary>
		/// Property definitions
		/// </summary>
		public Asn1PropertyDefinition[] Properties => this.properties;

		/// <summary>
		/// Optional syntax.
		/// </summary>
		public Asn1SyntaxNode[] Syntax
		{
			get => this.syntax;
			internal set => this.syntax = value;
		}

		/// <summary>
		/// If the class definition has syntax defined.
		/// </summary>
		public bool HasSyntax => !(this.syntax is null);

		/// <inheritdoc/>
		public override string ToString() => this.name;

		/// <summary>
		/// If the named element must be qualified.
		/// </summary>
		public bool MustBeQualified => false;

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
		/// Adds an identification record.
		/// </summary>
		/// <param name="IdentifiedProperty">Property being identified</param>
		/// <param name="IdentifierProperty">Property used to identify property</param>
		public void AddIdentifier(string IdentifiedProperty, string IdentifierProperty)
		{
			if (this.identifiers is null)
				this.identifiers = new Dictionary<string, string>();

			this.identifiers[IdentifiedProperty] = IdentifierProperty;
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
			if (Pass == CSharpExportPass.Preprocess || Pass == CSharpExportPass.Variables)
			{
				foreach (Asn1TypeDeclaration TypeDeclaration in this.typeDeclarations)
					await TypeDeclaration.ExportCSharp(Output, State, Indent, Pass);

				foreach (Asn1PropertyDefinition PropertyDefinition in this.properties)
					await PropertyDefinition.ExportCSharp(Output, State, Indent, Pass);
			}
			else
			{
				if (Pass == CSharpExportPass.Implicit)
				{
					State.ClosePending(Output);

					foreach (Asn1TypeDeclaration TypeDeclaration in this.typeDeclarations)
						await TypeDeclaration.ExportCSharp(Output, State, Indent, Pass);

					foreach (Asn1PropertyDefinition PropertyDefinition in this.properties)
						await PropertyDefinition.ExportCSharp(Output, State, Indent, Pass);
				}

				if (Pass == CSharpExportPass.Implicit || Pass == CSharpExportPass.Explicit)
				{
					foreach (Asn1TypeDeclaration TypeDeclaration in this.typeDeclarations)
						await TypeDeclaration.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					foreach (Asn1PropertyDefinition PropertyDefinition in this.properties)
						await PropertyDefinition.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					// Class definition

					JSON.Indent(Output, Indent);
					Output.Append("public partial class ");
					Output.Append(ToCSharp(this.Name));
					Output.AppendLine(" : Asn1.Construct");

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					// Properties

					Indent++;

					foreach (Asn1TypeDeclaration TypeDeclaration in this.typeDeclarations)
						await TypeDeclaration.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					foreach (Asn1PropertyDefinition PropertyDefinition in this.properties)
						await PropertyDefinition.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					// Constructor

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public ");
					Output.Append(ToCSharp(State, this, this.Name));
					Output.AppendLine("()");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");
					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public static T Create(Reader)

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public static ");
					Output.Append(ToCSharp(State, this, this.Name));
					Output.AppendLine(" Create(Asn1F.AsnReader Reader)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.Append(ToCSharp(State, this, this.Name));
					Output.Append(" _Result = new ");
					Output.Append(ToCSharp(State, this, this.Name));
					Output.AppendLine("();");

					JSON.Indent(Output, Indent);
					Output.AppendLine("_Result.Read(Reader);");

					JSON.Indent(Output, Indent);
					Output.AppendLine("return _Result;");

					Indent--;
					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public void Read(Reader)

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.AppendLine("public override void Read(Asn1F.AsnReader Reader0)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("Asn1F.AsnReader Reader = Reader0.ReadSequence();");

					int Index = 0;

					foreach (Asn1PropertyDefinition PropertyDefinition in this.properties)
					{
						await PropertyDefinition.ExportReadCSharp(Output, State, Indent, "this", Index++);
						await PropertyDefinition.ExportRestrictionCheck(Output, State, Indent, "this");
					}

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public static T TryCreate(Reader, out Result)

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public static bool TryCreate(Asn1F.AsnReader Reader0, out ");
					Output.Append(ToCSharp(State, this, this.Name));
					Output.AppendLine(" _Result)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);");
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
					Output.AppendLine("();");

					JSON.Indent(Output, Indent);
					Output.AppendLine("_Result.Read(Reader);");

					JSON.Indent(Output, Indent);
					Output.AppendLine("return true;");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public void Write(Writer)

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.AppendLine("public override void Write(Asn1F.AsnWriter Writer)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("using (Writer.PushSequence())");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					Index = 0;

					Asn1Tags? Tags = this.Tags;
					if (Tags.HasValue && Tags.Value == Asn1Tags.Automatic)
						Tags = Asn1Tags.Implicit;   // For SEQUENCE, AUTOMATIC equals IMPLICIT

					foreach (Asn1PropertyDefinition PropertyDefinition in this.properties)
					{
						await PropertyDefinition.ExportRestrictionCheck(Output, State, Indent, "this");
						await PropertyDefinition.ExportWriteCSharp(Output, State, Indent, "this", Index++, Tags);
					}

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public bool Equals(object)

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.AppendLine("public override bool Equals(object Object)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.Append("if (!(Object is ");
					Output.Append(ToCSharp(this.Name));
					Output.AppendLine(" _Typed)) return false;");

					foreach (Asn1PropertyDefinition PropertyDefinition in this.properties)
					{
						JSON.Indent(Output, Indent);
						Output.Append("if (!AreEqual(this.");
						Output.Append(ToCSharp(PropertyDefinition.Name));
						Output.Append(", _Typed.");
						Output.Append(ToCSharp(PropertyDefinition.Name));
						Output.AppendLine(")) return false;");
					}

					JSON.Indent(Output, Indent);
					Output.AppendLine("return true;");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public int GetHashCode()

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.AppendLine("public override int GetHashCode()");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("int _Result = 0;");

					foreach (Asn1PropertyDefinition PropertyDefinition in this.properties)
					{
						JSON.Indent(Output, Indent);
						Output.Append("_Result ^= _Result << 5 ^ CalcHashCode(this.");
						Output.Append(ToCSharp(PropertyDefinition.Name));
						Output.AppendLine(");");
					}

					JSON.Indent(Output, Indent);
					Output.AppendLine("return _Result;");

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

	}
}
