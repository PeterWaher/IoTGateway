using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual Task<object> PostAsync(Uri Uri, object Data, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return this.PostAsync(Uri, Data, Certificate, 60000, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual async Task<object> PostAsync(Uri Uri, object Data, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			KeyValuePair<byte[], string> P = await InternetContent.EncodeAsync(Data, System.Text.Encoding.UTF8);
			KeyValuePair<byte[], string> Result = await this.PostAsync(Uri, P.Key, P.Value, Certificate, TimeoutMs, Headers);
			return await InternetContent.DecodeAsync(Result.Value, Result.Key, Uri);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public virtual Task<KeyValuePair<byte[], string>> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return this.PostAsync(Uri, EncodedData, ContentType, Certificate, 60000, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public abstract Task<KeyValuePair<byte[], string>> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers);

	}
}
