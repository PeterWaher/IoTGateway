using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// DATE-TIME
	/// </summary>
	public class Asn1DateTime : Asn1Type
	{
		/// <summary>
		/// DATE-TIME
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1DateTime(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				if (this.Optional.HasValue && this.Optional.Value)
					return "System.DateTime?";
				else
					return "System.DateTime";
			}
		}

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
			{
				Output.Append("System.DateTime");
				if (this.Optional.HasValue && this.Optional.Value)
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
			Output.Append("Asn1.Common.ReadDateTime(Reader)");
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
			Output.Append("Asn1.Common.TryReadDateTime(Reader, out ");

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
