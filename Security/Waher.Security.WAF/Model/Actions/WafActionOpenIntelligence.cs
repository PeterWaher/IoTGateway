using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Abstract base class for Web Application Firewall Open Intelligence actions.
	/// </summary>
	public abstract class WafActionOpenIntelligence : WafActionWithTags
	{
		private readonly Duration duration;
		private readonly string vector;
		private readonly string protocol;
		private readonly string classification;
		private readonly string code;
		private readonly string message;

		/// <summary>
		/// Abstract base class for Web Application Firewall Open Intelligence actions.
		/// </summary>
		public WafActionOpenIntelligence()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall Open Intelligence actions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafActionOpenIntelligence(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.duration = XML.Attribute(Xml, "duration", Duration.Zero);
			this.vector = XML.Attribute(Xml, "vector");
			this.protocol = XML.Attribute(Xml, "protocol");
			this.classification = XML.Attribute(Xml, "classification");
			this.code = XML.Attribute(Xml, "code");
			this.message = XML.Attribute(Xml, "message");
		}

		/// <summary>
		/// Duration of record.
		/// </summary>
		public Duration Duration => this.duration;

		/// <summary>
		/// Attack vector.
		/// </summary>
		public string Vector => this.vector;

		/// <summary>
		/// Associated protocol.
		/// </summary>
		public string Protocol => this.protocol;

		/// <summary>
		/// Classification
		/// </summary>
		public string Classification => this.classification;

		/// <summary>
		/// Code
		/// </summary>
		public string Code => this.code;

		/// <summary>
		/// Message
		/// </summary>
		public string Message => this.message;
	}
}
