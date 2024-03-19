using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Delegate for proxy response event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task ProxyResponseEventHandler(object Sender, ProxyResponseEventArgs e);

	/// <summary>
	/// Event arguments for proxy response events.
	/// </summary>
	public class ProxyResponseEventArgs : EventArgs
	{
		private readonly HttpResponseMessage message;
		private readonly HttpRequest request;
		private readonly HttpResponse response;

		/// <summary>
		/// Event arguments for proxy response events.
		/// </summary>
		/// <param name="Message">Message being returned.</param>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		public ProxyResponseEventArgs(HttpResponseMessage Message, HttpRequest Request, HttpResponse Response)
		{
			this.message = Message;
			this.request = Request;
			this.response = Response;
		}

		/// <summary>
		/// Message being returned.
		/// </summary>
		public HttpResponseMessage Message => this.message;

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
