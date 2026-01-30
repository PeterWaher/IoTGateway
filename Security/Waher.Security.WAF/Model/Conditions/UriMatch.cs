using System.Xml;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Checks for a match against the URI of the request.
	/// </summary>
	public class UriMatch : WafCondition
	{
		/// <summary>
		/// Checks for a match against the URI of the request.
		/// </summary>
		public UriMatch()
			: base()
		{
		}

		/// <summary>
		/// Checks for a match against the URI of the request.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public UriMatch(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(UriMatch);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new UriMatch(Xml);
	}
}
