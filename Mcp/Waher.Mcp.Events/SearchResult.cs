using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Events
{
	/// <summary>
	/// Contains the result of a search for events.
	/// </summary>
	public class SearchResult
	{
		/// <summary>
		/// Contains the result of a search for events.
		/// </summary>
		/// <param name="More">If more events are available.</param>
		/// <param name="NextOffset">Next offset, if more events are available.</param>
		/// <param name="Events">Array of events returned by the search.</param>
		public SearchResult(bool More, int? NextOffset, Event[] Events)
		{
			this.More = More;
			this.NextOffset = NextOffset;
			this.Events = Events;
		}

		[McpParameter("More", "If more events are available.")]
		public bool More { get; }

		[McpIntegerParameter("Next Offset", "Offset into search result set where the next page of events begin, if the More parameter is true.", 0, int.MaxValue)]
		public int? NextOffset { get; }

		[McpParameter("Events", "Array of events returned by the search.")]
		public Event[] Events { get; }
	}
}
