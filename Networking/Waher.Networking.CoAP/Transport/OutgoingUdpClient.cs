using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.CoAP.Transport
{
	internal class OutgoingUdpClient : ClientUdpBase
	{
		public IPEndPoint MulticastAddress;

		public override bool CanSend(Message Message)
		{
			if (this.Client.Client.AddressFamily != Message.destination.AddressFamily)
				return false;

			if (IPAddress.IsLoopback(Message.destination.Address) ^ this.IsLoopback)
				return false;

			return true;
		}
	}
}
