using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Defines a name-value tag.
	/// </summary>
	public class Tag : WafAction
	{
		private readonly string name;
		private readonly string value;

		/// <summary>
		/// Defines a name-value tag.
		/// </summary>
		public Tag()
			: base()
		{
		}

		/// <summary>
		/// Defines a name-value tag.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public Tag(XmlElement Xml)
			: base(Xml)
		{
			this.name = XML.Attribute(Xml, "name");
			this.value = XML.Attribute(Xml, "value");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Tag);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new Tag(Xml);
	}
}
