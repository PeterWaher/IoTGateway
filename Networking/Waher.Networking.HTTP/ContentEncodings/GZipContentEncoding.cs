using System.IO;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.ContentEncodings
{
	/// <summary>
	/// Content-Encoding: gzip
	/// </summary>
	public class GZipContentEncoding : IContentEncoding
	{
		/// <summary>
		/// Label identifying the Content-Encoding
		/// </summary>
		public string Label => "gzip";

		/// <summary>
		/// If encoding can be used for dynamic encoding.
		/// </summary>
		public bool SupportsDynamicEncoding => true;

		/// <summary>
		/// If encoding can be used for static encoding.
		/// </summary>
		public bool SupportsStaticEncoding => true;

		/// <summary>
		/// How well the Content-Encoding handles the encoding specified by <paramref name="Label"/>.
		/// </summary>
		/// <param name="Label">Content-Encoding label.</param>
		/// <returns>How well the Content-Encoding is supported.</returns>
		public Grade Supports(string Label) => Label == this.Label ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets an encoder.
		/// </summary>
		/// <param name="Output">Output stream.</param>
		/// <param name="ExpectedContentLength">Expected content length, if known.</param>
		/// <param name="ETag">Optional ETag value for content.</param>
		/// <returns>Encoder</returns>
		public TransferEncoding GetEncoder(TransferEncoding Output, long? ExpectedContentLength, string ETag)
		{
			GZipEncoder Encoder = new GZipEncoder(Output, ExpectedContentLength);
			Encoder.PrepareForCompression();
			return Encoder;
		}

		/// <summary>
		/// Tries to get a reference to the precompressed file, if available.
		/// </summary>
		/// <param name="ETag">Optional ETag value for content.</param>
		/// <returns>Reference to precompressed file, if available, or null if not.</returns>
		public FileInfo TryGetPrecompressedFile(string ETag)
		{
			return null;
		}
	}
}
