using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content.Binary;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;
using Waher.Security;

namespace Waher.Content.Getters
{
	/// <summary>
	/// Gets resources from the Web (i.e. using HTTP or HTTPS).
	/// </summary>
	public class WebGetter : IContentGetter, IContentHeader
	{
		/// <summary>
		/// Gets resources from the Web (i.e. using HTTP or HTTPS).
		/// </summary>
		public WebGetter()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public string[] UriSchemes => new string[] { "http", "https" };

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
				case "http":
				case "https":
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
		public Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, 
			params KeyValuePair<string, string>[] Headers)
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
		public async Task<ContentResponse> GetAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, 
			int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			HttpClientHandler Handler = GetClientHandler(Certificate, RemoteCertificateValidator);
			using (HttpClient HttpClient = new HttpClient(Handler, true)
			{
				Timeout = TimeSpan.FromMilliseconds(TimeoutMs)
			})
			{
				using (HttpRequestMessage Request = new HttpRequestMessage()
				{
					RequestUri = Uri,
					Method = HttpMethod.Get
				})
				{
					PrepareHeaders(Request, Headers, Handler);

					HttpResponseMessage Response = await HttpClient.SendAsync(Request);

					return await ProcessResponse(Response, Uri);
				}
			}
		}

		/// <summary>
		/// Gets a HTTP Client handler
		/// </summary>
		/// <returns>Http Client Handler</returns>
		public static HttpClientHandler GetClientHandler()
		{
			return GetClientHandler(null, null);
		}

		/// <summary>
		/// Gets a HTTP Client handler
		/// </summary>
		/// <param name="Certificate">Optional Client certificate</param>
		/// <param name="RemoteCertificateValidator">Optional validator.</param>
		/// <returns>Http Client Handler</returns>
		public static HttpClientHandler GetClientHandler(X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator)
		{
			RemoteCertificateValidator Validator = new RemoteCertificateValidator(RemoteCertificateValidator);

			HttpClientHandler Handler = new HttpClientHandler()
			{
				AllowAutoRedirect = true,
				CheckCertificateRevocationList = true,
				ClientCertificateOptions = ClientCertificateOption.Automatic,
				ServerCertificateCustomValidationCallback = Validator.RemoteCertificateValidationCallback,
				AutomaticDecompression = (DecompressionMethods)(-1)		// All
			};

			try
			{
				Handler.SslProtocols = Crypto.TlsOnly;
			}
			catch (PlatformNotSupportedException)
			{
				// Ignore
			}

			if (!(Certificate is null))
			{
				Handler.ClientCertificateOptions = ClientCertificateOption.Manual;
				Handler.ClientCertificates.Add(Certificate);
			}

			return Handler;
		}

		/// <summary>
		/// Validator of remote certificate.
		/// </summary>
		private class RemoteCertificateValidator
		{
			private readonly RemoteCertificateEventHandler callback;

			/// <summary>
			/// Validator of remote certificate.
			/// </summary>
			/// <param name="Callback">Optional callback method.</param>
			public RemoteCertificateValidator(RemoteCertificateEventHandler Callback)
			{
				this.callback = Callback;
			}

			public bool RemoteCertificateValidationCallback(object Sender,
				X509Certificate Certificate, X509Chain Chain, SslPolicyErrors SslPolicyErrors)
			{
				RemoteCertificateEventArgs e = new RemoteCertificateEventArgs(Certificate, Chain, SslPolicyErrors);

				if (!(this.callback is null))
					this.callback(Sender, e);

				if (e.IsValid.HasValue)
					return e.IsValid.Value;
				else
					return SslPolicyErrors == SslPolicyErrors.None;
			}
		}

		/// <summary>
		/// Decodes a response from the web. If the response is a success, the decoded response is returned.
		/// If the response is not a success, a <see cref="WebException"/> is thrown.
		/// </summary>
		/// <param name="Response">Web response.</param>
		/// <param name="Uri">Original URI of request.</param>
		/// <returns>Decoded response, if success.</returns>
		public static async Task<ContentResponse> ProcessResponse(HttpResponseMessage Response, Uri Uri)
		{
			byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
			string ContentType;
			ContentResponse Decoded;

			if (Bin.Length == 0 || Response.Content.Headers.ContentType is null)
				Decoded = new ContentResponse(BinaryCodec.DefaultContentType, Bin, Bin);
			else
			{
				ContentType = Response.Content.Headers.ContentType.ToString();
				Decoded = await InternetContent.DecodeAsync(ContentType, Bin, Uri);
				if (Decoded.HasError)
					return Decoded;
			}

			if (Decoded.Decoded is IWebServerMetaContent WebServerMetaContent)
				await WebServerMetaContent.DecodeMetaInformation(Response);

			if (!Response.IsSuccessStatusCode)
			{
				if (!(Decoded.Decoded is string Message))
				{
					if (Decoded.Decoded is null ||
						(Decoded.Decoded is byte[] Bin2 && Bin2.Length == 0) ||
						Decoded.Decoded is Dictionary<string, object>)
					{
						Message = Response.ReasonPhrase;
					}
					else
						Message = Decoded.ToString();
				}

				Decoded = new ContentResponse(new WebException(Message, Response.StatusCode, 
					Decoded.ContentType, Bin, Decoded.Decoded, Response.Headers));
			}

			return Decoded;
		}

		internal static void PrepareHeaders(HttpRequestMessage Request, KeyValuePair<string, string>[] Headers, HttpClientHandler Handler)
		{
			if (!(Headers is null))
			{
				foreach (KeyValuePair<string, string> Header in Headers)
				{
					switch (Header.Key)
					{
						case "Accept":
							if (!Request.Headers.Accept.TryParseAdd(Header.Value))
								throw new InvalidOperationException("Invalid Accept header value: " + Header.Value);
							break;

						case "Authorization":
							int i = Header.Value.IndexOf(' ');
							if (i < 0)
								Request.Headers.Authorization = new AuthenticationHeaderValue(Header.Value);
							else
								Request.Headers.Authorization = new AuthenticationHeaderValue(Header.Value.Substring(0, i), Header.Value.Substring(i + 1).TrimStart());
							break;

						case "Cookie":
							foreach (KeyValuePair<string, string> P in CommonTypes.ParseFieldValues(Header.Value))
								Handler.CookieContainer.Add(Request.RequestUri, new Cookie(P.Key, P.Value));
							break;

						default:
							Request.Headers.Add(Header.Key, Header.Value);
							break;
					}
				}
			}
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
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
		public async Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			HttpClientHandler Handler = GetClientHandler(Certificate, RemoteCertificateValidator);
			using (HttpClient HttpClient = new HttpClient(Handler, true)
			{
				Timeout = TimeSpan.FromMilliseconds(10000)
			})
			{
				using (HttpRequestMessage Request = new HttpRequestMessage()
				{
					RequestUri = Uri,
					Method = HttpMethod.Get
				})
				{
					PrepareHeaders(Request, Headers, Handler);

					HttpResponseMessage Response = await HttpClient.SendAsync(Request);

					if (!Response.IsSuccessStatusCode)
					{
						ContentResponse Temp = await ProcessResponse(Response, Uri);
						return new ContentStreamResponse(Temp.Error);
					}

					string ContentType = Response.Content.Headers.ContentType.ToString();
					TemporaryStream File = new TemporaryStream();
					try
					{
						await Response.Content.CopyToAsync(File);
					}
					catch (Exception ex)
					{
						File.Dispose();
						File = null;

						return new ContentStreamResponse(ex);
					}

					return new ContentStreamResponse(ContentType, File);
				}
			}
		}

		/// <summary>
		/// If the getter is able to get headers of a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the header would be able to get the headers of a resource given the indicated URI.</param>
		/// <returns>If the header can get the headers of a resource with the indicated URI.</returns>
		public bool CanHead(Uri Uri, out Grade Grade)
		{
			return this.CanGet(Uri, out Grade);
		}

		/// <summary>
		/// Gets the headers of a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded headers object.</returns>
		public Task<ContentResponse> HeadAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, 
			params KeyValuePair<string, string>[] Headers)
		{
			return this.HeadAsync(Uri, Certificate, RemoteCertificateValidator, 60000, Headers);
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
		public async Task<ContentResponse> HeadAsync(Uri Uri, X509Certificate Certificate,
			RemoteCertificateEventHandler RemoteCertificateValidator, 
			int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			HttpClientHandler Handler = GetClientHandler(Certificate, RemoteCertificateValidator);
			using (HttpClient HttpClient = new HttpClient(Handler, true)
			{
				Timeout = TimeSpan.FromMilliseconds(TimeoutMs)
			})
			{
				using (HttpRequestMessage Request = new HttpRequestMessage()
				{
					RequestUri = Uri,
					Method = HttpMethod.Head
				})
				{
					PrepareHeaders(Request, Headers, Handler);

					HttpResponseMessage Response = await HttpClient.SendAsync(Request);
					Dictionary<string, object> Result = new Dictionary<string, object>()
					{
						{ "Status", Response.StatusCode },
						{ "StatusCode", (int)Response.StatusCode },
						{ "Message", Response.ReasonPhrase },
						{ "IsSuccessStatusCode", Response.IsSuccessStatusCode },
						{ "Version", Response.Version.ToString() }
					};

					foreach (KeyValuePair<string, IEnumerable<string>> Header in Response.Headers)
					{
						string s = null;
						List<string> List = null;

						foreach (string Value in Header.Value)
						{
							if (s is null)
								s = Value;
							else
							{
								if (List is null)
									List = new List<string>() { s };

								List.Add(Value);
							}
						}

						if (List is null)
							Result[Header.Key] = s;
						else
							Result[Header.Key] = List.ToArray();
					}

					return new ContentResponse(Response.Content?.Headers.ContentType?.ToString() ?? string.Empty, Result, null);
				}
			}
		}

	}
}
