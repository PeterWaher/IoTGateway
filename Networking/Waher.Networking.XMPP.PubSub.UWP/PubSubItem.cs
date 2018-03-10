using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Represents a published item.
	/// </summary>
    public class PubSubItem
	{
		private string itemId = string.Empty;
		private string node = string.Empty;
		private string publisher = string.Empty;
		private string payload = string.Empty;

		/// <summary>
		/// Represents a published item.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="Xml">XML definition.</param>
		public PubSubItem(string Node, XmlElement Xml)
		{
			this.node = Node;
			this.itemId = XML.Attribute(Xml, "id");
			this.publisher = XML.Attribute(Xml, "publisher");
			this.payload = Xml.InnerXml;
		}

		/// <summary>
		/// Item ID.
		/// </summary>
		public string ItemId
		{
			get { return this.itemId; }
			set { this.itemId = value; }
		}

		/// <summary>
		/// Name of node.
		/// </summary>
		public string Node
		{
			get { return this.node; }
			set { this.node = value; }
		}

		/// <summary>
		/// Publisher of content.
		/// </summary>
		public string Publisher
		{
			get { return this.publisher; }
			set { this.publisher = value; }
		}

		/// <summary>
		/// Payload
		/// </summary>
		public string Payload
		{
			get { return this.payload; }
			set { this.payload = value; }
		}

	}
}
