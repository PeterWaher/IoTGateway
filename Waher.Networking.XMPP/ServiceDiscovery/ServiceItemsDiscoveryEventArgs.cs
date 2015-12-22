using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Delegate for service items discovery events or callback methods.
	/// </summary>
	/// <param name="Client"></param>
	/// <param name="e"></param>
	public delegate void ServiceItemsDiscoveryEventHandler(XmppClient Client, ServiceItemsDiscoveryEventArgs e);

	/// <summary>
	/// Event arguments for service items discovery responses.
	/// </summary>
	public class ServiceItemsDiscoveryEventArgs : IqResultEventArgs
	{
		private Item[] items;

		internal ServiceItemsDiscoveryEventArgs(IqResultEventArgs e, Item[] Items)
			: base(e)
		{
			this.items = Items;
		}

		/// <summary>
		/// Items
		/// </summary>
		public Item[] Items { get { return this.items; } }
	}
}
