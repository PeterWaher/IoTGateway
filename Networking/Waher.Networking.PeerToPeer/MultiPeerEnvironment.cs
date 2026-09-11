//#define LineListener

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Waher.Events;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
#if LineListener
using Waher.Runtime.Console;
#endif

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// State of multi-peer environment.
	/// </summary>
	public enum MultiPeerState
	{
		/// <summary>
		/// Object created
		/// </summary>
		Created,

		/// <summary>
		/// Reinitializing after a network change.
		/// </summary>
		Reinitializing,

		/// <summary>
		/// Searching for Internet gateway.
		/// </summary>
		SearchingForGateway,

		/// <summary>
		/// Registering application in gateway.
		/// </summary>
		RegisteringApplicationInGateway,

		/// <summary>
		/// Peforms negotiation to find peers.
		/// </summary>
		FindingPeers,

		/// <summary>
		/// Creates inter-peer peer-to-peer connections.
		/// </summary>
		ConnectingPeers,

		/// <summary>
		/// Ready to interact.
		/// </summary>
		Ready,

		/// <summary>
		/// Unable to create a multi-peer environment.
		/// </summary>
		Error,

		/// <summary>
		/// Environment is closed
		/// </summary>
		Closed
	}

	/// <summary>
	/// Manages a multi-peer environment.
	/// </summary>
	public class MultiPeerEnvironment : IDisposableAsync
	{
		private PeerToPeerNetwork p2pNetwork;
		private MqttClient mqttConnection = null;
		private MultiPeerState state = MultiPeerState.Created;
		private ManualResetEvent ready = new ManualResetEvent(false);
		private ManualResetEvent error = new ManualResetEvent(false);
		private Exception exception;
		private Peer[] remotePeers = Array.Empty<Peer>();
		private int mqttTerminatedPacketIdentifier;
		private int peerCount = 1;
		private int connectionCount = 0;
		private readonly Peer localPeer;
		private readonly Dictionary<IPEndPoint, Peer> remotePeersByEndpoint = new Dictionary<IPEndPoint, Peer>();
		private readonly Dictionary<IPAddress, bool> remotePeerIPs = new Dictionary<IPAddress, bool>();
		private readonly Dictionary<Guid, Peer> peersById = new Dictionary<Guid, Peer>();
		private readonly SortedDictionary<int, Peer> remotePeersByIndex = new SortedDictionary<int, Peer>();
		private readonly string applicationName;
		private readonly string mqttServer;
		private readonly int mqttPort;
		private readonly string mqttNegotiationTopic;
		private readonly string mqttUserName;
		private readonly string mqttPassword;
		private readonly bool mqttTls;

		/// <summary>
		/// Manages a multi-peer environment.
		/// </summary>
		/// <param name="ApplicationName">Name of application.</param>
		/// <param name="AllowMultipleApplicationsOnSameMachine">Allow multiple application on the same machine.</param>
		/// <param name="MqttServer">MQTT server host.</param>
		/// <param name="MqttPort">MQTT port to use.</param>
		/// <param name="MqttTls">If TLS is to be used for the MQTT connection.</param>
		/// <param name="MqttUserName">MQTT user name.</param>
		/// <param name="MqttPassword">MQTT password.</param>
		/// <param name="MqttNegotiationTopic">MQTT topic to use for multipeer negotiation.</param>
		/// <param name="EstimatedMaxNrPeers">Estimated number of maximum peers.</param>
		/// <param name="PeerId">Peer ID.</param>
		/// <param name="PeerMetaInfo">Meta-information about peer.</param>
		public MultiPeerEnvironment(string ApplicationName, bool AllowMultipleApplicationsOnSameMachine,
			string MqttServer, int MqttPort, bool MqttTls, string MqttUserName, string MqttPassword,
			string MqttNegotiationTopic, int EstimatedMaxNrPeers, Guid PeerId, params KeyValuePair<string, string>[] PeerMetaInfo)
		{
			this.localPeer = new Peer(PeerId, new IPEndPoint(IPAddress.Any, 0), new IPEndPoint(IPAddress.Any, 0), PeerMetaInfo);
			this.peersById[PeerId] = this.localPeer;
			this.applicationName = ApplicationName;

			this.mqttServer = MqttServer;
			this.mqttPort = MqttPort;
			this.mqttTls = MqttTls;
			this.mqttUserName = MqttUserName;
			this.mqttPassword = MqttPassword;
			this.mqttNegotiationTopic = MqttNegotiationTopic;

			this.p2pNetwork = new PeerToPeerNetwork(AllowMultipleApplicationsOnSameMachine ? this.applicationName + " (" + PeerId.ToString() + ")" :
				this.applicationName, 0, 0, EstimatedMaxNrPeers);
			this.p2pNetwork.OnStateChange += this.P2PNetworkStateChange;
			this.p2pNetwork.OnPeerConnected += this.P2pNetwork_OnPeerConnected;
			this.p2pNetwork.OnUdpDatagramReceived += this.P2pNetwork_OnUdpDatagramReceived;
		}

		private async Task P2pNetwork_OnUdpDatagramReceived(object Sender, UdpDatagramEventArgs e)
		{
			Peer Peer;

			lock (this.remotePeersByEndpoint)
			{
				if (!this.remotePeersByEndpoint.TryGetValue(e.RemoteEndpoint, out Peer))
					return;
			}

			if (!(Peer.Connection is null))
				await Peer.Connection.UdpDatagramReceived(Sender, e);
		}

		private async Task P2PNetworkStateChange(object Sender, PeerToPeerNetworkState NewState)
		{
			switch (NewState)
			{
				case PeerToPeerNetworkState.Created:
					await this.SetState(MultiPeerState.Created);
					break;

				case PeerToPeerNetworkState.Reinitializing:
					await this.SetState(MultiPeerState.Reinitializing);
					break;

				case PeerToPeerNetworkState.SearchingForGateway:
					await this.SetState(MultiPeerState.SearchingForGateway);
					break;

				case PeerToPeerNetworkState.RegisteringApplicationInGateway:
					await this.SetState(MultiPeerState.RegisteringApplicationInGateway);
					break;

				case PeerToPeerNetworkState.Ready:
					try
					{
						this.exception = null;

						this.localPeer.SetEndpoints(this.p2pNetwork.ExternalEndpoint, this.p2pNetwork.LocalEndpoint);

						this.mqttConnection = new MqttClient(this.mqttServer, this.mqttPort, this.mqttTls, this.mqttUserName, this.mqttPassword);
						this.mqttConnection.OnConnectionError += this.MqttConnection_OnConnectionError;
						this.mqttConnection.OnError += this.MqttConnection_OnError;
						this.mqttConnection.OnStateChanged += this.MqttConnection_OnStateChanged;
						this.mqttConnection.OnContentReceived += this.MqttConnection_OnContentReceived;

						await this.SetState(MultiPeerState.FindingPeers);
					}
					catch (Exception ex)
					{
						this.exception = ex;
						await this.SetState(MultiPeerState.Error);
					}
					break;

				case PeerToPeerNetworkState.Error:
					this.exception = this.p2pNetwork.Exception;
					await this.SetState(MultiPeerState.Error);
					break;

				case PeerToPeerNetworkState.Closed:
					await this.SetState(MultiPeerState.Closed);
					break;
			}
		}

		private async Task MqttConnection_OnStateChanged(object Sender, MqttState NewState)
		{
			if (NewState == MqttState.Connected)
			{
				await this.mqttConnection.SUBSCRIBE(this.mqttNegotiationTopic);

				BinaryOutput Output = new BinaryOutput();
				Output.WriteByte(0);
				Output.WriteString16BitLen(this.applicationName);

				this.localPeer.SetEndpoints(this.p2pNetwork.ExternalEndpoint, this.p2pNetwork.LocalEndpoint);
				this.Serialize(this.localPeer, Output);

				await this.mqttConnection.PUBLISH(this.mqttNegotiationTopic, MqttQualityOfService.AtLeastOnce, false, Output);

#if LineListener
				ConsoleOut.WriteLine("Tx: HELLO(" + this.localPeer.ToString() + ")");
#endif
			}
		}

		private void Serialize(Peer Peer, BinaryOutput Output)
		{
			Output.WriteString16BitLen(Peer.PublicEndpoint.Address.ToString());
			Output.WriteUInt16((ushort)Peer.PublicEndpoint.Port);

			Output.WriteString16BitLen(Peer.LocalEndpoint.Address.ToString());
			Output.WriteUInt16((ushort)Peer.LocalEndpoint.Port);

			Output.WriteGuid(Peer.PeerId);
			Output.WriteVarLenUInt((uint)Peer.Count);

			foreach (KeyValuePair<string, string> P in Peer)
			{
				Output.WriteString16BitLen(P.Key);
				Output.WriteString16BitLen(P.Value);
			}
		}

		private Peer Deserialize(BinaryInput Input)
		{
			IPAddress PublicAddress = IPAddress.Parse(Input.ReadString16BitLen());
			ushort PublicPort = Input.ReadUInt16();
			IPEndPoint PublicEndpoint = new IPEndPoint(PublicAddress, PublicPort);

			IPAddress LocalAddress = IPAddress.Parse(Input.ReadString16BitLen());
			ushort LocalPort = Input.ReadUInt16();
			IPEndPoint LocalEndpoint = new IPEndPoint(LocalAddress, LocalPort);

			Guid PeerId = Input.ReadGuid();
			bool LocalPeer = PeerId == this.localPeer.PeerId;
			int i, c = (int)Input.ReadVarLenUInt();
			KeyValuePair<string, string>[] PeerMetaInfo = LocalPeer ? null : new KeyValuePair<string, string>[c];
			string Key, Value;

			for (i = 0; i < c; i++)
			{
				Key = Input.ReadString16BitLen();
				Value = Input.ReadString16BitLen();
				if (!LocalPeer)
					PeerMetaInfo[i] = new KeyValuePair<string, string>(Key, Value);
			}

			if (LocalPeer)
				return null;
			else
				return new Peer(PeerId, PublicEndpoint, LocalEndpoint, PeerMetaInfo);
		}

		private async Task MqttConnection_OnContentReceived(object Sender, MqttContent Content)
		{
			BinaryInput Input = Content.DataInput;
			byte Command = Input.ReadByte();

			switch (Command)
			{
				case 0: // Hello
					string ApplicationName = Input.ReadString16BitLen();
					if (ApplicationName != this.applicationName)
						break;

					Peer Peer = this.Deserialize(Input);
					if (Peer is null)
						break;

#if LineListener
					ConsoleOut.WriteLine("Rx: HELLO(" + Peer.ToString() + ")");
#endif
					IPEndPoint ExpectedEndpoint = Peer.GetExpectedEndpoint(this.p2pNetwork);

					lock (this.remotePeersByEndpoint)
					{
						this.remotePeersByEndpoint[ExpectedEndpoint] = Peer;
						this.remotePeerIPs[ExpectedEndpoint.Address] = true;
						this.peersById[Peer.PeerId] = Peer;

						this.UpdateRemotePeersLocked();
					}

					await this.OnPeerAvailable.Raise(this, Peer);
					break;

				case 1:     // Interconnect
					ApplicationName = Input.ReadString16BitLen();
					if (ApplicationName != this.applicationName)
						break;

					Peer = this.Deserialize(Input);
					if (Peer is null)
						break;

#if LineListener
					ConsoleOut.Write("Rx: INTERCONNECT(" + Peer.ToString());
#endif
					int Index = 0;
					int i, c;
					LinkedList<Peer> Peers = new LinkedList<Peer>();
					bool LocalPeerIncluded = false;

					Peer.Index = Index++;
					Peers.AddLast(Peer);

					c = (int)Input.ReadVarLenUInt();
					for (i = 0; i < c; i++)
					{
						Peer = this.Deserialize(Input);
						if (Peer is null)
						{
#if LineListener
							ConsoleOut.Write("," + this.localPeer.ToString());
#endif
							this.localPeer.Index = Index++;
							LocalPeerIncluded = true;
						}
						else
						{
#if LineListener
							ConsoleOut.Write("," + Peer.ToString());
#endif
							Peer.Index = Index++;
							Peers.AddLast(Peer);
						}
					}

#if LineListener
					ConsoleOut.WriteLine(")");
#endif
					if (!LocalPeerIncluded)
						break;

					await this.mqttConnection.DisposeAsync();
					this.mqttConnection = null;

					lock (this.remotePeersByEndpoint)
					{
						this.remotePeersByEndpoint.Clear();
						this.remotePeerIPs.Clear();
						this.remotePeersByIndex.Clear();
						this.peersById.Clear();

						this.remotePeersByIndex[this.localPeer.Index] = this.localPeer;
						this.peersById[this.localPeer.PeerId] = this.localPeer;

						foreach (Peer Peer2 in Peers)
						{
							ExpectedEndpoint = Peer2.GetExpectedEndpoint(this.p2pNetwork);

							this.remotePeersByIndex[Peer2.Index] = Peer2;
							this.remotePeersByEndpoint[ExpectedEndpoint] = Peer2;
							this.remotePeerIPs[ExpectedEndpoint.Address] = true;
							this.peersById[Peer2.PeerId] = Peer2;
						}

						this.UpdateRemotePeersLocked();
					}

					await this.SetState(MultiPeerState.ConnectingPeers);
					await this.StartConnecting();
					break;

				case 2:     // Bye
					ApplicationName = Input.ReadString16BitLen();
					if (ApplicationName != this.applicationName)
						break;

					Guid PeerId = Input.ReadGuid();
					lock (this.remotePeersByEndpoint)
					{
						if (!this.peersById.TryGetValue(PeerId, out Peer))
							break;

#if LineListener
						ConsoleOut.WriteLine("Rx: BYE(" + Peer.ToString() + ")");
#endif
						ExpectedEndpoint = Peer.GetExpectedEndpoint(this.p2pNetwork);

						this.peersById.Remove(PeerId);
						this.remotePeersByEndpoint.Remove(ExpectedEndpoint);
						this.remotePeersByIndex.Remove(Peer.Index);

						IPAddress ExpectedAddress = ExpectedEndpoint.Address;
						bool AddressFound = false;

						foreach (IPEndPoint EP in this.remotePeersByEndpoint.Keys)
						{
							if (IPAddress.Equals(EP.Address, ExpectedAddress))
							{
								AddressFound = true;
								break;
							}
						}

						if (!AddressFound)
							this.remotePeerIPs.Remove(ExpectedAddress);

						this.UpdateRemotePeersLocked();
					}
					break;
			}
		}

		private void UpdateRemotePeersLocked()
		{
			int c = this.remotePeersByEndpoint.Count;

			this.peerCount = 1 + c;
			this.remotePeers = new Peer[c];
			this.remotePeersByEndpoint.Values.CopyTo(this.remotePeers, 0);
		}

		private async Task P2pNetwork_OnPeerConnected(object Listener, PeerConnection Peer)
		{
			IPEndPoint Endpoint = (IPEndPoint)Peer.Tcp.Client.Client.RemoteEndPoint;

#if LineListener
			ConsoleOut.WriteLine("Receiving connection from " + Endpoint.ToString());
#endif

			bool Dispose = false;

			lock (this.remotePeersByEndpoint)
			{
				if (!this.remotePeerIPs.ContainsKey(Endpoint.Address))
					Dispose = true;
			}

			if (Dispose)
			{
				await Peer.DisposeAsync();
				return;
			}

			Peer.OnClosed += this.Peer_OnClosed;
			Peer.OnReceived += this.Peer_OnReceived;

			BinaryOutput Output = new BinaryOutput();

			Output.WriteGuid(this.localPeer.PeerId);
			Output.WriteString16BitLen(this.ExternalEndpoint.Address.ToString());
			Output.WriteUInt16((ushort)this.ExternalEndpoint.Port);

			await Peer.SendTcp(true, Output.GetPacket());
		}

		private async Task<bool> Peer_OnReceived(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			PeerConnection Connection = (PeerConnection)Sender;
			Peer Peer;
			byte[] Packet;

			if (Connection.StateObject is null)
			{
				BinaryInput Input = new BinaryInput(Buffer, Offset, Count);
				Guid PeerId;
				IPAddress PeerRemoteAddress;
				IPEndPoint PeerRemoteEndpoint;

				try
				{
					PeerId = Input.ReadGuid();
					PeerRemoteAddress = IPAddress.Parse(Input.ReadString16BitLen());
					PeerRemoteEndpoint = new IPEndPoint(PeerRemoteAddress, Input.ReadUInt16());
				}
				catch (Exception)
				{
					if (!(Connection is null))
						await Connection.DisposeAsync();

					return true;
				}

				if (Input.BytesLeft == 0)
					Packet = null;
				else
					Packet = Input.GetRemainingData();

				bool AllConnected = false;
				bool DisposeConnection = false;
				PeerConnection ObsoleteConnection = null;

				lock (this.remotePeersByEndpoint)
				{
					if (!this.peersById.TryGetValue(PeerId, out Peer))
						DisposeConnection = true;
					else
					{
						if (Peer.Connection is null)
							this.connectionCount++;
						else
							ObsoleteConnection = Peer.Connection;

						Peer.Connection = Connection;
						Connection.StateObject = Peer;
						Connection.RemoteEndpoint = Peer.GetExpectedEndpoint(this.p2pNetwork);

						AllConnected = this.connectionCount + 1 == this.peerCount;
					}
				}

				if (DisposeConnection)
				{
					if (!(Connection is null))
						await Connection.DisposeAsync();

					return true;
				}

                if (!(ObsoleteConnection is null))
					await ObsoleteConnection.DisposeAsync();

				await this.OnPeerConnected.Raise(this, Peer);

				if (AllConnected)
					await this.SetState(MultiPeerState.Ready);

				if (Packet is null)
					return true;
			}
			else
			{
				Peer = (Peer)Connection.StateObject;
				Packet = SnifferBase.CloneSection(Buffer, Offset, Count);
			}

			await this.PeerDataReceived(Peer, Connection, Packet);

			return true;
		}

		/// <summary>
		/// Is called when peer data has been received.
		/// </summary>
		/// <param name="FromPeer">Data came from this peer.</param>
		/// <param name="Connection">Data came over this connection.</param>
		/// <param name="Packet">Data received.</param>
		protected virtual Task PeerDataReceived(Peer FromPeer, PeerConnection Connection, byte[] Packet)
		{
			return this.OnPeerDataReceived.Raise(this, new PeerDataEventArgs(FromPeer, Connection, Packet));
		}

		/// <summary>
		/// Event raised when peer data has been received from a peer.
		/// </summary>
		public event EventHandlerAsync<PeerDataEventArgs> OnPeerDataReceived = null;

		/// <summary>
		/// Sends a packet to all remote peers using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task SendTcpToAll(byte[] Packet)
		{
			return this.SendTcpToAll(false, Packet);
		}

		/// <summary>
		/// Sends a packet to all remote peers using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		public async Task SendTcpToAll(bool ConstantBuffer, byte[] Packet)
		{
			if (this.state != MultiPeerState.Ready)
				throw new Exception("The multipeer environment is not ready to exchange data between peers.");

			PeerConnection Connection;
			foreach (Peer Peer in this.remotePeers)
			{
				if (!((Connection = Peer.Connection) is null))
					await Connection.SendTcp(ConstantBuffer, Packet);
			}
		}

		/// <summary>
		/// Sends a packet to a specific peer using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="Peer">Peer to send the packet to.</param>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task SendTcpTo(Peer Peer, byte[] Packet)
		{
			return this.SendTcpTo(Peer, false, Packet);
		}

		/// <summary>
		/// Sends a packet to a specific peer using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="Peer">Peer to send the packet to.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		public Task SendTcpTo(Peer Peer, bool ConstantBuffer, byte[] Packet)
		{
			if (this.state != MultiPeerState.Ready)
				throw new Exception("The multipeer environment is not ready to exchange data between peers.");

			PeerConnection Connection = Peer.Connection;
			return Connection?.SendTcp(ConstantBuffer, Packet) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Sends a packet to a specific peer using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="PeerId">ID of peer to send the packet to.</param>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task SendTcpTo(Guid PeerId, byte[] Packet)
		{
			return this.SendTcpTo(PeerId, false, Packet);
		}

		/// <summary>
		/// Sends a packet to a specific peer using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="PeerId">ID of peer to send the packet to.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		public Task SendTcpTo(Guid PeerId, bool ConstantBuffer, byte[] Packet)
		{
			Peer Peer;

			lock (this.remotePeersByEndpoint)
			{
				if (!this.peersById.TryGetValue(PeerId, out Peer))
					throw new ArgumentException("No peer with that ID.", nameof(PeerId));
			}

			PeerConnection Connection = Peer.Connection;
			return Connection?.SendTcp(ConstantBuffer, Packet) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Sends a packet to all remote peers using UDP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="IncludeNrPreviousPackets">Number of previous packets to include in the datagram. Note that the network limits
		/// total size of datagram packets.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		public async Task SendUdpToAll(byte[] Packet, int IncludeNrPreviousPackets)
		{
			if (this.state != MultiPeerState.Ready)
				throw new Exception("The multipeer environment is not ready to exchange data between peers.");

			PeerConnection Connection;
			foreach (Peer Peer in this.remotePeers)
			{
				if (!((Connection = Peer.Connection) is null))
					await Connection.SendUdp(Packet, IncludeNrPreviousPackets);
			}
		}

		/// <summary>
		/// Sends a packet to a specific peer using UDP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="Peer">Peer to send the packet to.</param>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="IncludeNrPreviousPackets">Number of previous packets to include in the datagram. Note that the network limits
		/// total size of datagram packets.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		public Task SendUdpTo(Peer Peer, byte[] Packet, int IncludeNrPreviousPackets)
		{
			if (this.state != MultiPeerState.Ready)
				throw new Exception("The multipeer environment is not ready to exchange data between peers.");

			PeerConnection Connection = Peer.Connection;
			return Connection?.SendUdp(Packet, IncludeNrPreviousPackets) ?? Task.CompletedTask;
		}

		/// <summary>
		/// Sends a packet to a specific peer using UDP. Can only be done if <see cref="State"/>=<see cref="MultiPeerState.Ready"/>.
		/// </summary>
		/// <param name="PeerId">ID of peer to send the packet to.</param>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="IncludeNrPreviousPackets">Number of previous packets to include in the datagram. Note that the network limits
		/// total size of datagram packets.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPeerState.Ready"/>.</exception>
		public Task SendUdpTo(Guid PeerId, byte[] Packet, int IncludeNrPreviousPackets)
		{
			Peer Peer;

			lock (this.remotePeersByEndpoint)
			{
				if (!this.peersById.TryGetValue(PeerId, out Peer))
					throw new ArgumentException("No peer with that ID.", nameof(PeerId));
			}

			PeerConnection Connection = Peer.Connection;
			return Connection?.SendUdp(Packet, IncludeNrPreviousPackets) ?? Task.CompletedTask;
		}

		private async Task Peer_OnClosed(object Sender, EventArgs e)
		{
			PeerConnection Connection = (PeerConnection)Sender;
			Peer Peer = (Peer)Connection.StateObject;
			if (Peer is null)
				return;

			if (Peer.Connection != Connection)
				return;

			lock (this.remotePeersByEndpoint)
			{
				Peer.Connection = null;
				this.connectionCount--;

				Connection.StateObject = null;
			}

			await this.OnPeerDisconnected.Raise(this, Peer);
		}

		/// <summary>
		/// Event raised when a new peer is available.
		/// </summary>
		public event EventHandlerAsync<Peer> OnPeerAvailable = null;

		/// <summary>
		/// Event raised when a peer has been connected to the local macine.
		/// </summary>
		public event EventHandlerAsync<Peer> OnPeerConnected = null;

		/// <summary>
		/// Event raised when a peer has been disconnected from the local macine.
		/// </summary>
		public event EventHandlerAsync<Peer> OnPeerDisconnected = null;

		/// <summary>
		/// Creates inter-peer peer-to-peer connections between known peers.
		/// </summary>
		public async Task ConnectPeers()
		{
			if (this.state != MultiPeerState.FindingPeers)
				throw new Exception("The multipeer environment is not in the state of finding peers.");

			await this.SetState(MultiPeerState.ConnectingPeers);

			int Index = 0;
			BinaryOutput Output = new BinaryOutput();
			Output.WriteByte(1);
			Output.WriteString16BitLen(this.applicationName);
			this.localPeer.Index = Index++;
			this.Serialize(this.localPeer, Output);

#if LineListener
			ConsoleOut.Write("Tx: INTERCONNECT(" + this.localPeer.ToString());
#endif
			lock (this.remotePeersByEndpoint)
			{
				Output.WriteVarLenUInt((uint)this.remotePeersByEndpoint.Count);

				foreach (Peer Peer in this.remotePeersByEndpoint.Values)
				{
					Peer.Index = Index++;
					this.Serialize(Peer, Output);

#if LineListener
					ConsoleOut.Write("," + Peer.ToString());
#endif
				}
			}

			this.mqttTerminatedPacketIdentifier = await this.mqttConnection.PUBLISH(this.mqttNegotiationTopic, MqttQualityOfService.AtLeastOnce, false, Output);
			this.mqttConnection.OnPublished += this.MqttConnection_OnPublished;

#if LineListener
			ConsoleOut.WriteLine(")");
#endif
			await this.StartConnecting();
		}

		private async Task StartConnecting()
		{
#if LineListener
			ConsoleOut.WriteLine("Current peer has index " + this.localPeer.Index.ToString());
#endif
			if (this.remotePeers.Length == 0)
				await this.SetState(MultiPeerState.Ready);
			else
			{
				foreach (Peer Peer in this.remotePeers)
				{
					if (Peer.Index < this.localPeer.Index)
					{
#if LineListener
						ConsoleOut.WriteLine("Connecting to " + Peer.ToString() + " (index " + Peer.Index.ToString() + ")");
#endif
						PeerConnection Connection = await this.p2pNetwork.ConnectToPeer(Peer.PublicEndpoint);

						Connection.StateObject = Peer;
						Connection.OnClosed += this.Peer_OnClosed;
						Connection.OnReceived += this.Connection_OnReceived;

						Connection.Start();
					}
					else
					{
#if LineListener
						ConsoleOut.WriteLine("Waiting for connection from " + Peer.ToString() + " (index " + Peer.Index.ToString() + ")");
#endif
					}
				}
			}
		}

		private async Task<bool> Connection_OnReceived(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			PeerConnection Connection = (PeerConnection)Sender;
			Guid PeerId;
			IPAddress PeerRemoteAddress;
			IPEndPoint PeerRemoteEndpoint;

			try
			{
				BinaryInput Input = new BinaryInput(Buffer, Offset, Count);

				PeerId = Input.ReadGuid();
				PeerRemoteAddress = IPAddress.Parse(Input.ReadString16BitLen());
				PeerRemoteEndpoint = new IPEndPoint(PeerRemoteAddress, Input.ReadUInt16());
			}
			catch (Exception)
			{
				await Connection.DisposeAsync();
				return true;
			}

			Peer Peer = (Peer)Connection.StateObject;
			bool DisposeConnection = false;

			lock (this.remotePeersByEndpoint)
			{
				if (!this.peersById.TryGetValue(PeerId, out Peer Peer2) || Peer2.PeerId != Peer.PeerId)
					DisposeConnection = true;
				else
					Peer.Connection = Connection;
			}

			if (DisposeConnection)
			{
				await Connection.DisposeAsync();
				return true;
			}

			Connection.RemoteEndpoint = Peer.GetExpectedEndpoint(this.p2pNetwork);

			Connection.OnReceived -= this.Connection_OnReceived;
			Connection.OnReceived += this.Peer_OnReceived;
			Connection.OnSent += this.Connection_OnSent;

			BinaryOutput Output = new BinaryOutput();

			Output.WriteGuid(this.localPeer.PeerId);
			Output.WriteString16BitLen(this.ExternalAddress.ToString());
			Output.WriteUInt16((ushort)this.ExternalEndpoint.Port);

			await Connection.SendTcp(true, Output.GetPacket());

			await this.OnPeerConnected.Raise(this, Peer);

			return true;
		}

		private async Task<bool> Connection_OnSent(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			PeerConnection Connection = (PeerConnection)Sender;
			Peer Peer = (Peer)Connection.StateObject;
			bool AllConnected;

			Connection.OnSent -= this.Connection_OnSent;

			bool DisposePeerConnection = false;

			lock (this.remotePeersByEndpoint)
			{
				if (Peer.Connection == Connection)
					this.connectionCount++;
				else
					DisposePeerConnection = true;

				AllConnected = this.connectionCount + 1 == this.peerCount;
			}

			if (DisposePeerConnection)
				await Peer.Connection.DisposeAsync();

			if (AllConnected)
				await this.SetState(MultiPeerState.Ready);

			return true;
		}

		private async Task MqttConnection_OnError(object Sender, Exception Exception)
		{
			this.exception = Exception;
			await this.SetState(MultiPeerState.Error);
		}

		private async Task MqttConnection_OnConnectionError(object Sender, Exception Exception)
		{
			this.exception = Exception;
			await this.SetState(MultiPeerState.Error);
		}

		/// <summary>
		/// Current state of the multi-peer environment.
		/// </summary>
		public MultiPeerState State => this.state;

		internal async Task SetState(MultiPeerState NewState)
		{
			if (this.state != NewState)
			{
				this.state = NewState;

				switch (NewState)
				{
					case MultiPeerState.Ready:
						this.ready.Set();
						break;

					case MultiPeerState.Error:
						this.error.Set();
						break;
				}

				await this.OnStateChange.Raise(this, NewState);
			}
		}

		/// <summary>
		/// Event raised when the state of the peer-to-peer network changes.
		/// </summary>
		public event EventHandlerAsync<MultiPeerState> OnStateChange = null;

		/// <summary>
		/// Application Name
		/// </summary>
		public string ApplicationName => this.applicationName;

		/// <summary>
		/// External IP Address.
		/// </summary>
		public IPAddress ExternalAddress
		{
			get { return this.p2pNetwork.ExternalAddress; }
		}

		/// <summary>
		/// External IP Endpoint.
		/// </summary>
		public IPEndPoint ExternalEndpoint
		{
			get { return this.p2pNetwork.ExternalEndpoint; }
		}

		/// <summary>
		/// Local IP Address.
		/// </summary>
		public IPAddress LocalAddress
		{
			get { return this.p2pNetwork.LocalAddress; }
		}

		/// <summary>
		/// Local IP Endpoint.
		/// </summary>
		public IPEndPoint LocalEndpoint
		{
			get { return this.p2pNetwork.LocalEndpoint; }
		}

		/// <summary>
		/// In case <see cref="State"/>=<see cref="MultiPeerState.Error"/>, this exception object contains details about the error.
		/// </summary>
		public Exception Exception => this.exception;

		/// <summary>
		/// Waits for the multi-peer environment object to be ready to play.
		/// </summary>
		/// <returns>true, if environment is ready to play, false if an error has occurred.</returns>
		public bool Wait()
		{
			return this.Wait(10000);
		}

		/// <summary>
		/// Waits for the multi-peer environment object to be ready to play.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds. Default=10000.</param>
		/// <returns>true, if environment is ready to play, false if an error has occurred, or the environment could not be setup in the allotted time frame.</returns>
		public bool Wait(int TimeoutMilliseconds)
		{
			return WaitHandle.WaitAny(new WaitHandle[] { this.ready, this.error }, TimeoutMilliseconds) switch
			{
				0 => true,
				_ => false,
			};
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use the DisposeAsync() method.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public async Task DisposeAsync()
		{ 
			await this.CloseMqtt();

			await this.SetState(MultiPeerState.Closed);

			if (!(this.p2pNetwork is null))
			{
				await this.p2pNetwork.DisposeAsync();
				this.p2pNetwork = null;
			}

			this.ready?.Dispose();
			this.ready = null;

			this.error?.Dispose();
			this.error = null;

			if (!(this.remotePeersByEndpoint is null))
			{
				Peer[] ToDispose;

				lock (this.remotePeersByEndpoint)
				{
					this.peersById.Clear();
					this.remotePeersByIndex.Clear();

					ToDispose = new Peer[this.remotePeersByEndpoint.Count];
					this.remotePeersByEndpoint.Values.CopyTo(ToDispose, 0);

					this.remotePeersByEndpoint.Clear();
					this.remotePeers = null;
				}

				foreach (Peer Peer in ToDispose)
				{
					if (!(Peer.Connection is null))
						await Peer.Connection.DisposeAsync();
				}
			}
		}

		private async Task CloseMqtt()
		{
			if (!(this.mqttConnection is null))
			{
				if (this.mqttConnection.State == MqttState.Connected)
				{
					BinaryOutput Output = new BinaryOutput();
					Output.WriteByte(2);
					Output.WriteString16BitLen(this.applicationName);
					Output.WriteGuid(this.localPeer.PeerId);

					this.mqttTerminatedPacketIdentifier = await this.mqttConnection.PUBLISH(this.mqttNegotiationTopic, MqttQualityOfService.AtLeastOnce, false, Output);
					this.mqttConnection.OnPublished += this.MqttConnection_OnPublished;

#if LineListener
					ConsoleOut.WriteLine("Tx: BYE(" + this.localPeer.ToString() + ")");
#endif
				}
				else
				{
					await this.mqttConnection.DisposeAsync();
					this.mqttConnection = null;
				}
			}
		}

		private async Task MqttConnection_OnPublished(object Sender, ushort PacketIdentifier)
		{
			if (!(this.mqttConnection is null) && PacketIdentifier == this.mqttTerminatedPacketIdentifier)
			{
				await this.mqttConnection.DisposeAsync();
				this.mqttConnection = null;
			}
		}

		/// <summary>
		/// Number of peers
		/// </summary>
		public int PeerCount => this.peerCount;

		/// <summary>
		/// If the local peer is the first peer in the list of peers. Can be used to determine which machine controls peer logic.
		/// </summary>
		public bool LocalPeerIsFirst
		{
			get { return this.localPeer.Index == 0; }
		}

	}
}
