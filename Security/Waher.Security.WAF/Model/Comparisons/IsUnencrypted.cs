using System.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the request is unencrypted.
	/// </summary>
	public class IsUnencrypted : WafComparison
	{
		/// <summary>
		/// Checks if the request is unencrypted.
		/// </summary>
		public IsUnencrypted()
			: base()
		{
		}

		/// <summary>
		/// Checks if the request is unencrypted.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public IsUnencrypted(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsUnencrypted);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new IsUnencrypted(Xml);
	}
}
