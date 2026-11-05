using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// OCTET STRING
	/// </summary>
	public class Asn1OctetString : Asn1Type
	{
		/// <summary>
		/// OCTET STRING
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1OctetString(Asn1Document Document)
			: base(Document)
		{
		}

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
	}
}
