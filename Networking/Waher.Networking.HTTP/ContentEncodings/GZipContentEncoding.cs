using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.ContentEncodings
{
	/// <summary>
	/// Content-Encoding: gzip
	/// </summary>
	internal class GZipContentEncoding : IContentEncoding
	{
		/// <summary>
		/// Label identifying the Content-Encoding
		/// </summary>
		public string Label => "gzip";

		/// <summary>
		/// How well the Content-Encoding handles the encoding specified by <paramref name="Label"/>.
		/// </summary>
		/// <param name="Label">Content-Encoding label.</param>
		/// <returns>How well the Content-Encoding is supported.</returns>
		public Grade Supports(string Label) => Label == this.Label ? Grade.Ok : Grade.NotAtAll;
	}
}
