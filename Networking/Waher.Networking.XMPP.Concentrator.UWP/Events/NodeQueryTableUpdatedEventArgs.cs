using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.Events;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for node query table events.
	/// </summary>
	public class NodeQueryTableUpdatedEventArgs : NodeQueryTableEventArgs
	{
		private readonly Record[] newRecords;

		internal NodeQueryTableUpdatedEventArgs(QueryTable Table, NodeQuery Query, Record[] NewRecords, MessageEventArgs Message)
			: base(Table, Query, Message)
		{
			this.newRecords = NewRecords;
		}

		/// <summary>
		/// New Records
		/// </summary>
		public Record[] NewRecords => this.newRecords;
	}
}
