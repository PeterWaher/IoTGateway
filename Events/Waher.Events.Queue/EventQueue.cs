using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Events.Queue
{
	/// <summary>
	/// Creates an even sink that queues incoming events in a local persisted queue, for
	/// processing by in order of arrival by another party.
	/// </summary>
	public class EventQueue : EventSink
	{
		private readonly string queueName;
		private IPersistedQueue queue = null;
		private bool errorState = false;

		/// <summary>
		/// Creates an even sink that queues incoming events in a local persisted queue, for
		/// processing by in order of arrival by another party.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="QueueName">Name of queue.</param>
		public EventQueue(string ObjectID, string QueueName)
			: base(ObjectID)
		{
			this.queueName = QueueName;
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync()"/>
		/// </summary>
		public override async Task DisposeAsync()
		{
			if (!(this.queue is null))
			{
				await this.queue.DisposeAsync();
				this.queue = null;
			}

			await base.DisposeAsync();
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override async Task Queue(Event Event)
		{
			if (this.queue is null)
				this.queue = await Database.GetQueue(this.queueName);

			QueuedEvent Item = new QueuedEvent(Event);

			if (await this.queue.Enqueue(Item, this.errorState ? 0 : 10000))
				this.errorState = false;
			else if (!this.errorState)
			{
				this.errorState = true;

				Event Error = new Event(EventType.Emergency, "Event Queue " + this.queueName +
					" is full and new events cannot be queued.", this.ObjectID, string.Empty,
					string.Empty, EventLevel.Major, string.Empty,
					typeof(EventQueue).Namespace, string.Empty);

				Error.Avoid(this);

				Log.Event(Error);
			}
		}

		/// <summary>
		/// Clears the queue.
		/// </summary>
		public async Task ClearQueue()
		{
			if (this.queue is null)
				this.queue = await Database.GetQueue(this.queueName);

			await this.queue.Clear();
		}
	}
}
