using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Toon;
using Waher.Networking.HTTP.Mcp.Model.Resources;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.XMPP;

namespace Waher.Mcp.Xmpp
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
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="RosterItem">Roster item.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public RosterItemResource(string Name, string Title, string Description, Uri Uri,
			RosterItem RosterItem, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, MetaData)
		{
			this.rosterItem = RosterItem;
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
