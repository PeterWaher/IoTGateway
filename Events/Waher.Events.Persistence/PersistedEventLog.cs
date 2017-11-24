using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;

namespace Waher.Events.Persistence
{
	/// <summary>
	/// Creates an even sink that stores incoming (logged) events in the local object database, as defined by <see cref="Waher.Persistence.Database"/>. 
	/// Event life time in the database is defined in the constructor. Searches can be made for historical events.
	/// </summary>
	public class PersistedEventLog : EventSink
	{
		private Timer timer;
		private int eventLifetimeDays;

		/// <summary>
		/// Creates an even sink that stores incoming (logged) events in the local object database, as defined by <see cref="Waher.Persistence.Database"/>. 
		/// Event life time in the database is defined in the constructor. Searches can be made for historical events.
		/// </summary>
		/// <param name="EventLifetimeDays">Number of days to store events in the database.</param>
		public PersistedEventLog(int EventLifetimeDays)
			: this(EventLifetimeDays, null)
		{
		}

		/// <summary>
		/// Creates an even sink that stores incoming (logged) events in the local object database, as defined by <see cref="Waher.Persistence.Database"/>. 
		/// Event life time in the database is defined in the constructor. Searches can be made for historical events.
		/// </summary>
		/// <param name="EventLifetimeDays">Number of days to store events in the database.</param>
		/// <param name="CleanupTime">Optional time of day when event log cleaning is to occur.</param>
		public PersistedEventLog(int EventLifetimeDays, TimeSpan? CleanupTime)
			: base("Persisted Event Log")
		{
			if (EventLifetimeDays <= 0)
				throw new ArgumentException("The lifetime must be a positive number of days.", nameof(EventLifetimeDays));

			this.eventLifetimeDays = EventLifetimeDays;

			if (CleanupTime.HasValue)
				this.TurnOnDailyPurge(EventLifetimeDays, CleanupTime.Value);
			else
				this.timer = null;
		}

		/// <summary>
		/// Turns on the daily purge of old events.
		/// </summary>
		/// <param name="EventLifetimeDays">Number of days to store events in the database.</param>
		/// <param name="CleanupTime">Time of day when event log cleaning is to occur.</param>
		public void TurnOnDailyPurge(int EventLifetimeDays, TimeSpan CleanupTime)
		{
			this.TurnOffDailyPurge();

			if (CleanupTime < TimeSpan.Zero || CleanupTime.TotalDays >= 1.0)
				throw new ArgumentException("Invalid time.", nameof(CleanupTime));

			int MillisecondsPerDay = 1000 * 60 * 60 * 24;
			int MsUntilNext = (int)((DateTime.Today.AddDays(1).Add(CleanupTime) - DateTime.Now).TotalMilliseconds + 0.5);

			this.eventLifetimeDays = EventLifetimeDays;
			this.timer = new Timer(this.DoCleanup, null, MsUntilNext, MillisecondsPerDay);
		}

		/// <summary>
		/// Turns off the daily purge of old events.
		/// </summary>
		/// <returns>If it was turned off.</returns>
		public bool TurnOffDailyPurge()
		{
			if (this.timer != null)
			{
				this.timer.Dispose();
				this.timer = null;

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public override void Dispose()
		{
			this.TurnOffDailyPurge();
			base.Dispose();
		}

		private async void DoCleanup(object P)
		{
			try
			{
				await this.DeleteOld(DateTime.Now.AddDays(-this.eventLifetimeDays));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Deletes old events. This method is called once a day automatically. It can also be called manually to delete events at other intervals
		/// or used other time limits.
		/// </summary>
		/// <param name="Limit">Events older than this time stamp (or equal to it) will be deleted.</param>
		/// <returns>Number of events deleted.</returns>
		public async Task<int> DeleteOld(DateTime Limit)
		{
			int NrEvents = 0;

			foreach (PersistedEvent Event in await Database.Find<PersistedEvent>(new FilterFieldLesserOrEqualTo("Timestamp", Limit)))
			{
				await Database.Delete(Event);
				NrEvents++;
			}

			KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[]
			{
				new KeyValuePair<string, object>("Limit", Limit),
				new KeyValuePair<string, object>("NrEvents", NrEvents)
			};

			if (NrEvents == 1)
				Log.Informational("Deleting 1 event from the database.", this.ObjectID, Tags);
			else
				Log.Informational("Deleting " + NrEvents.ToString() + " events from the database.", this.ObjectID, Tags);

			return NrEvents;
		}

		/// <summary>
		/// Gets events between two timepoints, ordered by descending timestamp.
		/// </summary>
		/// <param name="Offset">Offset into result set.</param>
		/// <param name="MaxCount">Maximum number of events to return.</param>
		/// <param name="From">Return events greater than or equal to this timestamp.</param>
		/// <param name="To">Return events lesser than or equal to this timestamp.</param>
		/// <returns></returns>
		public Task<IEnumerable<PersistedEvent>> GetEvents(int Offset, int MaxCount, DateTime From, DateTime To)
		{
			return Database.Find<PersistedEvent>(Offset, MaxCount, new FilterAnd(
				new FilterFieldGreaterOrEqualTo("Timestamp", From),
				new FilterFieldLesserOrEqualTo("Timestamp", To)), "-Timestamp");
		}

		/// <summary>
		/// Gets events of a specific type between two timepoints, ordered by descending timestamp.
		/// </summary>
		/// <param name="Offset">Offset into result set.</param>
		/// <param name="MaxCount">Maximum number of events to return.</param>
		/// <param name="Type">Event Type</param>
		/// <param name="From">Return events greater than or equal to this timestamp.</param>
		/// <param name="To">Return events lesser than or equal to this timestamp.</param>
		/// <returns></returns>
		public Task<IEnumerable<PersistedEvent>> GetEventsOfType(int Offset, int MaxCount, EventType Type, DateTime From, DateTime To)
		{
			return Database.Find<PersistedEvent>(Offset, MaxCount, new FilterAnd(
				new FilterFieldGreaterOrEqualTo("Type", Type),
				new FilterFieldGreaterOrEqualTo("Timestamp", From),
				new FilterFieldLesserOrEqualTo("Timestamp", To)), "-Timestamp");
		}

		/// <summary>
		/// Gets events beloinging to a specific object between two timepoints, ordered by descending timestamp.
		/// </summary>
		/// <param name="Offset">Offset into result set.</param>
		/// <param name="MaxCount">Maximum number of events to return.</param>
		/// <param name="Object">Object identity.</param>
		/// <param name="From">Return events greater than or equal to this timestamp.</param>
		/// <param name="To">Return events lesser than or equal to this timestamp.</param>
		/// <returns></returns>
		public Task<IEnumerable<PersistedEvent>> GetEventsOfObject(int Offset, int MaxCount, string Object, DateTime From, DateTime To)
		{
			return Database.Find<PersistedEvent>(Offset, MaxCount, new FilterAnd(
				new FilterFieldGreaterOrEqualTo("Object", Object),
				new FilterFieldGreaterOrEqualTo("Timestamp", From),
				new FilterFieldLesserOrEqualTo("Timestamp", To)), "-Timestamp");
		}

		/// <summary>
		/// Gets events relating to a specific actor between two timepoints, ordered by descending timestamp.
		/// </summary>
		/// <param name="Offset">Offset into result set.</param>
		/// <param name="MaxCount">Maximum number of events to return.</param>
		/// <param name="Actor">Actor identity.</param>
		/// <param name="From">Return events greater than or equal to this timestamp.</param>
		/// <param name="To">Return events lesser than or equal to this timestamp.</param>
		/// <returns></returns>
		public Task<IEnumerable<PersistedEvent>> GetEventsOfActor(int Offset, int MaxCount, string Actor, DateTime From, DateTime To)
		{
			return Database.Find<PersistedEvent>(Offset, MaxCount, new FilterAnd(
				new FilterFieldGreaterOrEqualTo("Actor", Actor),
				new FilterFieldGreaterOrEqualTo("Timestamp", From),
				new FilterFieldLesserOrEqualTo("Timestamp", To)), "-Timestamp");
		}

		/// <summary>
		/// Gets events relating to a specific event identity between two timepoints, ordered by descending timestamp.
		/// </summary>
		/// <param name="Offset">Offset into result set.</param>
		/// <param name="MaxCount">Maximum number of events to return.</param>
		/// <param name="EventId">Event identity.</param>
		/// <param name="From">Return events greater than or equal to this timestamp.</param>
		/// <param name="To">Return events lesser than or equal to this timestamp.</param>
		/// <returns></returns>
		public Task<IEnumerable<PersistedEvent>> GetEventsOfEventId(int Offset, int MaxCount, string EventId, DateTime From, DateTime To)
		{
			return Database.Find<PersistedEvent>(Offset, MaxCount, new FilterAnd(
				new FilterFieldGreaterOrEqualTo("EventId", EventId),
				new FilterFieldGreaterOrEqualTo("Timestamp", From),
				new FilterFieldLesserOrEqualTo("Timestamp", To)), "-Timestamp");
		}

		/// <summary>
		/// Gets events relating to a specific facility between two timepoints, ordered by descending timestamp.
		/// </summary>
		/// <param name="Offset">Offset into result set.</param>
		/// <param name="MaxCount">Maximum number of events to return.</param>
		/// <param name="Facility">Facility identity.</param>
		/// <param name="From">Return events greater than or equal to this timestamp.</param>
		/// <param name="To">Return events lesser than or equal to this timestamp.</param>
		/// <returns></returns>
		public Task<IEnumerable<PersistedEvent>> GetEventsOfFacility(int Offset, int MaxCount, string Facility, DateTime From, DateTime To)
		{
			return Database.Find<PersistedEvent>(Offset, MaxCount, new FilterAnd(
				new FilterFieldGreaterOrEqualTo("Facility", Facility),
				new FilterFieldGreaterOrEqualTo("Timestamp", From),
				new FilterFieldLesserOrEqualTo("Timestamp", To)), "-Timestamp");
		}

		/// <summary>
		/// Number of days to store events in the database.
		/// </summary>
		public int EventLifetimeDays
		{
			get { return this.eventLifetimeDays; }
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override async void Queue(Event Event)
		{
			try
			{
				PersistedEvent PersistedEvent = new PersistedEvent(Event);
				await Database.Insert(PersistedEvent);
			}
			catch (Exception ex)
			{
				Event Event2 = new Event(DateTime.Now, EventType.Critical, ex.Message, this.ObjectID, string.Empty, string.Empty, EventLevel.Major,
					string.Empty, ex.Source, ex.StackTrace);

				Event2.Avoid(this);

				Log.Event(Event2);
			}
		}
	}
}
