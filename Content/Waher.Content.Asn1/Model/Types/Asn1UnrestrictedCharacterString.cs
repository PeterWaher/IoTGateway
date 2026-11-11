using System.Formats.Asn1;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Unrestricted CHARACTER STRING
	/// </summary>
	public class Asn1UnrestrictedCharacterString : Asn1StringType
	{
		/// <summary>
		/// Unrestricted CHARACTER STRING
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1UnrestrictedCharacterString(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Type of string
		/// </summary>
		public override UniversalTagNumber StringTag => UniversalTagNumber.UnrestrictedCharacterString;
	}
}
