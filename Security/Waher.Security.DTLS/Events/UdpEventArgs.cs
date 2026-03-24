using System;
using System.Net;

namespace Waher.Security.DTLS.Events
{
	/// <summary>
	/// Base Event arguments class for UDP events.
	/// </summary>
    public abstract class UdpEventArgs : EventArgs
    {
		private readonly EndpointState state;
		private readonly DtlsOverUdp dtlsOverUdp;

		/// <summary>
		/// Event arguments for UDP datagram events.
		/// </summary>
		/// <param name="DtlsOverUdp">DTLS over UDP class.</param>
		/// <param name="State">Remote endpoint state object.</param>
		public UdpEventArgs(DtlsOverUdp DtlsOverUdp, EndpointState State)
		{
			this.dtlsOverUdp = DtlsOverUdp;
			this.state = State;
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public IPEndPoint RemoteEndpoint => (IPEndPoint)this.state.remoteEndpoint;

		/// <summary>
		/// DTLS over UDP.
		/// </summary>
		public DtlsOverUdp DtlsOverUdp => this.dtlsOverUdp;

		/// <summary>
		/// UTF-8 encoded authenticated Identity, if available, null otherwise.
		/// </summary>
		public byte[] Identity => this.state.credentials?.Identity;

		/// <summary>
		/// Authenticated Identity string, if available, null otherwise.
		/// </summary>
		public string IdentityString => this.state.credentials?.IdentityString;
	}
}
