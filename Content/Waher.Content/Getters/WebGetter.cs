using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

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
		public async Task<object> GetAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
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
					if (!(Headers is null))
					{
						foreach (KeyValuePair<string, string> Header in Headers)
						{
							switch (Header.Key)
							{
								case "Accept":
									Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Header.Value));
									break;

								default:
									Request.Headers.Add(Header.Key, Header.Value);
									break;
							}
						}
					}

					HttpResponseMessage Response = await HttpClient.SendAsync(Request);
					Response.EnsureSuccessStatusCode();

					byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
					string ContentType = Response.Content.Headers.ContentType.ToString();
					object Decoded = InternetContent.Decode(ContentType, Bin, Uri);

					return Decoded;
				}
			}
		}
	}
}
