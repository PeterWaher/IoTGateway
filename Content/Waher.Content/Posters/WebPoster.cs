using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content.Posters
{
	/// <summary>
	/// Posts to resources on the Web (i.e. using HTTP or HTTPS).
	/// </summary>
	public class WebPoster : PosterBase
	{
		/// <summary>
		/// Posts to resources on the Web (i.e. using HTTP or HTTPS).
		/// </summary>
		public WebPoster()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public override string[] UriSchemes => new string[] { "http", "https" };

		/// <summary>
		/// If the poster is able to post to a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the poster would be able to post to a resource given the indicated URI.</param>
		/// <returns>If the poster can post to a resource with the indicated URI.</returns>
		public override bool CanPost(Uri Uri, out Grade Grade)
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
		/// Posts to a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="EncodedData">Encoded data to be posted.</param>
		/// <param name="ContentType">Content-Type of encoded data in <paramref name="EncodedData"/>.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public override async Task<KeyValuePair<byte[], string>> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			using (HttpClient HttpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMilliseconds(TimeoutMs)
			})
			{
				using (HttpRequestMessage Request = new HttpRequestMessage()
				{
					RequestUri = Uri,
					Method = HttpMethod.Post, 
					Content = new ByteArrayContent(EncodedData)
				})
				{
					Request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
					Getters.WebGetter.PrepareHeaders(Request, Headers);

					HttpResponseMessage Response = await HttpClient.SendAsync(Request);
					Response.EnsureSuccessStatusCode();

					byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
					string ContentType2 = Response.Content.Headers.ContentType.ToString();

					return new KeyValuePair<byte[], string>(Bin, ContentType2);
				}
			}
		}

	}
}
