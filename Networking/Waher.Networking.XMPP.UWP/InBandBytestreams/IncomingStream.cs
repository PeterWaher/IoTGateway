using System;
using Waher.Content;
using Waher.Events;

namespace Waher.Networking.XMPP.InBandBytestreams
{
	/// <summary>
	/// Reason for closing stream.
	/// </summary>
	public enum CloseReason
	{
		/// <summary>
		/// Stream complete.
		/// </summary>
		Done,

		/// <summary>
		/// Stream aborted.
		/// </summary>
		Aborted,

		/// <summary>
		/// Stream timeout.
		/// </summary>
		Timeout
	}

	/// <summary>
	/// Class managing the reception of an in-band bytestream.
	/// </summary>
	public class IncomingStream : IDisposable
	{
		private DataReceivedEventHandler dataCallback;
		private StreamClosedEventHandler closeCallback;
		private TemporaryFile tempFile = null;
		private object synchObj = new object();
		private object state;
		private int expectedSeq = 0;
		private int baseSeq = 0;
		private int blockSize;
		private bool upperEnd = false;

		/// <summary>
		/// Class managing the reception of an in-band bytestream.
		/// </summary>
		/// <param name="DataCallback">Method called when binary data has been received.</param>
		/// <param name="CloseCallback">Method called when stream has been closed.</param>
		/// <param name="State">State object</param>
		/// <param name="BlockSize">Block size.</param>
		public IncomingStream(DataReceivedEventHandler DataCallback, StreamClosedEventHandler CloseCallback, object State, int BlockSize)
		{
			this.dataCallback = DataCallback;
			this.closeCallback = CloseCallback;
			this.state = State;
			this.blockSize = BlockSize;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.tempFile != null)
			{
				this.tempFile.Dispose();
				this.tempFile = null;
			}
		}

		internal int BaseSeq
		{
			get { return this.baseSeq; }
			set { this.baseSeq = value; }
		}

		internal bool UpperEnd
		{
			get { return this.upperEnd; }
			set { this.upperEnd = value; }
		}

		internal int BlockSize
		{
			get { return this.blockSize; }
		}

		internal bool BlocksMissing
		{
			get { return this.tempFile != null; }
		}

		internal bool DataReceived(byte[] Data, int Seq)
		{
			TemporaryFile File;

			lock (this.synchObj)
			{
				if (Seq < this.expectedSeq)
					return false;	// Probably a retry
				else if (Seq > this.expectedSeq)
				{
					long ExpectedPos = (Seq - this.expectedSeq) * this.blockSize;

					if (this.tempFile == null)
						this.tempFile = new TemporaryFile();

					if (this.tempFile.Length < ExpectedPos)
					{
						byte[] Block = new byte[this.blockSize];
						int Len;

						this.tempFile.Position = this.tempFile.Length;

						while (this.tempFile.Length < ExpectedPos)
						{
							Len = (int)Math.Min(ExpectedPos - this.tempFile.Length, this.blockSize);
							this.tempFile.Write(Block, 0, Len);
						}
					}
					else
						this.tempFile.Position = ExpectedPos;

					this.tempFile.Write(Data, 0, Data.Length);

					return true;
				}
				else
				{
					File = this.tempFile;
					this.tempFile = null;
					this.expectedSeq++;
				}
			}

			if (File != null)
			{
				try
				{
					byte[] Buf = null;
					long NrBytes = File.Length;
					int c;

					File.Position = 0;
					while (NrBytes > 0)
					{
						c = (int)Math.Min(NrBytes, this.blockSize);

						if (Buf == null || c != Buf.Length)
							Buf = new byte[c];

						File.Read(Buf, 0, c);
						NrBytes -= c;

						this.DataReceived(Buf);
					}
				}
				finally
				{
					File.Dispose();
				}
			}

			this.DataReceived(Data);

			return true;
		}

		private void DataReceived(byte[] Bin)
		{
			if (this.dataCallback != null)
			{
				try
				{
					this.dataCallback(this, new DataReceivedEventArgs(Bin, this.state));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		internal void Closed(CloseReason Reason)
		{
			if (this.closeCallback != null)
			{
				try
				{
					this.closeCallback(this, new StreamClosedEventArgs(Reason, this.state));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
				finally
				{
					this.closeCallback = null;
					this.dataCallback = null;
				}
			}
		}

	}
}
