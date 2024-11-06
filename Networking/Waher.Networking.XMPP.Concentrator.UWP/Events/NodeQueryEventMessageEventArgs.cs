using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.Events;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for node query section events.
	/// </summary>
	public class NodeQueryEventMessageEventArgs : NodeQueryEventArgs
	{
		private readonly string eventMessage;
		private readonly QueryEventType type;
		private readonly QueryEventLevel level;

		internal NodeQueryEventMessageEventArgs(QueryEventType Type, QueryEventLevel Level, string EventMessage, NodeQuery Query, MessageEventArgs Message)
			: base(Query, Message)
		{
			this.type = Type;
			this.level = Level;
			this.eventMessage = EventMessage;
		}

		/// <summary>
		/// Event type
		/// </summary>
		public QueryEventType EventType => this.type;

		/// <summary>
		/// Event level
		/// </summary>
		public QueryEventLevel EventLevel => this.level;

		/// <summary>
		/// Event Message
		/// </summary>
		public string EventMessage => this.eventMessage;
	}
}
