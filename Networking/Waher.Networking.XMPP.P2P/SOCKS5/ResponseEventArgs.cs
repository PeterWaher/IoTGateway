using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// SOCKS5 command.
	/// </summary>
	public enum Command
	{
		/// <summary>
		/// CONNECT command (1)
		/// </summary>
		CONNECT = 1,

		/// <summary>
		/// BIND command (2)
		/// </summary>
		BIND = 2,

		/// <summary>
		/// UDP_ASSOCIATE command (3)
		/// </summary>
		UDP_ASSOCIATE = 3
	}

	/// <summary>
	/// Delegate for SOCKS5 response events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ResponseEventHandler(object Sender, ResponseEventArgs e);

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
		public IPAddress IpAddress
		{
			get { return this.ipAddress; }
		}

		/// <summary>
		/// Domain Name, or null if destiation address is an IP address.
		/// </summary>
		public string DomainName
		{
			get { return this.domainName; }
		}

		/// <summary>
		/// Port Number.
		/// </summary>
		public int PortNumber
		{
			get { return this.port; }
		}
	}
}
