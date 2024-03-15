using System.IO;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.ContentEncodings
{
	/// <summary>
	/// Content-Encoding: gzip
	/// </summary>
	public class GZipContentEncoding : IContentEncoding
	{
		private static bool compressDynamic = true;
		private static bool compressStatic = true;

		/// <summary>
		/// Label identifying the Content-Encoding
		/// </summary>
		public string Label => "gzip";

		/// <summary>
		/// If encoding can be used for dynamic encoding.
		/// </summary>
		public bool SupportsDynamicEncoding => compressDynamic;

		/// <summary>
		/// If encoding can be used for static encoding.
		/// </summary>
		public bool SupportsStaticEncoding => compressStatic;

		/// <summary>
		/// Configures support for the algorithm.
		/// </summary>
		/// <param name="Dynamic">Compression of dynamicly generated files supported.</param>
		/// <param name="Static">Compression of static files supported.</param>
		public void ConfigureSupport(bool Dynamic, bool Static)
		{
			compressDynamic = Dynamic;
			compressStatic = Static;
		}

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
