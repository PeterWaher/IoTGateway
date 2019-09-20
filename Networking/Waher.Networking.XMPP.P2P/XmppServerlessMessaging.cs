using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.PeerToPeer;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Event handler for peer connection callbacks.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void PeerConnectionEventHandler(object Sender, PeerConnectionEventArgs e);

	/// <summary>
	/// Class managing peer-to-peer serveless XMPP communication.
	/// </summary>
	public class XmppServerlessMessaging : Sniffable, IDisposable
	{
		private Dictionary<string, PeerState> peersByFullJid = new Dictionary<string, PeerState>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Dictionary<string, AddressInfo> addressesByFullJid = new Dictionary<string, AddressInfo>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Dictionary<string, Dictionary<int, AddressInfo>> addressesByExternalIPPort = new Dictionary<string, Dictionary<int, AddressInfo>>();
		private readonly Dictionary<string, Dictionary<int, AddressInfo>> addressesByLocalIPPort = new Dictionary<string, Dictionary<int, AddressInfo>>();
		private PeerToPeerNetwork p2pNetwork = null;
		private string fullJid;

		/// <summary>
		/// Class managing peer-to-peer serveless XMPP communication.
		/// </summary>
		/// <param name="ApplicationName">Name of application, as it will be registered in Internet Gateways.</param>
		/// <param name="FullJid">Bare JID of local end-point.</param>
		/// <param name="Sniffers">Sniffers</param>
		public XmppServerlessMessaging(string ApplicationName, string FullJid, params ISniffer[] Sniffers)
			: this(ApplicationName, FullJid, PeerToPeerNetwork.DefaultPort, PeerToPeerNetwork.DefaultPort,
				  PeerToPeerNetwork.DefaultBacklog, Sniffers)
		{
		}

		/// <summary>
		/// Class managing peer-to-peer serveless XMPP communication.
		/// </summary>
		/// <param name="ApplicationName">Name of application, as it will be registered in Internet Gateways.</param>
		/// <param name="FullJid">Bare JID of local end-point.</param>
		/// <param name="LocalPort">Desired local port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="ExternalPort">Desired external port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="Sniffers">Sniffers</param>
		public XmppServerlessMessaging(string ApplicationName, string FullJid, ushort LocalPort, ushort ExternalPort, params ISniffer[] Sniffers)
			: this(ApplicationName, FullJid, LocalPort, ExternalPort, PeerToPeerNetwork.DefaultBacklog, Sniffers)
		{
		}

		/// <summary>
		/// Class managing peer-to-peer serveless XMPP communication.
		/// </summary>
		/// <param name="ApplicationName">Name of application, as it will be registered in Internet Gateways.</param>
		/// <param name="FullJid">Bare JID of local end-point.</param>
		/// <param name="LocalPort">Desired local port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="ExternalPort">Desired external port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="Backlog">Connection backlog.</param>
		/// <param name="Sniffers">Sniffers</param>
		public XmppServerlessMessaging(string ApplicationName, string FullJid, ushort LocalPort, ushort ExternalPort, int Backlog,
			params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.fullJid = FullJid;
			this.p2pNetwork = new PeerToPeerNetwork(ApplicationName, LocalPort, ExternalPort, Backlog, Sniffers)
			{
				EncapsulatePackets = false
			};

			// TODO: Implement support for NAT-PMP

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
		/// Full JID
		/// </summary>
		public string FullJid
		{
			get { return this.fullJid; }
			set { this.fullJid = value; }
		}

		private void P2PNetwork_OnPeerConnected(object Listener, PeerConnection Peer)
		{
			PeerState State = new PeerState(Peer, this);
			this.Information("Peer connected from " + Peer.RemoteEndpoint.ToString());
		}

		/// <summary>
		/// Removes a JID from the recognized set of JIDs.
		/// </summary>
		/// <param name="FullJID">Full JID.</param>
		public void RemovePeerAddresses(string FullJID)
		{
			string ThisExternalIp = this.p2pNetwork.ExternalAddress is null ? string.Empty : this.p2pNetwork.ExternalAddress.ToString();

			lock (this.addressesByFullJid)
			{
				if (this.addressesByFullJid.TryGetValue(FullJID, out AddressInfo Info))
				{
					this.addressesByFullJid.Remove(FullJID);

					if (this.addressesByExternalIPPort.TryGetValue(Info.ExternalIp, out Dictionary<int, AddressInfo> Infos))
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
				else
					return;
			}

			this.Information("Removing JID from set of recognized JIDs: " + FullJID);

			PeerAddressEventHandler h = this.PeerAddressRemoved;
			if (h != null)
			{
				try
				{
					h(this, new PeerAddressEventArgs(FullJID, string.Empty, 0, string.Empty, 0));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when address information about a peer has been removed.
		/// </summary>
		public event PeerAddressEventHandler PeerAddressRemoved = null;

		/// <summary>
		/// Reports recognized peer addresses.
		/// </summary>
		/// <param name="FullJID">XMPP Address (full JID).</param>
		/// <param name="ExternalIp">External IP address.</param>
		/// <param name="ExternalPort">External Port number.</param>
		/// <param name="LocalIp">Local IP address.</param>
		/// <param name="LocalPort">Local Port number.</param>
		public void ReportPeerAddresses(string FullJID, string ExternalIp, int ExternalPort, string LocalIp, int LocalPort)
		{
			Dictionary<int, AddressInfo> Infos;
			string ThisExternalIp;

			if (this.p2pNetwork.ExternalAddress is null)
				ThisExternalIp = string.Empty;
			else
				ThisExternalIp = this.p2pNetwork.ExternalAddress.ToString();

			lock (this.addressesByFullJid)
			{
				if (this.addressesByFullJid.TryGetValue(FullJID, out AddressInfo Info))
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

				Info = new AddressInfo(FullJID, ExternalIp, ExternalPort, LocalIp, LocalPort);
				this.addressesByFullJid[FullJID] = Info;

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

			this.Information("P2P information available for " + FullJID + ". External: " + ExternalIp + ":" + ExternalPort.ToString() +
				", Local: " + LocalIp + ":" + LocalPort.ToString());

			PeerAddressEventHandler h = this.PeerAddressReceived;
			if (h != null)
			{
				try
				{
					h(this, new PeerAddressEventArgs(FullJID, ExternalIp, ExternalPort, LocalIp, LocalPort));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when address information about a peer has been received.
		/// </summary>
		public event PeerAddressEventHandler PeerAddressReceived = null;

		internal void AuthenticatePeer(PeerConnection Peer, string FullJID)
		{
			AddressInfo Info;

			lock (this.addressesByFullJid)
			{
				if (!this.addressesByFullJid.TryGetValue(FullJID, out Info))
					throw new XmppException("Peer JID " + FullJID + " not recognized.");
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

			// End-to-end encryption will asure communication is only read by the indended receiver.
		}

		internal void PeerAuthenticated(PeerState State)
		{
			lock (this.peersByFullJid)
			{
				this.peersByFullJid[State.RemoteFullJid] = State;
			}

			this.Information("Peer authenticated: " + State.RemoteFullJid);
		}

		internal void NewXmppClient(XmppClient Client, string LocalJid, string RemoteJid)
		{
			this.Information("Serverless XMPP connection established with " + RemoteJid);

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
			this.Information("Serverless XMPP connection with " + State.RemoteFullJid + " closed.");

			if (this.peersByFullJid != null)
			{
				lock (this.peersByFullJid)
				{
					if (this.peersByFullJid.TryGetValue(State.RemoteFullJid, out PeerState State2) && State2 == State)
						this.peersByFullJid.Remove(State.RemoteFullJid);
				}
			}
		}

		/// <summary>
		/// Gets a peer XMPP connection.
		/// </summary>
		/// <param name="FullJID">Bare JID of peer to connect to.</param>
		/// <param name="Callback">Method to call when connection is established.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetPeerConnection(string FullJID, PeerConnectionEventHandler Callback, object State)
		{
			this.GetPeerConnection(FullJID, Callback, State, this.OnResynch);
		}

		/// <summary>
		/// Event raised when the peer-to-peer connection parameters need to be updated for a given remote JID.
		/// </summary>
		public event ResynchEventHandler OnResynch = null;

		/// <summary>
		/// If it is possible to connect directly to a given peer, given it's bare JID.
		/// </summary>
		/// <param name="FullJID">Full JID.</param>
		/// <returns>If it is possible to connect directly to the peer.</returns>
		public bool CanConnectToPeer(string FullJID)
		{
			AddressInfo Info;

			lock (this.addressesByFullJid)
			{
				if (!this.addressesByFullJid.TryGetValue(FullJID, out Info))
					return false;
			}

			return !string.IsNullOrEmpty(Info.ExternalIp);
		}

		/// <summary>
		/// Gets peer-to-peer address information
		/// </summary>
		/// <param name="FullJID">Full JID</param>
		/// <param name="Address">IP address information</param>
		/// <returns></returns>
		public bool TryGetAddressInfo(string FullJID, out AddressInfo Address)
		{
			lock (this.addressesByFullJid)
			{
				return this.addressesByFullJid.TryGetValue(FullJID, out Address);
			}
		}

		private void GetPeerConnection(string FullJID, PeerConnectionEventHandler Callback, object State, ResynchEventHandler ResynchMethod)
		{
			PeerState Result;
			PeerState Old = null;
			AddressInfo Info;
			string Header = null;
			bool b;

			if (this.p2pNetwork is null || this.p2pNetwork.State != PeerToPeerNetworkState.Ready)
			{
				if (Callback != null)
				{
					try
					{
						Callback(this, new PeerConnectionEventArgs(null, State, this.fullJid, FullJID));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

				return;
			}

			lock (this.addressesByFullJid)
			{
				b = this.addressesByFullJid.TryGetValue(FullJID, out Info);
			}

			if (!b)
			{
				Callback(this, new PeerConnectionEventArgs(null, State, this.fullJid, FullJID));
				return;
			}

			lock (this.peersByFullJid)
			{
				b = this.peersByFullJid.TryGetValue(FullJID, out Result);

				if (b)
				{
					if (Result.AgeSeconds >= 30 && (Result.HasCallbacks || Result.XmppClient is null || !Result.Peer.Tcp.Connected))
					{
						this.peersByFullJid.Remove(FullJID);
						Old = Result;
						Result = null;
						b = false;
					}
					else if (Result.State != XmppState.Connected)
					{
						Result.AddCallback(Callback, State);
						return;
					}
				}

				if (!b)
				{
					Header = "<?xml version='1.0'?><stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' from='" +
						this.fullJid + "' to='" + FullJID + "' version='1.0'>";

					Result = new PeerState(null, this, FullJID, Header, "</stream:stream>", string.Empty, 1.0, Callback, State);
					this.peersByFullJid[FullJID] = Result;
				}
			}

			if (b)
			{
				try
				{
					Callback(this, new PeerConnectionEventArgs(Result.XmppClient, State, this.fullJid, FullJID));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				return;
			}
			else if (Old != null)
			{
				Old.CallCallbacks();
				Old.Dispose();
			}

			Task.Run(async () =>
			{
				PeerConnection Connection;

				try
				{
					Connection = await this.ConnectToAsync(FullJID, Info);
				}
				catch (Exception ex)
				{
					this.Error(ex.Message);
					Connection = null;

					if (ResynchMethod != null)
					{
						try
						{
							ResynchEventArgs e = new ResynchEventArgs(FullJID, (sender, e2) =>
							{
								try
								{
									if (e2.Ok)
										this.GetPeerConnection(FullJID, Callback, State, null);
									else
									{
										lock (this.peersByFullJid)
										{
											this.peersByFullJid.Remove(FullJID);
										}

										Result.CallCallbacks();
									}
								}
								catch (Exception ex2)
								{
									Log.Critical(ex2);
								}
							});

							ResynchMethod(this, e);
						}
						catch (Exception ex2)
						{
							Log.Critical(ex2);
						}

						return;
					}
				}

				if (Connection is null)
				{
					lock (this.peersByFullJid)
					{
						this.peersByFullJid.Remove(FullJID);
					}

					Result.CallCallbacks();
				}
				else
				{
					Result.Peer = Connection;
					Connection.Start((sender, e) =>
					{
						if (ResynchMethod != null)
						{
							try
							{
								ResynchMethod(this, new ResynchEventArgs(FullJID, async (sender2, e2) =>
								{
									try
									{
										if (e2.Ok)
										{
											Result.Peer = null;
											Connection = await this.ConnectToAsync(FullJID, Info);
											Result.Peer = Connection;
											Connection.Start();
											Result.HeaderSent = true;
											Result.Send(Header);
											this.TransmitText(Header);
										}
										else
											Result.CallCallbacks();
									}
									catch (Exception ex)
									{
										Log.Critical(ex);
									}
								}));
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
								Result.CallCallbacks();
							}
						}
						else
							Result.CallCallbacks();
					});
					Result.HeaderSent = true;
					Result.Send(Header);
					this.TransmitText(Header);
				}
			});
		}

		private async Task<PeerConnection> ConnectToAsync(string FullJID, AddressInfo Info)
		{
			PeerConnection Connection;
			IPAddress Addr;

			if (Info.ExternalIp == this.p2pNetwork.ExternalAddress.ToString())
			{
				if (IPAddress.TryParse(Info.LocalIp, out Addr))
				{
					this.Information("Connecting to " + Addr + ":" + Info.LocalPort.ToString() + " (" + FullJID + ")");
					Connection = await this.p2pNetwork.ConnectToPeer(new IPEndPoint(Addr, Info.LocalPort));
					this.Information("Connected to to " + Addr + ":" + Info.LocalPort.ToString() + " (" + FullJID + ")");
				}
				else
					Connection = null;
			}
			else
			{
				if (IPAddress.TryParse(Info.ExternalIp, out Addr))
				{
					this.Information("Connecting to " + Addr + ":" + Info.ExternalPort.ToString() + " (" + FullJID + ")");
					Connection = await this.p2pNetwork.ConnectToPeer(new IPEndPoint(Addr, Info.ExternalPort));
					this.Information("Connected to " + Addr + ":" + Info.ExternalPort.ToString() + " (" + FullJID + ")");
				}
				else
					Connection = null;
			}

			return Connection;
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

			if (this.peersByFullJid != null)
			{
				PeerState[] States;

				lock (this.peersByFullJid)
				{
					States = new PeerState[this.peersByFullJid.Count];
					this.peersByFullJid.Values.CopyTo(States, 0);

					this.peersByFullJid.Clear();
				}

				this.peersByFullJid = null;

				foreach (PeerState State in States)
				{
					State.ClearCallbacks();
					State.Close();
				}
			}
		}

		/// <summary>
		/// Appends P2P information to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public void AppendP2pInfo(StringBuilder Xml)
		{
			if (this.p2pNetwork != null && this.p2pNetwork.State == PeerToPeerNetworkState.Ready)
			{
				Xml.Append("<p2p xmlns='");
				Xml.Append(EndpointSecurity.IoTHarmonizationP2P);
				Xml.Append("' extIp='");
				Xml.Append(this.p2pNetwork.ExternalAddress.ToString());
				Xml.Append("' extPort='");
				Xml.Append(this.p2pNetwork.ExternalEndpoint.Port.ToString());
				Xml.Append("' locIp='");
				Xml.Append(this.p2pNetwork.LocalAddress.ToString());
				Xml.Append("' locPort='");
				Xml.Append(this.p2pNetwork.LocalEndpoint.Port.ToString());
				Xml.Append("'/>");
			}
		}

		/// <summary>
		/// Adds P2P address information about a peer.
		/// </summary>
		/// <param name="FullJID">Full JID of peer.</param>
		/// <param name="P2P">P2P address information.</param>
		/// <returns>If information was found and added.</returns>
		public bool AddPeerAddressInfo(string FullJID, XmlElement P2P)
		{
			string ExtIp = null;
			int? ExtPort = null;
			string LocIp = null;
			int? LocPort = null;

			if (P2P != null)
			{
				foreach (XmlAttribute Attr in P2P.Attributes)
				{
					switch (Attr.Name)
					{
						case "extIp":
							ExtIp = Attr.Value;
							break;

						case "extPort":
							if (int.TryParse(Attr.Value, out int i) && i >= 0 && i < 65536)
								ExtPort = i;
							else
								return false;
							break;

						case "locIp":
							LocIp = Attr.Value;
							break;

						case "locPort":
							if (int.TryParse(Attr.Value, out i) && i >= 0 && i < 65536)
								LocPort = i;
							else
								return false;
							break;
					}
				}
			}

			if (ExtIp != null && ExtPort.HasValue && LocIp != null && LocPort.HasValue)
			{
				this.ReportPeerAddresses(FullJID, ExtIp, ExtPort.Value, LocIp, LocPort.Value);
				return true;
			}
			else
			{
				this.RemovePeerAddresses(FullJID);
				return false;
			}
		}

	}
}
