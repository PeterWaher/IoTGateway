using System;
using System.IO;
using System.Text;
using Waher.Content;
using Waher.Events;

namespace Waher.Networking.XMPP.InBandBytestreams
{
	/// <summary>
	/// Delegate for open transmission stream callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void OpenStreamEventHandler(object Sender, OpenStreamEventArgs e);

	/// <summary>
	/// Event argument for open transmission stream callback methods.
	/// </summary>
	public class OpenStreamEventArgs : IqResultEventArgs
	{
		private OutgoingStream output;

		internal OpenStreamEventArgs(IqResultEventArgs e, OutgoingStream Output)
			: base(e)
		{
			this.output = Output;
		}

		/// <summary>
		/// Outgoing stream.
		/// </summary>
		public OutgoingStream Output
		{
			get { return this.output; }
		}

	}
}
