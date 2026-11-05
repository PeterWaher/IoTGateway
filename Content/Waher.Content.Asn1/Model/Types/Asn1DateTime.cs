using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// DATE-TIME
	/// </summary>
	public class Asn1DateTime : Asn1Type
	{
		/// <summary>
		/// DATE-TIME
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1DateTime(Asn1Document Document)
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
			{
				Output.Append("System.DateTime");
				if (this.Optional.HasValue && this.Optional.Value)
					Output.Append('?');
			}
		
			return Task.CompletedTask;
		}
	}
}
