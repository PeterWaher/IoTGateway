namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Defines a property name in a class definition.
	/// </summary>
	public class Asn1PropertyDefinition : Asn1Node
	{
		private readonly Asn1Type type;
		private readonly string name;

		/// <summary>
		/// Defines a property name in a class definition.
		/// </summary>
		/// <param name="Name">Property name.</param>
		/// <param name="Type">Type definition.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1PropertyDefinition(string Name, Asn1Type Type, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.type = Type;
		}
	}
}
