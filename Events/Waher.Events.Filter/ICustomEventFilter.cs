namespace Waher.Events.Filter
{
	/// <summary>
	/// Interface for custom event filters.
	/// </summary>
	public interface ICustomEventFilter
	{
		/// <summary>
		/// Checks if an event is allowed.
		/// </summary>
		/// <param name="Event">Event to check.</param>
		/// <returns>If event is allowed.</returns>
		bool IsAllowed(Event Event);
	}
}
