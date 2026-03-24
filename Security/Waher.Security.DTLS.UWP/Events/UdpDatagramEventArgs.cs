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
		/// <param name="State">Remote endpoint state object.</param>
		/// <param name="Datagram">Datagram.</param>
		public UdpDatagramEventArgs(DtlsOverUdp DtlsOverUdp, EndpointState State,
			byte[] Datagram)
			: base(DtlsOverUdp, State)
		{
			this.datagram = Datagram;
		}

		/// <summary>
		/// Datagram.
		/// </summary>
		public byte[] Datagram => this.datagram;
	}
}
