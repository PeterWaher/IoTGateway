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
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte read.</param>
		/// <param name="Count">Number of bytes read.</param>
		public ServerConnectionDataEventArgs(ServerTcpConnection Connection,
			bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
			: base(Connection)
		{
			this.ConstantBuffer = ConstantBuffer;
			this.Buffer = Buffer;
			this.Offset = Offset;
			this.Count = Count;
		}

		/// <summary>
		/// If the buffer is used only for this call (true),
		/// or if it will be used for multiple calls with different data (false).
		/// </summary>
		public bool ConstantBuffer { get; }

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