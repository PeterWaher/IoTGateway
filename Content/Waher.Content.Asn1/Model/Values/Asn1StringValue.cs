using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// String value
	/// </summary>
	public class Asn1StringValue : Asn1Value
	{
		private readonly string value;

		/// <summary>
		/// String value
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1StringValue(string Value, Asn1Document Document) 
			: base(Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public string Value => this.value;

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.ObjectDescriptor),
			new Asn1Tag(UniversalTagNumber.UTF8String),
			new Asn1Tag(UniversalTagNumber.NumericString),
			new Asn1Tag(UniversalTagNumber.PrintableString),
			new Asn1Tag(UniversalTagNumber.TeletexString),
			new Asn1Tag(UniversalTagNumber.VideotexString),
			new Asn1Tag(UniversalTagNumber.IA5String),
			new Asn1Tag(UniversalTagNumber.GraphicString),
			new Asn1Tag(UniversalTagNumber.VisibleString),
			new Asn1Tag(UniversalTagNumber.GeneralString),
			new Asn1Tag(UniversalTagNumber.UniversalString),
			new Asn1Tag(UniversalTagNumber.UnrestrictedCharacterString),
			new Asn1Tag(UniversalTagNumber.BMPString)
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
			{
				Output.Append('"');
				Output.Append(JSON.Encode(this.value));
				Output.Append('"');
			}
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "string";

		/// <inheritdoc/>
		public override string ToString() => "\"" + JSON.Encode(this.value) + "\"";
	}
}
