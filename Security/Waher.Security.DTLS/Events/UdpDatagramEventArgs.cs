using System.Net;

namespace Waher.Security.DTLS.Events
{
	/// <summary>
	/// Event arguments for UDP datagram events.
	/// </summary>
    public class UdpDatagramEventArgs : UdpEventArgs
    {
		private readonly byte[] datagram;

		/// <summary>
		/// Event arguments for UDP datagram events.
		/// </summary>
		/// <param name="DtlsOverUdp">DTLS over UDP class.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Datagram">Datagram.</param>
		public UdpDatagramEventArgs(DtlsOverUdp DtlsOverUdp, IPEndPoint RemoteEndpoint,
			byte[] Datagram)
			: base(DtlsOverUdp, RemoteEndpoint)
		{
			this.datagram = Datagram;
		}

		/// <summary>
		/// Datagram.
		/// </summary>
		public byte[] Datagram => this.datagram;
	}
}
