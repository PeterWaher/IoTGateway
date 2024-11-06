using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for node query object events.
	/// </summary>
	public class NodeQueryObjectEventArgs : NodeQueryEventArgs
	{
		private readonly QueryObject obj;

		internal NodeQueryObjectEventArgs(QueryObject Object, NodeQuery Query, MessageEventArgs Message)
			: base(Query, Message)
		{
			this.obj = Object;
		}

		/// <summary>
		/// Object
		/// </summary>
		public QueryObject Object => this.obj;
	}
}
