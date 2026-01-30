using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Adds a record to the Open Intelligence database
	/// </summary>
	public class AddOpenIntelligence : WafActionWithTags
	{
		private readonly Duration duration;
		private readonly string vector;
		private readonly string protocol;
		private readonly string classification;
		private readonly string code;
		private readonly string message;

		/// <summary>
		/// Adds a record to the Open Intelligence database
		/// </summary>
		public AddOpenIntelligence()
			: base()
		{
		}

		/// <summary>
		/// Adds a record to the Open Intelligence database
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public AddOpenIntelligence(XmlElement Xml)
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
		public override string LocalName => nameof(AddOpenIntelligence);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new AddOpenIntelligence(Xml);
	}
}
