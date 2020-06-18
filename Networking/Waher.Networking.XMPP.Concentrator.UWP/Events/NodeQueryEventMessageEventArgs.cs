using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node query section callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodeQueryEventMessageEventHandler(object Sender, NodeQueryEventMessageEventArgs e);

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
		public string EventMessage
		{
			get { return this.eventMessage; }
		}
	}
}
