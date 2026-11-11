using System.Formats.Asn1;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Iso646String
	/// </summary>
	public class Asn1Iso646String : Asn1StringType
	{
		/// <summary>
		/// Iso646String
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Iso646String(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Type of string
		/// </summary>
		public override UniversalTagNumber StringTag => UniversalTagNumber.ISO646String;
	}
}
