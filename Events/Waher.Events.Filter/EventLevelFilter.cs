namespace Waher.Events.Filter
{
	/// <summary>
	/// Allows events from a certain level.
	/// </summary>
	public enum FromEventLevel
	{
		/// <summary>
		/// Allows Minor, Medium, and Major events of a given type.
		/// </summary>
		Minor,

		/// <summary>
		/// Allows Medium, and Major events of a given type.
		/// </summary>
		Medium,

		/// <summary>
		/// Allows Major events of a given type.
		/// </summary>
		Major,

		/// <summary>
		/// Does not allow events of a given type.
		/// </summary>
		None
	};

	/// <summary>
	/// Filters events based on their <see cref="EventLevel"/> value.
	/// </summary>
	public class EventLevelFilter : ICustomEventFilter
	{
		/// <summary>
		/// Filters events based on their <see cref="EventLevel"/> value.
		/// </summary>
		/// <param name="AllowType">If the type (i.e. all levels) are allowed or not.</param>
		public EventLevelFilter(bool AllowType)
			: this(AllowType, AllowType, AllowType)
		{
		}

		/// <summary>
		/// Filters events based on their <see cref="EventLevel"/> value.
		/// </summary>
		/// <param name="FromLevel">From what event level events are allowed.</param>
		public EventLevelFilter(EventLevel FromLevel)
			: this(FromLevel <= EventLevel.Major, 
				  FromLevel <= EventLevel.Medium, 
				  FromLevel <= EventLevel.Minor)
		{
		}

		/// <summary>
		/// Filters events based on their <see cref="EventLevel"/> value.
		/// </summary>
		/// <param name="FromLevel">From what event level events are allowed.</param>
		public EventLevelFilter(FromEventLevel FromLevel)
			: this(FromLevel <= FromEventLevel.Major, 
				  FromLevel <= FromEventLevel.Medium, 
				  FromLevel <= FromEventLevel.Minor)
		{
		}

		/// <summary>
		/// Filters events based on their <see cref="EventLevel"/> value.
		/// </summary>
		/// <param name="AllowMajor">If major events are allowed.</param>
		/// <param name="AllowMedium">If medium events are allowed.</param>
		/// <param name="AllowMinor">If minor events are allowed.</param>
		public EventLevelFilter(bool AllowMajor, bool AllowMedium, bool AllowMinor)
		{
			this.AllowMajor = AllowMajor;
			this.AllowMedium = AllowMedium;
			this.AllowMinor = AllowMinor;
		}

		/// <summary>
		/// Filters events based on their <see cref="EventLevel"/> value.
		/// </summary>
		/// <param name="AllowType">If the type (i.e. all levels) are allowed or not.</param>
		public static implicit operator EventLevelFilter(bool AllowType)
		{
			return new EventLevelFilter(AllowType);
		}

		/// <summary>
		/// Filters events based on their <see cref="EventLevel"/> value.
		/// </summary>
		/// <param name="FromLevel">From what event level events are allowed.</param>
		public static implicit operator EventLevelFilter(EventLevel FromLevel)
		{
			return new EventLevelFilter(FromLevel);
		}

		/// <summary>
		/// Filters events based on their <see cref="EventLevel"/> value.
		/// </summary>
		/// <param name="FromLevel">From what event level events are allowed.</param>
		public static implicit operator EventLevelFilter(FromEventLevel FromLevel)
		{
			return new EventLevelFilter(FromLevel);
		}

		/// <summary>
		/// If major events are allowed.
		/// </summary>
		public bool AllowMajor { get; set; }

		/// <summary>
		/// If medium events are allowed.
		/// </summary>
		public bool AllowMedium { get; set; }

		/// <summary>
		/// If minor events are allowed.
		/// </summary>
		public bool AllowMinor { get; set; }

		/// <summary>
		/// Checks if an event is allowed.
		/// </summary>
		/// <param name="Event">Event to check.</param>
		/// <returns>If event is allowed.</returns>
		public bool IsAllowed(Event Event)
		{
			return this.IsAllowed(Event.Level);
		}

		/// <summary>
		/// Checks if an event level is allowed.
		/// </summary>
		/// <param name="Level">Event level.</param>
		/// <returns>If allowed.</returns>
		public bool IsAllowed(EventLevel Level)
		{
			switch (Level)
			{
				case EventLevel.Major:
					return this.AllowMajor;

				case EventLevel.Medium:
					return this.AllowMedium;

				case EventLevel.Minor:
					return this.AllowMinor;

				default:
					return false;
			}
		}
	}
}
