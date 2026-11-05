namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// WITH COMPONENTS
	/// </summary>
	public class Asn1WithComponents : Asn1ValueRestriction
	{
		/// <summary>
		/// WITH COMPONENTS
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1WithComponents(Asn1Value Value, Asn1Document Document) 
			: base(Value, Document)
		{
		}
	}
}
