using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// Delegate for UDP datagram events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task UdpDatagramEvent(object Sender, UdpDatagramEventArgs e);

	/// <summary>
	/// Event arguments for UDP Datagram events.
	/// </summary>
	public class UdpDatagramEventArgs
	{
		private readonly IPEndPoint remoteEndpoint;
		private readonly byte[] data;

		/// <summary>
		/// Event arguments for UDP Datagram events.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote Endpoint</param>
		/// <param name="Data">Binary datagram</param>
		public UdpDatagramEventArgs(IPEndPoint RemoteEndpoint, byte[] Data)
		{
			this.remoteEndpoint = RemoteEndpoint;
			this.data = Data;
		}

		/// <summary>
		/// Remote Endpoint
		/// </summary>
		public IPEndPoint RemoteEndpoint { get { return this.remoteEndpoint; } }

		/// <summary>
		/// Binary Datagram
		/// </summary>
		public byte[] Data { get { return this.data; } }
	}
}
