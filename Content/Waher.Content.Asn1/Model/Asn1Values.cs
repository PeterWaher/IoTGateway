namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for sets of values
	/// </summary>
	public abstract class Asn1Values : Asn1Node
	{
		/// <summary>
		/// Abstract base class for sets of values
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Values(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
