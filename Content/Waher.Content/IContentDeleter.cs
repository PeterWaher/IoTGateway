using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Internet Content deleters. A class implementing this interface and having a default constructor, will be able
	/// to partake in deleting content through the static <see cref="InternetContent"/> class. No registration is required.
	/// </summary>
	public interface IContentDeleter
	{
		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		string[] UriSchemes
		{
			get;
		}

		/// <summary>
		/// If the deleter is able to delete to a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the deleter would be able to delete to a resource given the indicated URI.</param>
		/// <returns>If the deleter can delete to a resource with the indicated URI.</returns>
		bool CanDelete(Uri Uri, out Grade Grade);

		/// <summary>
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		Task<ContentResponse> DeleteAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator,
			params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		Task<ContentResponse> DeleteAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator,
			int TimeoutMs, params KeyValuePair<string, string>[] Headers);
	}
}
