using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Types;
using Waher.Content.Asn1.Model.Values;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 field definition.
	/// </summary>
	public class Asn1FieldDefinition : Asn1Node, INamedNode
	{
		private readonly string fieldName;
		private readonly Asn1Type type;
		private int? tag;

		/// <summary>
		/// Represents an ASN.1 field definition.
		/// </summary>
		/// <param name="FieldName">Field name.</param>
		/// <param name="Tag">Tag</param>
		/// <param name="Type">Type.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1FieldDefinition(string FieldName, int? Tag, Asn1Type Type,
			Asn1Document Document)
			: base(Document)
		{
			this.fieldName = FieldName;
			this.type = Type;
			this.tag = Tag;
	
			this.SetAsChild(this.type);
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
		/// Type
		/// </summary>
		public Asn1Type Type => this.type;

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
		/// Tag
		/// </summary>
		public int? Tag
		{
			get => this.tag;
			internal set => this.tag = value;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.fieldName);
			sb.Append(" : ");
			sb.Append(this.type.ToString());

			if (this.tag.HasValue)
			{
				sb.Append(" [");
				sb.Append(this.tag.Value.ToString());
				sb.Append(']');
			}

			return sb.ToString();
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
			if (Pass == CSharpExportPass.Explicit)
			{
				JSON.Indent(Output, Indent);
				Output.Append("public ");
				await this.type.ExportCSharp(Output, State, Indent, Pass);
				Output.Append(' ');
				Output.Append(ToCSharp(this.fieldName));

				if (!(this.type.Default is null))
				{
					Output.Append(" = ");

					if (this.type.Default is Asn1ValueReference ValueReference &&
						this.type is Asn1TypeReference TypeReference)
					{
						await TypeReference.ExportCSharp(Output, State, Indent, Pass);
						Output.Append('.');
						await ValueReference.ExportCSharp(Output, State, Indent, Pass);
					}
					else
						await this.type.Default.ExportCSharp(Output, State, Indent, Pass);
				}

				Output.AppendLine(";");
			}
			else
				await this.type.ExportCSharp(Output, State, Indent, Pass);
		}
	}
}
