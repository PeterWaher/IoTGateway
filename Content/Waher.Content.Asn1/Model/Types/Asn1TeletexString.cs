using System.Formats.Asn1;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// TeletexString
	/// CCITT and T.101 character sets
	/// </summary>
	public class Asn1TeletexString : Asn1StringType
	{
		/// <summary>
		/// TeletexString
		/// CCITT and T.101 character sets
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1TeletexString(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Type of string
		/// </summary>
		public override UniversalTagNumber StringTag => UniversalTagNumber.TeletexString;
	}
}
