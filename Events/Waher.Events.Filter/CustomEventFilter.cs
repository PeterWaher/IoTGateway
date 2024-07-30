namespace Waher.Events.Filter
{
	/// <summary>
	/// Delegate for custom event filters.
	/// </summary>
	/// <param name="Event"></param>
	/// <returns></returns>
	public delegate bool CustomEventFilterDelegate(Event Event);

	/// <summary>
	/// Implements a custom event filter based on a delegate.
	/// </summary>
	public class CustomEventFilter : ICustomEventFilter
	{
		private readonly CustomEventFilterDelegate callback;

		/// <summary>
		/// Implements a custom event filter based on a delegate.
		/// </summary>
		/// <param name="Callback">Method called when an event is being evaluated.</param>
		public CustomEventFilter(CustomEventFilterDelegate Callback)
		{
			this.callback = Callback;
		}

		/// <summary>
		/// Converts a delegate to a custom event filter.
		/// </summary>
		/// <param name="Callback">Callback method.</param>
		public static implicit operator CustomEventFilter(CustomEventFilterDelegate Callback)
		{
			return new CustomEventFilter(Callback);
		}

		/// <summary>
		/// Checks if an event is allowed.
		/// </summary>
		/// <param name="Event">Event to check.</param>
		/// <returns>If event is allowed.</returns>
		public bool IsAllowed(Event Event)
		{
			return this.callback(Event);
		}
	}
}
