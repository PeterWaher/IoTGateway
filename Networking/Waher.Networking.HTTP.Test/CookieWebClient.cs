using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Test
{
	internal class CookieWebClient(Version ProtocolVersion) : IDisposable
	{
		private readonly Version protocolVersion = ProtocolVersion;
		private readonly CookieContainer cookies = new();
		private NetworkCredential credentials = null;
		private DateTime? ifModifiedSince = null;
		private DateTime? ifUnmodifiedSince = null;
		private string accept = null;

		public void Dispose()
		{
		}

		public DateTime? IfModifiedSince
		{
			get => this.ifModifiedSince;
			set => this.ifModifiedSince = value;
		}

		public DateTime? IfUnmodifiedSince
		{
			get => this.ifUnmodifiedSince;
			set => this.ifUnmodifiedSince = value;
		}

		public string Accept
		{
			get => this.accept;
			set => this.accept = value;
		}

		public CookieContainer Cookies
		{
			get => this.cookies;
		}

		public NetworkCredential Credentials
		{
			get => this.credentials;
			set => this.credentials = value;
		}

		public async Task<byte[]> DownloadData(string Url)
		{
			using HttpClient Client = this.GetClient();
			HttpRequestMessage Request = this.GetRequest(HttpMethod.Get, Url);
			HttpResponseMessage Response = await Client.SendAsync(Request);
			Response.EnsureSuccessStatusCode();
			return await Response.Content.ReadAsByteArrayAsync();
		}

		private HttpClient GetClient()
		{
			SocketsHttpHandler Handler = new()
			{
				Credentials = this.credentials,
				CookieContainer = this.cookies,
				AllowAutoRedirect = true,
				UseCookies = true,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				InitialHttp2StreamWindowSize = 65535,
				ConnectTimeout = TimeSpan.FromSeconds(10),
				SslOptions = new SslClientAuthenticationOptions()
				{
					RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
				}
			};

			HttpClient Client = new(Handler)
			{
				DefaultRequestVersion = this.protocolVersion,
				DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
				Timeout = TimeSpan.FromSeconds(10)
			};

			Client.DefaultRequestHeaders.ConnectionClose = false;

			return Client;
		}

		private HttpRequestMessage GetRequest(HttpMethod Method, string Url)
		{
			HttpRequestMessage Request = new(Method, Url)
			{
				Version = this.protocolVersion,
				VersionPolicy = HttpVersionPolicy.RequestVersionExact
			};

			if (this.ifModifiedSince.HasValue)
				Request.Headers.IfModifiedSince = this.ifModifiedSince.Value;

			if (this.ifUnmodifiedSince.HasValue)
				Request.Headers.IfUnmodifiedSince = this.ifUnmodifiedSince.Value;

			if (!string.IsNullOrEmpty(this.accept))
				Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(this.accept));

			return Request;
		}

		public async Task<byte[]> UploadData(string Url, HttpMethod Method, byte[] Data)
		{
			using HttpClient Client = this.GetClient();
			HttpRequestMessage Request = this.GetRequest(Method, Url);
			Request.Content = new ByteArrayContent(Data);

			HttpResponseMessage Response = await Client.SendAsync(Request);
			Response.EnsureSuccessStatusCode();
			return await Response.Content.ReadAsByteArrayAsync();
		}
	}
}
