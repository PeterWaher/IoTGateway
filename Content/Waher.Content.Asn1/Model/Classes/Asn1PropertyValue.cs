namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Defines a property name and value in a class instance definition.
	/// </summary>
	public class Asn1PropertyValue : Asn1Node
	{
		private readonly Asn1Value value;
		private readonly string name;
		private Asn1Type type = null;

		/// <summary>
		/// Defines a property name and value in a class instance definition.
		/// </summary>
		/// <param name="Name">Property value.</param>
		/// <param name="Value">Type definition.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1PropertyValue(string Name, Asn1Value Value, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.value = Value;
		}

		/// <summary>
		/// Property name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Property value
		/// </summary>
		public Asn1Value Value => this.value;

		/// <summary>
		/// Associated type of property.
		/// </summary>
		public Asn1Type Type
		{
			get => this.type;
			internal set => this.type = value;
		}

		/// <inheritdoc/>
		public override string ToString() => this.name + ": " + this.value.ToString();
	}
}
