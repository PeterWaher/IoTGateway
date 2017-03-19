using System;
using System.IO;
using System.Text;
using Waher.Content;

namespace Waher.Networking.XMPP.InBandBytestreams
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
		private StreamClosedEventHandler closeCallback = null;
		private XmppClient client;
		private object state = null;
		private string streamId;
		private int blockSize;

		internal ValidateStreamEventArgs(XmppClient Client, IqEventArgs e, string StreamId, int BlockSize)
			: base(e)
		{
			this.client = Client;
			this.streamId = StreamId;
			this.blockSize = BlockSize;
		}

		/// <summary>
		/// Stream ID
		/// </summary>
		public string StreamId
		{
			get { return this.streamId; }
		}

		/// <summary>
		/// Block Size
		/// </summary>
		public int BlockSize
		{
			get { return this.blockSize; }
		}

		internal DataReceivedEventHandler DataCallback
		{
			get { return this.dataCallback; }
		}

		internal StreamClosedEventHandler CloseCallback
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
		public bool AcceptStream(DataReceivedEventHandler DataCallback, StreamClosedEventHandler CloseCallback, object State)
		{
			if (this.dataCallback == null)
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
