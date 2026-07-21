using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Resources
{
	/// <summary>
	/// Text resource content
	/// </summary>
	public class TextContent : IResourceContent
	{
		private readonly Uri uri;
		private readonly string text;
		private readonly string? contentType;
		private readonly Dictionary<string, object>? metaData;

		/// <summary>
		/// Text resource content
		/// </summary>
		/// <summary>
		/// BLOB resource content
		/// </summary>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="Text">Text content of the resource.</param>
		/// <param name="ContentType">Content-Type of resource.</param>
		/// <param name="MetaData">Optional associated meta-data.</param>
		public TextContent(Uri Uri, string Text, string? ContentType,
			Dictionary<string, object>? MetaData)
		{
			this.uri = Uri;
			this.text = Text;
			this.contentType = ContentType;
			this.metaData = MetaData;
		}

		/// <summary>
		/// Encodes a resource content.
		/// </summary>
		/// <returns>MCP-encoded content block.</returns>
		public Dictionary<string, object> Encode()
		{
			Dictionary<string, object> Result = new Dictionary<string, object>
			{
				{ "uri", this.uri.OriginalString },
				{ "text", this.text }
			};

			if (!string.IsNullOrEmpty(this.contentType))
				Result["mimeType"] = this.contentType;

			if (!(this.metaData is null))
				Result["_meta"] = this.metaData;

			return Result;
		}
	}
}
