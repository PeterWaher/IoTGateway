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
		public Asn1Pattern(Asn1Value RegularExpression)
			: base(RegularExpression)
		{
		}
	}
}
