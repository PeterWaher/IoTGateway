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
		/// <summary>
		/// application/jose+json
		/// </summary>
		public const string JwsContentType = "application/jose+json";

		private readonly string directoryEndpoint;
		private HttpClient httpClient;
		private AcmeDirectory directory = null;
		private string nonce = null;

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
			this.httpClient.DefaultRequestHeaders.Add("Accept", JwsContentType);
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
				this.directory = new AcmeDirectory(this, await this.GET(this.directoryEndpoint));

			return this.directory;
		}

		internal async Task<Dictionary<string, object>> GET(string URL)
		{
			HttpResponseMessage Response = await this.httpClient.GetAsync(URL);
			Response.EnsureSuccessStatusCode();

			Stream Stream = await Response.Content.ReadAsStreamAsync();
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

			return Obj;
		}

		internal async Task<string> GetNonce()
		{
			if (!string.IsNullOrEmpty(this.nonce))
			{
				string s = this.nonce;
				this.nonce = null;
				return s;
			}

			if (this.directory == null)
				await this.GetDirectory();

			HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Head, this.directory.NewNonce);
			HttpResponseMessage Response = await this.httpClient.SendAsync(Request);
			Response.EnsureSuccessStatusCode();

			if (Response.Headers.TryGetValues("Replay-Nonce", out IEnumerable<string> Values))
			{
				foreach (string s in Values)
					return s;
			}

			throw new Exception("No nonce returned from server.");
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

		/// <summary>
		/// Creates an account on the ACME server.
		/// </summary>
		/// <param name="ContactURLs">URLs for contacting the account holder.</param>
		/// <param name="TermsOfServiceAgreed">If the terms of service have been accepted.</param>
		/// <returns>ACME account object.</returns>
		public async Task<AcmeAccount> CreateAccount(string[] ContactURLs, bool TermsOfServiceAgreed)
		{
			if (this.directory == null)
				await this.GetDirectory();

			return new AcmeAccount(this, await this.POST(this.directory.NewAccount, 
				new Dictionary<string, object>()
				{
					{ "termsOfServiceAgreed", TermsOfServiceAgreed },
					{ "contact", ContactURLs }
				}));
		}

		internal async Task<Dictionary<string, object>> POST(string URL, Dictionary<string, object> Payload)
		{
			HttpResponseMessage Response = await this.httpClient.GetAsync(URL);
			Response.EnsureSuccessStatusCode();

			Stream Stream = await Response.Content.ReadAsStreamAsync();
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

			return Obj;
		}

		private void GetNextNonce(HttpResponseMessage Response)
		{
			if (Response.Headers.TryGetValues("Replay-Nonce", out IEnumerable<string> Values))
			{
				foreach (string s in Values)
				{
					this.nonce = s;
					return;
				}
			}
		}


	}
}
