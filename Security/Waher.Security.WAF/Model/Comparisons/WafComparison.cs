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
		public WafComparison(XmlElement Xml)
			: base(Xml)
		{
		}
	}
}
