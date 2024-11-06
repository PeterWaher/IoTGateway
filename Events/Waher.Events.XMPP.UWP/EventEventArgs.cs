using Waher.Networking.XMPP.Events;

namespace Waher.Events.XMPP
{
	/// <summary>
	/// Event arguments for <see cref="XmppEventReceptor.OnEvent"/> events.
	/// </summary>
	public class EventEventArgs : MessageEventArgs
	{
		private readonly Event ev;

		internal EventEventArgs(MessageEventArgs e, Event Event)
			: base(e)
		{
			this.ev = Event;
		}

		/// <summary>
		/// Event.
		/// </summary>
		public Event Event => this.ev;
	}
}
