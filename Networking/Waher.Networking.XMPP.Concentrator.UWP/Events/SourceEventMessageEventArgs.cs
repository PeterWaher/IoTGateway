using Waher.Networking.XMPP.Events;
using Waher.Things.SourceEvents;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for data source section events.
	/// </summary>
	public class SourceEventMessageEventArgs : MessageEventArgs
	{
		private readonly SourceEvent sourceEvent;

		internal SourceEventMessageEventArgs(SourceEvent SourceEvent, MessageEventArgs Message)
			: base(Message)
		{
			this.sourceEvent = SourceEvent;
		}

		/// <summary>
		/// Source Event
		/// </summary>
		public SourceEvent Event => this.sourceEvent;
	}
}
