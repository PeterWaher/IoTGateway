using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Contains information about peer addresses.
	/// </summary>
	public class AddressInfo
	{
		private string xmppAddress;
		private string externalIp;
		private string localIp;
		private int externalPort;
		private int localPort;

		/// <summary>
		/// Contains information about peer addresses.
		/// </summary>
		/// <param name="XmppAddress">XMPP Address (bare JID).</param>
		/// <param name="ExternalIp">External IP address.</param>
		/// <param name="ExternalPort">External Port number.</param>
		/// <param name="LocalIp">Local IP address.</param>
		/// <param name="LocalPort">Local Port number.</param>
		public AddressInfo(string XmppAddress, string ExternalIp, int ExternalPort, string LocalIp, int LocalPort)
		{
			this.xmppAddress = XmppAddress;
			this.externalIp = ExternalIp;
			this.externalPort = ExternalPort;
			this.localIp = LocalIp;
			this.localPort = LocalPort;
		}

		/// <summary>
		/// XMPP Address (Bare JID).
		/// </summary>
		public string XmppAddress
		{
			get { return this.xmppAddress; }
		}

		/// <summary>
		/// External IP Address.
		/// </summary>
		public string ExternalIp
		{
			get { return this.externalIp; }
		}

		/// <summary>
		/// External Port Number.
		/// </summary>
		public int ExternalPort
		{
			get { return this.externalPort; }
		}

		/// <summary>
		/// Local IP Address.
		/// </summary>
		public string LocalIp
		{
			get { return this.localIp; }
		}

		/// <summary>
		/// Local Port Number.
		/// </summary>
		public int LocalPort
		{
			get { return this.localPort; }
		}
	}
}
