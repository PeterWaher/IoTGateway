using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Creates an entry in the ledger
	/// </summary>
	public class LedgerEntry : WafActionWithTags
	{
		private readonly StringAttribute collection;
		private readonly StringAttribute type;
		private readonly PositiveIntegerAttribute archivingTimeDays;

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
			this.collection = new StringAttribute(Xml, "collection");
			this.type = new StringAttribute(Xml, "type");
			this.archivingTimeDays = new PositiveIntegerAttribute(Xml, "archivingTimeDays");
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

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			Variables Variables = State.Variables;
			string Collection = await this.collection.EvaluateAsync(Variables, string.Empty);
			string Type = await this.type.EvaluateAsync(Variables, string.Empty);
			int ArchivingTimeDays = await this.archivingTimeDays.EvaluateAsync(Variables, 0);
			KeyValuePair<string, object>[] Tags = await this.EvaluateTags(State);
			GenericObject Obj = new GenericObject(Collection, Type, Guid.NewGuid(), Tags)
			{
				ArchivingTime = ArchivingTimeDays
			};

			await Ledger.NewEntry(Obj);

			return null;
		}
	}
}
