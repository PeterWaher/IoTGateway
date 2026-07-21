using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Resources
{
	/// <summary>
	/// Interface for MCP Resource Content.
	/// </summary>
	public interface IResourceContent
	{
		/// <summary>
		/// Encodes a resource content.
		/// </summary>
		/// <returns>MCP-encoded content block.</returns>
		public abstract Dictionary<string, object> Encode();
	}
}
