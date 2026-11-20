using System.Collections.Generic;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Instance of a class.
	/// </summary>
	public class Asn1ClassInstance : Asn1Node
	{
		private readonly Dictionary<string, Asn1PropertyValue> properties;
		private readonly Asn1ClassDefinition @class;

		/// <summary>
		/// Instance of a class.
		/// </summary>
		/// <param name="Class">Class definition</param>
		/// <param name="Properties">Instance property values.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassInstance(Asn1ClassDefinition Class,
			Dictionary<string, Asn1PropertyValue> Properties, Asn1Document Document)
			: base(Document)
		{
			this.@class = Class;
			this.properties = Properties;
		}

		/// <summary>
		/// Class definition.
		/// </summary>
		public Asn1ClassDefinition Class => this.@class;

		/// <summary>
		/// Class instance property values.
		/// </summary>
		public Dictionary<string, Asn1PropertyValue> Properties => this.properties;

		/// <inheritdoc/>
		public override string ToString() => this.@class.ToString() + " instance";
	}
}
