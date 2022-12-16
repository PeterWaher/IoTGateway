using System;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Contains information about an item of an entity.
	/// </summary>
	public class Item
	{
		private readonly string jid;
		private readonly string node;
		private readonly string name;

		internal Item(XmlElement E)
		{
			this.jid = XML.Attribute(E, "jid");
			this.node = XML.Attribute(E, "node");
			this.name = XML.Attribute(E, "name");
		}

		/// <summary>
		/// Contains information about an item of an entity.
		/// </summary>
		/// <param name="Jid">JID of item</param>
		/// <param name="Node">Node</param>
		/// <param name="Name">Name</param>
		public Item(string Jid, string Node, string Name)
		{
			this.jid = Jid;
			this.node = Node;
			this.name = Name;
		}

		/// <summary>
		/// JID of item
		/// </summary>
		public string JID => this.jid;

		/// <summary>
		/// Node
		/// </summary>
		public string Node => this.node;

		/// <summary>
		/// Name
		/// </summary>
		public string Name => this.name;

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.jid);
			sb.Append(", ");
			sb.Append(this.node);
			sb.Append(", ");
			sb.Append(this.name);

			return sb.ToString();
		}
	}
}
