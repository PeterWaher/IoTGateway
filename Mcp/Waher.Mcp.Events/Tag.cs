using Waher.Events.Persistence;
using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Events
{
	/// <summary>
	/// Information about a tag in an event.
	/// </summary>
	public class Tag
	{
		/// <summary>
		/// Information about an event in the event log.
		/// </summary>
		/// <param name="Tag">Persisted tag.</param>
		public Tag(PersistedTag Tag)
		{
			this.Name = Tag.Name;
			this.Value = Tag.Value;
		}

		[McpParameter("Name", "Name of tag")]
		public string Name { get; }

		[McpParameter("Value", "Value of tag")]
		public object Value { get; }
	}
}
