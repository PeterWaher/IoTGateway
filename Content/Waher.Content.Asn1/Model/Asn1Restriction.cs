namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for ASN.1 restrictions.
	/// </summary>
	public abstract class Asn1Restriction : Asn1Node
	{
		/// <summary>
		/// Abstract base class for ASN.1 restrictions.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Restriction(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
