using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 field value definition.
	/// </summary>
	public class Asn1FieldValueDefinition : Asn1Node, INamedNode
	{
		/// <summary>
		/// Name of structure holding public constant definitions.
		/// </summary>
		public const string DefinedValues = "_DefinedValues";

		private readonly string fieldName;
		private readonly string fieldNameCSharp;
		private readonly Asn1Type type;
		private readonly Asn1Value value;

		/// <summary>
		/// Represents an ASN.1 field value definition.
		/// </summary>
		/// <param name="FieldName">Field name.</param>
		/// <param name="Type">Type</param>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 Document in where the value is defined.</param>
		public Asn1FieldValueDefinition(string FieldName, Asn1Type Type, 
			Asn1Value Value, Asn1Document Document)
			: base(Document)
		{
			this.fieldName = FieldName;
			this.fieldNameCSharp = ToCSharp(FieldName);
			this.type= Type;
			this.value = Value;

			this.SetAsChild(this.type);
			this.SetAsChild(this.value);
		}

		/// <summary>
		/// Field Name
		/// </summary>
		public string Name => this.fieldName;

		/// <summary>
		/// If the named node represents a type.
		/// </summary>
		public bool IsType => false;

		/// <summary>
		/// Gets the fully qualified name, in reverse order.
		/// </summary>
		public void GetFullyQualifiedName(ChunkedList<string> Fqn)
		{
			Fqn.Add(this.Name);
			Fqn.Add(DefinedValues);

			if (this.TryGetAncestor(out INamedNode Container))
				Container.GetFullyQualifiedName(Fqn);
		}

		/// <summary>
		/// If the named element must be qualified.
		/// </summary>
		public bool MustBeQualified => true;

		/// <summary>
		/// Type
		/// </summary>
		public Asn1Type Type => this.type;

		/// <summary>
		/// Value
		/// </summary>
		public Asn1Value Value => this.value;

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
				await this.type.ExportRestrictionCSharp(Output, State, Indent,
					this.fieldNameCSharp, Pass);
			}
			else if (Pass == CSharpExportPass.Variables)
			{
				if (!State.ExportingValues)
				{
					State.ExportingValues = true;

					JSON.Indent(Output, Indent);
					Output.Append("public static partial class ");
					Output.AppendLine(DefinedValues);

					JSON.Indent(Output, Indent);
					Output.AppendLine("{");

					State.ExportingValuesIndent = Indent + 1;
				}

				await this.type.ExportRestrictionCSharp(Output, State, Indent,
					this.fieldNameCSharp, CSharpExportPass.Variables);

				JSON.Indent(Output, State.ExportingValuesIndent);
				Output.Append("public static readonly ");

				await this.type.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

				Output.Append(' ');
				Output.Append(this.fieldNameCSharp);
				Output.Append(" = ");
				await this.value.ExportCSharp(Output, State, State.ExportingValuesIndent, CSharpExportPass.Explicit);
				Output.AppendLine(";");
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.fieldName);

			if (!(this.type is null))
			{
				sb.Append(" : ");
				sb.Append(this.type.ToString());
			}

			if (!(this.value is null))
			{
				sb.Append(" = ");
				sb.Append(this.value.ToString());
			}

			return sb.ToString();
		}
	}
}
