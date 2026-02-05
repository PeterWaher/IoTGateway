using System.Xml;
using Waher.Content.Xml.Attributes;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Abstract base class for WAF actions with a name.
	/// </summary>
	public abstract class WafActionWithName : WafAction
	{
		/// <summary>
		/// Name attribute
		/// </summary>
		protected readonly StringAttribute name;

		/// <summary>
		/// Abstract base class for WAF actions with a name.
		/// </summary>
		public WafActionWithName()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for WAF actions with a name.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafActionWithName(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.name = new StringAttribute(Xml, "name");
		}
	}
}
