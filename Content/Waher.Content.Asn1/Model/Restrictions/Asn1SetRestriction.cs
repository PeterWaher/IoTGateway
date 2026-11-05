namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Abstract base class for set-based restrictions
	/// </summary>
	public abstract class Asn1SetRestriction : Asn1Restriction
	{
		private readonly Asn1Values set;

		/// <summary>
		/// Abstract base class for set-based restrictions
		/// </summary>
		/// <param name="Set">Set</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1SetRestriction(Asn1Values Set, Asn1Document Document)
			: base(Document)
		{
			this.set = Set;
			this.SetAsChild(this.set);
		}

		/// <summary>
		/// Set
		/// </summary>
		public Asn1Values Set => this.set;
	}
}
