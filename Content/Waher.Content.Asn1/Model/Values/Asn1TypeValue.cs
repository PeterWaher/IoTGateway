using System.Formats.Asn1;

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
	}
}
