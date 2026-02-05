using System.Xml;
using Waher.Content.Xml.Attributes;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Abstract base class for WAF actions with a name and delta.
	/// </summary>
	public abstract class WafActionWithNameAndDelta : WafActionWithName
	{
		/// <summary>
		/// Name attribute
		/// </summary>
		protected readonly Int64Attribute delta;

		/// <summary>
		/// Abstract base class for WAF actions with a name and delta.
		/// </summary>
		public WafActionWithNameAndDelta()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for WAF actions with a name and delta.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafActionWithNameAndDelta(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.delta = new Int64Attribute(Xml, "delta");
		}
	}
}
