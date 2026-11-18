using System.Formats.Asn1;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// BmpString (utf-16-be encoded string)
	/// Basic Multilingual Plane of ISO/IEC/ITU 10646-1
	/// </summary>
	public class Asn1BmpString : Asn1StringType
	{
		/// <summary>
		/// BmpString (utf-16-be encoded string)
		/// Basic Multilingual Plane of ISO/IEC/ITU 10646-1
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1BmpString(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Type of string
		/// </summary>
		public override UniversalTagNumber StringTag => UniversalTagNumber.BMPString;
	}
}
