using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Event handler for HTTPX response data events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task HttpxResponseDataEventHandler(object Sender, HttpxResponseDataEventArgs e);

	/// <summary>
	/// Event arguments for HTTPX data responses.
	/// </summary>
	public class HttpxResponseDataEventArgs : EventArgs
	{
		private readonly HttpxResponseEventArgs response;
		private readonly byte[] data;
		private readonly string streamId;
		private readonly bool last;
		private readonly object state;

		/// <summary>
		/// Event arguments for HTTPX data responses.
		/// </summary>
		/// <param name="Response">Response event arguments.</param>
		/// <param name="Data">Data response, possibly partial.</param>
		/// <param name="StreamId">Stream ID.</param>
		/// <param name="Last">If it is the last data block.</param>
		/// <param name="State">State object.</param>
		public HttpxResponseDataEventArgs(HttpxResponseEventArgs Response, byte[] Data, string StreamId, bool Last, object State)
			: base()
		{
			this.response = Response;
			this.data = Data;
			this.streamId = StreamId;
			this.last = Last;
			this.state = State;
		}

		/// <summary>
		/// HTTPX Response.
		/// </summary>
		public HttpxResponseEventArgs HttpxResponse
		{
			get { return this.response; }
		}

		/// <summary>
		/// Data response, possibly partial.
		/// </summary>
		public byte[] Data
		{
			get { return this.data; }
		}

		/// <summary>
		/// Stream ID.
		/// </summary>
		public string StreamId
		{
			get { return this.streamId; }
		}

		/// <summary>
		/// If it is the last data block.
		/// </summary>
		public bool Last
		{
			get { return this.last; }
		}

		/// <summary>
		/// State object passed to the original request.
		/// </summary>
		public object State
		{
			get { return this.state; }
		}

	}
}
