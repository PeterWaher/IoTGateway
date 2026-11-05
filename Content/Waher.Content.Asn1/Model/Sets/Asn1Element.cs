namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Set of one element.
	/// </summary>
	public class Asn1Element : Asn1Values
	{
		private readonly Asn1Value element;

		/// <summary>
		/// Set of one element.
		/// </summary>
		/// <param name="Element">Element</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1Element(Asn1Value Element, Asn1Document Document)
			: base(Document)
		{
			this.element = Element;
			this.SetAsChild(this.element);
		}

		/// <summary>
		/// Element
		/// </summary>
		public Asn1Value Element => this.element;
	}
}
