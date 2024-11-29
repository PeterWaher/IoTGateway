using System;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// HTTP/2 stream
	/// </summary>
	internal class Http2Stream
	{
		private readonly int streamId;
		private readonly HttpClientConnection connection;
		private int bufferSize;
		private byte[] outputBuffer;

		/// <summary>
		/// HTTP/2 stream
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Connection">Connection</param>
		public Http2Stream(int StreamId, HttpClientConnection Connection)
		{
			this.streamId = StreamId;
			this.connection = Connection;
			this.bufferSize = this.connection.Settings.BufferSize;
			this.outputBuffer = null;
		}

		/// <summary>
		/// Stream ID
		/// </summary>
		public int StreamId => this.streamId;

		/// <summary>
		/// Sets the window size increment for stream, modified using the WINDOW_UPDATE
		/// frame.
		/// </summary>
		public bool SetWindowSizeIncrement(uint Increment)
		{
			long Size = this.connection.Settings.BufferSize;
			Size += Increment;

			if (Size > int.MaxValue - 1)
				return false;

			this.bufferSize = (int)Size;

			if (!(this.outputBuffer is null) && this.outputBuffer.Length < this.bufferSize)
				Array.Resize(ref this.outputBuffer, this.bufferSize);

			return true;
		}
	}
}
