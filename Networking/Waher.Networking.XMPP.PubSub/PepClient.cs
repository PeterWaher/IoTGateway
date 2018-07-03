using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Client managing the Personal Eventing Protocol (XEP-0163).
	/// https://xmpp.org/extensions/xep-0163.html
	/// </summary>
	public class PepClient : XmppExtension
	{
		private PubSubClient pubSubClient;

		/// <summary>
		/// Client managing the Personal Eventing Protocol (XEP-0163).
		/// https://xmpp.org/extensions/xep-0163.html
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		public PepClient(XmppClient Client)
			: base(Client)
		{
			this.pubSubClient = new PubSubClient(Client, string.Empty);

			this.pubSubClient.ItemNotification += PubSubClient_ItemNotification;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			if (this.pubSubClient != null)
			{
				this.pubSubClient.ItemNotification -= PubSubClient_ItemNotification;

				this.pubSubClient.Dispose();
				this.pubSubClient = null;
			}

			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0163" };

		#region Publish Item

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Publish(string Node, ItemResultEventHandler Callback, object State)
		{
			this.pubSubClient?.Publish(Node, Callback, State);
		}

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="PayloadXml">Payload XML.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Publish(string Node, string PayloadXml, ItemResultEventHandler Callback, object State)
		{
			this.pubSubClient?.Publish(Node, PayloadXml, Callback, State);
		}

		/// <summary>
		/// Publishes an item on a node.
		/// </summary>
		/// <param name="Node">Node name.</param>
		/// <param name="ItemId">Item identity, if available. If used, and an existing item
		/// is available with that identity, it will be updated with the new content.</param>
		/// <param name="PayloadXml">Payload XML.</param>
		/// <param name="Callback">Method to call when operation completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Publish(string Node, string ItemId, string PayloadXml, ItemResultEventHandler Callback, object State)
		{
			this.pubSubClient?.Publish(Node, ItemId, PayloadXml, Callback, State);
		}

		private void PubSubClient_ItemNotification(object Sender, ItemNotificationEventArgs e)
		{
			this.ItemNotification?.Invoke(this, e);
		}

		/// <summary>
		/// Event raised whenever an item notification has been received.
		/// </summary>
		public event ItemNotificationEventHandler ItemNotification = null;

		#endregion

	}
}
