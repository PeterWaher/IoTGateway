using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Event arguments for service items discovery responses.
	/// </summary>
	public class ServiceItemsDiscoveryEventArgs : IqResultEventArgs
	{
		private readonly Item[] items;

		internal ServiceItemsDiscoveryEventArgs(IqResultEventArgs e, Item[] Items)
			: base(e)
		{
			this.items = Items;
		}

		/// <summary>
		/// Items
		/// </summary>
		public Item[] Items => this.items;
	}
}
