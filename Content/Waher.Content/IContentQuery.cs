using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Internet Content query handlers. A class implementing this interface and having a default constructor, will be able
	/// to partake in querying content through the static <see cref="InternetContent"/> class. No registration is required.
	/// </summary>
	public interface IContentQuery
	{
		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		string[] UriSchemes
		{
			get;
		}

		/// <summary>
		/// If the query handler is able to query a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the query handler would be able to query a resource given the indicated URI.</param>
		/// <returns>If the query handler can query a resource with the indicated URI.</returns>
		bool CanQuery(Uri Uri, out Grade Grade);

		/// <summary>
		/// Queries a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to query.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		Task<ContentResponse> QueryAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, 
			params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Queries a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to query.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		Task<ContentResponse> QueryAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, 
			int TimeoutMs, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Queries a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be queried.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		Task<ContentBinaryResponse> QueryAsync(Uri Uri, byte[] EncodedData, 
			string ContentType, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, 
			params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be put.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		Task<ContentBinaryResponse> QueryAsync(Uri Uri, byte[] EncodedData, 
			string ContentType, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, 
			int TimeoutMs, params KeyValuePair<string, string>[] Headers);
	}
}
