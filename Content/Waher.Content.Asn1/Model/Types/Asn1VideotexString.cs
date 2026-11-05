namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// VideotexString
	/// CCITT's T.100 and T.101 character sets 
	/// </summary>
	public class Asn1VideotexString : Asn1StringType
	{
		/// <summary>
		/// VideotexString
		/// CCITT's T.100 and T.101 character sets 
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1VideotexString(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
