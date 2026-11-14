using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// ENCODED BY
	/// </summary>
	public class Asn1EncodedBy : Asn1ValueRestriction
	{
		/// <summary>
		/// ENCODED BY
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1EncodedBy(Asn1Value Value, Asn1Document Document) 
			: base(Value, Document)
		{
		}

		/// <summary>
		/// Gets checks to perform.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="VariableName">Variable being checked.</param>
		/// <param name="Type">Data type.</param>
		public override Task RenderChecks(StringBuilder Output,
			CSharpExportState State, string VariableName, Asn1Type Type)
		{
			Output.Append("Asn1.Construct.CheckEncodedBy(");
			Output.Append(VariableName);
			Output.Append(", ");
			this.Value.ExportCSharp(Output, State, 0, CSharpExportPass.Explicit);
			Output.Append(')');

			return Task.CompletedTask;
		}
	}
}
