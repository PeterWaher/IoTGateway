using System;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Data Source event types
	/// </summary>
	[Flags]
    public enum SourceEventType
    {
		/// <summary>
		/// Event raised when a node has been added to the data source.
		/// </summary>
		NodeAdded = 1,

		/// <summary>
		/// Event raised when a node has been updated in the data source.
		/// </summary>
		NodeUpdated = 2,

		/// <summary>
		/// Event raised when the status of a node in the data source changed.
		/// </summary>
		NodeStatusChanged = 4,

		/// <summary>
		/// Event raised when a node has been removed from the data source.
		/// </summary>
		NodeRemoved = 8,

		/// <summary>
		/// Event raised when an ordered node has been moved up one step.
		/// </summary>
		NodeMovedUp = 16,

		/// <summary>
		/// Event raised when an ordered node has been moved down one step.
		/// </summary>
		NodeMovedDown = 32,

		/// <summary>
		/// No events
		/// </summary>
		None = 0,

		/// <summary>
		/// All events
		/// </summary>
		All = 63
    }
}
