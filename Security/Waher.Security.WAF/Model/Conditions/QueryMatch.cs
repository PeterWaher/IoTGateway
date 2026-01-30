using System.Xml;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Checks for a match against the Query of the request.
	/// </summary>
	public class QueryMatch : WafNamedCondition
	{
		/// <summary>
		/// Checks for a match against the Query of the request.
		/// </summary>
		public QueryMatch()
			: base()
		{
		}

		/// <summary>
		/// Checks for a match against the Query of the request.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public QueryMatch(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(QueryMatch);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new QueryMatch(Xml);
	}
}
