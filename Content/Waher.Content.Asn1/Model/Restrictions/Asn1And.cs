using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Both restrictions apply
	/// </summary>
	public class Asn1And : Asn1BinaryRestrictionOperator
	{
		/// <summary>
		/// Both restrictions apply
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1And(Asn1Restriction Left, Asn1Restriction Right, Asn1Document Document)
			: base(Left, Right, Document)
		{
		}

		/// <summary>
		/// Gets checks to perform.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="VariableName">Variable being checked.</param>
		/// <param name="Type">Data type.</param>
		public override async Task RenderChecks(StringBuilder Output,
			CSharpExportState State, string VariableName, Asn1Type Type)
		{
			Output.Append('(');
			await this.Left.RenderChecks(Output, State, VariableName, Type);
			Output.Append(") && (");
			await this.Right.RenderChecks(Output, State, VariableName, Type);
			Output.Append(')');
		}
	}
}
