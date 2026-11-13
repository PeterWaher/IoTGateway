using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// All elements (in current context).
	/// </summary>
	public class Asn1All : Asn1SetOfValues
	{
		/// <summary>
		/// All elements (in current context).
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1All(Asn1Document Document) 
			: base(Document)
		{
		}

		/// <summary>
		/// Exports the set definition to C#.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		public override Task ExportSetDefinitionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent)
		{
			Output.Append("new Asn1.Construct.AllValues()");
			return Task.CompletedTask;
		}
	}
}
