using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml.Text;
using Waher.Content.Xsl;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Runtime.Temporary;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Getter for well-known contract schemas.
	/// </summary>
	public class ContractSchemasGetter : IContentGetter, IContentHeader
	{
		/// <summary>
		/// Getter for well-known contract schemas.
		/// </summary>
		public ContractSchemasGetter()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public string[] UriSchemes => new string[] { "urn" };

		/// <summary>
		/// If the getter is able to get a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the getter would be able to get a resource given the indicated URI.</param>
		/// <returns>If the getter can get a resource with the indicated URI.</returns>
		public bool CanGet(Uri Uri, out Grade Grade)
		{
			if (string.IsNullOrEmpty(UriToResource(Uri)))
			{
				Grade = Grade.NotAtAll;
				return false;
			}
			else
			{
				Grade = Grade.Excellent;
				return true;
			}
		}

		private static string UriToResource(Uri Uri)
		{
			string SchemaName;

			switch (Uri.OriginalString)
			{
				case "urn:nf:iot:e2e:1.0":
					SchemaName = "E2E.xsd";
					break;

				case "urn:nf:iot:leg:id:1.0":
					SchemaName = "LegalIdentities.xsd";
					break;

				case "urn:nf:iot:p2p:1.0":
					SchemaName = "P2P.xsd";
					break;

				case "urn:nf:iot:leg:sc:1.0":
					SchemaName = "SmartContracts.xsd";
					break;

				default:
					return null;
			}

			return typeof(ContractsClient).Namespace + ".Schema." + SchemaName;
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator,
			params KeyValuePair<string, string>[] Headers)
		{
			string SchemaName = UriToResource(Uri);
			if (string.IsNullOrEmpty(SchemaName))
				return Task.FromResult(new ContentResponse(new Exception("URI not recognized.")));

			return Task.FromResult(new ContentResponse(XmlCodec.SchemaContentType, XSL.LoadSchema(SchemaName), Array.Empty<byte>()));
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=<see cref="InternetContent.DefaultTimeout"/>)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator,
			int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional Client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, InternetContent.DefaultTimeout, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, TemporaryStream Destination, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, InternetContent.DefaultTimeout, Destination, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional callback method for validating remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=<see cref="InternetContent.DefaultTimeout"/>)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetTempStreamAsync(Uri, Certificate, RemoteCertificateValidator, TimeoutMs, null, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=<see cref="InternetContent.DefaultTimeout"/>)</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public async Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, TemporaryStream Destination,
			params KeyValuePair<string, string>[] Headers)
		{
			string SchemaName = UriToResource(Uri);
			if (string.IsNullOrEmpty(SchemaName))
				return new ContentStreamResponse(new Exception("URI not recognized."));

			byte[] Bin = Resources.LoadResource(SchemaName);

			Destination ??= new TemporaryStream();
			await Destination.WriteAsync(Bin, 0, Bin.Length);

			return new ContentStreamResponse(XmlCodec.DefaultContentType, Destination);
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
		public Task<ContentResponse> HeadAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator,
			params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, Certificate, RemoteCertificateValidator, Headers);
		}

		/// <summary>
		/// Gets the headers of a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=<see cref="InternetContent.DefaultTimeout"/>)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded headers object.</returns>
		public Task<ContentResponse> HeadAsync(Uri Uri, X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator,
			int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, Certificate, RemoteCertificateValidator, TimeoutMs, Headers);
		}
	}
}
