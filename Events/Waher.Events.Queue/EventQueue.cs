using System;
using System.Collections.Generic;
using System.Threading;
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
		private Timer timer;
		private int eventLifetimeDays;
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
			: this(ObjectID, QueueName, 0, null)
		{
		}

		/// <summary>
		/// Creates an even sink that queues incoming events in a local persisted queue, for
		/// processing by in order of arrival by another party.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="QueueName">Name of queue.</param>
		/// <param name="EventLifetimeDays">Number of days to store events in the queue maximum.</param>
		/// <param name="CleanupTime">Optional time of day when old events are purged from the queue.</param>
		public EventQueue(string ObjectID, string QueueName, int EventLifetimeDays,
			TimeSpan? CleanupTime)
			: base(ObjectID)
		{
			this.queueName = QueueName;

			if (EventLifetimeDays <= 0 || !CleanupTime.HasValue)
			{
				this.timer = null;
				this.eventLifetimeDays = 0;
			}
			else
			{
				if (CleanupTime.Value < TimeSpan.Zero || CleanupTime.Value.TotalDays >= 1.0)
					throw new ArgumentOutOfRangeException("Invalid time.", nameof(CleanupTime));

				int MillisecondsPerDay = 1000 * 60 * 60 * 24;
				int MsUntilNext = (int)((DateTime.Today.AddDays(1).Add(CleanupTime.Value) - DateTime.Now).TotalMilliseconds + 0.5);

				this.eventLifetimeDays = EventLifetimeDays;
				this.timer = new Timer(this.PurgeOldItems, null, MsUntilNext, MillisecondsPerDay);
			}
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

			this.timer?.Dispose();
			this.timer = null;

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

		private async void PurgeOldItems(object P)
		{
			try
			{
				await this.PurgeOldItems(DateTime.UtcNow.AddDays(-this.eventLifetimeDays));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Purges old events from the queue. This method is called once a day 
		/// automatically. It can also be called manually to delete events at other 
		/// intervals or using other time limits.
		/// </summary>
		/// <param name="Limit">Events older than this time stamp (or equal to it) will be 
		/// purged.</param>
		/// <returns>Number of events purged.</returns>
		public async Task<int> PurgeOldItems(DateTime Limit)
		{
			int NrEvents = 0;
			object Item;

			Item = await this.queue.Peek();

			while (!(Item is null))
			{
				if (Item is QueuedEvent QueuedEvent && QueuedEvent.Timestamp > Limit)
					break;
					
				await this.queue.Dequeue();
				NrEvents++;

				Item = await this.queue.Peek();
			}

			if (NrEvents > 0)
			{
				KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("Limit", Limit),
					new KeyValuePair<string, object>("NrEvents", NrEvents)
				};

				if (NrEvents == 1)
					Log.Informational("Purging 1 event from the queue.", this.ObjectID, Tags);
				else
					Log.Informational("Purging " + NrEvents.ToString() + " events from the queue.", this.ObjectID, Tags);
			}

			return NrEvents;
		}

	}
}
