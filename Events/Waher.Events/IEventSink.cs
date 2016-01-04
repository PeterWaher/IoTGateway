using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events
{
	/// <summary>
	/// Interface for all event sinks.
	/// </summary>
	public interface IEventSink : ILogObject
	{
		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		void Queue(Event Event);
	}
}
