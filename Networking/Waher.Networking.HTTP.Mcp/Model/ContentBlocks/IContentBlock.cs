using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Mcp.Model.ContentBlocks
{
	/// <summary>
	/// Interface for MCP Content Block.
	/// </summary>
	public interface IContentBlock 
	{
		/// <summary>
		/// What types the content block encodes.
		/// </summary>
		public abstract Type[] Encodes { get; }

		/// <summary>
		/// If the content block is encoded as structured content.
		/// </summary>
		public bool IsStructuredContent { get; }

		/// <summary>
		/// Encodes a content block, given the content to encode and available
		/// annotations and meta-data.
		/// </summary>
		/// <param name="Content">Content to encode.</param>
		/// <returns>MCP-encoded content block.</returns>
		public abstract Task<Dictionary<string, object?>> Encode(object Content);
	}
}
