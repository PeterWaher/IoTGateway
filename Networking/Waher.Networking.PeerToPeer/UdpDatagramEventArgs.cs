using System.Net;

namespace Waher.Networking.PeerToPeer
{
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
		public IPEndPoint RemoteEndpoint => this.remoteEndpoint;

		/// <summary>
		/// Binary Datagram
		/// </summary>
		public byte[] Data => this.data;
	}
}
