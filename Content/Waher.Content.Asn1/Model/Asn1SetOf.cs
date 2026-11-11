using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 SET OF construct.
	/// </summary>
	public class Asn1SetOf : Asn1Type
	{
		private readonly Asn1Values size;
		private readonly Asn1Type elementType;

		/// <summary>
		/// Represents a ASN.1 SET OF construct.
		/// </summary>
		/// <param name="Size">Optional SIZE</param>
		/// <param name="ElementType">Element type.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1SetOf(Asn1Values Size, Asn1Type ElementType, Asn1Document Document)
			: base(Document)
		{
			this.size = Size;
			this.elementType = ElementType;

			this.SetAsChild(this.size);
			this.SetAsChild(this.elementType);
		}

		/// <summary>
		/// Optional SIZE
		/// </summary>
		public Asn1Values Size => this.size;

		/// <summary>
		/// Type.
		/// </summary>
		public Asn1Type Type => this.elementType;

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType
		{
			get
			{
				return "Asn1.Array<" + this.elementType.CSharpType + ">";
			}
		}

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.SetOf)
		};

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override async Task ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Implicit)
				await this.elementType.ExportCSharp(Output, State, Indent, Pass);
			else if (Pass == CSharpExportPass.Explicit)
			{
				Output.Append("Asn1.Array<");
				await this.elementType.ExportCSharp(Output, State, Indent, Pass);
				Output.Append(">");
			}
		}

		/// <summary>
		/// Exports read logic to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="MemberOf">Member of statement.</param>
		/// <param name="Index">Item index.</param>
		public override async Task ExportReadCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, string MemberOf, int Index)
		{
			Output.Append("Asn1.Common.ReadArray<");
			Output.Append(this.elementType.CSharpType);
			Output.Append(">(Reader, (Reader, out _Item) => ");
			await this.elementType.ExportTryReadCSharp(Output, State, Indent + 1,
				string.Empty, "_Item", Index);
			Output.Append(")");
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
		public override async Task ExportTryReadCSharp(StringBuilder Output,
			CSharpExportState State, int Indent, string MemberOf, string ItemName, 
			int Index)
		{
			Output.Append("Asn1.Common.TryReadArray<");
			Output.Append(this.elementType.CSharpType);
			Output.Append(">(Reader, (Reader, out _Item) => ");

			await this.elementType.ExportTryReadCSharp(Output, State, Indent + 1,
				string.Empty, "_Item", Index);

			if (string.IsNullOrEmpty(ItemName))
				Output.Append(", out var _Item2) ? _Item2 : default");
			else
			{
				Output.Append(", out ");
				Output.Append(ItemName);
				Output.Append(')');
			}
		}
	}
}
