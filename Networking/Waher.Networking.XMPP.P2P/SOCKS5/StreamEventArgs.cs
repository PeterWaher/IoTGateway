using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Delegate for stream callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task StreamEventHandler(object Sender, StreamEventArgs e);

	/// <summary>
	/// Event arguments for stream callbacks.
	/// </summary>
	public class StreamEventArgs : EventArgs
	{
		private readonly bool ok;
		private readonly Socks5Client stream;
		private readonly object state;

		/// <summary>
		/// Event arguments for stream callbacks.
		/// </summary>
		/// <param name="Ok">If request was successful.</param>
		/// <param name="Stream">Stream object.</param>
		/// <param name="State">State object.</param>
		public StreamEventArgs(bool Ok, Socks5Client Stream, object State)
		{
			this.ok = Ok;
			this.stream = Stream;
			this.state = State;
		}

		/// <summary>
		/// If request was successful.
		/// </summary>
		public bool Ok
		{
			get { return this.ok; }
		}

		/// <summary>
		/// Stream object.
		/// </summary>
		public Socks5Client Stream
		{
			get { return this.stream; }
		}

		/// <summary>
		/// State object.
		/// </summary>
		public object State
		{
			get { return this.state; }
		}
	}
}
