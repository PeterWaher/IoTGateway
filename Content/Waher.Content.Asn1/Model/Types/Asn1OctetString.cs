using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// OCTET STRING
	/// </summary>
	public class Asn1OctetString : Asn1Type
	{
		/// <summary>
		/// OCTET STRING
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1OctetString(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "byte[]";

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.OctetString)
		};

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
				Output.Append("byte[]");
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="Index">Item index.</param>
		public override Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf, int Index)
		{
			Output.Append("Asn1.Common.ReadOctetString");

			if (this.Tags.HasValue)
				Output.Append(this.Tags.Value.ToString());

			Output.Append("(Reader");

			if (this.Tags.HasValue)
			{
				Output.Append(", ");
				Output.Append(Index.ToString());
			}

			Output.Append(')');

			return Task.CompletedTask;
		}

		/// <summary>
		/// Exports optional read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="ItemName">Name of item variable.</param>
		/// <param name="Index">Item index.</param>
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string ItemName,
			int Index)
		{
			Output.Append("Asn1.Common.TryReadOctetString");

			if (this.Tags.HasValue)
				Output.Append(this.Tags.Value.ToString());

			Output.Append("(Reader");

			if (this.Tags.HasValue)
			{
				Output.Append(", ");
				Output.Append(Index.ToString());
			}

			Output.Append(", out ");

			if (string.IsNullOrEmpty(ItemName))
				Output.Append("var Item) ? Item : null");
			else
			{
				Output.Append(ItemName);
				Output.Append(')');
			}

			return Task.CompletedTask;
		}
	}
}
