using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		private PeerToPeerNetwork p2pNetwork = null;

		/// <summary>
		/// Class managing peer-to-peer serveless XMPP communication.
		/// </summary>
		/// <param name="ApplicationName">Name of application, as it will be registered in Internet Gateways.</param>
		public XmppServerlessMessaging(string ApplicationName)
		{
			this.p2pNetwork = new PeerToPeerNetwork(ApplicationName);
			// TODO: Implement support for NAT-PMP

			this.p2pNetwork.OnPeerConnected += P2PNetwork_OnPeerConnected;
		}

		/// <summary>
		/// Peer-to-peer network.
		/// </summary>
		public PeerToPeerNetwork Network
		{
			get { return this.Network; }
		}

		private void P2PNetwork_OnPeerConnected(object Listener, PeerConnection Peer)
		{
			Peer.OnSent += Peer_OnSent;
			Peer.OnReceived += Peer_OnReceived;
			Peer.OnClosed += Peer_OnClosed;
			Log.Informational("Peer connected from " + Peer.RemoteEndpoint.ToString());
		}

		private void Peer_OnClosed(object sender, EventArgs e)
		{
			throw new System.NotImplementedException();
		}

		private void Peer_OnReceived(object Sender, byte[] Packet)
		{
			throw new System.NotImplementedException();
		}

		private void Peer_OnSent(object Sender, byte[] Packet)
		{
			throw new System.NotImplementedException();
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
