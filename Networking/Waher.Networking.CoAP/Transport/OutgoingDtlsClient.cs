using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.CoAP.Transport
{
	internal class OutgoingDtlsClient : ClientDtlsBase
	{
		public override bool CanSend(Message Message)
		{
			if (this.Dtls.Client.Client.AddressFamily != Message.destination.AddressFamily)
				return false;

			if (IPAddress.IsLoopback(Message.destination.Address) ^ this.IsLoopback)
				return false;

			return true;
		}
	}
}
