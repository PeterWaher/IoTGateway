namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Set of classes.
	/// </summary>
	public class Asn1ClassSet : Asn1Node
	{
		private readonly Asn1ClassInstanceElement[] classes;

		/// <summary>
		/// Set of classes.
		/// </summary>
		/// <param name="Classes">Class instance elements.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassSet(Asn1ClassInstanceElement[] Classes, Asn1Document Document)
			: base(Document)
		{
			this.classes = Classes;
		}

		/// <summary>
		/// Class instance elements.
		/// </summary>
		public Asn1ClassInstanceElement[] Classes => this.classes;
	}
}
