using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Transport
{
	internal abstract class ClientUdpBase : ClientBase
	{
		public UdpClient Client;
		private bool disposed = false;
		private LinkedList<Message> outputQueue = new LinkedList<Message>();
		private bool isWriting = false;

		public override void Dispose()
		{
			this.disposed = true;

			if (this.Client != null)
			{
				this.Client.Dispose();
				this.Client = null;
			}
		}

		public override bool IsEncrypted => false;

		public async override void BeginReceive()
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await this.Client.ReceiveAsync();
					if (this.disposed)
						return;

					this.Endpoint.ReceiveBinary(Data.Buffer);

					try
					{
						this.Endpoint.Decode(this, Data.Buffer, Data.RemoteEndPoint);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			catch (Exception ex)
			{
				this.Endpoint.Error(ex.Message);
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
				while (Message != null)
				{
					this.Endpoint.TransmitBinary(Message.encoded);

					await this.Client.SendAsync(Message.encoded, Message.encoded.Length, Message.destination);

					if (this.disposed)
						return;

					lock (this.outputQueue)
					{
						if (this.outputQueue.First == null)
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
				this.Endpoint.Error(ex.Message);
			}
		}

	}
}
