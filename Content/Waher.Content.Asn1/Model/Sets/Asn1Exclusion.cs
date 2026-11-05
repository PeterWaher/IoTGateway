namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Exclusion of sets.
	/// </summary>
	public class Asn1Exclusion : Asn1BinarySetOperator
	{
		/// <summary>
		/// Exclusion of sets.
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Exclusion(Asn1Values Left, Asn1Values Right, Asn1Document Document)
			: base(Left, Right, Document)
		{
		}
	}
}
