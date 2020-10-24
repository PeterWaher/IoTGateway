using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Threading;
using Waher.Runtime.Temporary;

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
		private TemporaryStream tempStream = null;
		private MultiReadSingleWriteObject syncObject = new MultiReadSingleWriteObject();
		private readonly object state;
		private int expectedSeq = 0;
		private int baseSeq = 0;
		private readonly int blockSize;
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
			this.tempStream?.Dispose();
			this.tempStream = null;

			this.syncObject?.Dispose();
			this.syncObject = null;
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
			get { return this.tempStream != null; }
		}

		internal async Task<bool> DataReceived(byte[] Data, int Seq)
		{
			TemporaryStream File;

			if (!await this.syncObject.TryBeginWrite(10000))
				throw new TimeoutException();

			try
			{
				if (Seq < this.expectedSeq)
					return false;   // Probably a retry
				else if (Seq > this.expectedSeq)
				{
					long ExpectedPos = (Seq - this.expectedSeq) * this.blockSize;

					if (this.tempStream is null)
						this.tempStream = new TemporaryStream();

					if (this.tempStream.Length < ExpectedPos)
					{
						byte[] Block = new byte[this.blockSize];
						int Len;

						this.tempStream.Position = this.tempStream.Length;

						while (this.tempStream.Length < ExpectedPos)
						{
							Len = (int)Math.Min(ExpectedPos - this.tempStream.Length, this.blockSize);
							await this.tempStream.WriteAsync(Block, 0, Len);
						}
					}
					else
						this.tempStream.Position = ExpectedPos;

					await this.tempStream.WriteAsync(Data, 0, Data.Length);

					return true;
				}
				else
				{
					File = this.tempStream;
					this.tempStream = null;
					this.expectedSeq++;
				}
			}
			finally
			{
				await this.syncObject.EndWrite();
			}

			if (!(File is null))
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

						if (Buf is null || c != Buf.Length)
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
			if (!(this.dataCallback is null))
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
			if (!(this.closeCallback is null))
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
