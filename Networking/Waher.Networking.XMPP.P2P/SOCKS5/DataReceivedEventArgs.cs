using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Delegate for data reception events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void DataReceivedEventHandler(object Sender, DataReceivedEventArgs e);

	/// <summary>
	/// Event arguments for data reception events.
	/// </summary>
	public class DataReceivedEventArgs : EventArgs
	{
		private byte[] data;
		private Socks5Client stream;

		/// <summary>
		/// Event arguments for data reception events.
		/// </summary>
		/// <param name="Data">Data received.</param>
		/// <param name="Stream">SOCKS5 client stream.</param>
		internal DataReceivedEventArgs(byte[] Data, Socks5Client Stream)
		{
			this.data = Data;
			this.stream = Stream;
		}

		/// <summary>
		/// Data received.
		/// </summary>
		public byte[] Data
		{
			get { return this.data; }
		}

		/// <summary>
		/// SOCKS5 client stream.
		/// </summary>
		public Socks5Client Stream
		{
			get { return this.stream; }
		}
	}
}
