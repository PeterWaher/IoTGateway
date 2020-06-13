using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content.Posters
{
	/// <summary>
	/// Abstract base class for posters.
	/// </summary>
	public abstract class PosterBase : IContentPoster
	{
		/// <summary>
		/// Abstract base class for posters.
		/// </summary>
		public PosterBase()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public abstract string[] UriSchemes
		{
			get;
		}

		/// <summary>
		/// If the poster is able to post to a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the poster would be able to post to a resource given the indicated URI.</param>
		/// <returns>If the poster can post to a resource with the indicated URI.</returns>
		public abstract bool CanPost(Uri Uri, out Grade Grade);

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual async Task<object> PostAsync(Uri Uri, object Data, params KeyValuePair<string, string>[] Headers)
		{
			byte[] Bin = InternetContent.Encode(Data, System.Text.Encoding.UTF8, out string ContentType);
			KeyValuePair<byte[], string> Result = await this.PostAsync(Uri, Bin, ContentType, Headers);
			return InternetContent.Decode(Result.Value, Result.Key, Uri);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public abstract Task<KeyValuePair<byte[], string>> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, params KeyValuePair<string, string>[] Headers);

	}
}
