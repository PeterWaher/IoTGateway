using System;
using System.Net.Http;

namespace Waher.Content.Getters
{
	/// <summary>
	/// Event arguments for the <see cref="WebGetter.HttpUriEventHandler"/> event.
	/// </summary>
	public class HttpUriEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for the <see cref="WebGetter.HttpUriEventHandler"/> event.
		/// </summary>
		/// <param name="Uri">URI being processed.</param>
		/// <param name="Request">Request object being prepared.</param>
		public HttpUriEventArgs(Uri Uri, HttpRequestMessage Request)
		{
			this.Uri = Uri;
			this.Request = Request;
		}

		/// <summary>
		/// URI being processed
		/// </summary>
		public Uri Uri { get; set; }

		/// <summary>
		/// Request object being prepared.
		/// </summary>
		public HttpRequestMessage Request { get; }
	}
}
