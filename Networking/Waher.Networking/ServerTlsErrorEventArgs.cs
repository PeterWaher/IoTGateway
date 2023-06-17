using System;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Delegate for TLS upgrade error events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task ServerTlsErrorEventHandler(object Sender, ServerTlsErrorEventArgs e);

	/// <summary>
	/// Event arguments for TLS upgrade error events.
	/// </summary>
	public class ServerTlsErrorEventArgs : ServerConnectionEventArgs 
	{
		/// <summary>
		/// Event arguments for TLS upgrade error events.
		/// </summary>
		/// <param name="Id">Connection ID</param>
		/// <param name="Client">Client connection</param>
		public ServerTlsErrorEventArgs(Guid Id, BinaryTcpClient Client, Exception ex)
			: base(Id, Client)
		{
			this.Exception = ex;
		}

		/// <summary>
		/// Exception object
		/// </summary>
		public Exception Exception { get; }
	}
}