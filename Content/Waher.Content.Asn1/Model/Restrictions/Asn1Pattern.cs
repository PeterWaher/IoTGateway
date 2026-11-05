namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// PATTERN
	/// </summary>
	public class Asn1Pattern : Asn1ValueRestriction
	{
		/// <summary>
		/// PATTERN
		/// </summary>
		/// <param name="RegularExpression">Regular Expression</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Pattern(Asn1Value RegularExpression, Asn1Document Document)
			: base(RegularExpression, Document)
		{
		}
	}
}
