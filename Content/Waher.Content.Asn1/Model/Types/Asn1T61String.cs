using System.Formats.Asn1;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// T61String
	/// </summary>
	public class Asn1T61String : Asn1StringType
	{
		/// <summary>
		/// T61String
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1T61String(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Type of string
		/// </summary>
		public override UniversalTagNumber StringTag => UniversalTagNumber.T61String;
	}
}
