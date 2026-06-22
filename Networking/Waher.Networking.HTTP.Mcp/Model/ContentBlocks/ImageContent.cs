using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Images;
using Waher.Script.Graphs;

namespace Waher.Networking.HTTP.Mcp.Model.ContentBlocks
{
	/// <summary>
	/// Image Content Block
	/// </summary>
	public class ImageContent : ContentBlock 
	{
		/// <summary>
		/// Image Content Block
		/// </summary>
		public ImageContent()
			: base()
		{
		}

		/// <summary>
		/// Image Content Block
		/// </summary>
		public ImageContent(McpRole[]? Audience, double? Priority, DateTime? LastModified,
			Dictionary<string, object>? MetaData)
			: base(Audience, Priority, LastModified, MetaData)
		{
		}

		/// <summary>
		/// What types the content block encodes.
		/// </summary>
		public override Type[] Encodes => new Type[] 
		{ 
			typeof(SKImage),
			typeof(SKBitmap),
			typeof(Graph),
			typeof(PixelInformation)
		};

		/// <summary>
		/// Encodes a content block, given the content to encode and available
		/// annotations and meta-data.
		/// </summary>
		/// <param name="Content">Content to encode.</param>
		/// <returns>MCP-encoded content block.</returns>
		public override async Task<Dictionary<string, object?>> Encode(object Content)
		{
			ContentResponse Encoded = await InternetContent.EncodeAsync(Content, 
				Encoding.UTF8, ImageCodec.ImageContentTypes);

			Encoded.AssertOk();

			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "type", "image" },
				{ "data", Convert.ToBase64String(Encoded.Encoded) },
				{ "mimeType", Encoded.ContentType }
			};

			this.Annotate(Result);

			return Result;
		}
	}
}
