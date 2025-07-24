﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content.Binary;
using Waher.Content.Getters;
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
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public override async Task<ContentBinaryResponse> PostAsync(Uri Uri, byte[] EncodedData, string ContentType, 
			X509Certificate Certificate, EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, 
			params KeyValuePair<string, string>[] Headers)
		{
			HttpClientHandler Handler = WebGetter.GetClientHandler(Certificate, RemoteCertificateValidator, Uri);
			using (HttpClient HttpClient = new HttpClient(Handler, true)
			{
				Timeout = TimeSpan.FromMilliseconds(TimeoutMs)
			})
			{
				using (HttpRequestMessage Request = new HttpRequestMessage()
				{
					RequestUri = Uri,
					Method = HttpMethod.Post, 
					Content = new ByteArrayContent(EncodedData),
				})
				{
					Request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
					WebGetter.PrepareHeaders(Request, Headers, Handler);

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
		public static async Task<ContentBinaryResponse> ProcessResponse(HttpResponseMessage Response, Uri Uri)
		{
			if (!Response.IsSuccessStatusCode)
			{
				ContentResponse Temp = await WebGetter.ProcessResponse(Response, Uri);
				return new ContentBinaryResponse(Temp.Error);
			}

			byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
			string ContentType = Response.Content.Headers.ContentType?.ToString() ?? BinaryCodec.DefaultContentType;

			return new ContentBinaryResponse(ContentType, Bin);
		}

	}
}
