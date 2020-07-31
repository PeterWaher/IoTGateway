using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.HttpFileUpload
{
	/// <summary>
	/// Delegate for HTTP File Upload callback methods.
	/// </summary>
	/// <param name="Sender">Sender.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task HttpFileUploadEventHandler(object Sender, HttpFileUploadEventArgs e);

	/// <summary>
	/// Event arguments for HTTP File Upload callback methods.
	/// </summary>
	public class HttpFileUploadEventArgs : IqResultEventArgs
	{
		private readonly KeyValuePair<string, string>[] putHeaders = null;
		private readonly string putUrl = null;
		private readonly string getUrl = null;

		/// <summary>
		/// Event arguments for HTTP File Upload callback methods.
		/// </summary>
		/// <param name="e">IQ response.</param>
		/// <param name="GetUrl">GET URL.</param>
		/// <param name="PutUrl">PUT URL.</param>
		/// <param name="PutHeaders">HTTP Headers for PUT request.</param>
		public HttpFileUploadEventArgs(IqResultEventArgs e, string GetUrl,
			string PutUrl, KeyValuePair<string, string>[] PutHeaders)
			: base(e)
		{
			this.getUrl = GetUrl;
			this.putUrl = PutUrl;
			this.putHeaders = PutHeaders;
		}

		/// <summary>
		/// GET URL.
		/// </summary>
		public string GetUrl => this.getUrl;

		/// <summary>
		/// PUT URL.
		/// </summary>
		public string PutUrl => this.putUrl;

		/// <summary>
		/// HTTP Headers for PUT request.
		/// </summary>
		public KeyValuePair<string, string>[] PutHeaders => this.putHeaders;

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task PUT(byte[] Content, string ContentType, int Timeout)
		{
			HttpContent Body = new ByteArrayContent(Content);
			return this.PUT(Body, ContentType, Timeout);
		}

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task PUT(Stream Content, string ContentType, int Timeout)
		{
			HttpContent Body = new StreamContent(Content);
			return this.PUT(Body, ContentType, Timeout);
		}

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public async Task PUT(HttpContent Content, string ContentType, int Timeout)
		{
			using (HttpClient HttpClient = new HttpClient())
			{
				HttpClient.Timeout = TimeSpan.FromMilliseconds(Timeout);
				HttpClient.DefaultRequestHeaders.ExpectContinue = false;

				if (!(this.putHeaders is null))
				{
					foreach (KeyValuePair<string, string> P in this.putHeaders)
						Content.Headers.Add(P.Key, P.Value);

					Content.Headers.Add("Content-Type", ContentType);
				}

				HttpResponseMessage Response = await HttpClient.PutAsync(this.putUrl, Content);
				if (!Response.IsSuccessStatusCode)
				{
					string Msg = await Response.Content.ReadAsStringAsync();
					throw new IOException("Unable to PUT content: " + Msg);
				}

				if (Response.StatusCode != System.Net.HttpStatusCode.Created)
					throw new IOException("Unexpected response.");
			}
		}

	}
}
