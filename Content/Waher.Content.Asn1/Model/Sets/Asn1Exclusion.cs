using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Exclusion of sets.
	/// </summary>
	public class Asn1Exclusion : Asn1BinarySetOperator
	{
		/// <summary>
		/// Exclusion of sets.
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Exclusion(Asn1SetOfValues Left, Asn1SetOfValues Right, Asn1Document Document)
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
			Output.Append("new Asn1.Construct.Complement(");
			await this.Left.ExportSetDefinitionCSharp(Output, State, Indent);
			Output.Append(", ");
			await this.Right.ExportSetDefinitionCSharp(Output, State, Indent);
			Output.Append(')');
		}
	}
}
