using System;
using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Octet String value
	/// </summary>
	public class Asn1OctetStringValue : Asn1Value
	{
		private readonly byte[] value;

		/// <summary>
		/// Octet String value
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1OctetStringValue(byte[] Value, Asn1Document Document) 
			: base(Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public byte[] Value => this.value;

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.OctetString),
			new Asn1Tag(UniversalTagNumber.BitString)
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
				bool First = true;

				Output.Append("new byte[] { ");

				foreach (byte b in this.value)
				{
					if (First)
						First = false;
					else
						Output.Append(", ");

					Output.Append(b.ToString());
				}

				Output.Append(" }");
			}
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "byte[]";

		/// <inheritdoc/>
		public override string ToString() => Convert.ToBase64String(this.value);
	}
}
