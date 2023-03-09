using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;

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
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<object> GetAsync(Uri Uri, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, Certificate, 60000, Headers);
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public async Task<object> GetAsync(Uri Uri, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			HttpClientHandler Handler = GetClientHandler(Certificate);
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
			return GetClientHandler(null);
		}

		/// <summary>
		/// Gets a HTTP Client handler
		/// </summary>
		/// <param name="Certificate">Optional Client certificate</param>
		/// <returns>Http Client Handler</returns>
		public static HttpClientHandler GetClientHandler(X509Certificate Certificate)
		{
			HttpClientHandler Handler = new HttpClientHandler()
			{
				AllowAutoRedirect = true,
				CheckCertificateRevocationList = true,
				ClientCertificateOptions = ClientCertificateOption.Automatic
			};

			try
			{
				Handler.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
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
		/// Decodes a response from the web. If the response is a success, the decoded response is returned.
		/// If the response is not a success, a <see cref="WebException"/> is thrown.
		/// </summary>
		/// <param name="Response">Web response.</param>
		/// <param name="Uri">Original URI of request.</param>
		/// <returns>Decoded response, if success.</returns>
		/// <exception cref="WebException">If response does not indicate a success.</exception>
		public static async Task<object> ProcessResponse(HttpResponseMessage Response, Uri Uri)
		{
			byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
			string ContentType;
			object Decoded;

			if (Bin.Length == 0 || Response.Content.Headers.ContentType is null)
			{
				Decoded = Bin;
				ContentType = string.Empty;
			}
			else
			{
				ContentType = Response.Content.Headers.ContentType.ToString();
				Decoded = await InternetContent.DecodeAsync(ContentType, Bin, Uri);
			}

			if (!Response.IsSuccessStatusCode)
			{
				if (!(Decoded is string Message))
				{
					if (Decoded is null ||
						(Decoded is byte[] Bin2 && Bin2.Length == 0) ||
						Decoded is Dictionary<string, object>)
					{
						Message = Response.ReasonPhrase;
					}
					else
						Message = Decoded.ToString();
				}

				throw new WebException(Message, Response.StatusCode, ContentType, Bin, Decoded, Response.Headers);
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
							string Name = null;
							string Value = null;
							string Path = null;
							string Domain = null;
							bool First = true;

							foreach (KeyValuePair<string, string> P in CommonTypes.ParseFieldValues(Header.Value))
							{
								if (First)
								{
									Name = P.Key;
									Value = P.Value;
									First = false;
								}
								else
								{
									switch (P.Key.ToLower())
									{
										case "path":
											Path = P.Value;
											break;

										case "domain":
											Domain = P.Value;
											break;
									}
								}
							}

							if (First)
								break;

							Cookie Cookie;

							if (!string.IsNullOrEmpty(Domain))
								Cookie = new Cookie(Name, Value, Path ?? string.Empty, Domain);
							else if (!string.IsNullOrEmpty(Path))
								Cookie = new Cookie(Name, Value, Path);
							else
								Cookie = new Cookie(Name, Value);

							Handler.CookieContainer.Add(Request.RequestUri, Cookie);
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
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetTempStreamAsync(Uri, Certificate, 60000, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public async Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			HttpClientHandler Handler = GetClientHandler(Certificate);
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
						await ProcessResponse(Response, Uri);

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

						ExceptionDispatchInfo.Capture(ex).Throw();
					}

					return new KeyValuePair<string, TemporaryStream>(ContentType, File);
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
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded headers object.</returns>
		public Task<object> HeadAsync(Uri Uri, X509Certificate Certificate, params KeyValuePair<string, string>[] Headers)
		{
			return this.HeadAsync(Uri, Certificate, 60000, Headers);
		}

		/// <summary>
		/// Gets the headers of a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded headers object.</returns>
		public async Task<object> HeadAsync(Uri Uri, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			HttpClientHandler Handler = GetClientHandler(Certificate);
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

					return Result;
				}
			}
		}

	}
}
