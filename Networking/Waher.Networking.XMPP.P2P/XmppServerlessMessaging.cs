using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.PeerToPeer;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Class managing peer-to-peer serveless XMPP communication.
	/// </summary>
    public class XmppServerlessMessaging : IDisposable
    {
		private Dictionary<string, PeerState> peersByJid = new Dictionary<string, PeerState>();
		private Dictionary<string, AddressInfo> addressesByJid = new Dictionary<string, AddressInfo>();
		private Dictionary<string, Dictionary<int, AddressInfo>> addressesByExternalIPPort = new Dictionary<string, Dictionary<int, AddressInfo>>();
		private Dictionary<string, Dictionary<int, AddressInfo>> addressesByLocalIPPort = new Dictionary<string, Dictionary<int, AddressInfo>>();
		private PeerToPeerNetwork p2pNetwork = null;
		private string bareJid;

		/// <summary>
		/// Class managing peer-to-peer serveless XMPP communication.
		/// </summary>
		/// <param name="ApplicationName">Name of application, as it will be registered in Internet Gateways.</param>
		public XmppServerlessMessaging(string ApplicationName, string BareJid)
		{
			this.bareJid = BareJid;
			this.p2pNetwork = new PeerToPeerNetwork(ApplicationName);	// TODO: Implement support for NAT-PMP

			this.p2pNetwork.OnPeerConnected += P2PNetwork_OnPeerConnected;
		}

		/// <summary>
		/// Peer-to-peer network.
		/// </summary>
		public PeerToPeerNetwork Network
		{
			get { return this.p2pNetwork; }
		}

		/// <summary>
		/// Bare JID
		/// </summary>
		public string BareJid
		{
			get { return this.bareJid; }
		}

		private void P2PNetwork_OnPeerConnected(object Listener, PeerConnection Peer)
		{
			PeerState State = new PeerState(Peer, this);
			Log.Informational("Peer connected from " + Peer.RemoteEndpoint.ToString());
		}

		/// <summary>
		/// Reports recognized peer addresses.
		/// </summary>
		/// <param name="XmppAddress">XMPP Address (bare JID).</param>
		/// <param name="ExternalIp">External IP address.</param>
		/// <param name="ExternalPort">External Port number.</param>
		/// <param name="LocalIp">Local IP address.</param>
		/// <param name="LocalPort">Local Port number.</param>
		public void ReportPeerAddresses(string XmppAddress, string ExternalIp, int ExternalPort, string LocalIp, int LocalPort)
		{
			Dictionary<int, AddressInfo> Infos;
			AddressInfo Info;

			string ThisExternalIp = this.p2pNetwork.ExternalAddress.ToString();

			lock (this.addressesByJid)
			{
				if (this.addressesByJid.TryGetValue(XmppAddress, out Info))
				{
					if (Info.ExternalIp == ExternalIp && Info.ExternalPort == ExternalPort &&
						Info.LocalIp == LocalIp && Info.LocalPort == LocalPort)
					{
						return;
					}

					if (Info.ExternalIp != ExternalIp)
					{
						if (this.addressesByExternalIPPort.TryGetValue(Info.ExternalIp, out Infos))
						{
							if (Infos.Remove(Info.ExternalPort) && Infos.Count == 0)
								this.addressesByExternalIPPort.Remove(Info.ExternalIp);
						}
					}

					if (ExternalIp == ThisExternalIp && Info.LocalIp != LocalIp)
					{
						if (this.addressesByLocalIPPort.TryGetValue(Info.LocalIp, out Infos))
						{
							if (Infos.Remove(Info.LocalPort) && Infos.Count == 0)
								this.addressesByLocalIPPort.Remove(Info.LocalIp);
						}
					}
				}

				Info = new AddressInfo(XmppAddress, ExternalIp, ExternalPort, LocalIp, LocalPort);
				this.addressesByJid[XmppAddress] = Info;

				if (!this.addressesByExternalIPPort.TryGetValue(ExternalIp, out Infos))
				{
					Infos = new Dictionary<int, AddressInfo>();
					this.addressesByExternalIPPort[ExternalIp] = Infos;
				}

				Infos[ExternalPort] = Info;

				if (ExternalIp == ThisExternalIp)
				{
					if (!this.addressesByLocalIPPort.TryGetValue(LocalIp, out Infos))
					{
						Infos = new Dictionary<int, AddressInfo>();
						this.addressesByLocalIPPort[LocalIp] = Infos;
					}

					Infos[LocalPort] = Info;
				}
			}
		}

		internal bool AuthenticatePeer(PeerConnection Peer, string BareJID)
		{
			AddressInfo Info;

			lock (this.addressesByJid)
			{
				if (!this.addressesByJid.TryGetValue(BareJID, out Info))
					return false;
			}

			if (Peer.RemoteEndpoint.Address == this.p2pNetwork.ExternalAddress)
				return (Peer.RemoteEndpoint.Address.ToString() == Info.LocalIp && Peer.RemoteEndpoint.Port == Info.LocalPort);
			else
				return (Peer.RemoteEndpoint.Address.ToString() == Info.ExternalIp && Peer.RemoteEndpoint.Port == Info.ExternalPort);
		}

		internal void PeerAuthenticated(PeerState State)
		{
			lock (this.peersByJid)
			{
				this.peersByJid[State.RemoteBareJid] = State;
			}
		}

		internal void PeerClosed(PeerState State)
		{
			PeerState State2;

			lock (this.peersByJid)
			{
				if (this.peersByJid.TryGetValue(State.RemoteBareJid, out State2) && State2 == State)
					this.peersByJid.Remove(State.RemoteBareJid);
			}
		}

		public PeerState GetPeerConnection(string BareJID)
		{
			PeerState Result;

			lock (this.peersByJid)
			{
				if (this.peersByJid.TryGetValue(BareJID, out Result))
					return Result;
			}

			return null;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.p2pNetwork != null)
			{
				this.p2pNetwork.Dispose();
				this.p2pNetwork = null;
			}
		}
	}
}
