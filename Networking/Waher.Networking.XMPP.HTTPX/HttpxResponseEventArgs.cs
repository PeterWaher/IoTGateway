using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Event arguments for HTTPX responses.
	/// </summary>
	public class HttpxResponseEventArgs : IqResultEventArgs
	{
		private readonly HttpResponse response;
		private readonly string statusMessage;
		private readonly double version;
		private readonly int statusCode;
		private readonly bool hasData;
		private readonly byte[] data;

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
		///	<param name="Data">Any binary data directly available in the response.</param>
		public HttpxResponseEventArgs(IqResultEventArgs e, HttpResponse Response, object State, 
			double Version, int StatusCode, string StatusMessage, bool HasData, byte[] Data)
			: base(e)
		{
			this.response = Response;
			this.State = State;
			this.version = Version;
			this.statusCode = StatusCode;
			this.statusMessage = StatusMessage;
			this.hasData = HasData;
			this.data = Data;
		}

		/// <summary>
		/// HTTP Response.
		/// </summary>
		public HttpResponse HttpResponse => this.response;

		/// <summary>
		/// HTTP version.
		/// </summary>
		public double Version => this.version;

		/// <summary>
		/// HTTP status code.
		/// </summary>
		public int StatusCode => this.statusCode;

		/// <summary>
		/// HTTP status message.
		/// </summary>
		public string StatusMessage => this.statusMessage;

		/// <summary>
		/// If the response has data.
		/// </summary>
		public bool HasData => this.hasData;

		/// <summary>
		/// Any binary data directly available in the response. If <see cref="HasData"/>=true and <see cref="Data"/>=null,
		/// binary data will be received in chunks.
		/// </summary>
		public byte[] Data => this.data;
	}
}
