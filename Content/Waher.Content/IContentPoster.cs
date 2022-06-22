using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Internet Content posters. A class implementing this interface and having a default constructor, will be able
	/// to partake in posting content through the static <see cref="InternetContent"/> class. No registration is required.
	/// </summary>
	public interface IContentPoster
	{
		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		string[] UriSchemes
		{
			get;
		}

		/// <summary>
		/// If the poster is able to post to a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the poster would be able to post to a resource given the indicated URI.</param>
		/// <returns>If the poster can post to a resource with the indicated URI.</returns>
		bool CanPost(Uri Uri, out Grade Grade);

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		Task<object> PostAsync(Uri Uri, object Data, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		Task<object> PostAsync(Uri Uri, object Data, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		Task<KeyValuePair<byte[], string>> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		Task<KeyValuePair<byte[], string>> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers);
	}
}
