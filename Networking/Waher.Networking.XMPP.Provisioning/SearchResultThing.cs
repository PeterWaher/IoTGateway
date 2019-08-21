using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Contains information about a thing in a search result.
	/// </summary>
	public class SearchResultThing
	{
		private readonly MetaDataTag[] tags;
		private readonly ThingReference node;
		private readonly string jid;

		/// <summary>
		/// Contains information about a thing in a search result.
		/// </summary>
		/// <param name="Jid">JID of thing.</param>
		/// <param name="Node">Node reference. Can be equal to <see cref="ThingReference.Empty"/> if not behind a concentrator.</param>
		/// <param name="Tags">Meta-data tags.</param>
		public SearchResultThing(string Jid, ThingReference Node, params MetaDataTag[] Tags)
		{
			this.jid = Jid;
			this.node = Node;
			this.tags = Tags;
		}

		/// <summary>
		/// JID of thing.
		/// </summary>
		public string Jid
		{
			get { return this.jid; }
		}

		/// <summary>
		/// Node reference. Can be equal to <see cref="ThingReference.Empty"/> if not behind a concentrator.
		/// </summary>
		public ThingReference Node
		{
			get { return this.node; }
		}

		/// <summary>
		/// Meta-data tags.
		/// </summary>
		public MetaDataTag[] Tags
		{
			get { return this.tags; }
		}
	}
}
