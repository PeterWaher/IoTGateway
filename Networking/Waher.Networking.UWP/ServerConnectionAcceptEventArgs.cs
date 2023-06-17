using System;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Delegate for server connection accept event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task ServerConnectionAcceptEventHandler(object Sender, ServerConnectionAcceptEventArgs e);

	/// <summary>
	/// Event arguments for connection accept events.
	/// </summary>
	public class ServerConnectionAcceptEventArgs : ServerConnectionEventArgs
	{
		/// <summary>
		/// Event arguments for connection accept events.
		/// </summary>
		/// <param name="Id">Connection ID</param>
		/// <param name="Client">Client connection</param>
		public ServerConnectionAcceptEventArgs(Guid Id, BinaryTcpClient Client)
			: base(Id, Client)
		{
			this.Accept = true;
		}

		/// <summary>
		/// Exception object
		/// </summary>
		public bool Accept { get; set; }
	}
}