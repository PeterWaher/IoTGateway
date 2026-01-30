using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Creates an entry in the ledger
	/// </summary>
	public class LedgerEntry : WafActionWithTags
	{
		private readonly string collection;
		private readonly string type;

		/// <summary>
		/// Creates an entry in the ledger
		/// </summary>
		public LedgerEntry()
			: base()
		{
		}

		/// <summary>
		/// Creates an entry in the ledger
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public LedgerEntry(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.collection = XML.Attribute(Xml, "collection");
			this.type = XML.Attribute(Xml, "type");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(LedgerEntry);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new LedgerEntry(Xml, Parent, Document);
	}
}
