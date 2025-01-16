using System;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Event arguments for HTTPX data responses.
	/// </summary>
	public class HttpxResponseDataEventArgs : EventArgs
	{
		private readonly HttpxResponseEventArgs response;
		private readonly byte[] data;
		private readonly string streamId;
		private readonly bool last;
		private readonly bool constantBuffer;
		private readonly object state;

		/// <summary>
		/// Event arguments for HTTPX data responses.
		/// </summary>
		/// <param name="Response">Response event arguments.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Data response, possibly partial.</param>
		/// <param name="StreamId">Stream ID.</param>
		/// <param name="Last">If it is the last data block.</param>
		/// <param name="State">State object.</param>
		public HttpxResponseDataEventArgs(HttpxResponseEventArgs Response, bool ConstantBuffer,
			byte[] Data, string StreamId, bool Last, object State)
			: base()
		{
			this.response = Response;
			this.constantBuffer = ConstantBuffer;
			this.data = Data;
			this.streamId = StreamId;
			this.last = Last;
			this.state = State;
		}

		/// <summary>
		/// HTTPX Response.
		/// </summary>
		public HttpxResponseEventArgs HttpxResponse => this.response;

		/// <summary>
		/// If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).
		/// </summary>
		public bool ConstantBuffer => this.constantBuffer;

		/// <summary>
		/// Data response, possibly partial.
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// Stream ID.
		/// </summary>
		public string StreamId => this.streamId;

		/// <summary>
		/// If it is the last data block.
		/// </summary>
		public bool Last => this.last;

		/// <summary>
		/// State object passed to the original request.
		/// </summary>
		public object State => this.state;
	}
}
