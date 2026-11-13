using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Restricted to elements in set.
	/// </summary>
	public class Asn1InSet : Asn1SetRestriction
	{
		/// <summary>
		/// Restricted to elements in set.
		/// </summary>
		/// <param name="Set">Set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1InSet(Asn1SetOfValues Set, Asn1Document Document)
			: base(Set, Document)
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
			Output.Append("Asn1.Construct.CheckInSet(");
			Output.Append(VariableName);
			Output.Append(", ");
			Output.Append(this.Set.GetSetName(VariableName));
			Output.Append(')');

			return Task.CompletedTask;
		}
	}
}
