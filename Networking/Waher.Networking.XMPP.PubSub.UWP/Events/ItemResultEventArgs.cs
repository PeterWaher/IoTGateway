using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for item result event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments</param>
	public delegate Task ItemResultEventHandler(object Sender, ItemResultEventArgs e);

	/// <summary>
	/// Event argument for item result events.
	/// </summary>
	public class ItemResultEventArgs : NodeEventArgs
    {
		private readonly string itemId;

		/// <summary>
		/// Event argument for item result events.
		/// </summary>
		/// <param name="NodeName">Node name.</param>
		/// <param name="ItemId">Item ID</param>
		/// <param name="e">Message event arguments</param>
		public ItemResultEventArgs(string NodeName, string ItemId, IqResultEventArgs e)
			: base(NodeName, e)
		{
			this.itemId = ItemId;
		}

		/// <summary>
		/// Item identity
		/// </summary>
		public string ItemId
		{
			get { return this.itemId; }
		}

	}
}
