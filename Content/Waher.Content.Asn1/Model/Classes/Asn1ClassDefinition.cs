namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Defines a class.
	/// </summary>
	public class Asn1ClassDefinition : Asn1Node
	{
		private readonly Asn1PropertyDefinition[] properties;
		private readonly string name;

		/// <summary>
		/// Defines a class.
		/// </summary>
		/// <param name="Name">Class name.</param>
		/// <param name="Properties">Class properties.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassDefinition(string Name, Asn1PropertyDefinition[] Properties, 
			Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.properties = Properties;
		}
	}
}
