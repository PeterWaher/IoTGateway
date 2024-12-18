using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content.Putters
{
	/// <summary>
	/// Abstract base class for putters.
	/// </summary>
	public abstract class PutterBase : IContentPutter
	{
		/// <summary>
		/// Abstract base class for putters.
		/// </summary>
		public PutterBase()
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
		/// If the putter is able to put to a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the putter would be able to put to a resource given the indicated URI.</param>
		/// <returns>If the putter can put to a resource with the indicated URI.</returns>
		public abstract bool CanPut(Uri Uri, out Grade Grade);

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual Task<ContentResponse> PutAsync(Uri Uri, object Data, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.PutAsync(Uri, Data, Certificate, RemoteCertificateValidator, 60000, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Data">Data to put.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded response.</returns>
		public virtual async Task<ContentResponse> PutAsync(Uri Uri, object Data, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			ContentResponse P = await InternetContent.EncodeAsync(Data, System.Text.Encoding.UTF8);
			ContentBinaryResponse Result = await this.PutAsync(Uri, P.Encoded, P.ContentType, Certificate,
				RemoteCertificateValidator, TimeoutMs, Headers);

			return await InternetContent.DecodeAsync(Result.ContentType, Result.Encoded, Uri);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be puted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public virtual Task<ContentBinaryResponse> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, RemoteCertificateEventHandler RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.PutAsync(Uri, EncodedData, ContentType, Certificate, RemoteCertificateValidator, 60000, Headers);
		}

		/// <summary>
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be puted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public abstract Task<ContentBinaryResponse> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, RemoteCertificateEventHandler RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers);

	}
}
