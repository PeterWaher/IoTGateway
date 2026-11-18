namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents an ASN.1 named field.
	/// </summary>
	public class Asn1Field : Asn1Node
	{
		/// <summary>
		/// Represents an ASN.1 named field.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Field(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
