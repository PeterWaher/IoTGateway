namespace Waher.Networking
{
	/// <summary>
	/// Event arguments for connection accept events.
	/// </summary>
	public class ServerConnectionAcceptEventArgs : ServerConnectionEventArgs
	{
		/// <summary>
		/// Event arguments for connection accept events.
		/// </summary>
		/// <param name="Connection">Server connection</param>
		public ServerConnectionAcceptEventArgs(ServerTcpConnection Connection)
			: base(Connection)
		{
			this.Accept = true;
		}

		/// <summary>
		/// Exception object
		/// </summary>
		public bool Accept { get; set; }
	}
}