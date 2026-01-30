using System.Xml;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the remote endpoint has made too many connections.
	/// </summary>
	public class ConnectionsExceeded : RateLimitComparison
	{
		/// <summary>
		/// Checks if the remote endpoint has made too many connections.
		/// </summary>
		public ConnectionsExceeded()
			: base()
		{
		}

		/// <summary>
		/// Checks if the remote endpoint has made too many connections.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public ConnectionsExceeded(XmlElement Xml)
			: base(Xml)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(ConnectionsExceeded);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new ConnectionsExceeded(Xml);
	}
}
