namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Abstract base class for class instance elements.
	/// </summary>
	public abstract class Asn1ClassInstanceElement : Asn1Type
	{
		/// <summary>
		/// Abstract base class for class instance elements.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassInstanceElement(Asn1Document Document)
			: base(Document)
		{
		}
	}
}
