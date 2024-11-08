using System;
using System.Net;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Event arguments for SOCKS5 responses.
	/// </summary>
	public class ResponseEventArgs : EventArgs
	{
		private readonly IPAddress ipAddress;
		private readonly string domainName;
		private readonly int port;
		private readonly byte responseCode;

		/// <summary>
		/// Event arguments for SOCKS5 responses.
		/// </summary>
		/// <param name="ResponseCode">Response code.</param>
		/// <param name="IpAddress">IP Address, if applicable, or null otherwise.</param>
		/// <param name="DomainName">Domain Name, if applicable, or null otherwise.</param>
		/// <param name="Port">Port number.</param>
		internal ResponseEventArgs(byte ResponseCode, IPAddress IpAddress, string DomainName, int Port)
		{
			this.responseCode = ResponseCode;
			this.ipAddress = IpAddress;
			this.domainName = DomainName;
			this.port = Port;
		}

		/// <summary>
		/// If response is OK.
		/// </summary>
		public bool Ok
		{
			get { return this.responseCode == 0; }
		}

		/// <summary>
		/// IP Address, or null if destination address is a domain name.
		/// </summary>
		public IPAddress IpAddress => this.ipAddress;

		/// <summary>
		/// Domain Name, or null if destiation address is an IP address.
		/// </summary>
		public string DomainName => this.domainName;

		/// <summary>
		/// Port Number.
		/// </summary>
		public int PortNumber => this.port;
	}
}
