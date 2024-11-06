using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.InBandBytestreams
{
	/// <summary>
	/// Event argument for open transmission stream callback methods.
	/// </summary>
	public class OpenStreamEventArgs : IqResultEventArgs
	{
		private readonly OutgoingStream output;

		internal OpenStreamEventArgs(IqResultEventArgs e, OutgoingStream Output)
			: base(e)
		{
			this.output = Output;
		}

		/// <summary>
		/// Outgoing stream.
		/// </summary>
		public OutgoingStream Output => this.output;
	}
}
