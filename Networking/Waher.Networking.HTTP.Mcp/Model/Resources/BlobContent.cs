using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Resources
{
	/// <summary>
	/// BLOB resource content
	/// </summary>
	public class BlobContent : IResourceContent
	{
		private readonly Uri uri;
		private readonly byte[] encoded;
		private readonly string contentType;
		private readonly Dictionary<string, object>? metaData;

		/// <summary>
		/// BLOB resource content
		/// </summary>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="Encoded">Encoded content of the resource.</param>
		/// <param name="ContentType">Content-Type of resource.</param>
		/// <param name="MetaData">Optional associated meta-data.</param>
		public BlobContent(Uri Uri, byte[] Encoded, string ContentType,
			Dictionary<string, object>? MetaData)
		{
			this.uri = Uri;
			this.encoded = Encoded;
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
				{ "blob", Convert.ToBase64String(this.encoded) }
			};

			if (!string.IsNullOrEmpty(this.contentType))
				Result["mimeType"] = this.contentType;

			if (!(this.metaData is null))
				Result["_meta"] = this.metaData;

			return Result;
		}
	}
}
