using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;
using Waher.Content.Asn1.Model.Types;
using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 Type definition.
	/// </summary>
	public class Asn1TypeDefinition : Asn1Type, INamedNode
	{
		private readonly string typeName;
		private readonly Asn1Type definition;
		private readonly int? tag;

		/// <summary>
		/// Represents an ASN.1 Type definition.
		/// </summary>
		/// <param name="TypeName">Type name.</param>
		/// <param name="Tag">Tag</param>
		/// <param name="Definition">Definition</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1TypeDefinition(string TypeName, int? Tag, Asn1Type Definition,
			Asn1Document Document)
			: base(Document)
		{
			this.typeName = TypeName;
			this.definition = Definition;
			this.tag = Tag;

			this.SetAsChild(this.definition);
		}

		/// <summary>
		/// Type Name
		/// </summary>
		public string Name => this.typeName;

		/// <summary>
		/// If the named node represents a type.
		/// </summary>
		public bool IsType => true;

		/// <summary>
		/// Gets the fully qualified name, in reverse order.
		/// </summary>
		public void GetFullyQualifiedName(ChunkedList<string> Fqn)
		{
			if (Fqn.Count == 0 || Fqn[Fqn.Count - 1] != this.Name)
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
		public int? Tag => this.tag;

		/// <summary>
		/// Type definition
		/// </summary>
		public Asn1Type Definition => this.definition;

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.typeName);

			if (this.tag.HasValue)
			{
				sb.Append(" [");
				sb.Append(this.tag.Value.ToString());
				sb.Append(']');
			}

			return sb.ToString();
		}

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			return this.definition.Parse(Document, Macro);
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				if (this.definition is Asn1TypeReference TypeRef)
					return TypeRef.CSharpType;
				else if (this.definition is Asn1Type Type)
					return Type.CSharpType;
				else
					return ToCSharp(this.Name);
			}
		}

		/// <summary>
		/// If the node is optional
		/// </summary>
		public override bool? Optional => this.definition.Optional;

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
			if (!this.definition.ConstructedType)
				return;

			if (Pass == CSharpExportPass.Preprocess)
				await this.definition.ExportCSharp(Output, State, Indent, CSharpExportPass.Preprocess);
			else if (Pass == CSharpExportPass.Explicit)
				await this.definition.ExportCSharp(Output, State, Indent, CSharpExportPass.Implicit);
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
			return this.definition.ExportReadCSharp(Output, State, Indent, MemberOf);
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
			return this.definition.ExportTryReadCSharp(Output, State, Indent, MemberOf, ItemName);
		}
	}
}
