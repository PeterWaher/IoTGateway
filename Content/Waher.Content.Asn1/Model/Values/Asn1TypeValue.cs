using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents an ASN.1 type value.
	/// </summary>
	public class Asn1TypeValue : Asn1Value
	{
		private readonly Asn1Type type;

		/// <summary>
		/// Represents an ASN.1 type value.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <param name="Document">ASN.1 Document containing the reference</param>
		public Asn1TypeValue(Asn1Type Type, Asn1Document Document)
			: base(Document)
		{
			this.type = Type;
		}

		/// <summary>
		/// Type
		/// </summary>
		public Asn1Type Type => this.type;

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => this.type.AcceptedTags;

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => this.type.CSharpType;

		/// <inheritdoc/>
		public override string ToString() => this.type.ToString();

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
				Output.Append("typeof(");
				Output.Append(this.type.CSharpType);
				Output.Append(')');

				return Task.CompletedTask;
			}
			else
			{
				if (Pass == CSharpExportPass.Implicit &&
					string.IsNullOrEmpty(this.type.CSharpType) &&
					this.type is Asn1ComplexType ComplexType &&
					string.IsNullOrEmpty(ComplexType.Name))      // Implicit definition
				{
					int i = 1;
					string s = "N1";

					while (this.Document.namedNodes.ContainsKey(s))
					{
						i++;
						s = "N" + i.ToString();
					}

					ComplexType.Name = s;
					this.Document.namedNodes[s] = ComplexType;
				}

				return this.type.ExportCSharp(Output, State, Indent, Pass);
			}
		}
	}
}
