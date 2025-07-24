using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Content.Getters;
using Waher.Runtime.Inventory;

namespace Waher.Content.Deleters
{
	/// <summary>
	/// Deletes to resources on the Web (i.e. using HTTP or HTTPS).
	/// </summary>
	public class WebDeleter : DeleterBase
	{
		/// <summary>
		/// Deletes to resources on the Web (i.e. using HTTP or HTTPS).
		/// </summary>
		public WebDeleter()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public override string[] UriSchemes => new string[] { "http", "https" };

		/// <summary>
		/// If the deleter is able to delete to a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the deleter would be able to delete to a resource given the indicated URI.</param>
		/// <returns>If the deleter can delete to a resource with the indicated URI.</returns>
		public override bool CanDelete(Uri Uri, out Grade Grade)
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
		/// Deletes a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Certificate">Optional client certificate to use in a Mutual TLS session.</param>
		/// <param name="RemoteCertificateValidator">Optional validator of remote certificates.</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Encoded response.</returns>
		public override async Task<ContentResponse> DeleteAsync(Uri Uri, X509Certificate Certificate,
			EventHandler<RemoteCertificateEventArgs> RemoteCertificateValidator, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
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
					Method = HttpMethod.Delete
				})
				{
					WebGetter.PrepareHeaders(Request, Headers, Handler);

					HttpResponseMessage Response = await HttpClient.SendAsync(Request);

					return await WebGetter.ProcessResponse(Response, Uri);
				}
			}
		}
	}
}
