using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the content size of the current request exceeds a limit.
	/// </summary>
	public class ContentSizeExceeded : WafComparison
	{
		private readonly long limit;

		/// <summary>
		/// Checks if the content size of the current request exceeds a limit.
		/// </summary>
		public ContentSizeExceeded()
			: base()
		{
		}

		/// <summary>
		/// Checks if the content size of the current request exceeds a limit.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public ContentSizeExceeded(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.limit = XML.Attribute(Xml, "limit", 0L);
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(ContentSizeExceeded);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new ContentSizeExceeded(Xml, Parent, Document);
	}
}
