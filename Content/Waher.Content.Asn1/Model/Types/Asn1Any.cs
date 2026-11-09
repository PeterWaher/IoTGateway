using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// ANY
	/// </summary>
	public class Asn1Any : Asn1Type
	{
		private readonly string definedBy;

		/// <summary>
		/// ANY
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Any(Asn1Document Document)
			: this(null, Document)
		{
		}

		/// <summary>
		/// ANY
		/// </summary>
		/// <param name="DefinedBy">Defined by reference.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Any(string DefinedBy, Asn1Document Document)
			: base(Document)
		{
			this.definedBy = DefinedBy;
		}

		/// <summary>
		/// Defined-by reference
		/// </summary>
		public string DefinedBy => this.definedBy;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			return Document.ParseValue();
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "object";

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
				Output.Append("object");

			return Task.CompletedTask;
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
			Output.Append("Asn1.Common.ReadAny(Reader)");
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
			Output.Append("Asn1.Common.TryReadAny(Reader, out ");
			
			if (string.IsNullOrEmpty(ItemName))
				Output.Append("var Item) ? Item : null");
			else
			{
				Output.Append(ItemName);
				Output.Append(')');
			}

			return Task.CompletedTask;
		}
	}
}
