using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Mcp.Model.ContentBlocks
{
	/// <summary>
	/// Abstract base class for an MCP Content Block.
	/// </summary>
	public abstract class ContentBlock : Annotations, IContentBlock
	{
		/// <summary>
		/// Abstract base class for an MCP Content Block.
		/// </summary>
		public ContentBlock()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for an MCP Content Block.
		/// </summary>
		public ContentBlock(McpRole[]? Audience, double? Priority, DateTime? LastModified,
			Dictionary<string, object>? MetaData)
			: base(Audience, Priority, LastModified)
		{
			this.MetaData = MetaData;
		}

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<string, object>? MetaData { get; private set; }

		/// <summary>
		/// What types the content block encodes.
		/// </summary>
		public abstract Type[] Encodes { get; }

		/// <summary>
		/// Encodes a content block, given the content to encode and available
		/// annotations and meta-data.
		/// </summary>
		/// <param name="Content">Content to encode.</param>
		/// <returns>MCP-encoded content block.</returns>
		public abstract Task<Dictionary<string, object?>> Encode(object Content);

		/// <summary>
		/// Annotates an object.
		/// </summary>
		/// <param name="Object">Object to annotate.</param>
		public override void Annotate(Dictionary<string, object?> Object)
		{
			base.Annotate(Object);

			if (!(this.MetaData is null))
				Object["_meta"] = this.MetaData;
		}
	}
}
