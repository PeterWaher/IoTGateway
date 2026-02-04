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
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual Task<ContentResponse> PostAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.PostAsync(Uri, Data, Certificate, RemoteCertificateValidator, InternetContent.DefaultTimeout, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to post.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=<see cref="InternetContent.DefaultTimeout"/>)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual async Task<ContentResponse> PostAsync(Uri Uri, object Data, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			ContentResponse P = await InternetContent.EncodeAsync(Data, System.Text.Encoding.UTF8);
			ContentBinaryResponse Result = await this.PostAsync(Uri, P.Encoded, P.ContentType, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
			if (Result.HasError)
				return new ContentResponse(Result.Error);
			else
				return await InternetContent.DecodeAsync(Result.ContentType, Result.Encoded, Uri);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public virtual Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.PostAsync(Uri, EncodedData, ContentType, Certificate, RemoteCertificateValidator, InternetContent.DefaultTimeout, Headers);
		}

		/// <summary>
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public abstract Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, 
			params KeyValuePair<string, string>[] Headers);

	}
}
