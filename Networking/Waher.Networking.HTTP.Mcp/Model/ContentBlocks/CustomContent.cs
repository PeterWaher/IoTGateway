using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Binary;
using Waher.Runtime.IO;

namespace Waher.Networking.HTTP.Mcp.Model.ContentBlocks
{
	/// <summary>
	/// Custom Content Block
	/// </summary>
	public class CustomContent : ContentBlock 
	{
		/// <summary>
		/// Custom Content Block
		/// </summary>
		public CustomContent()
			: base()
		{
		}

		/// <summary>
		/// Custom Content Block
		/// </summary>
		public CustomContent(McpRole[]? Audience, double? Priority, DateTime? LastModified,
			Dictionary<string, object>? MetaData)
			: base(Audience, Priority, LastModified, MetaData)
		{
		}

		/// <summary>
		/// What types the content block encodes.
		/// </summary>
		public override Type[] Encodes => new Type[] 
		{ 
			typeof(CustomEncoding)
		};

		/// <summary>
		/// If the content block is encoded as structured content.
		/// </summary>
		public override bool IsStructuredContent => false;

		/// <summary>
		/// Encodes a content block, given the content to encode and available
		/// annotations and meta-data.
		/// </summary>
		/// <param name="Content">Content to encode.</param>
		/// <returns>MCP-encoded content block.</returns>
		public override Task<Dictionary<string, object?>> Encode(object Content)
		{
			Dictionary<string, object?> Result = new Dictionary<string, object?>();
			CustomEncoding Encoded = (CustomEncoding)Content;

			if (Encoded.ContentType.StartsWith("text/plain", StringComparison.InvariantCultureIgnoreCase))
			{
				Result["type"] = "text";
				Result["text"] = Strings.GetString(Encoded.Encoded, Encoding.UTF8);
			}
			else if (Encoded.ContentType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
			{
				Result["type"] = "image";
				Result["mimeType"] = Encoded.ContentType;
				Result["data"] = Convert.ToBase64String(Encoded.Encoded);
			}
			else if (Encoded.ContentType.StartsWith("audio/", StringComparison.InvariantCultureIgnoreCase))
			{
				Result["type"] = "audio";
				Result["mimeType"] = Encoded.ContentType;
				Result["data"] = Convert.ToBase64String(Encoded.Encoded);
			}
			else
			{
				Result["type"] = "resource";
				Result["resource"] = new Dictionary<string, object>()
				{
					{ "uri", Encoded.Uri?.OriginalString ?? string.Empty },
					{ "mimeType", Encoded.ContentType },
					{ "blob", Convert.ToBase64String(Encoded.Encoded) }
				};
			}

			this.Annotate(Result);

			return Task.FromResult(Result);
		}
	}
}
