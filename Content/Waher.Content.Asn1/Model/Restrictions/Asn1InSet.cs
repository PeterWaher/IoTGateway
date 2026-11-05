namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Restricted to elements in set.
	/// </summary>
	public class Asn1InSet : Asn1SetRestriction
	{
		/// <summary>
		/// Restricted to elements in set.
		/// </summary>
		/// <param name="Set">Set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1InSet(Asn1Values Set, Asn1Document Document)
			: base(Set, Document)
		{
		}
	}
}
