using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator.Queries;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node query table callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodeQueryTableEventHandler(object Sender, NodeQueryTableEventArgs e);

	/// <summary>
	/// Event arguments for node query table events.
	/// </summary>
	public class NodeQueryTableEventArgs : NodeQueryEventArgs
	{
		private readonly QueryTable table;

		internal NodeQueryTableEventArgs(QueryTable Table, NodeQuery Query, MessageEventArgs Message)
			: base(Query, Message)
		{
			this.table = Table;
		}

		/// <summary>
		/// Table
		/// </summary>
		public QueryTable Table
		{
			get { return this.table; }
		}
	}
}
