using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Security.DTLS.Events;

namespace Waher.Security.DTLS
{
	internal class DtlsOverUdpState
	{
		public IPEndPoint RemoteEndpoint;
		public LinkedList<Tuple<byte[], EventHandlerAsync<UdpTransmissionEventArgs>, object>> Queue;
		public DTLS.DtlsState CurrentState;

		public void AddToQueue(byte[] Packet, EventHandlerAsync<UdpTransmissionEventArgs> Callback, object State)
		{
			lock (this.Queue)
			{
				this.Queue.AddLast(new Tuple<byte[], EventHandlerAsync<UdpTransmissionEventArgs>, object>(Packet,
					Callback, State));
			}
		}

		public async Task Done(DtlsOverUdp DtlsOverUdp, bool Successful)
		{
			Tuple<byte[], EventHandlerAsync<UdpTransmissionEventArgs>, object> Rec;

			do
			{
				lock (this.Queue)
				{
					if (!(this.Queue.First is null))
					{
						Rec = this.Queue.First.Value;
						this.Queue.RemoveFirst();
					}
					else
						Rec = null;
				}

				if (!(Rec is null))
				{
					if (Successful)
						await DtlsOverUdp.DTLS.SendApplicationData(Rec.Item1, this.RemoteEndpoint);

					await Rec.Item2.Raise(this, new UdpTransmissionEventArgs(DtlsOverUdp,
						this.RemoteEndpoint, Successful, Rec.Item3));
				}
			}
			while (!(Rec is null));
		}
	}
}
