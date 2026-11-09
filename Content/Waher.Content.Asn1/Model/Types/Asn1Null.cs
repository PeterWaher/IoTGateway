using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// NULL
	/// </summary>
	public class Asn1Null : Asn1Type
	{
		/// <summary>
		/// NULL
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Null(Asn1Document Document)
			: base(Document)
		{
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
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			return null;
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
			Output.Append("Asn1.Common.ReadNull(Reader)");
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
			if (!string.IsNullOrEmpty(ItemName))
			{
				Output.Append(ItemName);
				Output.Append(" = ");
			}

			Output.Append("Asn1.Common.TryReadNull(Reader)");

			return Task.CompletedTask;
		}

	}
}
