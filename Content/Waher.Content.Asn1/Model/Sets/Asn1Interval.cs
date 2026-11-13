using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Interval of elements.
	/// </summary>
	public class Asn1Interval : Asn1SetOfValues
	{
		private readonly Asn1Value from;
		private readonly Asn1Value to;

		/// <summary>
		/// Interval of elements.
		/// </summary>
		/// <param name="From">From</param>
		/// <param name="To">To</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Interval(Asn1Value From, Asn1Value To, Asn1Document Document)
			: base(Document)
		{
			this.from = From;
			this.to = To;
	
			this.SetAsChild(this.from);
			this.SetAsChild(this.to);
		}

		/// <summary>
		/// From
		/// </summary>
		public Asn1Value From => this.from;

		/// <summary>
		/// To
		/// </summary>
		public Asn1Value To => this.to;

		/// <summary>
		/// Exports the set definition to C#.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		public override Task ExportSetDefinitionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent)
		{
			string Type = this.from.CSharpType;
			if (Type != this.to.CSharpType)
				throw new AsnContentException("Incompatible types in interval.");

			Output.Append("new Asn1.Construct.Interval<");
			Output.Append(Type);
			Output.Append(">(");
			this.from.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
			Output.Append(", ");
			this.to.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
			Output.Append(')');

			return Task.CompletedTask;
		}
	}
}
