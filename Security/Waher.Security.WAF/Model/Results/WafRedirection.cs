using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Abstract base class for redirection results.
	/// </summary>
	public abstract class WafRedirection : WafAction
	{
		private readonly string location;

		/// <summary>
		/// Abstract base class for redirection results.
		/// </summary>
		public WafRedirection()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for redirection results.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafRedirection(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.location = XML.Attribute(Xml, "location");
		}

		/// <summary>
		/// Location URL for the redirection
		/// </summary>
		public string Location => this.location;
	}
}
