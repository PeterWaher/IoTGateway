using System.Xml;

namespace Waher.Security.WAF.Model
{
	/// <summary>
	/// interface for Web Application Firewall actions.
	/// </summary>
	public interface IWafAction
	{
		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		string LocalName { get; }

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document);
	}
}
