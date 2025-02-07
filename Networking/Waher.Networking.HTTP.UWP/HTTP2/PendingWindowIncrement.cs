namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Contains information about a stream window increment that needs to be reported
	/// to the client.
	/// </summary>
	public class PendingWindowIncrement
	{
		/// <summary>
		/// Stream reporting the increment.
		/// </summary>
		public Http2Stream Stream;

		/// <summary>
		/// Number of bytes
		/// </summary>
		public int NrBytes;
	}
}
