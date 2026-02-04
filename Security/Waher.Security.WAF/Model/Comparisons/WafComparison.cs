using System.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Abstract base class for Web Application Firewall comparisons.
	/// </summary>
	public abstract class WafComparison : WafActions
	{
		/// <summary>
		/// Abstract base class for Web Application Firewall comparisons.
		/// </summary>
		public WafComparison()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall comparisons.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafComparison(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}
	}
}
