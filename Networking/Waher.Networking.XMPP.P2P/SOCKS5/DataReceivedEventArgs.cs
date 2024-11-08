using System;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Event arguments for data reception events.
	/// </summary>
	public class DataReceivedEventArgs : EventArgs
	{
		private readonly byte[] buffer;
		private readonly int offset;
		private readonly int count;
		private readonly Socks5Client stream;
		private readonly object state;

		/// <summary>
		/// Event arguments for data reception events.
		/// </summary>
		/// <param name="Buffer">Binary Data Buffer</param>
		/// <param name="Offset">Start index of first byte received.</param>
		/// <param name="Count">Number of bytes received.</param>
		/// <param name="Stream">SOCKS5 client stream.</param>
		/// <param name="State">State</param>
		internal DataReceivedEventArgs(byte[] Buffer, int Offset, int Count, Socks5Client Stream, object State)
		{
			this.buffer = Buffer;
			this.offset = Offset;
			this.count = Count;
			this.stream = Stream;
			this.state = State;
		}

		/// <summary>
		/// Buffer holding received data.
		/// </summary>
		public byte[] Buffer => this.buffer;

		/// <summary>
		/// Start index of first byte received.
		/// </summary>
		public int Offset => this.offset;

		/// <summary>
		/// Number of bytes received.
		/// </summary>
		public int Count => this.count;

		/// <summary>
		/// SOCKS5 client stream.
		/// </summary>
		public Socks5Client Stream => this.stream;

		/// <summary>
		/// State object.
		/// </summary>
		public object State => this.state;

	}
}
