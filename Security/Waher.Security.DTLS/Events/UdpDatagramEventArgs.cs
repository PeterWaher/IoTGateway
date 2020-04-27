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
		public byte[] Datagram
		{
			get { return this.datagram; }
		}

	}
}
