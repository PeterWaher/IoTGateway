using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Intersection of sets.
	/// </summary>
	public class Asn1Intersection : Asn1BinarySetOperator
	{
		/// <summary>
		/// Intersection of sets.
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Intersection(Asn1SetOfValues Left, Asn1SetOfValues Right, Asn1Document Document)
			: base(Left, Right, Document)
		{
		}

		/// <summary>
		/// Exports the set definition to C#.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		public override async Task ExportSetDefinitionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent)
		{
			Output.Append("new Asn1.Construct.Intersection(");
			await this.ExportSetDefinitionCSharp(Output, State, Indent);
			Output.Append(", ");
			await this.ExportSetDefinitionCSharp(Output, State, Indent);
			Output.Append(')');
		}
	}
}
