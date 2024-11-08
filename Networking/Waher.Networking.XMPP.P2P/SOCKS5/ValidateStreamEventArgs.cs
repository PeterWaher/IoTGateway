using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Event argument for stream validation events.
	/// </summary>
	public class ValidateStreamEventArgs : IqEventArgs
	{
		private EventHandlerAsync<DataReceivedEventArgs> dataCallback = null;
		private EventHandlerAsync<StreamEventArgs> closeCallback = null;
		private readonly XmppClient client;
		private object state = null;
		private readonly string streamId;

		internal ValidateStreamEventArgs(XmppClient Client, IqEventArgs e, string StreamId)
			: base(e)
		{
			this.client = Client;
			this.streamId = StreamId;
		}

		/// <summary>
		/// Stream ID
		/// </summary>
		public string StreamId => this.streamId;

		/// <summary>
		/// XMPP Client.
		/// </summary>
		public XmppClient Client => this.client;

		internal EventHandlerAsync<DataReceivedEventArgs> DataCallback => this.dataCallback;

		internal EventHandlerAsync<StreamEventArgs> CloseCallback => this.closeCallback;

		internal object State => this.state;

		/// <summary>
		/// Call this method to accept the incoming stream.
		/// </summary>
		/// <param name="DataCallback">Method called when data has been received.</param>
		/// <param name="CloseCallback">Method called when stream has been closed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the stream acceptance was completed (true), or if somebody else accepted the stream beforehand (false).</returns>
		public bool AcceptStream(EventHandlerAsync<DataReceivedEventArgs> DataCallback, 
			EventHandlerAsync<StreamEventArgs> CloseCallback, object State)
		{
			if (this.dataCallback is null)
			{
				this.dataCallback = DataCallback;
				this.closeCallback = CloseCallback;
				this.state = State;

				return true;
			}
			else
				return false;
		}
	}
}
