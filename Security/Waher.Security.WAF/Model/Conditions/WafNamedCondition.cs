using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Abstract base class for named Web Application Firewall conditions.
	/// </summary>
	public abstract class WafNamedCondition : WafCondition
	{
		private readonly string name;

		/// <summary>
		/// Abstract base class for named Web Application Firewall conditions.
		/// </summary>
		public WafNamedCondition()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall conditions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafNamedCondition(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.name = XML.Attribute(Xml, "name");
		}

		/// <summary>
		/// Name reference
		/// </summary>
		public string Name => this.name;
	}
}
