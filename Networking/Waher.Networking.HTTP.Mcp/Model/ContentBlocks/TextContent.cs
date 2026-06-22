using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Networking.HTTP.Mcp.Model.ContentBlocks
{
	/// <summary>
	/// Text Content Block
	/// </summary>
	public class TextContent : ContentBlock 
	{
		/// <summary>
		/// Text Content Block
		/// </summary>
		public TextContent()
			: base()
		{
		}

		/// <summary>
		/// Text Content Block
		/// </summary>
		public TextContent(McpRole[]? Audience, double? Priority, DateTime? LastModified,
			Dictionary<string, object>? MetaData)
			: base(Audience, Priority, LastModified, MetaData)
		{
		}

		/// <summary>
		/// What types the content block encodes.
		/// </summary>
		public override Type[] Encodes => new Type[] 
		{ 
			typeof(string),
			typeof(CaseInsensitiveString)
		};

		/// <summary>
		/// Encodes a content block, given the content to encode and available
		/// annotations and meta-data.
		/// </summary>
		/// <param name="Content">Content to encode.</param>
		/// <returns>MCP-encoded content block.</returns>
		public override Task<Dictionary<string, object?>> Encode(object Content)
		{
			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "type", "text" },
				{ "text", Content?.ToString() }
			};

			this.Annotate(Result);

			return Task.FromResult(Result);
		}
	}
}
