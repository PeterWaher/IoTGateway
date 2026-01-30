using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the requests come from a specific IP or IPs using a comma-separated
	/// list of CIDR ranges.
	/// </summary>
	public class FromIp : WafComparison
	{
		private readonly string value;

		/// <summary>
		/// Checks if the requests come from a specific IP or IPs using a comma-separated
		/// list of CIDR ranges.
		/// </summary>
		public FromIp()
			: base()
		{
		}

		/// <summary>
		/// Checks if the requests come from a specific IP or IPs using a comma-separated
		/// list of CIDR ranges.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public FromIp(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.value = XML.Attribute(Xml, "value");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(FromIp);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new FromIp(Xml, Parent, Document);
	}
}
