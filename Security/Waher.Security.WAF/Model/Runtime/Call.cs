using System.Xml;

namespace Waher.Security.WAF.Model.Runtime
{
	/// <summary>
	/// Calls another action with a given identity. Operation continues, if no
	/// result has been returned.
	/// </summary>
	public class Call : WafActionReference
	{
		/// <summary>
		/// Calls another action with a given identity. Operation continues, if no
		/// result has been returned.
		/// </summary>
		public Call()
			: base()
		{
		}

		/// <summary>
		/// Calls another action with a given identity. Operation continues, if no
		/// result has been returned.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public Call(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Call);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new Call(Xml, Parent, Document);
	}
}
