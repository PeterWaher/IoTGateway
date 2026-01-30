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
		public IsContent(XmlElement Xml)
			: base(Xml)
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
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new IsContent(Xml);
	}
}
