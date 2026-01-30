using System.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the remote endpoint of the request is blocked.
	/// </summary>
	public class IsBlocked : WafComparison
	{
		/// <summary>
		/// Checks if the remote endpoint of the request is blocked.
		/// </summary>
		public IsBlocked()
			: base()
		{
		}

		/// <summary>
		/// Checks if the remote endpoint of the request is blocked.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public IsBlocked(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsBlocked);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new IsBlocked(Xml);
	}
}
