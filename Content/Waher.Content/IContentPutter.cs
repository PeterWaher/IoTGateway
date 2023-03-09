using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Internet Content putters. A class implementing this interface and having a default constructor, will be able
	/// to partake in putting content through the static <see cref="InternetContent"/> class. No registration is required.
	/// </summary>
	public interface IContentPutter
	{
		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		string[] UriSchemes
		{
			get;
		}

		/// <summary>
		/// If the putter is able to put to a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the putter would be able to put to a resource given the indicated URI.</param>
		/// <returns>If the putter can put to a resource with the indicated URI.</returns>
		bool CanPut(Uri Uri, out Grade Grade);

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		Task<object> PutAsync(Uri Uri, object Data, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		Task<object> PutAsync(Uri Uri, object Data, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be puted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		Task<KeyValuePair<byte[], string>> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be puted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		Task<KeyValuePair<byte[], string>> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers);
	}
}
