using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the request is encrypted.
	/// </summary>
	public class IsEncrypted : WafComparison
	{
		private readonly int minSecurityStrength;

		/// <summary>
		/// Checks if the request is encrypted.
		/// </summary>
		public IsEncrypted()
			: base()
		{
		}

		/// <summary>
		/// Checks if the request is encrypted.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public IsEncrypted(XmlElement Xml)
			: base(Xml)
		{
			this.minSecurityStrength = XML.Attribute(Xml, "minSecurityStrength", 0);
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsEncrypted);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new IsEncrypted(Xml);
	}
}
