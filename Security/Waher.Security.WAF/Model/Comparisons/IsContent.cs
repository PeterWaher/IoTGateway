using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the request is encrypted.
	/// </summary>
	public class IsContent : WafComparison
	{
		private readonly string manifestFile;

		/// <summary>
		/// Checks if the request is encrypted.
		/// </summary>
		public IsContent()
			: base()
		{
		}

		/// <summary>
		/// Checks if the request is encrypted.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public IsContent(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.manifestFile = XML.Attribute(Xml, "manifestFile");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsContent);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new IsContent(Xml, Parent, Document);
	}
}
