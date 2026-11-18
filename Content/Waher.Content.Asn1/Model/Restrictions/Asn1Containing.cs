using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// CONTAINING
	/// </summary>
	public class Asn1Containing : Asn1ValueRestriction
	{
		/// <summary>
		/// CONTAINING
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Containing(Asn1Value Value, Asn1Document Document)
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
			Output.Append("Asn1.Construct.CheckContaining(");
			Output.Append(VariableName);
			Output.Append(", ");
			this.Value.ExportCSharp(Output, State, 0, CSharpExportPass.Explicit);
			Output.Append(')');

			return Task.CompletedTask;
		}
	}
}
