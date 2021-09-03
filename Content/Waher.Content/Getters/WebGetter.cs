using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;

namespace Waher.Content.Getters
{
	/// <summary>
	/// Gets resources from the Web (i.e. using HTTP or HTTPS).
	/// </summary>
	public class WebGetter : IContentGetter
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
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<object> GetAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, 60000, Headers);
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public async Task<object> GetAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			using (HttpClient HttpClient = new HttpClient()
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
					PrepareHeaders(Request, Headers);

					HttpResponseMessage Response = await HttpClient.SendAsync(Request);

					return await ProcessResponse(Response, Uri);
				}
			}
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
				Decoded = InternetContent.Decode(ContentType, Bin, Uri);
			}

			if (!Response.IsSuccessStatusCode)
			{
				if (!(Decoded is string Message))
				{
					if (Decoded is null || (Decoded is byte[] Bin2 && Bin2.Length == 0))
						Message = Response.ReasonPhrase;
					else
						Message = Decoded.ToString();
				}

				throw new WebException(Message, Response.StatusCode, ContentType, Bin, Decoded);
			}

			return Decoded;
		}

		internal static void PrepareHeaders(HttpRequestMessage Request, KeyValuePair<string, string>[] Headers)
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
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetTempStreamAsync(Uri, 60000, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public async Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			using (HttpClient HttpClient = new HttpClient()
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
					PrepareHeaders(Request, Headers);

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

	}
}
