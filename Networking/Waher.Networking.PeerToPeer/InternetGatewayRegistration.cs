using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// Represents a registraing in an UPnP-compatible Internet Gateway.
	/// </summary>
	public class InternetGatewayRegistration
	{
		/// <summary>
		/// Name of application to be registered.
		/// </summary>
		public string ApplicationName = string.Empty;

		/// <summary>
		/// Port on local machine.
		/// </summary>
		public ushort LocalPort = 0;

		/// <summary>
		/// Port on external side of gateway.
		/// </summary>
		public ushort ExternalPort = 0;

		/// <summary>
		/// If the TCP port is to be registered.
		/// </summary>
		public bool Tcp = true;

		/// <summary>
		/// If the UDP port is to be registered.
		/// </summary>
		public bool Udp = true;

		/// <summary>
		/// If TCP has been registered.
		/// </summary>
		internal bool TcpRegistered = false;

		/// <summary>
		/// If UDP has been registered.
		/// </summary>
		internal bool UdpRegistered = false;
	}
}
