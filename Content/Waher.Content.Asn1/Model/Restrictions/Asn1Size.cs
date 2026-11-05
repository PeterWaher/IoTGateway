namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// SIZE()
	/// </summary>
	public class Asn1Size : Asn1SetRestriction
	{
		/// <summary>
		/// SIZE()
		/// </summary>
		/// <param name="Set">Set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Size(Asn1Values Set, Asn1Document Document)
			: base(Set, Document)
		{
		}
	}
}
