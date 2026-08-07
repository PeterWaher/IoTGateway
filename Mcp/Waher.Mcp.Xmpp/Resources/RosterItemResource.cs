using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Toon;
using Waher.Networking.HTTP.Mcp.Model.Resources;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.XMPP;

namespace Waher.Mcp.Xmpp.Resources
{
	/// <summary>
	/// Contains information about a Roster Item.
	/// </summary>
	public class RosterItemResource : Resource
	{
		private readonly RosterItem rosterItem;

		/// <summary>
		/// Contains information about a Roster Item.
		/// </summary>
		/// <param name="Item">Roster item.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public RosterItemResource(RosterItem Item, params KeyValuePair<string, object>[] MetaData)
			: base(Item.BareJid, Item.NameOrBareJid, "Roster item for " + Item.BareJid,
				  CreateRosterUri(Item), MetaData)
		{
			this.rosterItem = Item;
		}

		/// <summary>
		/// Creates a Roster Item Resource URI.
		/// </summary>
		/// <param name="Item">Roster Item</param>
		/// <returns>URI</returns>
		public static Uri CreateRosterUri(RosterItem Item)
		{
			return CreateRosterUri(Item.BareJid);
		}

		/// <summary>
		/// Creates a Roster Item Resource URI.
		/// </summary>
		/// <param name="BareJid">Bare JID of roster item.</param>
		/// <returns>URI</returns>
		public static Uri CreateRosterUri(string BareJid)
		{
			return new Uri("xmpp:" + BareJid);
		}

		/// <summary>
		/// Reads the resource.
		/// </summary>
		/// <param name="MetaData">Associated meta-data, if available.</param>
		/// <returns>Content objects read.</returns>
		public override Task<IResourceContent[]> Read(
			Dictionary<string, object>? MetaData)
		{
			Dictionary<string, object> Contents = new Dictionary<string, object>()
			{
				{ "BareJid", this.rosterItem.BareJid },
				{ "Name", this.rosterItem.Name },
				{ "State", this.rosterItem.State },
				{ "Pending", this.rosterItem.PendingSubscription },
				{ "Groups", this.rosterItem.Groups }
			};

			string s = TOON.Encode(Contents, false);

			return Task.FromResult(new IResourceContent[]
			{
				new TextContent(this.Uri, s, ToonEncoder.DefaultContentType, MetaData)
			});
		}
	}
}
