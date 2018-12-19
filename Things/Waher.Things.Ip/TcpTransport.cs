using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Waher.Events;
using Waher.Networking;

namespace Waher.Things.Ip
{
	/// <summary>
	/// TCP Transport layer.
	/// </summary>
	public class TcpTransport : IBinaryTransportLayer
	{
		private const int BufferSize = 65536;

		private TcpClient client;
		private NetworkStream stream;
		private byte[] buffer = new byte[BufferSize];
		private bool disposed = false;
		private LinkedList<byte[]> outputQueue = new LinkedList<byte[]>();
		private bool isWriting = false;

		/// <summary>
		/// TCP Transport layer.
		/// </summary>
		/// <param name="Client">Connected TCP Client</param>
		public TcpTransport(TcpClient Client)
		{
			this.client = Client;
			this.stream = this.client.GetStream();

			this.stream.BeginRead(this.buffer, 0, BufferSize, this.ReadCallback, null);
		}

		private void ReadCallback(IAsyncResult ar)
		{
			try
			{
				int NrRead = this.stream.EndRead(ar);
				if (NrRead > 0)
				{
					byte[] Bin = new byte[NrRead];
					Array.Copy(this.buffer, 0, Bin, 0, NrRead);

					try
					{
						this.OnReceived?.Invoke(this, Bin);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

				if (!this.disposed)
					this.stream.BeginRead(this.buffer, 0, BufferSize, this.ReadCallback, null);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// TCP Client.
		/// </summary>
		public TcpClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event BinaryEventHandler OnSent;

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		public event BinaryEventHandler OnReceived;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;

			lock (this.outputQueue)
			{
				this.outputQueue.Clear();
			}

			if (this.stream != null)
			{
				this.stream.Dispose();
				this.stream = null;
			}

			if (this.client != null)
			{
				this.client.Dispose();
				this.client = null;
			}
		}

		/// <summary>
		/// Sends a binary packet.
		/// </summary>
		/// <param name="Packet">Binary packet.</param>
		public void Send(byte[] Packet)
		{
			if (this.disposed)
				return;

			lock (this.outputQueue)
			{
				if (this.isWriting)
				{
					this.outputQueue.AddLast(Packet);
					return;
				}

				this.isWriting = true;
			}

			this.stream.BeginWrite(Packet, 0, Packet.Length, this.SendCallback, Packet);
		}

		private void SendCallback(IAsyncResult ar)
		{
			try
			{
				this.stream.EndWrite(ar);

				byte[] Packet = (byte[])ar.AsyncState;

				try
				{
					this.OnSent?.Invoke(this, Packet);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				lock (this.outputQueue)
				{
					if (this.outputQueue.First is null)
					{
						this.isWriting = false;
						return;
					}

					Packet = this.outputQueue.First.Value;
					this.outputQueue.RemoveFirst();
				}

				if (!this.disposed)
					this.stream.BeginWrite(Packet, 0, Packet.Length, this.SendCallback, Packet);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
