using System.Formats.Asn1;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents an ASN.1 Object ID
	/// </summary>
	public class Asn1Oid : Asn1Value
	{
		private readonly Asn1Node[] values;

		/// <summary>
		/// Represents an ASN.1 Object ID
		/// </summary>
		/// <param name="Values">OID values.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Oid(Asn1Node[] Values, Asn1Document Document) 
			: base(Document)
		{
			this.values = Values;
			this.SetAsChild(this.values);
		}

		/// <summary>
		/// OID values.
		/// </summary>
		public Asn1Node[] Values => this.values;

		/// <summary>
		/// Accepted ASN.1 tags
		/// </summary>
		public override Asn1Tag[] AcceptedTags => acceptedTags;

		private static readonly Asn1Tag[] acceptedTags = new Asn1Tag[]
		{
			new Asn1Tag(UniversalTagNumber.ObjectIdentifier)
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
			if (Pass == CSharpExportPass.Explicit)
			{
				bool First = true;

				Output.Append("new Asn1.ObjectId(");

				foreach (Asn1Node Node in this.values)
				{
					if (First)
						First = false;
					else
						Output.Append(", ");

					await Node.ExportCSharp(Output, State, Indent, Pass);
				}

				Output.Append(')');
			}
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "Asn1.ObjectId";

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (Asn1Node Value in this.values)
			{
				if (First)
					First = false;
				else
					sb.Append('.');

				sb.Append(Value.ToString());
			}

			return sb.ToString();
		}
	}
}
