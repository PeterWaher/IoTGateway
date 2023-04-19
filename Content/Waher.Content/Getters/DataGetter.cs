using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;

namespace Waher.Content.Getters
{
	/// <summary>
	/// Gets data encoded into data URIs.
	/// </summary>
	public class DataGetter : IContentGetter, IContentHeader
	{
		/// <summary>
		/// Gets data encoded into data URIs.
		/// </summary>
		public DataGetter()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public string[] UriSchemes => new string[] { "data" };

		/// <summary>
		/// If the getter is able to get a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the getter would be able to get a resource given the indicated URI.</param>
		/// <returns>If the getter can get a resource with the indicated URI.</returns>
		public bool CanGet(Uri Uri, out Grade Grade)
		{
			switch (Uri.Scheme.ToLower())
			{
				case "data":
					Grade = Grade.Ok;
					return true;

				default:
					Grade = Grade.NotAtAll;
					return false;
			}
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<object> GetAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, Certificate, RemoteCertificateValidator, 60000, Headers);
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<object> GetAsync(Uri Uri, X509Certificate Certificate, RemoteCertificateEventHandler RemoteCertificateValidator, 
			int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			KeyValuePair<byte[], string> P = Decode(Uri);
			return InternetContent.DecodeAsync(P.Value, P.Key, Uri);
		}

		/// <summary>
		/// Decodes a Data URI.
		/// </summary>
		/// <param name="Uri">Data URI</param>
		/// <returns>Decoded Data URI</returns>
		/// <exception cref="ArgumentException">If URI is invalid.</exception>
		public static KeyValuePair<byte[], string> Decode(Uri Uri)
		{
			string s = Uri.OriginalString;
			int i = s.IndexOf(':');
			if (i < 0)
				throw new ArgumentException("Invalid Data URI.", nameof(Uri));

			int j = s.IndexOf(';', i + 1);
			if (j < 0)
				throw new ArgumentException("Content-Type not encoded in Data URI.", nameof(Uri));

			string ContentType = s.Substring(i + 1, j - i - 1);

			int k = s.IndexOf(',', j + 1);
			if (k < 0)
				throw new ArgumentException("Data not encoded in Data URI.", nameof(Uri));

			byte[] Bin = Convert.FromBase64String(s.Substring(k + 1));

			return new KeyValuePair<byte[], string>(Bin, ContentType);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, 60000, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public async Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			KeyValuePair<byte[], string> P = Decode(Uri);
			TemporaryStream f = new TemporaryStream();
			await f.WriteAsync(P.Key, 0, P.Key.Length);

			return new KeyValuePair<string, TemporaryStream>(P.Value, f);
		}

		/// <summary>
		/// If the getter is able to get headers of a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the header would be able to get the headers of a resource given the indicated URI.</param>
		/// <returns>If the header can get the headers of a resource with the indicated URI.</returns>
		public bool CanHead(Uri Uri, out Grade Grade)
		{
			Grade = Grade.NotAtAll;
			return false;
		}

		/// <summary>
		/// Gets the headers of a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded headers object.</returns>
		public Task<object> HeadAsync(Uri Uri, X509Certificate Certificate, RemoteCertificateEventHandler RemoteCertificateValidator, 
			params KeyValuePair<string, string>[] Headers)
		{
			return Task.FromResult<object>(null);
		}

		/// <summary>
		/// Gets the headers of a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded headers object.</returns>
		public Task<object> HeadAsync(Uri Uri, X509Certificate Certificate, RemoteCertificateEventHandler RemoteCertificateValidator, 
			int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return Task.FromResult<object>(null);
		}

	}
}
