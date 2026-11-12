using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// INTEGER
	/// </summary>
	public class Asn1Integer : Asn1Type
	{
		/// <summary>
		/// INTEGER
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Integer(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			if (Document.ParseValue() is Values.Asn1IntegerValue Value)
				return Value;
			else
				throw Document.SyntaxError("String value expected.");
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				if (this.Optional ?? false)
					return "System.Numerics.BigInteger?";
				else
					return "System.Numerics.BigInteger";
			}
		}

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.Integer)
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
				Output.Append("System.Numerics.BigInteger");
				if (this.Optional ?? false)
					Output.Append('?');
			}
	
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
			Output.Append("ReadInteger");

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
		/// <param name="DefaultName">Name of default value.</param>
		/// <param name="ItemName">Name of item variable.</param>
		/// <param name="Index">Item index.</param>
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string DefaultName, 
			string ItemName, int Index)
		{
			Output.Append("TryReadInteger");

			if (this.Tags.HasValue)
				Output.Append(this.Tags.Value.ToString());

			Output.Append("(Reader");

			if (this.Tags.HasValue)
			{
				Output.Append(", ");
				Output.Append(Index.ToString());
			}

			if (string.IsNullOrEmpty(ItemName))
				Output.Append(", default, out var _Item) ? _Item : null");
			else
			{
				Output.Append(", ");
				Output.Append(DefaultName ?? "default");
				Output.Append(", out ");
				Output.Append(ItemName);
				Output.Append(')');
			}

			return Task.CompletedTask;
		}

	}
}
