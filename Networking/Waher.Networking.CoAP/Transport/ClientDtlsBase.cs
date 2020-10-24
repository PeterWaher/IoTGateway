using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Security.DTLS;
using Waher.Security.DTLS.Events;

namespace Waher.Networking.CoAP.Transport
{
	internal abstract class ClientDtlsBase : ClientBase
	{
		public DtlsOverUdp Dtls;

		public override void Dispose()
		{
			if (!(this.Dtls is null))
			{
				this.Dtls.Dispose();
				this.Dtls = null;
			}
		}

		public override bool IsEncrypted => true;

		public override void BeginReceive()
		{
			this.Dtls.OnDatagramReceived += Dtls_OnDatagramReceived;
		}

		private void Dtls_OnDatagramReceived(object Sender, UdpDatagramEventArgs e)
		{
			try
			{
				this.Endpoint.Decode(e.DtlsOverUdp.Tag as ClientBase, e.Datagram, e.RemoteEndpoint);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		public override void BeginTransmit(Message Message)
		{
			this.Dtls.Send(Message.encoded, Message.destination, Message.credentials,
				this.HandshakeCompleted, Message);
		}

		private void HandshakeCompleted(object Sender, UdpTransmissionEventArgs e)
		{
			if (!e.Successful)
				this.Endpoint.Fail(this, (Message)e.State);
		}
	}
}
