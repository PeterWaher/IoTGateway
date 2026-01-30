using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Runtime
{
	/// <summary>
	/// Abstract base class for WAF actions that has tags.
	/// </summary>
	public abstract class WafActionReference : WafActions
	{
		private readonly string reference;

		/// <summary>
		/// Abstract base class for WAF actions that has tags.
		/// </summary>
		public WafActionReference()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for WAF actions that has tags.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafActionReference(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.reference = XML.Attribute(Xml, "reference");
		}

		/// <summary>
		/// Node reference.
		/// </summary>
		public string Reference => this.reference;
	}
}
