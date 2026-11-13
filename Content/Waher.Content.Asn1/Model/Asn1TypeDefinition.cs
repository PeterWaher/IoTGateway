using System.Formats.Asn1;
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
		private readonly Asn1Tag? tag;

		/// <summary>
		/// Represents an ASN.1 Type definition.
		/// </summary>
		/// <param name="TypeName">Type name.</param>
		/// <param name="Tag">Tag</param>
		/// <param name="Definition">Definition</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1TypeDefinition(string TypeName, Asn1Tag? Tag, Asn1Type Definition,
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
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags
		{
			get
			{
				if (this.tag.HasValue)
					return new Asn1Tag[] { this.tag.Value };
				else
					return this.definition.AcceptedTags;
			}
		}

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
				int TagNr = this.tag.Value.TagValue;

				switch (this.tag.Value.TagClass)
				{
					case TagClass.Universal:
						sb.Append(' ');
						sb.Append((UniversalTagNumber)TagNr);
						break;

					case TagClass.Application:
						sb.Append(" [APPLICATION ");
						sb.Append(TagNr.ToString());
						sb.Append(']');
						break;

					case TagClass.Private:
						sb.Append(" [PRIVATE ");
						sb.Append(TagNr.ToString());
						sb.Append(']');
						break;

					case TagClass.ContextSpecific:
						sb.Append(" [CONTEXT SPECIFIC ");
						sb.Append(TagNr.ToString());
						sb.Append(']');
						break;

				}
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

			if (Pass == CSharpExportPass.Preprocess || Pass == CSharpExportPass.Variables)
				await this.definition.ExportCSharp(Output, State, Indent, Pass);
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
		/// <param name="Index">Item index.</param>
		public override async Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf, int Index)
		{
			await this.definition.ExportReadCSharp(Output, State, Indent, MemberOf, Index);
		}

		/// <summary>
		/// Exports optional read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="DefaultName">Name of default value.</param>
		/// <param name="ItemName">Name of item variable.</param>
		/// <param name="Index">Item index.</param>
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string DefaultName, 
			string ItemName, int Index)
		{
			return this.definition.ExportTryReadCSharp(Output, State, Indent, MemberOf, 
				DefaultName, ItemName, Index);
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="VariableName">Name of variable to check.</param>
		public override Task ExportRestrictionCheck(StringBuilder Output,
			CSharpExportState State, int Indent, string VariableName)
		{
			return this.definition.ExportRestrictionCheck(Output, State, Indent, 
				VariableName);
		}

	}
}
