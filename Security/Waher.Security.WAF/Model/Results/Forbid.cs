using System.Xml;

namespace Waher.Security.WAF.Model.Results
{
	/// <summary>
	/// Forbids the request to proceed.
	/// </summary>
	public class Forbid : WafAction
	{
		/// <summary>
		/// Forbids the request to proceed.
		/// </summary>
		public Forbid()
			: base()
		{
		}

		/// <summary>
		/// Forbids the request to proceed.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public Forbid(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Forbid);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new Forbid(Xml, Parent, Document);
	}
}
