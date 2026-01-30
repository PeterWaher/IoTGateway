using System.Xml;

namespace Waher.Security.WAF.Model.Runtime
{
	/// <summary>
	/// Goes to another action with a given identity.
	/// </summary>
	public class GoTo : WafActionReference
	{
		/// <summary>
		/// Goes to another action with a given identity.
		/// </summary>
		public GoTo()
			: base()
		{
		}

		/// <summary>
		/// Goes to another action with a given identity.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public GoTo(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(GoTo);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new GoTo(Xml, Parent, Document);
	}
}
