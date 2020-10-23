using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Runtime.Threading;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Class managing the transmission of a SOCKS5 bytestream.
	/// </summary>
	public class OutgoingStream : IDisposable
	{
		private Socks5Client client;
		private TemporaryFile tempFile;
		private MultiReadSingleWriteObject syncObject;
		private readonly IEndToEndEncryption e2e;
		private readonly string sid;
		private readonly string from;
		private readonly string to;
		private object state = null;
		private long pos = 0;
		private int id = 0;
		private readonly int blockSize;
		private bool isWriting;
		private bool done;
		private bool aborted = false;

		/// <summary>
		/// Class managing the transmission of a SOCKS5 bytestream.
		/// </summary>
		/// <param name="StreamId">Stream ID.</param>
		/// <param name="From">From</param>
		/// <param name="To">To</param>
		/// <param name="BlockSize">Block size</param>
		/// <param name="E2E">End-to-end encryption, if used.</param>
		public OutgoingStream(string StreamId, string From, string To, int BlockSize, IEndToEndEncryption E2E)
		{
			this.client = null;
			this.sid = StreamId;
			this.from = From;
			this.to = To;
			this.blockSize = BlockSize;
			this.e2e = E2E;
			this.isWriting = false;
			this.done = false;
			this.tempFile = new TemporaryFile();
		}

		/// <summary>
		/// Stream ID
		/// </summary>
		public string StreamId
		{
			get { return this.sid; }
		}

		/// <summary>
		/// Sender of stream.
		/// </summary>
		public string From
		{
			get { return this.from; }
		}

		/// <summary>
		/// Recipient of stream.
		/// </summary>
		public string To
		{
			get { return this.to; }
		}

		/// <summary>
		/// Block Size
		/// </summary>
		public int BlockSize
		{
			get { return this.blockSize; }
		}

		/// <summary>
		/// If the stream has been aborted.
		/// </summary>
		public bool Aborted
		{
			get { return this.aborted; }
		}

		/// <summary>
		/// State object.
		/// </summary>
		public object State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		/// <summary>
		/// Disposes allocated resources.
		/// </summary>
		public void Dispose()
		{
			this.aborted = true;

			this.tempFile?.Dispose();
			this.tempFile = null;

			this.syncObject?.Dispose();
			this.syncObject = null;
		}

		/// <summary>
		/// Writes data to the stram.
		/// </summary>
		/// <param name="Data">Data</param>
		public Task Write(byte[] Data)
		{
			return this.Write(Data, 0, Data.Length);
		}

		/// <summary>
		/// Writes data to the stram.
		/// </summary>
		/// <param name="Data">Data</param>
		/// <param name="Offset">Offset into array where writing is to start.</param>
		/// <param name="Count">Number of bytes to start.</param>
		public async Task Write(byte[] Data, int Offset, int Count)
		{
			if (this.tempFile is null || this.aborted || this.done)
				throw new IOException("Stream not open");

			if (!await this.syncObject.TryBeginWrite(10000))
				throw new TimeoutException();

			try
			{
				this.tempFile.Position = this.tempFile.Length;
				await this.tempFile.WriteAsync(Data, Offset, Count);

				if (this.client != null && !this.isWriting && this.tempFile.Length - this.pos >= this.blockSize)
					await this.WriteBlockLocked();
			}
			finally
			{
				await this.syncObject.EndWrite();
			}
		}

		private async Task WriteBlockLocked()
		{
			int BlockSize = (int)Math.Min(this.tempFile.Length - this.pos, this.blockSize);

			if (BlockSize == 0)
				await this.SendClose();
			else
			{
				byte[] Block;
				int i;

				if (this.e2e != null)
				{
					Block = new byte[BlockSize];
					i = 0;
				}
				else
				{
					Block = new byte[BlockSize + 2];
					i = 2;

					Block[0] = (byte)(BlockSize >> 8);
					Block[1] = (byte)BlockSize;
				}

				this.tempFile.Position = this.pos;
				int NrRead = this.tempFile.Read(Block, i, BlockSize);
				if (NrRead < BlockSize)
				{
					await this.Close();
					this.Dispose();

					throw new IOException("Unable to read from temporary file.");
				}

				this.pos += NrRead;

				if (this.e2e != null)
				{
					byte[] Encrypted = this.e2e.Encrypt(this.id.ToString(), this.sid, this.from, this.to, Block);
					this.id++;

					if (Encrypted is null)
					{
						this.Dispose();
						return;
					}

					i = Encrypted.Length;
					Block = new byte[i + 2];
					Block[0] = (byte)(i >> 8);
					Block[1] = (byte)i;

					Array.Copy(Encrypted, 0, Block, 2, i);
				}

				this.isWriting = true;
				await this.client.Send(Block);
			}
		}

		private async void WriteQueueEmpty(object Sender, EventArgs e)
		{
			if (this.tempFile is null)
				return;

			if (!await this.syncObject.TryBeginWrite(10000))
				throw new TimeoutException();

			try
			{
				if (this.aborted)
					return;

				long NrLeft = this.tempFile.Length - this.pos;

				if (NrLeft >= this.blockSize || (this.done && NrLeft > 0))
					await this.WriteBlockLocked();
				else
				{
					this.isWriting = false;

					if (this.done)
						await this.SendClose();
				}
			}
			finally
			{
				await this.syncObject.EndWrite();
			}
		}

		/// <summary>
		/// Opens the output.
		/// </summary>
		/// <param name="Client">SOCKS5 client with established connection.</param>
		public async Task Opened(Socks5Client Client)
		{
			Client.OnWriteQueueEmpty += this.WriteQueueEmpty;
			this.client = Client;

			if (!(this.tempFile is null))
			{
				if (!await this.syncObject.TryBeginWrite(10000))
					throw new TimeoutException();

				try
				{
					if (!this.isWriting && (this.tempFile.Length - this.pos >= this.blockSize ||
						(this.done && this.tempFile.Length > this.pos)))
					{
						await this.WriteBlockLocked();
					}
				}
				finally
				{
					await this.syncObject.EndWrite();
				}
			}
		}

		/// <summary>
		/// Closes the session.
		/// </summary>
		public async Task Close()
		{
			this.done = true;

			if (!(this.tempFile is null))
			{
				if (!await this.syncObject.TryBeginWrite(10000))
					throw new TimeoutException();

				try
				{
					if (this.client != null && !this.isWriting)
					{
						if (this.tempFile.Length > this.pos)
							await this.WriteBlockLocked();
						else
							await this.SendClose();
					}
				}
				finally
				{
					await this.syncObject.EndWrite();
				}
			}
		}

		private async Task SendClose()
		{
			this.client.OnWriteQueueEmpty -= this.WriteQueueEmpty;
			await this.client.Send(new byte[] { 0, 0 });
			this.client.CloseWhenDone();
			this.Dispose();
		}

		internal void Abort()
		{
			this.aborted = true;

			EventHandler h = this.OnAbort;
			if (!(h is null))
			{
				try
				{
					h(this, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when stream is aborted.
		/// </summary>
		public event EventHandler OnAbort = null;

	}
}
