using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for item event notificaction event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments</param>
	public delegate void ItemNotificationEventHandler(object Sender, ItemNotificationEventArgs e);

	/// <summary>
	/// Event argument for item notification events.
	/// </summary>
	public class ItemNotificationEventArgs : MessageEventArgs
    {
		private string nodeName;
		private string itemId;
		private string subscriptionId;
		private string publisher;
		private XmlElement item;

		/// <summary>
		/// Event argument for item notification events.
		/// </summary>
		/// <param name="NodeName">Node name.</param>
		/// <param name="ItemId">Item ID</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Publisher">Publisher.</param>
		/// <param name="Item">Item XML element</param>
		/// <param name="e">Message event arguments</param>
		public ItemNotificationEventArgs(string NodeName, string ItemId, string SubscriptionId,
			string Publisher, XmlElement Item, MessageEventArgs e)
			: base(e)
		{
			this.nodeName = NodeName;
			this.itemId = ItemId;
			this.subscriptionId = SubscriptionId;
			this.publisher = Publisher;
			this.item = Item;
		}

		/// <summary>
		/// Name of node.
		/// </summary>
		public string NodeName
		{
			get { return this.nodeName; }
		}

		/// <summary>
		/// Item identity
		/// </summary>
		public string ItemId
		{
			get { return this.itemId; }
		}

		/// <summary>
		/// Subscription identity, if provided in event notification.
		/// </summary>
		public string SubscriptionId
		{
			get { return this.subscriptionId; }
		}

		/// <summary>
		/// Publisher of content.
		/// </summary>
		public string Publisher
		{
			get { return this.publisher; }
		}

		/// <summary>
		/// Item element. The inner XML contains payload, if included in notification.
		/// </summary>
		public XmlElement Item
		{
			get { return this.item; }
		}
	}
}
