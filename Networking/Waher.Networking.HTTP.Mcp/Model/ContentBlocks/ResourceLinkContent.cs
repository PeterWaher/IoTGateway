using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;

namespace Waher.Networking.HTTP.Mcp.Model.ContentBlocks
{
	/// <summary>
	/// Resource Link Content Block
	/// </summary>
	public class ResourceLinkContent : ContentBlock 
	{
		/// <summary>
		/// Resource Link Content Block
		/// </summary>
		public ResourceLinkContent()
			: base()
		{
		}

		/// <summary>
		/// Resource Link Content Block
		/// </summary>
		public ResourceLinkContent(McpRole[]? Audience, double? Priority, DateTime? LastModified,
			Dictionary<string, object>? MetaData)
			: base(Audience, Priority, LastModified, MetaData)
		{
		}

		/// <summary>
		/// What types the content block encodes.
		/// </summary>
		public override Type[] Encodes => new Type[] 
		{ 
			typeof(Uri)
		};

		/// <summary>
		/// Encodes a content block, given the content to encode and available
		/// annotations and meta-data.
		/// </summary>
		/// <param name="Content">Content to encode.</param>
		/// <returns>MCP-encoded content block.</returns>
		public override Task<Dictionary<string, object?>> Encode(object Content)
		{
			// TODO: Resource-object
			// TODO: EmbeddedResource

			Uri Uri = (Uri)Content;
			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "type", "resource_link" },
				{ "uri", Uri.ToString() }
			};

			this.Annotate(Result);

			return Task.FromResult(Result);
		}
	}
}
