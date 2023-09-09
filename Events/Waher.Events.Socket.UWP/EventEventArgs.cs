using System;
using System.Threading.Tasks;

namespace Waher.Events.Socket
{
	/// <summary>
	/// Delegate for Event event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task EventEventHandler(object Sender, EventEventArgs e);

	/// <summary>
	/// Event arguments for Event events.
	/// </summary>
	public class EventEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for custom fragment events.
		/// </summary>
		/// <param name="Event">Event</param>
		public EventEventArgs(Event Event)
		{
			this.Event = Event;
		}

		/// <summary>
		/// Event
		/// </summary>
		public Event Event { get; }
	}
}
