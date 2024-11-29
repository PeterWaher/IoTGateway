namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// HTTP/2 stream
	/// </summary>
	public class Http2Stream
	{
		private readonly int streamId;

		/// <summary>
		/// HTTP/2 stream
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		public Http2Stream(int StreamId)
		{
			this.streamId = StreamId;
		}

		/// <summary>
		/// Stream ID
		/// </summary>
		public int StreamId => this.streamId;
	}
}
