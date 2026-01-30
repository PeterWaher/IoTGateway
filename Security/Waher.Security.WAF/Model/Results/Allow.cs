using System.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Allows the request to proceed.
	/// </summary>
	public class Allow : WafAction
	{
		/// <summary>
		/// Allows the request to proceed.
		/// </summary>
		public Allow()
			: base()
		{
		}

		/// <summary>
		/// Allows the request to proceed.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public Allow(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Allow);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new Allow(Xml, Parent, Document);
	}
}
