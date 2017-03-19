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

		/// <summary>
		/// Event arguments for data reception events.
		/// </summary>
		internal DataReceivedEventArgs(byte[] Data)
		{
			this.data = Data;
		}

		/// <summary>
		/// Data received.
		/// </summary>
		public byte[] Data
		{
			get { return this.data; }
		}
	}
}
