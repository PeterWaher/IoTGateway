namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// GraphicString
	/// all registered G sets and SPACE
	/// </summary>
	public class Asn1GraphicString : Asn1StringType
	{
		/// <summary>
		/// GraphicString
		/// all registered G sets and SPACE
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1GraphicString(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
