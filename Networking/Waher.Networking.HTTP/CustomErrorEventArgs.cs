using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Delegate for custom error content event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void CustomErrorEventHandler(object Sender, CustomErrorEventArgs e);

	/// <summary>
	/// Event arguments for custom error content events.
	/// </summary>
	public class CustomErrorEventArgs : EventArgs
	{
		private readonly HttpRequest request;
		private readonly HttpResponse response;
		private readonly int statusCode;
		private readonly string statusMessage;
		private string contentType;
		private byte[] content;

		/// <summary>
		/// Event arguments for custom error content events.
		/// </summary>
		/// <param name="StatusCode">Status code of error</param>
		/// <param name="StatusMessage">Status message of error</param>
		/// <param name="ContentType">Content-Type of any content.</param>
		/// <param name="Content">Any content.</param>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		public CustomErrorEventArgs(int StatusCode, string StatusMessage, string ContentType, byte[] Content, 
			HttpRequest Request, HttpResponse Response)
		{
			this.statusCode = StatusCode;
			this.statusMessage = StatusMessage;
			this.contentType = ContentType;
			this.content = Content;
			this.request = Request;
			this.response = Response;
		}

		/// <summary>
		/// Current request object.
		/// </summary>
		public HttpRequest Request => this.request;

		/// <summary>
		/// Current response object.
		/// </summary>
		public HttpResponse Response => this.response;

		/// <summary>
		/// Status code of error
		/// </summary>
		public int StatusCode => this.statusCode;

		/// <summary>
		/// Status message of error
		/// </summary>
		public string StatusMessage => this.statusMessage;
		
		/// <summary>
		/// Content-Type of any content.
		/// </summary>
		public string ContentType => this.contentType;

		/// <summary>
		/// Any content.
		/// </summary>
		public byte[] Content => this.content;

		/// <summary>
		/// Sets custom content to return.
		/// </summary>
		/// <param name="ContentType">Content-Type of custom content.</param>
		/// <param name="Content">Encoding of custom content.</param>
		public void SetContent(string ContentType, byte[] Content)
		{
			this.contentType = ContentType;
			this.content = Content;
		}
	}
}
