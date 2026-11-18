namespace Waher.Content.Asn1.Model.Restrictions
{
	/// <summary>
	/// Abstract base class for value-based restrictions
	/// </summary>
	public abstract class Asn1ValueRestriction : Asn1Restriction
	{
		private readonly Asn1Value value;

		/// <summary>
		/// Abstract base class for set-based restrictions
		/// </summary>
		/// <param name="Value">Item</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ValueRestriction(Asn1Value Value, Asn1Document Document) 
			: base(Document)
		{
			this.value = Value;
			this.SetAsChild(this.value);
		}

		/// <summary>
		/// Item
		/// </summary>
		public Asn1Node Value => this.value;
	}
}
