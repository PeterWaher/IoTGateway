﻿using System.Net;

namespace Waher.Security.DTLS.Events
{
	/// <summary>
	/// Event arguments for UDP transmission events.
	/// </summary>
	public class UdpTransmissionEventArgs : UdpEventArgs
    {
		private readonly bool successful;
		private readonly object state;

		/// <summary>
		/// Event arguments for UDP datagram events.
		/// </summary>
		/// <param name="DtlsOverUdp">DTLS over UDP class.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Successful">If operation concluded successfully.</param>
		/// <param name="State">State object passed in original call.</param>
		public UdpTransmissionEventArgs(DtlsOverUdp DtlsOverUdp, IPEndPoint RemoteEndpoint,
			bool Successful, object State)
			: base(DtlsOverUdp, RemoteEndpoint)
		{
			this.successful = Successful;
			this.state = State;
		}

		/// <summary>
		/// If operation concluded successfully.
		/// </summary>
		public bool Successful => this.successful;

		/// <summary>
		/// State object passed in original call.
		/// </summary>
		public object State => this.state;
	}
}
