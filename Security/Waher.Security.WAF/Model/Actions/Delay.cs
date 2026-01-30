using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Delays processing for a number of seconds.
	/// </summary>
	public class Delay : WafAction
	{
		private readonly int seconds;

		/// <summary>
		/// Delays processing for a number of seconds.
		/// </summary>
		public Delay()
			: base()
		{
		}

		/// <summary>
		/// Delays processing for a number of seconds.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public Delay(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.seconds = XML.Attribute(Xml, "seconds", 0);
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(Delay);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new Delay(Xml, Parent, Document);
	}
}
