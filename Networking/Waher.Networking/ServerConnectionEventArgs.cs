using System;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Delegate for connection events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task ServerConnectionEventHandler(object Sender, ServerConnectionEventArgs e);

	/// <summary>
	/// Event arguments for connection events.
	/// </summary>
	public class ServerConnectionEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for connection events.
		/// </summary>
		/// <param name="Id">Connection ID</param>
		/// <param name="Client">Client connection</param>
		public ServerConnectionEventArgs(Guid Id, BinaryTcpClient Client)
		{
			this.Id = Id;
			this.Client = Client;
		}

		/// <summary>
		/// Connection ID
		/// </summary>
		public Guid Id { get; }

		/// <summary>
		/// Client connection
		/// </summary>
		public BinaryTcpClient Client { get; }
	}
}