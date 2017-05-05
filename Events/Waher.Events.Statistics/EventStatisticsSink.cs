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
					LastStat = this.lastStat,
					CurrentStat = TP
				};
			}

			return Result;
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override void Queue(Event Event)
		{
			lock (this.synchObj)
			{
				this.IncTypeLocked(Event.Type);
				this.IncLevelLocked(Event.Level);
				this.IncActorLocked(Event.Actor);
				this.IncEventIdLocked(Event.EventId);
				this.IncFacilityLocked(Event.Facility);
				this.IncModuleLocked(Event.Module);
			}
		}

		private void IncActorLocked(string Actor)
		{
			if (!string.IsNullOrEmpty(Actor))
				this.IncLocked(Actor, this.perActor);
		}

		private void IncEventIdLocked(string EventId)
		{
			if (!string.IsNullOrEmpty(EventId))
				this.IncLocked(EventId, this.perEventId);
		}

		private void IncFacilityLocked(string Facility)
		{
			if (!string.IsNullOrEmpty(Facility))
				this.IncLocked(Facility, this.perFacility);
		}

		private void IncModuleLocked(string Module)
		{
			if (!string.IsNullOrEmpty(Module))
				this.IncLocked(Module, this.perModule);
		}

		private void IncLevelLocked(EventLevel Level)
		{
			this.IncLocked(Level.ToString(), this.perLevel);
		}

		private void IncTypeLocked(EventType Type)
		{
			this.IncLocked(Type.ToString(), this.perType);
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
