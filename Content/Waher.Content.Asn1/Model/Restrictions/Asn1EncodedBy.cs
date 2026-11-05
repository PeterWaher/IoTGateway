namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// ENCODED BY
	/// </summary>
	public class Asn1EncodedBy : Asn1ValueRestriction
	{
		/// <summary>
		/// ENCODED BY
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1EncodedBy(Asn1Value Value, Asn1Document Document) 
			: base(Value, Document)
		{
		}
	}
}
