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
			get => this.remoteBridge;
			internal set => this.remoteBridge = value;
		}

		public event DataReceivedEventHandler PacketReceived;

		public async Task SendPacket(byte[] Packet, object RemoteEndpoint)
		{
			if (this.remoteBridge is not null)
			{
				DataReceivedEventHandler h = this.remoteBridge.PacketReceived;
				if (h is not null)
					await h(Packet, RemoteEndpoint);
			}
		}
	}
}
