using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content.Queries
{
	/// <summary>
	/// Abstract base class for query handlers.
	/// </summary>
	public abstract class QueryBase : IContentQuery
	{
		/// <summary>
		/// Abstract base class for query handlers.
		/// </summary>
		public QueryBase()
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
		/// If the query handler is able to query a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the query handler would be able to query a resource given the indicated URI.</param>
		/// <returns>If the query handler can query a resource with the indicated URI.</returns>
		public abstract bool CanQuery(Uri Uri, out Grade Grade);

		/// <summary>
		/// Queries a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to query.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual Task<ContentResponse> QueryAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.QueryAsync(Uri, Data, Certificate, RemoteCertificateValidator, InternetContent.DefaultTimeout, Headers);
		}

		/// <summary>
		/// Queries a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to query.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=<see cref="InternetContent.DefaultTimeout"/>)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual async Task<ContentResponse> QueryAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			ContentResponse P = await InternetContent.EncodeAsync(Data, System.Text.Encoding.UTF8);
			if (P.HasError)
				return P;

			ContentBinaryResponse Result = await this.QueryAsync(Uri, P.Encoded, P.ContentType, Certificate,
				RemoteCertificateValidator, TimeoutMs, Headers);

			if (Result.HasError)
				return new ContentResponse(Result.Error);
			else
				return await InternetContent.DecodeAsync(Result.ContentType, Result.Encoded, Uri);
		}

		/// <summary>
		/// Queries a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be queried.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public virtual Task<ContentBinaryResponse> QueryAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.QueryAsync(Uri, EncodedData, ContentType, Certificate, RemoteCertificateValidator, InternetContent.DefaultTimeout, Headers);
		}

		/// <summary>
		/// Queries a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be queried.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public abstract Task<ContentBinaryResponse> QueryAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers);

	}
}
