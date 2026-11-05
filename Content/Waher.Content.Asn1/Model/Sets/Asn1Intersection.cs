namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Intersection of sets.
	/// </summary>
	public class Asn1Intersection : Asn1BinarySetOperator
	{
		/// <summary>
		/// Intersection of sets.
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Intersection(Asn1Values Left, Asn1Values Right, Asn1Document Document)
			: base(Left, Right, Document)
		{
		}
	}
}
