using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 CHOICE construct.
	/// </summary>
	public class Asn1Choice : Asn1List, INamedNode
	{
		/// <summary>
		/// Represents a ASN.1 CHOICE construct.
		/// </summary>
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Choice(string Name, bool TypeDef, Asn1Node[] Nodes, Asn1Document Document)
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
					s += "Choice";
				return s;
			}
		}

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			int Bak = Document.pos;

			foreach (Asn1Node Choice in this.Nodes)
			{
				if (Choice is Asn1Type Type)
				{
					try
					{
						Document.pos = Bak;
						return Type.Parse(Document, Macro);
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}

			throw Document.SyntaxError("Unable to parse choices.");
		}

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags
		{
			get
			{
				SortedDictionary<int, Asn1Tag> Tags = new SortedDictionary<int, Asn1Tag>();

				foreach (Asn1Node Node in this.Nodes)
				{
					foreach (Asn1Tag Tag in Node.AcceptedTags)
						Tags[Construct.ToInt32(Tag)] = Tag;
				}

				Asn1Tag[] Result = new Asn1Tag[Tags.Count];
				Tags.Values.CopyTo(Result, 0);
				return Result;
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
				Output.Append("Choice");
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
					// Class definition

					JSON.Indent(Output, Indent);
					Output.Append("public partial class ");
					Output.Append(ToCSharp(this.Name));
					if (!this.TypeDefinition)
						Output.Append("Choice");
					Output.AppendLine(" : Asn1.Construct");

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					// Properties

					Indent++;

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					JSON.Indent(Output, Indent);
					Output.AppendLine("public enum _Options");
					JSON.Indent(Output, Indent);
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
							JSON.Indent(Output, Indent);
							Output.Append(ToCSharp(Field.Name));
						}
					}

					Indent--;

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.AppendLine("}");
					Output.AppendLine();

					JSON.Indent(Output, Indent);
					Output.AppendLine("public _Options _choice;");

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					// public static T Create(Reader)

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public static ");
					Output.Append(ToCSharp(State, this, this.Name));
					if (!this.TypeDefinition)
						Output.Append("Choice");
					Output.AppendLine(" Create(Asn1F.AsnReader Reader)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.Append(ToCSharp(State, this, this.Name));
					if (!this.TypeDefinition)
						Output.Append("Choice");
					Output.AppendLine(" _Result = new();");

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
					Output.Append("public void ");
					Output.AppendLine(" Read(Asn1F.AsnReader Reader)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("Asn1F.Asn1Tag Tag = Reader.PeekTag();");
					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.AppendLine("switch (ToInt32(Tag))");

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");
					Indent++;

					Asn1Tag[] AcceptedTags;
					int Index = 0;

					foreach (Asn1Node Node in this.Nodes)
					{
						if (!(Node is Asn1FieldDefinition FieldDef))
							throw new Exception("Expected ASN.1 field definitions inside CHOICE.");

						Asn1Tags? Tags = this.Tags;

						switch (Tags)
						{
							case Asn1Tags.Implicit:
								AcceptedTags = new Asn1Tag[]
								{
									new Asn1Tag(TagClass.ContextSpecific, Index)
								};
								break;

							case Asn1Tags.Explicit:
							case Asn1Tags.Automatic:    // For CHOICE, AUTOMATIC equals EXPLICIT
								Tags = Asn1Tags.Explicit;
								AcceptedTags = new Asn1Tag[]
								{
									new Asn1Tag(TagClass.ContextSpecific, Index, true)
								};
								break;

							default:
								AcceptedTags = FieldDef.AcceptedTags;
								break;
						}

						foreach (Asn1Tag Tag in AcceptedTags)
						{
							JSON.Indent(Output, Indent);
							Output.Append("case ");
							Output.Append(Tag.TagValue + (int)Tag.TagClass);
							Output.Append(": // ");

							switch (Tag.TagClass)
							{
								case TagClass.Universal:
									Output.Append(((UniversalTagNumber)Tag.TagValue).ToString());
									break;

								case TagClass.Application:
									Output.Append("APPLICATION [");
									Output.Append(Tag.TagValue);
									Output.Append(']');
									break;

								case TagClass.ContextSpecific:
									Output.Append("CONTEXT SPECIFIC [");
									Output.Append(Tag.TagValue);
									Output.Append(']');
									break;

								case TagClass.Private:
									Output.Append("PRIVATE [");
									Output.Append(Tag.TagValue);
									Output.Append(']');
									break;
							}
							Output.AppendLine();
						}

						Indent++;

						await Node.ExportReadCSharp(Output, State, Indent, "this", Index++);

						JSON.Indent(Output, Indent);
						Output.Append("this._choice = _Options.");
						Output.Append(ToCSharp(FieldDef.Name));
						Output.AppendLine(";");

						JSON.Indent(Output, Indent);
						Output.AppendLine("break;");
						Output.AppendLine();

						Indent--;
					}

					JSON.Indent(Output, Indent);
					Output.AppendLine("default:");
					JSON.Indent(Output, Indent + 1);
					Output.Append("throw new Asn1F.AsnContentException(\"Unexpected tag: \" + ");
					Output.Append("Tag.TagClass.ToString() + \", \" + ");
					Output.AppendLine("Tag.TagValue.ToString() + \".\");");

					Indent--;
					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// TODO: ASN.1 Read method

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public static T TryCreate(Reader, out Result)

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public static bool TryCreate(Asn1F.AsnReader Reader, out ");
					Output.Append(ToCSharp(State, this, this.Name));
					if (!this.TypeDefinition)
						Output.Append("Choice");
					Output.AppendLine(" _Result)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("try");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.Append("_Result = new ");
					Output.Append(ToCSharp(State, this, this.Name));
					if (!this.TypeDefinition)
						Output.Append("Choice");
					Output.AppendLine("();");

					JSON.Indent(Output, Indent);
					Output.AppendLine("_Result.Read(Reader);");

					JSON.Indent(Output, Indent);
					Output.AppendLine("return true;");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");
					JSON.Indent(Output, Indent);
					Output.AppendLine("catch (System.Exception)");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					JSON.Indent(Output, Indent);
					Output.AppendLine("_Result = null;");
					JSON.Indent(Output, Indent);
					Output.AppendLine("return false;");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public void Write(Writer)

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
		/// <param name="Index">Item index.</param>
		public override Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf, int Index)
		{
			Output.Append(ToCSharp(State, this, this.Name));
			if (!this.TypeDefinition)
				Output.Append("Choice");
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
		/// <param name="Index">Item index.</param>
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string ItemName,
			int Index)
		{
			Output.Append(ToCSharp(State, this, this.Name));
			if (!this.TypeDefinition)
				Output.Append("Choice");

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
