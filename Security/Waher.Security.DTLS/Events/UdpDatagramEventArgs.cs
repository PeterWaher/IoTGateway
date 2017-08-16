using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Waher.Security.DTLS.Events
{
	/// <summary>
	/// Delegate for UDP datagram events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void UdpDatagramEventHandler(object Sender, UdpDatagramEventArgs e);

	/// <summary>
	/// Event arguments for UDP datagram events.
	/// </summary>
    public class UdpDatagramEventArgs : EventArgs
    {
		private IPEndPoint remoteEndpoint;
		private DtlsOverUdp dtlsOverUdp;
		private byte[] datagram;

		/// <summary>
		/// Event arguments for UDP datagram events.
		/// </summary>
		/// <param name="DtlsOverUdp">DTLS over UDP class.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Datagram">Datagram.</param>
		public UdpDatagramEventArgs(DtlsOverUdp DtlsOverUdp, IPEndPoint RemoteEndpoint,
			byte[] Datagram)
		{
			this.dtlsOverUdp = DtlsOverUdp;
			this.remoteEndpoint = RemoteEndpoint;
			this.datagram = Datagram;
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

		/// <summary>
		/// Datagram.
		/// </summary>
		public byte[] Datagram
		{
			get { return this.datagram; }
		}

	}
}
