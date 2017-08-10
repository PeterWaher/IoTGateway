using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Waher.Security.DTLS.Test
{
	public class Udp : ICommunicationLayer, IDisposable
	{
		private UdpClient client;

		public Udp(string Host, int Port)
		{
			this.client = new UdpClient(Port, AddressFamily.InterNetwork);
			this.client.Connect(Host, 5684);

			this.BeginRead();
		}

		private async void BeginRead()
		{
			try
			{
				while (this.client != null)
				{
					UdpReceiveResult Data = await this.client.ReceiveAsync();
					this.PacketReceived?.Invoke(Data.Buffer, Data.RemoteEndPoint);
				}
			}
			catch (Exception)
			{
			}
		}

		public event DataReceivedEventHandler PacketReceived;

		public void Dispose()
		{
			if (this.client != null)
			{
				this.client.Dispose();
				this.client = null;
			}
		}

		public void SendPacket(byte[] Packet, object RemoteEndpoint)
		{
			this.client.SendAsync(Packet, Packet.Length);
		}
	}
}
