using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Delegate for peer address events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void PeerAddressEventHandler(object Sender, PeerAddressEventArgs e);

	/// <summary>
	/// Peer address event arguments.
	/// </summary>
	public class PeerAddressEventArgs : EventArgs
	{
		private readonly string fullJID;
		private readonly string externalIp;
		private readonly string localIp;
		private readonly int externalPort;
		private readonly int localPort;

		/// <summary>
		/// Peer address event arguments.
		/// </summary>
		/// <param name="FullJid">Full JID of endpoint</param>
		/// <param name="ExternalIp">External IP Address</param>
		/// <param name="ExternalPort">c</param>
		/// <param name="LocalIp">Local IP Address</param>
		/// <param name="LocalPort">Local IP Address</param>
		public PeerAddressEventArgs(string FullJid, string ExternalIp, int ExternalPort, string LocalIp, int LocalPort)
		{
			this.fullJID = FullJid;
			this.externalIp = ExternalIp;
			this.externalPort = ExternalPort;
			this.localIp = LocalIp;
			this.localPort = LocalPort;
		}

		/// <summary>
		/// Full JID of endpoint
		/// </summary>
		public string FullJID => this.fullJID;

		/// <summary>
		/// External IP Address.
		/// </summary>
		public string ExternalIp => this.externalIp;

		/// <summary>
		/// External Port number.
		/// </summary>
		public int ExternalPort => this.externalPort;

		/// <summary>
		/// Local IP Address.
		/// </summary>
		public string LocalIp => this.localIp;

		/// <summary>
		/// Local Port number.
		/// </summary>
		public int LocalPort => this.localPort;
	}
}
