using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Checks if an endpoint has an Open Intelligence record associated with it.
	/// </summary>
	public class HasOpenIntelligence : WafActionWithTags
	{
		private readonly Duration duration;
		private readonly string vector;
		private readonly string protocol;
		private readonly string classification;
		private readonly string code;
		private readonly string message;

		/// <summary>
		/// Checks if an endpoint has an Open Intelligence record associated with it.
		/// </summary>
		public HasOpenIntelligence()
			: base()
		{
		}

		/// <summary>
		/// Checks if an endpoint has an Open Intelligence record associated with it.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public HasOpenIntelligence(XmlElement Xml)
			: base(Xml)
		{
			this.duration = XML.Attribute(Xml, "duration", Duration.Zero);
			this.vector = XML.Attribute(Xml, "vector");
			this.protocol = XML.Attribute(Xml, "protocol");
			this.classification = XML.Attribute(Xml, "classification");
			this.code = XML.Attribute(Xml, "code");
			this.message = XML.Attribute(Xml, "message");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(HasOpenIntelligence);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new HasOpenIntelligence(Xml);
	}
}
