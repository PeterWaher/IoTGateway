using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Implements an ACME client for the generation of certificates using ACME-compliant certificate servers.
	/// </summary>
	public class AcmeClient : IDisposable
	{
		private readonly string directoryEndpoint;
		private HttpClient httpClient;
		private AcmeDirectory directory = null;

		/// <summary>
		/// Implements an ACME client for the generation of certificates using ACME-compliant certificate servers.
		/// </summary>
		/// <param name="DirectoryEndpoint">HTTP endpoint for the ACME directory resource.</param>
		public AcmeClient(string DirectoryEndpoint)
		{
			this.directoryEndpoint = DirectoryEndpoint;
			this.httpClient = new HttpClient(new HttpClientHandler()
			{
				AllowAutoRedirect = true,
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				CheckCertificateRevocationList = true,
				SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
			}, true);

			this.httpClient.DefaultRequestHeaders.Add("User-Agent", typeof(AcmeClient).Namespace);
			this.httpClient.DefaultRequestHeaders.Add("Accept", "application/jose+json");
			this.httpClient.DefaultRequestHeaders.Add("Accept-Language", "en");
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.httpClient != null)
			{
				this.httpClient.Dispose();
				this.httpClient = null;
			}
		}

		/// <summary>
		/// Gets the ACME directory.
		/// </summary>
		/// <returns>Directory object.</returns>
		public async Task<AcmeDirectory> GetDirectory()
		{
			if (this.directory == null)
			{
				HttpResponseMessage Response = await this.httpClient.GetAsync(this.directoryEndpoint);
				Response.EnsureSuccessStatusCode();

				Stream Stream = await Response.Content.ReadAsStreamAsync(); // Regardless of status code, we check for XML content.
				byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
				string CharSet = Response.Content.Headers.ContentType.CharSet;
				Encoding Encoding;

				if (string.IsNullOrEmpty(CharSet))
					Encoding = Encoding.UTF8;
				else
					Encoding = System.Text.Encoding.GetEncoding(CharSet);

				string JsonResponse = Encoding.GetString(Bin);

				if (!(JSON.Parse(JsonResponse) is Dictionary<string, object> Obj))
					throw new Exception("Unexpected response returned.");

				this.directory = new AcmeDirectory(Obj);
			}

			return this.directory;
		}

		/// <summary>
		/// Directory object.
		/// </summary>
		public Task<AcmeDirectory> Directory
		{
			get
			{
				if (this.directory == null)
					return this.GetDirectory();
				else
					return Task.FromResult<AcmeDirectory>(this.directory);
			}
		}
	}
}
