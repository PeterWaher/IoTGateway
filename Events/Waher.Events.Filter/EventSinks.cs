using System.Threading.Tasks;

namespace Waher.Events.Filter
{
	/// <summary>
	/// Sends logged events to a collection of event sinks.
	/// </summary>
	public class EventSinks : EventSink
	{
		private readonly IEventSink[] sinks;
		private readonly int nrSinks;

		/// <summary>
		/// Sends logged events to a collection of event sinks.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="Sinks">Collection of event sinks.</param>
		public EventSinks(string ObjectID, params IEventSink[] Sinks)
			: base(ObjectID)
		{
			this.sinks = Sinks;
			this.nrSinks = this.sinks?.Length ?? 0;
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			Task[] Tasks = new Task[this.nrSinks];
			int i;

			for (i = 0; i < this.nrSinks; i++)
				Tasks[i] = this.sinks[i].Queue(Event);

			return Task.WhenAll(Tasks);
		}
	}
}
