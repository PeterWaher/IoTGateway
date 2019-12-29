using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Security;

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
		private string defaultFacility;
		private string defaultFacilityDigest = null;

		/// <summary>
		/// Creates an even sink that stores incoming (logged) events in the local object database, as defined by <see cref="Waher.Persistence.Database"/>. 
		/// Event life time in the database is defined in the constructor. Searches can be made for historical events.
		/// </summary>
		/// <param name="EventLifetimeDays">Number of days to store events in the database.</param>
		public PersistedEventLog(int EventLifetimeDays)
			: this(EventLifetimeDays, null, string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Creates an even sink that stores incoming (logged) events in the local object database, as defined by <see cref="Waher.Persistence.Database"/>. 
		/// Event life time in the database is defined in the constructor. Searches can be made for historical events.
		/// </summary>
		/// <param name="EventLifetimeDays">Number of days to store events in the database.</param>
		/// <param name="CleanupTime">Optional time of day when event log cleaning is to occur.</param>
		public PersistedEventLog(int EventLifetimeDays, TimeSpan? CleanupTime)
			: this(EventLifetimeDays, CleanupTime, string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Creates an even sink that stores incoming (logged) events in the local object database, as defined by <see cref="Waher.Persistence.Database"/>. 
		/// Event life time in the database is defined in the constructor. Searches can be made for historical events.
		/// </summary>
		/// <param name="EventLifetimeDays">Number of days to store events in the database.</param>
		/// <param name="CleanupTime">Optional time of day when event log cleaning is to occur.</param>
		/// <param name="DefaultFacility">Facility to use, if none is explicitly used in events being logged.</param>
		/// <param name="DefaultFacilityKey">Key necessary to update the default facility using <see cref="SetDefaultFacility(string, string)"/>.</param>
		public PersistedEventLog(int EventLifetimeDays, TimeSpan? CleanupTime, string DefaultFacility, string DefaultFacilityKey)
			: base("Persisted Event Log")
		{
			if (EventLifetimeDays <= 0)
				throw new ArgumentOutOfRangeException("The lifetime must be a positive number of days.", nameof(EventLifetimeDays));

			this.eventLifetimeDays = EventLifetimeDays;
			this.defaultFacility = DefaultFacility;
			this.defaultFacilityDigest = this.ComputeDigest(DefaultFacilityKey);

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
				throw new ArgumentOutOfRangeException("Invalid time.", nameof(CleanupTime));

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
			bool Deleted;

			do
			{
				Deleted = false;

				foreach (PersistedEvent Event in await Database.Find<PersistedEvent>(0, 100, new FilterFieldLesserOrEqualTo("Timestamp", Limit)))
				{
					await Database.Delete(Event);
					NrEvents++;
					Deleted = true;
				}
			}
			while (Deleted);

			if (NrEvents > 0)
			{
				KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("Limit", Limit),
					new KeyValuePair<string, object>("NrEvents", NrEvents)
				};

				if (NrEvents == 1)
					Log.Informational("Deleting 1 event from the database.", this.ObjectID, Tags);
				else
					Log.Informational("Deleting " + NrEvents.ToString() + " events from the database.", this.ObjectID, Tags);
			}

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
		public override async Task Queue(Event Event)
		{
			PersistedEvent PersistedEvent = new PersistedEvent(Event);

			if (string.IsNullOrEmpty(PersistedEvent.Facility))
				PersistedEvent.Facility = this.defaultFacility;

			await Database.Insert(PersistedEvent);
		}

		/// <summary>
		/// Sets the default facility. The default facility can only be reset by a caller presenting the same key as the first time
		/// the default facility was set.
		/// </summary>
		/// <param name="DefaultFacility">Default facility.</param>
		/// <param name="DefaultFacilityKey">Key necessary to update the default facility.</param>
		/// <exception cref="UnauthorizedAccessException">If trying to change the default facility.</exception>
		public void SetDefaultFacility(string DefaultFacility, string DefaultFacilityKey)
		{
			if (this.defaultFacility != DefaultFacility)
			{
				if (!string.IsNullOrEmpty(this.defaultFacility))
				{
					if (this.ComputeDigest(DefaultFacilityKey) != this.defaultFacilityDigest)
						throw new UnauthorizedAccessException("Unauthorized to change the default facility.");
				}

				this.defaultFacility = DefaultFacility;
				this.defaultFacilityDigest = this.ComputeDigest(DefaultFacilityKey);
			}
		}

		private string ComputeDigest(string Key)
		{
			return Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(Key + ":" + this.defaultFacility));
		}

		internal static int ArchiveDays
		{
			get
			{
				if (registeredLog is null)
				{
					foreach (IEventSink Sink in Log.Sinks)
					{
						if (Sink is PersistedEventLog PersistedEventLog)
						{
							registeredLog = PersistedEventLog;
							break;
						}
					}

					if (registeredLog is null)
						return 90;
				}

				return registeredLog.eventLifetimeDays;
			}
		}

		private static PersistedEventLog registeredLog = null;
	}
}
