using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// BIT STRING
	/// </summary>
	public class Asn1BitString : Asn1Type
	{
		/// <summary>
		/// BIT STRING
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1BitString(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "Asn1.Array<byte>";

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
				Output.Append("Asn1.Array<byte>");
		
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
			Output.Append("Asn1.Common.ReadBitString(Reader)");
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
			Output.Append("Asn1.Common.TryReadBitString(Reader, out ");

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
