using System;
using System.Collections.Generic;
using System.Net;
using Waher.Events;
using Waher.Security.DTLS.Events;

namespace Waher.Security.DTLS
{
	internal class DtlsOverUdpState
	{
		public IPEndPoint RemoteEndpoint;
		public LinkedList<Tuple<byte[], UdpTransmissionEventHandler, object>> Queue;
		public DTLS.DtlsState CurrentState;

		public void AddToQueue(byte[] Packet, UdpTransmissionEventHandler Callback, object State)
		{
			lock (this.Queue)
			{
				this.Queue.AddLast(new Tuple<byte[], UdpTransmissionEventHandler, object>(Packet, 
					Callback, State));
			}
		}

		public void Done(DtlsOverUdp DtlsOverUdp, bool Successful)
		{
			Tuple<byte[], UdpTransmissionEventHandler, object> Rec;

			do
			{
				lock (this.Queue)
				{
					if (this.Queue.First != null)
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
						DtlsOverUdp.DTLS.SendApplicationData(Rec.Item1, this.RemoteEndpoint);

					if (Rec.Item2 != null)
					{
						try
						{
							Rec.Item2(this, new UdpTransmissionEventArgs(DtlsOverUdp,
								this.RemoteEndpoint, Successful, Rec.Item3));
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}
			}
			while (Rec != null);
		}
	}
}
