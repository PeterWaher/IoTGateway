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
		public override Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf)
		{
			Output.Append("Asn1.Common.ReadInteger(Reader)");
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
		public override Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string ItemName)
		{
			Output.Append("Asn1.Common.TryReadInteger(Reader, out ");

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
