using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;
using Waher.Content.Asn1.Model.Types;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Declares a type in a class definition.
	/// </summary>
	public class Asn1TypeDeclaration : Asn1Type
	{
		private readonly string name;
		private Asn1Type declaration;

		/// <summary>
		/// Declares a type in a class definition.
		/// </summary>
		/// <param name="Name">Property name.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1TypeDeclaration(string Name, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.declaration = new Asn1Any(Document);
		}

		/// <summary>
		/// Property name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Type Declaration
		/// </summary>
		public Asn1Type Declaration
		{
			get => this.declaration;
			internal set => this.declaration = value;
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => this.declaration.CSharpType;

		/// <summary>
		/// If the node is optional
		/// </summary>
		public override bool? Optional => base.Optional ?? this.declaration.Optional;

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => this.declaration.AcceptedTags;

		/// <inheritdoc/>
		public override string ToString() => this.name;


		/// <summary>
		/// How tags are encoded
		/// </summary>
		public override Asn1Tags? Tags => this.declaration.Tags;

		/// <summary>
		/// Any restrictions placed on the type.
		/// </summary>
		public override Asn1Restriction Restriction => this.declaration.Restriction;

		/// <summary>
		/// If values must be unique.
		/// </summary>
		public override bool? Unique => this.declaration.Unique;

		/// <summary>
		/// If the type must be present
		/// </summary>
		public override bool? Present => this.declaration.Present;

		/// <summary>
		/// If the type must be absent
		/// </summary>
		public override bool? Absent => this.declaration.Absent;

		/// <summary>
		/// Default value for type
		/// </summary>
		public override Asn1Node Default => this.declaration.Default;

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public override bool ConstructedType => this.declaration.ConstructedType;

		/// <summary>
		/// If the type is passed by reference.
		/// </summary>
		public override bool ByReference => this.declaration.ByReference;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			return this.declaration.Parse(Document, Macro);
		}

		/// <summary>
		/// Exports any restriction to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="VariableName">Name of variable to check.</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportRestrictionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string VariableName, CSharpExportPass Pass)
		{
			return this.declaration.ExportRestrictionCSharp(Output, State, Indent,
				VariableName, Pass);
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				Output.Append("public ");
				this.declaration.ExportCSharp(Output, State, Indent, Pass);
				Output.Append(';');

				return Task.CompletedTask;
			}
			else
				return this.declaration.ExportCSharp(Output, State, Indent, Pass);
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
			return this.declaration.ExportReadCSharp(Output, State, Indent, MemberOf, Index);
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
			return this.declaration.ExportRestrictionCheck(Output, State, Indent, VariableName);
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
			return this.declaration.ExportTryReadCSharp(Output, State, Indent, MemberOf,
				DefaultName, ItemName, Index);
		}

		/// <summary>
		/// Exports write logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="Index">Item index.</param>
		/// <param name="Tags">How to encode tags.</param>
		public override Task ExportWriteCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf, int Index, Asn1Tags? Tags)
		{
			return this.declaration.ExportWriteCSharp(Output, State, Indent, MemberOf, Index, Tags);
		}

	}
}
