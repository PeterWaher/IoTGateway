using System.Formats.Asn1;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// VisibleString
	/// International ASCII printing character sets 
	/// </summary>
	public class Asn1VisibleString : Asn1StringType
	{
		/// <summary>
		/// VisibleString
		/// International ASCII printing character sets 
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1VisibleString(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Type of string
		/// </summary>
		public override UniversalTagNumber StringTag => UniversalTagNumber.VisibleString;
	}
}
