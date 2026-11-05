namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// GeneralString
	/// all registered graphic and character sets plus SPACE and DELETE
	/// </summary>
	public class Asn1GeneralString : Asn1StringType
	{
		/// <summary>
		/// GeneralString
		/// all registered graphic and character sets plus SPACE and DELETE
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1GeneralString(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
