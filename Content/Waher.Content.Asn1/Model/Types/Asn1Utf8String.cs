namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// UTF8String
	/// any character from a recognized alphabet (including ASCII control characters)
	/// </summary>
	public class Asn1Utf8String : Asn1StringType
	{
		/// <summary>
		/// UTF8String
		/// any character from a recognized alphabet (including ASCII control characters)
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Utf8String(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
