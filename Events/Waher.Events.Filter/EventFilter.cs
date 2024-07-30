using System.Threading.Tasks;

namespace Waher.Events.Filter
{
	/// <summary>
	/// Filters incoming events and passes remaining events to a secondary event sink.
	/// </summary>
	public class EventFilter : EventSink, ICustomEventFilter
	{
		private readonly IEventSink sink;
		private readonly int nrCustomFilters;
		private readonly ICustomEventFilter[] customEventFilters;

		/// <summary>
		/// Filters incoming events and passes remaining events to a secondary event sink.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="EventSink">Secondary event sink that will receive events that pass the filter.</param>
		/// <param name="FromType">From what event type events are allowed.</param>
		/// <param name="CustomFilter">Callback method implementing a custom event filter.</param>
		public EventFilter(string ObjectID, IEventSink EventSink, EventType FromType, CustomEventFilterDelegate CustomFilter)
			: this(ObjectID, EventSink, FromType, (CustomEventFilter)CustomFilter)
		{
		}

		/// <summary>
		/// Filters incoming events and passes remaining events to a secondary event sink.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="EventSink">Secondary event sink that will receive events that pass the filter.</param>
		/// <param name="FromType">From what event type events are allowed.</param>
		/// <param name="CustomEventFilters">Custom event filters.</param>
		public EventFilter(string ObjectID, IEventSink EventSink, EventType FromType, params ICustomEventFilter[] CustomEventFilters)
			: this(ObjectID, EventSink, FromType <= EventType.Debug, FromType <= EventType.Informational,
				  FromType <= EventType.Notice, FromType <= EventType.Warning, FromType <= EventType.Error,
				  FromType <= EventType.Critical, FromType <= EventType.Alert, FromType <= EventType.Emergency, CustomEventFilters)
		{
		}

		/// <summary>
		/// Filters incoming events and passes remaining events to a secondary event sink.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="EventSink">Secondary event sink that will receive events that pass the filter.</param>
		/// <param name="AllowDebug">What levels of debug events are allowed.</param>
		/// <param name="AllowInformational">What levels of informational events are allowed.</param>
		/// <param name="AllowNotice">What levels of notice events are allowed.</param>
		/// <param name="AllowWarning">What levels of warning events are allowed.</param>
		/// <param name="AllowError">What levels of error events are allowed.</param>
		/// <param name="AllowCritical">What levels of critical events are allowed.</param>
		/// <param name="AllowAlert">What levels of alert events are allowed.</param>
		/// <param name="AllowEmergency">What levels of emergency events are allowed.</param>param>
		/// <param name="CustomEventFilters">Custom event filters.</param>
		public EventFilter(string ObjectID, IEventSink EventSink,
			bool AllowDebug, bool AllowInformational, bool AllowNotice, bool AllowWarning,
			bool AllowError, bool AllowCritical, bool AllowAlert, bool AllowEmergency, params ICustomEventFilter[] CustomEventFilters)
			: this(ObjectID, EventSink, (EventLevelFilter)AllowDebug, (EventLevelFilter)AllowInformational,
				  (EventLevelFilter)AllowNotice, (EventLevelFilter)AllowWarning, (EventLevelFilter)AllowError,
				  (EventLevelFilter)AllowCritical, (EventLevelFilter)AllowAlert, (EventLevelFilter)AllowEmergency, CustomEventFilters)
		{
		}

		/// <summary>
		/// Filters incoming events and passes remaining events to a secondary event sink.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="EventSink">Secondary event sink that will receive events that pass the filter.</param>
		/// <param name="AllowDebug">What levels of debug events are allowed.</param>
		/// <param name="AllowInformational">What levels of informational events are allowed.</param>
		/// <param name="AllowNotice">What levels of notice events are allowed.</param>
		/// <param name="AllowWarning">What levels of warning events are allowed.</param>
		/// <param name="AllowError">What levels of error events are allowed.</param>
		/// <param name="AllowCritical">What levels of critical events are allowed.</param>
		/// <param name="AllowAlert">What levels of alert events are allowed.</param>
		/// <param name="AllowEmergency">What levels of emergency events are allowed.</param>param>
		/// <param name="CustomEventFilters">Custom event filters.</param>
		public EventFilter(string ObjectID, IEventSink EventSink,
			EventLevel AllowDebug, EventLevel AllowInformational, EventLevel AllowNotice, EventLevel AllowWarning,
			EventLevel AllowError, EventLevel AllowCritical, EventLevel AllowAlert, EventLevel AllowEmergency, 
			params ICustomEventFilter[] CustomEventFilters)
			: this(ObjectID, EventSink, (EventLevelFilter)AllowDebug, (EventLevelFilter)AllowInformational,
				  (EventLevelFilter)AllowNotice, (EventLevelFilter)AllowWarning, (EventLevelFilter)AllowError,
				  (EventLevelFilter)AllowCritical, (EventLevelFilter)AllowAlert, (EventLevelFilter)AllowEmergency, CustomEventFilters)
		{
		}

		/// <summary>
		/// Filters incoming events and passes remaining events to a secondary event sink.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="EventSink">Secondary event sink that will receive events that pass the filter.</param>
		/// <param name="AllowDebug">What levels of debug events are allowed.</param>
		/// <param name="AllowInformational">What levels of informational events are allowed.</param>
		/// <param name="AllowNotice">What levels of notice events are allowed.</param>
		/// <param name="AllowWarning">What levels of warning events are allowed.</param>
		/// <param name="AllowError">What levels of error events are allowed.</param>
		/// <param name="AllowCritical">What levels of critical events are allowed.</param>
		/// <param name="AllowAlert">What levels of alert events are allowed.</param>
		/// <param name="AllowEmergency">What levels of emergency events are allowed.</param>param>
		/// <param name="CustomEventFilters">Custom event filters.</param>
		public EventFilter(string ObjectID, IEventSink EventSink,
			EventLevelFilter AllowDebug, EventLevelFilter AllowInformational, EventLevelFilter AllowNotice, EventLevelFilter AllowWarning,
			EventLevelFilter AllowError, EventLevelFilter AllowCritical, EventLevelFilter AllowAlert, EventLevelFilter AllowEmergency, 
			params ICustomEventFilter[] CustomEventFilters)
			: base(ObjectID)
		{
			this.sink = EventSink;
			this.AllowDebug = AllowDebug;
			this.AllowInformational = AllowInformational;
			this.AllowNotice = AllowNotice;
			this.AllowWarning = AllowWarning;
			this.AllowError = AllowError;
			this.AllowCritical = AllowCritical;
			this.AllowAlert = AllowAlert;
			this.AllowEmergency = AllowEmergency;

			this.customEventFilters = CustomEventFilters;
			this.nrCustomFilters = this.customEventFilters?.Length ?? 0;
		}

		/// <summary>
		/// What levels of debug events are allowed.
		/// </summary>
		public EventLevelFilter AllowDebug { get; set; }

		/// <summary>
		/// What levels of informational events are allowed.
		/// </summary>
		public EventLevelFilter AllowInformational { get; set; }

		/// <summary>
		/// What levels of notice events are allowed.
		/// </summary>
		public EventLevelFilter AllowNotice { get; set; }

		/// <summary>
		/// What levels of warning events are allowed.
		/// </summary>
		public EventLevelFilter AllowWarning { get; set; }

		/// <summary>
		/// What levels of error events are allowed.
		/// </summary>
		public EventLevelFilter AllowError { get; set; }

		/// <summary>
		/// What levels of critical events are allowed.
		/// </summary>
		public EventLevelFilter AllowCritical { get; set; }

		/// <summary>
		/// What levels of alert events are allowed.
		/// </summary>
		public EventLevelFilter AllowAlert { get; set; }

		/// <summary>
		/// What levels of emergency events are allowed.
		/// </summary>
		public EventLevelFilter AllowEmergency { get; set; }

		/// <summary>
		/// Checks if an event is allowed.
		/// </summary>
		/// <param name="Event">Event to check.</param>
		/// <returns>If event is allowed.</returns>
		public bool IsAllowed(Event Event)
		{
			EventLevelFilter TypeFilter;

			switch (Event.Type)
			{
				case EventType.Debug:
					TypeFilter = this.AllowDebug;
					break;

				case EventType.Informational:
					TypeFilter = this.AllowInformational;
					break;

				case EventType.Notice:
					TypeFilter = this.AllowNotice;
					break;

				case EventType.Warning:
					TypeFilter = this.AllowWarning;
					break;

				case EventType.Error:
					TypeFilter = this.AllowError;
					break;

				case EventType.Critical:
					TypeFilter = this.AllowCritical;
					break;

				case EventType.Alert:
					TypeFilter = this.AllowAlert;
					break;

				case EventType.Emergency:
					TypeFilter = this.AllowEmergency;
					break;

				default:
					return false;
			}

			if (!TypeFilter.IsAllowed(Event.Level))
				return false;

			if (this.nrCustomFilters > 0)
			{
				for (int i = 0; i < this.nrCustomFilters; i++)
				{
					if (!this.customEventFilters[i].IsAllowed(Event))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			if (this.IsAllowed(Event))
				return this.sink.Queue(Event);
			else
				return Task.CompletedTask;
		}
	}
}
