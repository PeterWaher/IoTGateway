using System.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the remote endpoint of the request is temporarily blocked.
	/// </summary>
	public class IsTemporarilyBlocked : WafComparison
	{
		/// <summary>
		/// Checks if the remote endpoint of the request is temporarily blocked.
		/// </summary>
		public IsTemporarilyBlocked()
			: base()
		{
		}

		/// <summary>
		/// Checks if the remote endpoint of the request is temporarily blocked.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public IsTemporarilyBlocked(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsTemporarilyBlocked);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new IsTemporarilyBlocked(Xml);
	}
}
