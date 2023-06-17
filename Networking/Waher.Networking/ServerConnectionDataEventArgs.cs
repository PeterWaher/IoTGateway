namespace Waher.Networking
{
	/// <summary>
	/// Event arguments for server data reception events.
	/// </summary>
	public class ServerConnectionDataEventArgs : ServerConnectionEventArgs 
	{
		/// <summary>
		/// Event arguments for server data reception events.
		/// </summary>
		/// <param name="Connection">Server connection</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte read.</param>
		/// <param name="Count">Number of bytes read.</param>
		public ServerConnectionDataEventArgs(ServerTcpConnection Connection,
			byte[] Buffer, int Offset, int Count)
			: base(Connection)
		{
			this.Buffer = Buffer;
			this.Offset = Offset;
			this.Count = Count;
		}

		/// <summary>
		/// Binary Data Buffer
		/// </summary>
		public byte[] Buffer { get; }

		/// <summary>
		/// Start index of first byte read.
		/// </summary>
		public int Offset { get; }

		/// <summary>
		/// Number of bytes read.
		/// </summary>
		public int Count { get; }

	}
}