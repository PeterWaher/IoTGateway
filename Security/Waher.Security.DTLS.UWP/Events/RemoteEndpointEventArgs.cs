using System;
using System.Threading.Tasks;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Delegate for remote endpoint events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task RemoteEndpointEventHandler(object Sender, RemoteEndpointEventArgs e);

	/// <summary>
	/// Base class for event arguments with a remote endpoint attribute.
	/// </summary>
	public class RemoteEndpointEventArgs : EventArgs
	{
		private readonly object remoteEndpoint;

		/// <summary>
		/// Event arguments for state change events.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public RemoteEndpointEventArgs(object RemoteEndpoint)
		{
			this.remoteEndpoint = RemoteEndpoint;
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public object RemoteEndpoint => this.remoteEndpoint;
	}
}
