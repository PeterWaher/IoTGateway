using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Event handler for HTTPX response events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void HttpxResponseEventHandler(object Sender, HttpxResponseEventArgs e);

	/// <summary>
	/// Event arguments for HTTPX responses.
	/// </summary>
	public class HttpxResponseEventArgs : IqResultEventArgs
	{
		private HttpResponse response;
		private string statusMessage;
		private double version;
		private int statusCode;
		private bool hasData;

		/// <summary>
		/// Event arguments for HTTPX responses.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		///	<param name="Response">HTTP response object.</param>
		///	<param name="State">State object.</param>
		///	<param name="Version">HTTP Version.</param>
		///	<param name="StatusCode">HTTP Status Code.</param>
		///	<param name="StatusMessage">HTTP Status Message.</param>
		///	<param name="HasData">If the response has data.</param>
		public HttpxResponseEventArgs(IqResultEventArgs e, HttpResponse Response, object State, 
			double Version, int StatusCode, string StatusMessage, bool HasData)
			: base(e)
		{
			this.response = Response;
			this.State = State;
			this.version = Version;
			this.statusCode = StatusCode;
			this.statusMessage = StatusMessage;
			this.hasData = HasData;
		}

		/// <summary>
		/// HTTP Response.
		/// </summary>
		public HttpResponse HttpResponse
		{
			get { return this.response; }
		}

		/// <summary>
		/// HTTP version.
		/// </summary>
		public double Version
		{
			get { return this.version; }
		}

		/// <summary>
		/// HTTP status code.
		/// </summary>
		public int StatusCode
		{
			get { return this.StatusCode; }
		}

		/// <summary>
		/// HTTP status message.
		/// </summary>
		public string StatusMessage
		{
			get { return this.statusMessage; }
		}

		/// <summary>
		/// If the response has data.
		/// </summary>
		public bool HasData
		{
			get { return this.hasData; }
		}
	}
}
