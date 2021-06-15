using System;
using System.Threading.Tasks;

namespace Waher.Security.DTLS.Test
{
	public class DtlsBridge : ICommunicationLayer
	{
		private DtlsBridge remoteBridge;

		public DtlsBridge(DtlsBridge Bridge)
		{
			this.remoteBridge = Bridge;
		}

		public DtlsBridge RemoteBridge
		{
			get { return this.remoteBridge; }
			internal set { this.remoteBridge = value; }
		}

		public event DataReceivedEventHandler PacketReceived;

		public void SendPacket(byte[] Packet, object RemoteEndpoint)
		{
			if (this.remoteBridge != null)
			{
				Task.Run(() =>
				{
					this.remoteBridge.PacketReceived?.Invoke(Packet, RemoteEndpoint);
				});
			}
		}
	}
}
