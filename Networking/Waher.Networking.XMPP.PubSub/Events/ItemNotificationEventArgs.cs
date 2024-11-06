using System;
using System.Xml;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.PubSub.Events
{
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
		public string ItemId => this.itemId;

		/// <summary>
		/// Publisher of content.
		/// </summary>
		public string Publisher => this.publisher;

		/// <summary>
		/// Reply-to address
		/// </summary>
		public string ReplyTo => this.replyTo;

		/// <summary>
		/// Item element. The inner XML contains payload, if included in notification.
		/// </summary>
		public XmlElement Item => this.item;

		/// <summary>
		/// If provided, contains information about when the item was published.
		/// </summary>
		public DateTime? Delay => this.delay;
	}
}
