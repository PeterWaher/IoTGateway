using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events
{
	/// <summary>
	/// Base class for event sinks.
	/// </summary>
	public abstract class EventSink : LogObject, IEventSink 
	{
		/// <summary>
		/// Base class for event sinks.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		public EventSink(string ObjectID)
			: base(ObjectID)
		{
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public abstract void Queue(Event Event);
	}
}
