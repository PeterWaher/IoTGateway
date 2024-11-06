using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
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
		public QueryTable Table => this.table;
	}
}
