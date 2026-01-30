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
		public LedgerEntry(XmlElement Xml)
			: base(Xml)
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
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml) => new LedgerEntry(Xml);
	}
}
