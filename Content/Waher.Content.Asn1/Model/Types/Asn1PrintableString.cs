using System.Formats.Asn1;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// PrintableString
	/// a-z, A-Z, ' () +,-.?:/= and SPACE
	/// </summary>
	public class Asn1PrintableString : Asn1StringType
	{
		/// <summary>
		/// PrintableString
		/// a-z, A-Z, ' () +,-.?:/= and SPACE
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1PrintableString(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Type of string
		/// </summary>
		public override UniversalTagNumber StringTag => UniversalTagNumber.PrintableString;
	}
}
