using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for node query events.
	/// </summary>
	public class NodeQueryEventArgs : MessageEventArgs
	{
		private readonly NodeQuery query;

		internal NodeQueryEventArgs(NodeQuery Query, MessageEventArgs Message)
			: base(Message)
		{
			this.query = Query;
		}

		/// <summary>
		/// Node query
		/// </summary>
		public NodeQuery Query => this.query;
	}
}
