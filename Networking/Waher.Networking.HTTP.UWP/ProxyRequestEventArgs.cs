using System;
using System.Net.Http;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Event arguments for proxy request events.
	/// </summary>
	public class ProxyRequestEventArgs : EventArgs
	{
		private readonly HttpRequestMessage message;
		private readonly HttpRequest request;
		private readonly HttpResponse response;

		/// <summary>
		/// Event arguments for proxy request events.
		/// </summary>
		/// <param name="Message">Message being forwarded.</param>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		public ProxyRequestEventArgs(HttpRequestMessage Message, HttpRequest Request, HttpResponse Response)
		{
			this.message = Message;
			this.request = Request;
			this.response = Response;
		}

		/// <summary>
		/// Message being forwarded.
		/// </summary>
		public HttpRequestMessage Message => this.message;

		/// <summary>
		/// Current request object.
		/// </summary>
		public HttpRequest Request => this.request;

		/// <summary>
		/// Current response object.
		/// </summary>
		public HttpResponse Response => this.response;
	}
}
