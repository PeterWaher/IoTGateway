using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.InBandBytestreams
{
	/// <summary>
	/// Delegate for stream closed callback methods.
	/// </summary>
	/// <param name="Sender">Caller.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void StreamClosedEventHandler(object Sender, StreamClosedEventArgs e);

	/// <summary>
	/// Event arguments for stream close callbacks.
	/// </summary>
	public class StreamClosedEventArgs : EventArgs
	{
		private CloseReason reason;
		private object state;

		/// <summary>
		/// Event arguments for stream close callbacks.
		/// </summary>
		/// <param name="Reason">Reason for closing stream.</param>
		/// <param name="State">State object.</param>
		public StreamClosedEventArgs(CloseReason Reason, object State)
		{
			this.reason = Reason;
			this.state = State;
		}

		/// <summary>
		/// Reason for closing stream.
		/// </summary>
		public CloseReason Reason
		{
			get { return this.reason; }
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
