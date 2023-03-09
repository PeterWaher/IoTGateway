using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content.Getters;
using Waher.Content.Posters;
using Waher.Runtime.Inventory;

namespace Waher.Content.Putters
{
	/// <summary>
	/// Puts to resources on the Web (i.e. using HTTP or HTTPS).
	/// </summary>
	public class WebPutter : PutterBase
	{
		/// <summary>
		/// Puts to resources on the Web (i.e. using HTTP or HTTPS).
		/// </summary>
		public WebPutter()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public override string[] UriSchemes => new string[] { "http", "https" };

		/// <summary>
		/// If the putter is able to put to a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the putter would be able to put to a resource given the indicated URI.</param>
		/// <returns>If the putter can put to a resource with the indicated URI.</returns>
		public override bool CanPut(Uri Uri, out Grade Grade)
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
		/// Puts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be puted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public override async Task<KeyValuePair<byte[], string>> PutAsync(Uri Uri, byte[] EncodedData, string ContentType, X509Certificate Certificate, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			HttpClientHandler Handler = WebGetter.GetClientHandler(Certificate);
			using (HttpClient HttpClient = new HttpClient(Handler, true)
			{
				Timeout = TimeSpan.FromMilliseconds(TimeoutMs)
			})
			{
				using (HttpRequestMessage Request = new HttpRequestMessage()
				{
					RequestUri = Uri,
					Method = HttpMethod.Put, 
					Content = new ByteArrayContent(EncodedData)
				})
				{
					Request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
					WebGetter.PrepareHeaders(Request, Headers, Handler);

					HttpResponseMessage Response = await HttpClient.SendAsync(Request);

					return await WebPoster.ProcessResponse(Response, Uri);
				}
			}
		}

	}
}
