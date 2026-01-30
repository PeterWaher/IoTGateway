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
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public ConnectionsExceeded(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
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
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new ConnectionsExceeded(Xml, Parent, Document);
	}
}
