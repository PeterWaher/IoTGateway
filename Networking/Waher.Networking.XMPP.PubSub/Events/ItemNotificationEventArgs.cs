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
	public class ItemNotificationEventArgs : NodeNotificationEventArgs
    {
		private readonly string itemId;
		private readonly string publisher;
		private readonly string replyTo;
		private readonly XmlElement item;
		private readonly DateTime? delay;

		/// <summary>
		/// Event argument for item notification events.
		/// </summary>
		/// <param name="NodeName">Node name.</param>
		/// <param name="ItemId">Item ID</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		/// <param name="Publisher">Publisher.</param>
		/// <param name="ReplyTo">Reply To address.</param>
		/// <param name="Item">Item XML element</param>
		/// <param name="Delay">If provided, contains information about when the item was published.</param>
		/// <param name="e">Message event arguments</param>
		public ItemNotificationEventArgs(string NodeName, string ItemId, string SubscriptionId,
			string Publisher, string ReplyTo, XmlElement Item, DateTime? Delay, MessageEventArgs e)
			: base(NodeName, SubscriptionId, e)
		{
			this.itemId = ItemId;
			this.publisher = Publisher;
			this.replyTo = ReplyTo;
			this.item = Item;
			this.delay = Delay;
		}

		/// <summary>
		/// Event argument for item notification events.
		/// </summary>
		/// <param name="e">Item notification event arguments</param>
		public ItemNotificationEventArgs(ItemNotificationEventArgs e)
			: base(e)
		{
			this.itemId = e.itemId;
			this.publisher = e.publisher;
			this.replyTo = e.replyTo;
			this.item = e.item;
			this.delay = e.delay;
		}

		/// <summary>
		/// Item identity
		/// </summary>
		public string ItemId
		{
			get { return this.itemId; }
		}

		/// <summary>
		/// Publisher of content.
		/// </summary>
		public string Publisher
		{
			get { return this.publisher; }
		}

		/// <summary>
		/// Reply-to address
		/// </summary>
		public string ReplyTo
		{
			get { return this.replyTo; }
		}

		/// <summary>
		/// Item element. The inner XML contains payload, if included in notification.
		/// </summary>
		public XmlElement Item
		{
			get { return this.item; }
		}

		/// <summary>
		/// If provided, contains information about when the item was published.
		/// </summary>
		public DateTime? Delay
		{
			get { return this.delay; }
		}
	}
}
