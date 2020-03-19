using System;
using System.IO;
using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Delegate for stream validation events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ValidateStreamEventHandler(object Sender, ValidateStreamEventArgs e);

	/// <summary>
	/// Event argument for stream validation events.
	/// </summary>
	public class ValidateStreamEventArgs : IqEventArgs
	{
		private DataReceivedEventHandler dataCallback = null;
		private StreamEventHandler closeCallback = null;
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
		public string StreamId
		{
			get { return this.streamId; }
		}

		/// <summary>
		/// XMPP Client.
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		internal DataReceivedEventHandler DataCallback
		{
			get { return this.dataCallback; }
		}

		internal StreamEventHandler CloseCallback
		{
			get { return this.closeCallback; }
		}

		internal object State
		{
			get { return this.state; }
		}

		/// <summary>
		/// Call this method to accept the incoming stream.
		/// </summary>
		/// <param name="DataCallback">Method called when data has been received.</param>
		/// <param name="CloseCallback">Method called when stream has been closed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the stream acceptance was completed (true), or if somebody else accepted the stream beforehand (false).</returns>
		public bool AcceptStream(DataReceivedEventHandler DataCallback, StreamEventHandler CloseCallback, object State)
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
