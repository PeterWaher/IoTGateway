using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP;

namespace Waher.Events.XMPP
{
	/// <summary>
	/// Delegate for <see cref="XmppEventReceptor.OnEvent"/> events.
	/// </summary>
	/// <param name="Sender"></param>
	/// <param name="e"></param>
	public delegate Task EventEventHandlerAsync(object Sender, EventEventArgs e);

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
		public Event Event { get { return this.ev; } }
	}
}
