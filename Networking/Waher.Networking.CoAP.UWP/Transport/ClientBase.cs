using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Transport
{
	internal abstract class ClientBase : IDisposable
	{
		public CoapEndpoint Endpoint;
		public bool IsLoopback;

		public abstract bool IsEncrypted
		{
			get;
		}

		public abstract void Dispose();
		public abstract void BeginReceive();
		public abstract void BeginTransmit(Message Message);

		public virtual bool CanSend(Message Message)
		{
			return false;
		}
	}
}
