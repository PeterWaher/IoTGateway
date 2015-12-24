using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Contains information about an item of an entity.
	/// </summary>
	public class Item
	{
		private string jid;
		private string node;
		private string name;

		internal Item(XmlElement E)
		{
			this.jid = CommonTypes.XmlAttribute(E, "jid");
			this.node = CommonTypes.XmlAttribute(E, "node");
			this.name = CommonTypes.XmlAttribute(E, "name");
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
		public string JID { get { return this.jid; } }

		/// <summary>
		/// Node
		/// </summary>
		public string Node { get { return this.node; } }

		/// <summary>
		/// Name
		/// </summary>
		public string Name { get { return this.name; } }

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
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
