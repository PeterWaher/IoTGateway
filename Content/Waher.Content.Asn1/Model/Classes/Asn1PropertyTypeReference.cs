using System.Text;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Defines a property name and value reference in a class instance definition.
	/// </summary>
	public class Asn1PropertyTypeReference : Asn1Node
	{
		private readonly string name;
		private readonly Asn1Type type;

		/// <summary>
		/// Defines a property name and value reference in a class instance definition.
		/// </summary>
		/// <param name="Name">Property value.</param>
		/// <param name="Type">Type</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1PropertyTypeReference(string Name, Asn1Type Type, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
			this.type = Type;
		}

		/// <summary>
		/// Property name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Type reference
		/// </summary>
		public Asn1Type Type => this.type;

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.name);
			sb.Append(": ");
			sb.Append(this.type.ToString());

			return sb.ToString();
		}
	}
}
