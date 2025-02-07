using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Waher.Events;

namespace Waher.Networking.CoAP.Transport
{
	internal abstract class ClientUdpBase : ClientBase
	{
		public UdpClient Client;
		private bool disposed = false;
		private readonly LinkedList<Message> outputQueue = new LinkedList<Message>();
		private bool isWriting = false;

		public override void Dispose()
		{
			this.disposed = true;

			this.Client?.Dispose();
			this.Client = null;
		}

		public override bool IsEncrypted => false;

		public async override void BeginReceive()
		{
			try
			{
				while (!this.disposed)
				{
					UdpClient Client = this.Client;
					if (Client is null)
						return;

					UdpReceiveResult Data = await Client.ReceiveAsync();
					if (this.disposed)
						return;

					this.Endpoint.ReceiveBinary(true, Data.Buffer);

					try
					{
						await this.Endpoint.Decode(this, Data.Buffer, Data.RemoteEndPoint);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Closed.
			}
			catch (Exception ex)
			{
				this.Endpoint.Exception(ex);
			}
		}

		public async override void BeginTransmit(Message Message)
		{
			if (this.disposed)
				return;

			lock (this.outputQueue)
			{
				if (this.isWriting)
				{
					this.outputQueue.AddLast(Message);
					return;
				}
				else
					this.isWriting = true;
			}

			try
			{
				while (!(Message is null))
				{
					this.Endpoint.TransmitBinary(true, Message.encoded);

					await this.Client.SendAsync(Message.encoded, Message.encoded.Length, Message.destination);

					if (this.disposed)
						return;

					lock (this.outputQueue)
					{
						if (this.outputQueue.First is null)
						{
							this.isWriting = false;
							Message = null;
						}
						else
						{
							Message = this.outputQueue.First.Value;
							this.outputQueue.RemoveFirst();
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.Endpoint.Exception(ex);
			}
		}

	}
}
