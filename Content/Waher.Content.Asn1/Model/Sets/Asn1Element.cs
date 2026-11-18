using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Set of one element.
	/// </summary>
	public class Asn1Element : Asn1SetOfValues
	{
		private readonly Asn1Value element;

		/// <summary>
		/// Set of one element.
		/// </summary>
		/// <param name="Element">Element</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Element(Asn1Value Element, Asn1Document Document)
			: base(Document)
		{
			this.element = Element;
			this.SetAsChild(this.element);
		}

		/// <summary>
		/// Element
		/// </summary>
		public Asn1Value Element => this.element;

		/// <summary>
		/// Exports the set definition to C#.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		public override Task ExportSetDefinitionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent)
		{
			Output.Append("new Asn1.Construct.SingleValue<");
			Output.Append(this.element.CSharpType);
			Output.Append(">(");
			this.element.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
			Output.Append(')');

			return Task.CompletedTask;
		}
	}
}
