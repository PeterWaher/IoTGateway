using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Boolean value
	/// </summary>
	public class Asn1BooleanValue : Asn1Value
	{
		private readonly bool value;

		/// <summary>
		/// Boolean value
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1BooleanValue(bool Value, Asn1Document Document) 
			: base(Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public bool Value => this.value;

		/// <inheritdoc/>
		public override string ToString() => this.value.ToString();

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.Boolean)
		};

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
				Output.Append(this.value ? "true" : "false");
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "bool";
	}
}
