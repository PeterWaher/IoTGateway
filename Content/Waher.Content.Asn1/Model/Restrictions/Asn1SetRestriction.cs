using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Abstract base class for set-based restrictions
	/// </summary>
	public abstract class Asn1SetRestriction : Asn1Restriction
	{
		private readonly Asn1SetOfValues set;

		/// <summary>
		/// Abstract base class for set-based restrictions
		/// </summary>
		/// <param name="Set">Set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1SetRestriction(Asn1SetOfValues Set, Asn1Document Document)
			: base(Document)
		{
			this.set = Set;
			this.SetAsChild(this.set);
		}

		/// <summary>
		/// Set
		/// </summary>
		public Asn1SetOfValues Set => this.set;


		/// <summary>
		/// Exports restriction variable definitions to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="VariableName">Name of variable to check.</param>
		public override async Task ExportRestrictionVariables(StringBuilder Output,
			CSharpExportState State, int Indent, string VariableName)
		{
			if (!State.ExportingValues)
			{
				State.ExportingValues = true;

				JSON.Indent(Output, Indent);
				Output.Append("public static partial class ");
				Output.AppendLine(Asn1FieldValueDefinition.DefinedValues);

				JSON.Indent(Output, Indent);
				Output.AppendLine("{");

				State.ExportingValuesIndent = Indent + 1;
			}

			JSON.Indent(Output, State.ExportingValuesIndent);
			Output.Append("public static readonly Asn1.Construct.ISet ");
			Output.Append(this.set.GetSetName(VariableName));
			Output.Append(" = ");

			await this.set.ExportSetDefinitionCSharp(Output, State, Indent);

			Output.AppendLine(";");
		}
	}
}
