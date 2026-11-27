using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Instance of a class.
	/// </summary>
	public class Asn1ClassInstance : Asn1ClassInstanceElement, INamedNode
	{
		private readonly Dictionary<string, Asn1Node> properties;
		private readonly Asn1ClassDefinition @class;
		private readonly string name;

		/// <summary>
		/// Instance of a class.
		/// </summary>
		/// <param name="Name">Class instance name.</param>
		/// <param name="Class">Class definition</param>
		/// <param name="Tags">How tags are encoded</param>
		/// <param name="Properties">Instance property values.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassInstance(string Name, Asn1ClassDefinition Class,
			Asn1Tags? Tags, Dictionary<string, Asn1Node> Properties, 
			Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.@class = Class;
			this.properties = Properties;
			this.Tags = Tags;
		}

		/// <summary>
		/// Class instance name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Class definition.
		/// </summary>
		public Asn1ClassDefinition Class => this.@class;

		/// <summary>
		/// Class instance property values.
		/// </summary>
		public Dictionary<string, Asn1Node> Properties => this.properties;

		/// <inheritdoc/>
		public override string ToString() => this.Name + " : " + this.@class.ToString();

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
				// TODO: All properties in class definition also, if not available in the instance list

				foreach (Asn1Node PropertyValue in this.properties.Values)
					await PropertyValue.ExportCSharp(Output, State, Indent, Pass);
			}
			else
			{
				if (Pass == CSharpExportPass.Implicit)
				{
					State.ClosePending(Output);

					foreach (Asn1Node PropertyValue in this.properties.Values)
						await PropertyValue.ExportCSharp(Output, State, Indent, Pass);
				}

				if (Pass == CSharpExportPass.Implicit || Pass == CSharpExportPass.Explicit)
				{
					foreach (Asn1Node PropertyValue in this.properties.Values)
						await PropertyValue.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);

					// Class definition

					JSON.Indent(Output, Indent);
					Output.Append("public partial class ");
					Output.Append(ToCSharp(this.Name));
					Output.Append(" : ");
					Output.AppendLine(ToCSharp(this.@class.Name));

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					// Constructor

					JSON.Indent(Output, Indent);
					Output.Append("public ");
					Output.Append(ToCSharp(State, this, this.Name));
					Output.AppendLine("()");
					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					Indent++;

					foreach (Asn1Node PropertyValue in this.properties.Values)
						await PropertyValue.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");

					// public static T Create(Reader)

					Output.AppendLine();
					JSON.Indent(Output, Indent);
					Output.Append("public new static ");
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

					Indent--;

					JSON.Indent(Output, Indent);
					Output.AppendLine("}");
					Output.AppendLine();
				}
			}
		}

	}
}
