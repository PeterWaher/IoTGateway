using System.Xml;

namespace Waher.Security.WAF.Model.Runtime
{
	/// <summary>
	/// Contains a set of actions that can be referenced from other actions.
	/// </summary>
	public class References : WafActionReference
	{
		/// <summary>
		/// Contains a set of actions that can be referenced from other actions.
		/// </summary>
		public References()
			: base()
		{
		}

		/// <summary>
		/// Contains a set of actions that can be referenced from other actions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public References(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(References);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new References(Xml, Parent, Document);
	}
}
