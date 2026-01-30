using System.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Ignores the request.
	/// </summary>
	public class Ignore : WafAction
	{
		/// <summary>
		/// Ignores the request.
		/// </summary>
		public Ignore()
			: base()
		{
		}

		/// <summary>
		/// Ignores the request.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public Ignore(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Ignore);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new Ignore(Xml, Parent, Document);
	}
}
