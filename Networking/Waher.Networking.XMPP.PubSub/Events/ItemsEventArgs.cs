using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.ResultSetManagement;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Delegate for items callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ItemsEventHandler(object Sender, ItemsEventArgs e);

	/// <summary>
	/// Event arguments for items callback events.
	/// </summary>
	public class ItemsEventArgs : NodeEventArgs 
    {
		private PubSubItem[] items;
		private ResultPage page;

		/// <summary>
		/// Event arguments for items callback events.
		/// </summary>
		/// <param name="NodeName">Name of node.</param>
		/// <param name="Items">Items found.</param>
		/// <param name="Page">Pagination information.</param>
		/// <param name="e">IQ result event arguments.</param>
		public ItemsEventArgs(string NodeName, PubSubItem[] Items, ResultPage Page, IqResultEventArgs e)
			: base(NodeName, e)
		{
			this.items = Items;
			this.page = Page;
		}

		/// <summary>
		/// Event arguments for items callback events.
		/// </summary>
		/// <param name="e">Items event arguments.</param>
		public ItemsEventArgs(ItemsEventArgs e)
			: base(e)
		{
			this.items = e.Items;
			this.page = e.Page;
		}

		/// <summary>
		/// Items found.
		/// </summary>
		public PubSubItem[] Items
		{
			get { return this.items; }
		}

		/// <summary>
		/// Pagination information, if available, null otherwise.
		/// </summary>
		public ResultPage Page
		{
			get { return this.page; }
		}
	}
}
