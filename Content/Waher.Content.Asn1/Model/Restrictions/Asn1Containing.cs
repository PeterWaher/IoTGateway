namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// CONTAINING
	/// </summary>
	public class Asn1Containing : Asn1ValueRestriction
	{
		/// <summary>
		/// CONTAINING
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Containing(Asn1Value Value, Asn1Document Document)
			: base(Value, Document)
		{
		}
	}
}
