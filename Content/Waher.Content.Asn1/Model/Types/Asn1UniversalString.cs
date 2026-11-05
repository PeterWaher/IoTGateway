namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// UniversalString
	/// ISO10646 character set
	/// </summary>
	public class Asn1UniversalString : Asn1StringType
	{
		/// <summary>
		/// UniversalString
		/// ISO10646 character set
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1UniversalString(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
