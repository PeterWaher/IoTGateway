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
		/// <returns>Created action object.</returns>
		WafAction Create(XmlElement Xml);
	}
}
