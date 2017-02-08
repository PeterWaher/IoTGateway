using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.PeerToPeer;
using System.Collections;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Event handler for peer connection callbacks.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Client">Event arguments.</param>
	public delegate void PeerConnectionEventHandler(object Sender, PeerConnectionEventArgs e);

	/// <summary>
	/// Class managing peer-to-peer serveless XMPP communication.
	/// </summary>
	public class XmppServerlessMessaging : Sniffable, IDisposable
    {
		private Dictionary<string, PeerState> peersByJid = new Dictionary<string, PeerState>(StringComparer.InvariantCultureIgnoreCase);
		private Dictionary<string, AddressInfo> addressesByJid = new Dictionary<string, AddressInfo>(StringComparer.InvariantCultureIgnoreCase);
		private Dictionary<string, Dictionary<int, AddressInfo>> addressesByExternalIPPort = new Dictionary<string, Dictionary<int, AddressInfo>>();
		private Dictionary<string, Dictionary<int, AddressInfo>> addressesByLocalIPPort = new Dictionary<string, Dictionary<int, AddressInfo>>();
		private PeerToPeerNetwork p2pNetwork = null;
		private string bareJid;

		/// <summary>
		/// Class managing peer-to-peer serveless XMPP communication.
		/// </summary>
		/// <param name="ApplicationName">Name of application, as it will be registered in Internet Gateways.</param>
		/// <param name="BareJid">Bare JID of local end-point.</param>
		public XmppServerlessMessaging(string ApplicationName, string BareJid)
		{
			this.bareJid = BareJid;
			this.p2pNetwork = new PeerToPeerNetwork(ApplicationName);   // TODO: Implement support for NAT-PMP
			this.p2pNetwork.EncapsulatePackets = false;

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
			this.Information("Peer connected from " + Peer.RemoteEndpoint.ToString());
		}

		public void RemovePeerAddresses(string XmppAddress)
		{
			Dictionary<int, AddressInfo> Infos;
			AddressInfo Info;

			string ThisExternalIp = this.p2pNetwork.ExternalAddress == null ? string.Empty : this.p2pNetwork.ExternalAddress.ToString();

			lock (this.addressesByJid)
			{
				if (this.addressesByJid.TryGetValue(XmppAddress, out Info))
				{
					this.addressesByJid.Remove(XmppAddress);

					if (this.addressesByExternalIPPort.TryGetValue(Info.ExternalIp, out Infos))
					{
						if (Infos.Remove(Info.ExternalPort) && Infos.Count == 0)
							this.addressesByExternalIPPort.Remove(Info.ExternalIp);
					}

					if (Info.ExternalIp == ThisExternalIp)
					{
						if (this.addressesByLocalIPPort.TryGetValue(Info.LocalIp, out Infos))
						{
							if (Infos.Remove(Info.LocalPort) && Infos.Count == 0)
								this.addressesByLocalIPPort.Remove(Info.LocalIp);
						}
					}
				}
			}
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

			string ThisExternalIp;

			if (this.p2pNetwork.ExternalAddress == null)
				ThisExternalIp = string.Empty;
			else
				ThisExternalIp = this.p2pNetwork.ExternalAddress.ToString();

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

		internal void AuthenticatePeer(PeerConnection Peer, string BareJID)
		{
			AddressInfo Info;

			lock (this.addressesByJid)
			{
				if (!this.addressesByJid.TryGetValue(BareJID, out Info))
					throw new XmppException("Peer JID " + BareJID + " not recognized.");
			}

			if (Info.ExternalIp == this.p2pNetwork.ExternalAddress.ToString())
			{
				if (Peer.RemoteEndpoint.Address.ToString() != Info.LocalIp)
				{
					throw new XmppException("Expected connection from " + Info.LocalIp + ", but was from " +
						Peer.RemoteEndpoint.Address.ToString());
				}
			}
			else
			{
				if (Peer.RemoteEndpoint.Address.ToString() != Info.ExternalIp)
				{
					throw new XmppException("Expected connection from " + Info.ExternalIp + ", but was from " +
						Peer.RemoteEndpoint.ToString());
				}
			}

			// TODO: End-to-end encryption will asure communication is only read by the indended receiver.
			//       (Now, anybody from behind the same router, or same local machine, can pretend to be the claimed JID.)
		}

		internal void PeerAuthenticated(PeerState State)
		{
			lock (this.peersByJid)
			{
				this.peersByJid[State.RemoteBareJid] = State;
			}
		}

		internal void NewXmppClient(XmppClient Client, string LocalJid, string RemoteJid)
		{
			/*foreach (ISniffer Sniffer in this.Sniffers)
				Client.Add(Sniffer);*/

			PeerConnectionEventHandler h = this.OnNewXmppClient;
			if (h != null)
			{
				try
				{
					h(this, new PeerConnectionEventArgs(Client, null, LocalJid, RemoteJid));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a new XMPP client has been created.
		/// </summary>
		public event PeerConnectionEventHandler OnNewXmppClient = null;

		internal void PeerClosed(PeerState State)
		{
			PeerState State2;

			lock (this.peersByJid)
			{
				if (this.peersByJid.TryGetValue(State.RemoteBareJid, out State2) && State2 == State)
					this.peersByJid.Remove(State.RemoteBareJid);
			}
		}

		/// <summary>
		/// Gets a peer XMPP connection.
		/// </summary>
		/// <param name="BareJID">Bare JID of peer to connect to.</param>
		/// <param name="Callback">Method to call when connection is established.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetPeerConnection(string BareJID, PeerConnectionEventHandler Callback, object State)
		{
			PeerState Result;
			AddressInfo Info;
			string Header = null;
			bool b;

			lock (this.addressesByJid)
			{
				b = this.addressesByJid.TryGetValue(BareJID, out Info);
			}

			if (!b)
			{
				Callback(this, new PeerConnectionEventArgs(null, State, this.bareJid, BareJID));
				return;
			}

			lock (this.peersByJid)
			{
				b = this.peersByJid.TryGetValue(BareJID, out Result);

				if (!b)
				{
					Header = "<?xml version='1.0'?><stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' from='" +
						this.bareJid + "' to='" + BareJID + "' version='1.0'>";

					Result = new PeerState(null, this, BareJID, Header, "</stream:stream>", string.Empty, 1.0, Callback, State);
					this.peersByJid[BareJID] = Result;
				}
				else if (Result.HasCallbacks)
				{
					Result.AddCallback(Callback, State);
					return;
				}
			}

			if (b)
			{
				try
				{
					Callback(this, new PeerConnectionEventArgs(Result.XmppClient, State, this.bareJid, BareJID));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				return;
			}

			Task.Run(() =>
			{
				PeerConnection Connection;
				IPAddress Addr;

				try
				{
					if (Info.ExternalIp == this.p2pNetwork.ExternalAddress.ToString())
					{
						if (IPAddress.TryParse(Info.LocalIp, out Addr))
						{
							this.Information("Connecting to " + Addr + ":" + Info.LocalPort.ToString());
							Connection = this.p2pNetwork.ConnectToPeer(new IPEndPoint(Addr, Info.LocalPort));
						}
						else
							Connection = null;
					}
					else
					{
						if (IPAddress.TryParse(Info.ExternalIp, out Addr))
						{
							this.Information("Connecting to " + Addr + ":" + Info.ExternalPort.ToString());
							Connection = this.p2pNetwork.ConnectToPeer(new IPEndPoint(Addr, Info.ExternalPort));
						}
						else
							Connection = null;
					}
				}
				catch (Exception ex)
				{
					this.Error(ex.Message);
					Connection = null;
				}

				if (Connection == null)
				{
					lock (this.peersByJid)
					{
						this.peersByJid.Remove(BareJID);
					}

					Result.CallCallbacks();
				}
				else
				{
					Result.Peer = Connection;
					Connection.Start();
					Result.HeaderSent = true;
					Result.Send(Header);
					this.TransmitText(Header);
				}
			});
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

			if (this.peersByJid != null)
			{
				foreach (PeerState State in this.peersByJid.Values)
					State.Close();

				this.peersByJid.Clear();
				this.peersByJid = null;
			}
		}

	}
}
