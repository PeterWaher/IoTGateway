using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Waher.Security.DTLS.Events
{
	/// <summary>
	/// Base Event arguments class for UDP events.
	/// </summary>
    public abstract class UdpEventArgs : EventArgs
    {
		private readonly IPEndPoint remoteEndpoint;
		private readonly DtlsOverUdp dtlsOverUdp;

		/// <summary>
		/// Event arguments for UDP datagram events.
		/// </summary>
		/// <param name="DtlsOverUdp">DTLS over UDP class.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public UdpEventArgs(DtlsOverUdp DtlsOverUdp, IPEndPoint RemoteEndpoint)
		{
			this.dtlsOverUdp = DtlsOverUdp;
			this.remoteEndpoint = RemoteEndpoint;
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public IPEndPoint RemoteEndpoint
		{
			get { return this.remoteEndpoint; }
		}

		/// <summary>
		/// DTLS over UDP.
		/// </summary>
		public DtlsOverUdp DtlsOverUdp
		{
			get { return this.dtlsOverUdp; }
		}

	}
}
