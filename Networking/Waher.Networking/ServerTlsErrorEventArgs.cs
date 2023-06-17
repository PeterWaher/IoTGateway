using System;

namespace Waher.Networking
{
	/// <summary>
	/// Event arguments for TLS upgrade error events.
	/// </summary>
	public class ServerTlsErrorEventArgs : ServerConnectionEventArgs 
	{
		/// <summary>
		/// Event arguments for TLS upgrade error events.
		/// </summary>
		/// <param name="Connection">Server connection</param>
		/// <param name="Exception">Exception reporteded.</param>
		public ServerTlsErrorEventArgs(ServerTcpConnection Connection, Exception Exception)
			: base(Connection)
		{
			this.Exception = Exception;
		}

		/// <summary>
		/// Exception object
		/// </summary>
		public Exception Exception { get; }
	}
}