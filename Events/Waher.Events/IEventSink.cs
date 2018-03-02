using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
		Task Queue(Event Event);
	}
}
