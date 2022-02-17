using System;
using System.Net;
using System.Net.Http.Headers;

namespace Waher.Content.Getters
{
	/// <summary>
	/// Exception class for web exceptions.
	/// </summary>
	public class WebException : Exception
	{
		private readonly HttpStatusCode statusCode;
		private readonly string contentType;
		private readonly byte[] rawContent;
		private readonly object content;
		private readonly HttpHeaders headers;

		/// <summary>
		/// Exception class for web exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="StatusCode">HTTP Status Code returned.</param>
		/// <param name="ContentType">Content-Type of response.</param>
		/// <param name="RawContent">Raw undecoded content, in binary form.</param>
		/// <param name="Content">Decoded content.</param>
		/// <param name="Headers">HTTP Headers</param>
		public WebException(string Message, HttpStatusCode StatusCode, string ContentType, byte[] RawContent, object Content, HttpHeaders Headers)
			: base(Message)
		{
			this.statusCode = StatusCode;
			this.contentType = ContentType;
			this.rawContent = RawContent;
			this.content = Content;
			this.headers = Headers;
		}

		/// <summary>
		/// HTTP Status Code of content.
		/// </summary>
		public HttpStatusCode StatusCode => this.statusCode;

		/// <summary>
		/// Content-Type of response.
		/// </summary>
		public string ContentType => this.contentType;

		/// <summary>
		/// Raw undecoded content, in binary form.
		/// </summary>
		public byte[] RawContent => this.rawContent;

		/// <summary>
		/// Decoded content.
		/// </summary>
		public object Content => this.content;

		/// <summary>
		/// HTTP Headers
		/// </summary>
		public HttpHeaders Headers => this.headers;
	}
}
