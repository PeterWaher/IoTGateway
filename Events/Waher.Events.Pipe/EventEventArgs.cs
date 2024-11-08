using System;

namespace Waher.Events.Pipe
{
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
