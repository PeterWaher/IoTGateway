namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// IA5String
	/// International ASCII characters (International Alphabet 5)
	/// </summary>
	public class Asn1Ia5String : Asn1StringType
	{
		/// <summary>
		/// IA5String
		/// International ASCII characters (International Alphabet 5)
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Ia5String(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
