using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Set of classes.
	/// </summary>
	public class Asn1ClassSet : Asn1Type, INamedNode
	{
		private readonly Asn1ClassInstanceElement[] classes;
		private readonly Asn1ClassDefinition @class;
		private readonly string className;
		private readonly string name;

		/// <summary>
		/// Set of classes.
		/// </summary>
		/// <param name="Name">Class set name.</param>
		/// <param name="Class">Class definition</param>
		/// <param name="Classes">Class instance elements.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassSet(string Name, Asn1ClassDefinition Class,
			Asn1ClassInstanceElement[] Classes, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.@class = Class;
			this.className = Class.Name;
			this.classes = Classes;
		}

		/// <summary>
		/// Set of classes.
		/// </summary>
		/// <param name="Name">Class set name.</param>
		/// <param name="ClassName">Class name.</param>
		/// <param name="Classes">Class instance elements.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassSet(string Name, string ClassName,
			Asn1ClassInstanceElement[] Classes, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.@class = null;
			this.className = ClassName;
			this.classes = Classes;
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
		/// Class name.
		/// </summary>
		public string ClassName => this.className;

		/// <summary>
		/// Class instance elements.
		/// </summary>
		public Asn1ClassInstanceElement[] Classes => this.classes;

		/// <inheritdoc/>
		public override string ToString() => this.Name;

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
			if (Pass == CSharpExportPass.Preprocess)
			{
				foreach (Asn1ClassInstanceElement Element in this.classes)
					await Element.ExportCSharp(Output, State, Indent, Pass);
			}
			else if (Pass == CSharpExportPass.Variables)
			{
				if (!State.ExportingValues)
				{
					State.ExportingValues = true;

					JSON.Indent(Output, Indent);
					Output.Append("public static partial class ");
					Output.AppendLine(Asn1FieldValueDefinition.DefinedValues);

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					State.ExportingValuesIndent = Indent + 1;
				}

				Indent++;

				JSON.Indent(Output, Indent);
				Output.Append("public static ");
				Output.Append(ToCSharp(State, this.@class, this.@class.Name));
				Output.Append("[] ");
				Output.Append(ToCSharp(this.name));
				Output.Append(" = new ");
				Output.Append(ToCSharp(State, this.@class, this.@class.Name));
				Output.AppendLine("[]");

				JSON.Indent(Output, Indent);
				Output.AppendLine("{");

				Indent++;

				bool First = true;

				foreach (Asn1ClassInstanceElement Element in this.classes)
				{
					if (First)
						First = false;
					else
						Output.AppendLine(",");

					JSON.Indent(Output, Indent);
					await Element.ExportCSharp(Output, State, Indent, Pass);
				}

				if (!First)
					Output.AppendLine();

				Indent--;

				JSON.Indent(Output, Indent);
				Output.AppendLine("};");
			}
		}
	}
}
