using System.IO;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.ContentEncodings
{
	/// <summary>
	/// Interface for content encodings in HTTP transfers.
	/// </summary>
	public interface IContentEncoding : IProcessingSupport<string>
	{
		/// <summary>
		/// Label identifying the Content-Encoding
		/// </summary>
		string Label { get; }

		/// <summary>
		/// If encoding can be used for dynamic encoding.
		/// </summary>
		bool SupportsDynamicEncoding { get; }

		/// <summary>
		/// If encoding can be used for static encoding.
		/// </summary>
		bool SupportsStaticEncoding { get; }

		/// <summary>
		/// Configures support for the algorithm.
		/// </summary>
		/// <param name="Dynamic">Compression of dynamicly generated files supported.</param>
		/// <param name="Static">Compression of static files supported.</param>
		void ConfigureSupport(bool Dynamic, bool Static);

		/// <summary>
		/// Gets an encoder.
		/// </summary>
		/// <param name="Output">Output stream.</param>
		/// <param name="ExpectedContentLength">Expected content length, if known.</param>
		/// <param name="ETag">Optional ETag value for content.</param>
		/// <returns>Encoder</returns>
		TransferEncoding GetEncoder(TransferEncoding Output, long? ExpectedContentLength, string ETag);

		/// <summary>
		/// Tries to get a reference to the precompressed file, if available.
		/// </summary>
		/// <param name="ETag">Optional ETag value for content.</param>
		/// <returns>Reference to precompressed file, if available, or null if not.</returns>
		FileInfo TryGetPrecompressedFile(string ETag);
	}
}
