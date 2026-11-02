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
		public Asn1Any()
			: this(null)
		{
		}

		/// <summary>
		/// ANY
		/// </summary>
		/// <param name="DefinedBy">Defined by reference.</param>
		public Asn1Any(string DefinedBy)
			: base()
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
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
				Output.Append("Object");

			return Task.CompletedTask;
		}
	}
}
