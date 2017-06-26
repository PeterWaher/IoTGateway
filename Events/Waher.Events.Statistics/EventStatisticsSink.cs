using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Events.Statistics
{
	/// <summary>
	/// Calculates statistics on incoming events.
	/// </summary>
	public class EventStatisticsSink : EventSink
	{
		private Dictionary<string, Statistic> perActor = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perEventId = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perFacility = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perModule = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perLevel = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perType = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perStackTrace = new Dictionary<string, Statistic>();
		private DateTime lastStat = DateTime.MinValue;
		private object synchObj = new object();

		/// <summary>
		/// Calculates statistics on incoming events.
		/// </summary>
		/// <param name="ObjectID">Object ID of event sink.</param>
		public EventStatisticsSink(string ObjectID)
			: base(ObjectID)
		{
			this.lastStat = DateTime.Now;
		}

		/// <summary>
		/// Gets statistics of events logged since last call to <see cref="GetStatisticsSinceLast"/>.
		/// </summary>
		/// <returns>Event statistics.</returns>
		public EventStatistics GetStatisticsSinceLast()
		{
			EventStatistics Result;
			DateTime TP = DateTime.Now;

			lock (this.synchObj)
			{
				Result = new EventStatistics()
				{
					PerActor = this.perActor,
					PerEventId = this.perEventId,
					PerFacility = this.perFacility,
					PerModule = this.perModule,
					PerLevel = this.perLevel,
					PerType = this.perType,
					PerStackTrace = this.perStackTrace,
					LastStat = this.lastStat,
					CurrentStat = TP
				};

				this.perActor = new Dictionary<string, Statistic>();
				this.perEventId = new Dictionary<string, Statistic>();
				this.perFacility = new Dictionary<string, Statistic>();
				this.perModule = new Dictionary<string, Statistic>();
				this.perLevel = new Dictionary<string, Statistic>();
				this.perType = new Dictionary<string, Statistic>();
				this.perStackTrace = new Dictionary<string, Statistic>();
				this.lastStat = TP;
			}

			return Result;
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override void Queue(Event Event)
		{
			string s;

			lock (this.synchObj)
			{
				this.IncLocked(Event.Type.ToString(), this.perType);
				this.IncLocked(Event.Level.ToString(), this.perLevel);

				if (!string.IsNullOrEmpty(s = Event.Actor))
					this.IncLocked(s, this.perActor);

				if (!string.IsNullOrEmpty(s = Event.EventId))
					this.IncLocked(s, this.perEventId);

				if (!string.IsNullOrEmpty(s = Event.Facility))
					this.IncLocked(s, this.perFacility);

				if (!string.IsNullOrEmpty(s = Event.Module))
					this.IncLocked(s, this.perModule);

				if ((Event.Type == EventType.Critical || Event.Type == EventType.Alert || Event.Type == EventType.Emergency) &&
					!string.IsNullOrEmpty(Event.StackTrace))
				{
					this.IncLocked(Event.StackTrace, this.perStackTrace);
				}
			}
		}

		private void IncLocked(string Key, Dictionary<string, Statistic> PerKey)
		{
			if (PerKey.TryGetValue(Key, out Statistic Nr))
				Nr.Inc();
			else
				PerKey[Key] = new Statistic(1);
		}
	}
}
