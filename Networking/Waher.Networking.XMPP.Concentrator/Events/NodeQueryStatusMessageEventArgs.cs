using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for node query section events.
	/// </summary>
	public class NodeQueryStatusMessageEventArgs : NodeQueryEventArgs
	{
		private readonly string statusMessage;

		internal NodeQueryStatusMessageEventArgs(string StatusMessage, NodeQuery Query, MessageEventArgs Message)
			: base(Query, Message)
		{
			this.statusMessage = StatusMessage;
		}

		/// <summary>
		/// Status Message
		/// </summary>
		public string StatusMessage => this.statusMessage;
	}
}
