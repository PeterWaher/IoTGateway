//#define LineListener

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Waher.Events;
using Waher.Networking.MQTT;
using Waher.Script.Operators.Membership;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// State of multi-player environment.
	/// </summary>
	public enum MultiPlayerState
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
		/// Peforms negotiation to find players.
		/// </summary>
		FindingPlayers,

		/// <summary>
		/// Creates inter-player peer-to-peer connections.
		/// </summary>
		ConnectingPlayers,

		/// <summary>
		/// Ready to play.
		/// </summary>
		Ready,

		/// <summary>
		/// Unable to create a multi-player environment.
		/// </summary>
		Error,

		/// <summary>
		/// Environment is closed
		/// </summary>
		Closed
	}

	/// <summary>
	/// Event handler for multi-player environment state change events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewState">New state.</param>
	public delegate Task MultiPlayerEnvironmentStateChangeEventHandler(object Sender, MultiPlayerState NewState);

	/// <summary>
	/// Event handler for player information events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="PlayerInformation">Player information.</param>
	public delegate Task MultiPlayerEnvironmentPlayerInformationEventHandler(object Sender, Player PlayerInformation);

	/// <summary>
	/// Event handler for game data events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task GameDataEventHandler(object Sender, GameDataEventArgs e);

	/// <summary>
	/// Manages a multi-player environment.
	/// </summary>
	public class MultiPlayerEnvironment : IDisposable
	{
		private PeerToPeerNetwork p2pNetwork;
		private MqttClient mqttConnection = null;
		private MultiPlayerState state = MultiPlayerState.Created;
		private ManualResetEvent ready = new ManualResetEvent(false);
		private ManualResetEvent error = new ManualResetEvent(false);
		private Exception exception;
		private Player[] remotePlayers = new Player[0];
		private int mqttTerminatedPacketIdentifier;
		private int playerCount = 1;
		private int connectionCount = 0;
		private readonly Player localPlayer;
		private readonly Dictionary<IPEndPoint, Player> remotePlayersByEndpoint = new Dictionary<IPEndPoint, Player>();
		private readonly Dictionary<IPAddress, bool> remotePlayerIPs = new Dictionary<IPAddress, bool>();
		private readonly Dictionary<Guid, Player> playersById = new Dictionary<Guid, Player>();
		private readonly SortedDictionary<int, Player> remotePlayersByIndex = new SortedDictionary<int, Player>();
		private readonly string applicationName;
		private readonly string mqttServer;
		private readonly int mqttPort;
		private readonly string mqttNegotiationTopic;
		private readonly string mqttUserName;
		private readonly string mqttPassword;
		private readonly bool mqttTls;

		/// <summary>
		/// Manages a multi-player environment.
		/// </summary>
		/// <param name="ApplicationName">Name of application.</param>
		/// <param name="AllowMultipleApplicationsOnSameMachine">Allow multiple application on the same machine.</param>
		/// <param name="MqttServer">MQTT server host.</param>
		/// <param name="MqttPort">MQTT port to use.</param>
		/// <param name="MqttTls">If TLS is to be used for the MQTT connection.</param>
		/// <param name="MqttUserName">MQTT user name.</param>
		/// <param name="MqttPassword">MQTT password.</param>
		/// <param name="MqttNegotiationTopic">MQTT topic to use for multiplayer negotiation.</param>
		/// <param name="EstimatedMaxNrPlayers">Estimated number of maximum players.</param>
		/// <param name="PlayerId">Player ID.</param>
		/// <param name="PlayerMetaInfo">Meta-information about player.</param>
		public MultiPlayerEnvironment(string ApplicationName, bool AllowMultipleApplicationsOnSameMachine,
			string MqttServer, int MqttPort, bool MqttTls, string MqttUserName, string MqttPassword,
			string MqttNegotiationTopic, int EstimatedMaxNrPlayers, Guid PlayerId, params KeyValuePair<string, string>[] PlayerMetaInfo)
		{
			this.localPlayer = new Player(PlayerId, new IPEndPoint(IPAddress.Any, 0), new IPEndPoint(IPAddress.Any, 0), PlayerMetaInfo);
			this.playersById[PlayerId] = this.localPlayer;
			this.applicationName = ApplicationName;

			this.mqttServer = MqttServer;
			this.mqttPort = MqttPort;
			this.mqttTls = MqttTls;
			this.mqttUserName = MqttUserName;
			this.mqttPassword = MqttPassword;
			this.mqttNegotiationTopic = MqttNegotiationTopic;

			this.p2pNetwork = new PeerToPeerNetwork(AllowMultipleApplicationsOnSameMachine ? this.applicationName + " (" + PlayerId.ToString() + ")" :
				this.applicationName, 0, 0, EstimatedMaxNrPlayers);
			this.p2pNetwork.OnStateChange += this.P2PNetworkStateChange;
			this.p2pNetwork.OnPeerConnected += this.P2pNetwork_OnPeerConnected;
			this.p2pNetwork.OnUdpDatagramReceived += this.P2pNetwork_OnUdpDatagramReceived;
		}

		private async Task P2pNetwork_OnUdpDatagramReceived(object Sender, UdpDatagramEventArgs e)
		{
			Player Player;

			lock (this.remotePlayersByEndpoint)
			{
				if (!this.remotePlayersByEndpoint.TryGetValue(e.RemoteEndpoint, out Player))
					return;
			}

			if (Player.Connection != null)
				await Player.Connection.UdpDatagramReceived(Sender, e);
		}

		private async Task P2PNetworkStateChange(object Sender, PeerToPeerNetworkState NewState)
		{
			switch (NewState)
			{
				case PeerToPeerNetworkState.Created:
					await this.SetState(MultiPlayerState.Created);
					break;

				case PeerToPeerNetworkState.Reinitializing:
					await this.SetState(MultiPlayerState.Reinitializing);
					break;

				case PeerToPeerNetworkState.SearchingForGateway:
					await this.SetState(MultiPlayerState.SearchingForGateway);
					break;

				case PeerToPeerNetworkState.RegisteringApplicationInGateway:
					await this.SetState(MultiPlayerState.RegisteringApplicationInGateway);
					break;

				case PeerToPeerNetworkState.Ready:
					try
					{
						this.exception = null;

						this.localPlayer.SetEndpoints(this.p2pNetwork.ExternalEndpoint, this.p2pNetwork.LocalEndpoint);

						this.mqttConnection = new MqttClient(this.mqttServer, this.mqttPort, this.mqttTls, this.mqttUserName, this.mqttPassword);
						this.mqttConnection.OnConnectionError += this.MqttConnection_OnConnectionError;
						this.mqttConnection.OnError += this.MqttConnection_OnError;
						this.mqttConnection.OnStateChanged += this.MqttConnection_OnStateChanged;
						this.mqttConnection.OnContentReceived += this.MqttConnection_OnContentReceived;

						await this.SetState(MultiPlayerState.FindingPlayers);
					}
					catch (Exception ex)
					{
						this.exception = ex;
						await this.SetState(MultiPlayerState.Error);
					}
					break;

				case PeerToPeerNetworkState.Error:
					this.exception = this.p2pNetwork.Exception;
					await this.SetState(MultiPlayerState.Error);
					break;

				case PeerToPeerNetworkState.Closed:
					await this.SetState(MultiPlayerState.Closed);
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
				Output.WriteString(this.applicationName);

				this.localPlayer.SetEndpoints(this.p2pNetwork.ExternalEndpoint, this.p2pNetwork.LocalEndpoint);
				this.Serialize(this.localPlayer, Output);

				await this.mqttConnection.PUBLISH(this.mqttNegotiationTopic, MqttQualityOfService.AtLeastOnce, false, Output);

#if LineListener
				Console.Out.WriteLine("Tx: HELLO(" + this.localPlayer.ToString() + ")");
#endif
			}
		}

		private void Serialize(Player Player, BinaryOutput Output)
		{
			Output.WriteString(Player.PublicEndpoint.Address.ToString());
			Output.WriteUInt16((ushort)Player.PublicEndpoint.Port);

			Output.WriteString(Player.LocalEndpoint.Address.ToString());
			Output.WriteUInt16((ushort)Player.LocalEndpoint.Port);

			Output.WriteGuid(Player.PlayerId);
			Output.WriteUInt((uint)Player.Count);

			foreach (KeyValuePair<string, string> P in Player)
			{
				Output.WriteString(P.Key);
				Output.WriteString(P.Value);
			}
		}

		private Player Deserialize(BinaryInput Input)
		{
			IPAddress PublicAddress = IPAddress.Parse(Input.ReadString());
			ushort PublicPort = Input.ReadUInt16();
			IPEndPoint PublicEndpoint = new IPEndPoint(PublicAddress, PublicPort);

			IPAddress LocalAddress = IPAddress.Parse(Input.ReadString());
			ushort LocalPort = Input.ReadUInt16();
			IPEndPoint LocalEndpoint = new IPEndPoint(LocalAddress, LocalPort);

			Guid PlayerId = Input.ReadGuid();
			bool LocalPlayer = PlayerId == this.localPlayer.PlayerId;
			int i, c = (int)Input.ReadUInt();
			KeyValuePair<string, string>[] PlayerMetaInfo = LocalPlayer ? null : new KeyValuePair<string, string>[c];
			string Key, Value;

			for (i = 0; i < c; i++)
			{
				Key = Input.ReadString();
				Value = Input.ReadString();
				if (!LocalPlayer)
					PlayerMetaInfo[i] = new KeyValuePair<string, string>(Key, Value);
			}

			if (LocalPlayer)
				return null;
			else
				return new Player(PlayerId, PublicEndpoint, LocalEndpoint, PlayerMetaInfo);
		}

		private async Task MqttConnection_OnContentReceived(object Sender, MqttContent Content)
		{
			BinaryInput Input = Content.DataInput;
			byte Command = Input.ReadByte();

			switch (Command)
			{
				case 0: // Hello
					string ApplicationName = Input.ReadString();
					if (ApplicationName != this.applicationName)
						break;

					Player Player = this.Deserialize(Input);
					if (Player is null)
						break;

#if LineListener
					Console.Out.WriteLine("Rx: HELLO(" + Player.ToString() + ")");
#endif
					IPEndPoint ExpectedEndpoint = Player.GetExpectedEndpoint(this.p2pNetwork);

					lock (this.remotePlayersByEndpoint)
					{
						this.remotePlayersByEndpoint[ExpectedEndpoint] = Player;
						this.remotePlayerIPs[ExpectedEndpoint.Address] = true;
						this.playersById[Player.PlayerId] = Player;

						this.UpdateRemotePlayersLocked();
					}

					MultiPlayerEnvironmentPlayerInformationEventHandler h = this.OnPlayerAvailable;
					if (!(h is null))
					{
						try
						{
							await h(this, Player);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
					break;

				case 1:     // Interconnect
					ApplicationName = Input.ReadString();
					if (ApplicationName != this.applicationName)
						break;

					Player = this.Deserialize(Input);
					if (Player is null)
						break;

#if LineListener
					Console.Out.Write("Rx: INTERCONNECT(" + Player.ToString());
#endif
					int Index = 0;
					int i, c;
					LinkedList<Player> Players = new LinkedList<Player>();
					bool LocalPlayerIncluded = false;

					Player.Index = Index++;
					Players.AddLast(Player);

					c = (int)Input.ReadUInt();
					for (i = 0; i < c; i++)
					{
						Player = this.Deserialize(Input);
						if (Player is null)
						{
#if LineListener
							Console.Out.Write("," + this.localPlayer.ToString());
#endif
							this.localPlayer.Index = Index++;
							LocalPlayerIncluded = true;
						}
						else
						{
#if LineListener
							Console.Out.Write("," + Player.ToString());
#endif
							Player.Index = Index++;
							Players.AddLast(Player);
						}
					}

#if LineListener
					Console.Out.WriteLine(")");
#endif
					if (!LocalPlayerIncluded)
						break;

					this.mqttConnection.Dispose();
					this.mqttConnection = null;

					lock (this.remotePlayersByEndpoint)
					{
						this.remotePlayersByEndpoint.Clear();
						this.remotePlayerIPs.Clear();
						this.remotePlayersByIndex.Clear();
						this.playersById.Clear();

						this.remotePlayersByIndex[this.localPlayer.Index] = this.localPlayer;
						this.playersById[this.localPlayer.PlayerId] = this.localPlayer;

						foreach (Player Player2 in Players)
						{
							ExpectedEndpoint = Player2.GetExpectedEndpoint(this.p2pNetwork);

							this.remotePlayersByIndex[Player2.Index] = Player2;
							this.remotePlayersByEndpoint[ExpectedEndpoint] = Player2;
							this.remotePlayerIPs[ExpectedEndpoint.Address] = true;
							this.playersById[Player2.PlayerId] = Player2;
						}

						this.UpdateRemotePlayersLocked();
					}

					await this.SetState(MultiPlayerState.ConnectingPlayers);
					await this.StartConnecting();
					break;

				case 2:     // Bye
					ApplicationName = Input.ReadString();
					if (ApplicationName != this.applicationName)
						break;

					Guid PlayerId = Input.ReadGuid();
					lock (this.remotePlayersByEndpoint)
					{
						if (!this.playersById.TryGetValue(PlayerId, out Player))
							break;

#if LineListener
						Console.Out.WriteLine("Rx: BYE(" + Player.ToString() + ")");
#endif
						ExpectedEndpoint = Player.GetExpectedEndpoint(this.p2pNetwork);

						this.playersById.Remove(PlayerId);
						this.remotePlayersByEndpoint.Remove(ExpectedEndpoint);
						this.remotePlayersByIndex.Remove(Player.Index);

						IPAddress ExpectedAddress = ExpectedEndpoint.Address;
						bool AddressFound = false;

						foreach (IPEndPoint EP in this.remotePlayersByEndpoint.Keys)
						{
							if (IPAddress.Equals(EP.Address, ExpectedAddress))
							{
								AddressFound = true;
								break;
							}
						}

						if (!AddressFound)
							this.remotePlayerIPs.Remove(ExpectedAddress);

						this.UpdateRemotePlayersLocked();
					}
					break;
			}
		}

		private void UpdateRemotePlayersLocked()
		{
			int c = this.remotePlayersByEndpoint.Count;

			this.playerCount = 1 + c;
			this.remotePlayers = new Player[c];
			this.remotePlayersByEndpoint.Values.CopyTo(this.remotePlayers, 0);
		}

		private async Task P2pNetwork_OnPeerConnected(object Listener, PeerConnection Peer)
		{
			IPEndPoint Endpoint = (IPEndPoint)Peer.Tcp.Client.Client.RemoteEndPoint;

#if LineListener
			Console.Out.WriteLine("Receiving connection from " + Endpoint.ToString());
#endif

			lock (this.remotePlayersByEndpoint)
			{
				if (!this.remotePlayerIPs.ContainsKey(Endpoint.Address))
				{
					Peer.Dispose();
					return;
				}
			}

			Peer.OnClosed += this.Peer_OnClosed;
			Peer.OnReceived += this.Peer_OnReceived;

			BinaryOutput Output = new BinaryOutput();

			Output.WriteGuid(this.localPlayer.PlayerId);
			Output.WriteString(this.ExternalEndpoint.Address.ToString());
			Output.WriteUInt16((ushort)this.ExternalEndpoint.Port);

			await Peer.SendTcp(Output.GetPacket());
		}

		private async Task<bool> Peer_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			PeerConnection Connection = (PeerConnection)Sender;
			Player Player;
			byte[] Packet;

			if (Connection.StateObject is null)
			{
				BinaryInput Input = new BinaryInput(BinaryTcpClient.ToArray(Buffer, Offset, Count));
				Guid PlayerId;
				IPAddress PlayerRemoteAddress;
				IPEndPoint PlayerRemoteEndpoint;

				try
				{
					PlayerId = Input.ReadGuid();
					PlayerRemoteAddress = IPAddress.Parse(Input.ReadString());
					PlayerRemoteEndpoint = new IPEndPoint(PlayerRemoteAddress, Input.ReadUInt16());
				}
				catch (Exception)
				{
					Connection.Dispose();
					return true;
				}

				if (Input.BytesLeft == 0)
					Packet = null;
				else
					Packet = Input.GetRemainingData();

				bool AllConnected;

				lock (this.remotePlayersByEndpoint)
				{
					if (!this.playersById.TryGetValue(PlayerId, out Player))
					{
						Connection.Dispose();
						return true;
					}

					if (Player.Connection is null)
						this.connectionCount++;
					else
						Player.Connection.Dispose();

					Player.Connection = Connection;
					Connection.StateObject = Player;
					Connection.RemoteEndpoint = Player.GetExpectedEndpoint(this.p2pNetwork);

					AllConnected = this.connectionCount + 1 == this.playerCount;
				}

				MultiPlayerEnvironmentPlayerInformationEventHandler h = this.OnPlayerConnected;
				if (!(h is null))
				{
					try
					{
						await h(this, Player);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

				if (AllConnected)
					await this.SetState(MultiPlayerState.Ready);

				if (Packet is null)
					return true;
			}
			else
			{
				Player = (Player)Connection.StateObject;
				Packet = BinaryTcpClient.ToArray(Buffer, Offset, Count);
			}

			this.GameDataReceived(Player, Connection, Packet);

			return true;
		}

		/// <summary>
		/// Is called when game data has been received.
		/// </summary>
		/// <param name="FromPlayer">Data came from this player.</param>
		/// <param name="Connection">Data came over this connection.</param>
		/// <param name="Packet">Data received.</param>
		protected virtual void GameDataReceived(Player FromPlayer, PeerConnection Connection, byte[] Packet)
		{
			GameDataEventHandler h = this.OnGameDataReceived;
			if (!(h is null))
			{
				try
				{
					h(this, new GameDataEventArgs(FromPlayer, Connection, Packet));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when game data has been received from a player.
		/// </summary>
		public event GameDataEventHandler OnGameDataReceived = null;

		/// <summary>
		/// Sends a packet to all remote players using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPlayerState.Ready"/>.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPlayerState.Ready"/>.</exception>
		public void SendTcpToAll(byte[] Packet)
		{
			if (this.state != MultiPlayerState.Ready)
				throw new Exception("The multiplayer environment is not ready to exchange data between players.");

			PeerConnection Connection;
			foreach (Player Player in this.remotePlayers)
			{
				if ((Connection = Player.Connection) != null)
					Connection.SendTcp(Packet);
			}
		}

		/// <summary>
		/// Sends a packet to a specific player using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPlayerState.Ready"/>.
		/// </summary>
		/// <param name="Player">Player to send the packet to.</param>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPlayerState.Ready"/>.</exception>
		public void SendTcpTo(Player Player, byte[] Packet)
		{
			if (this.state != MultiPlayerState.Ready)
				throw new Exception("The multiplayer environment is not ready to exchange data between players.");

			PeerConnection Connection = Player.Connection;
			if (!(Connection is null))
				Connection.SendTcp(Packet);
		}

		/// <summary>
		/// Sends a packet to a specific player using TCP. Can only be done if <see cref="State"/>=<see cref="MultiPlayerState.Ready"/>.
		/// </summary>
		/// <param name="PlayerId">ID of player to send the packet to.</param>
		/// <param name="Packet">Packet to send.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPlayerState.Ready"/>.</exception>
		public void SendTcpTo(Guid PlayerId, byte[] Packet)
		{
			Player Player;

			lock (this.remotePlayersByEndpoint)
			{
				if (!this.playersById.TryGetValue(PlayerId, out Player))
					throw new ArgumentException("No player with that ID.", nameof(PlayerId));
			}

			PeerConnection Connection = Player.Connection;
			if (!(Connection is null))
				Connection.SendTcp(Packet);
		}

		/// <summary>
		/// Sends a packet to all remote players using UDP. Can only be done if <see cref="State"/>=<see cref="MultiPlayerState.Ready"/>.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="IncludeNrPreviousPackets">Number of previous packets to include in the datagram. Note that the network limits
		/// total size of datagram packets.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPlayerState.Ready"/>.</exception>
		public void SendUdpToAll(byte[] Packet, int IncludeNrPreviousPackets)
		{
			if (this.state != MultiPlayerState.Ready)
				throw new Exception("The multiplayer environment is not ready to exchange data between players.");

			PeerConnection Connection;
			foreach (Player Player in this.remotePlayers)
			{
				if ((Connection = Player.Connection) != null)
					Connection.SendUdp(Packet, IncludeNrPreviousPackets);
			}
		}

		/// <summary>
		/// Sends a packet to a specific player using UDP. Can only be done if <see cref="State"/>=<see cref="MultiPlayerState.Ready"/>.
		/// </summary>
		/// <param name="Player">Player to send the packet to.</param>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="IncludeNrPreviousPackets">Number of previous packets to include in the datagram. Note that the network limits
		/// total size of datagram packets.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPlayerState.Ready"/>.</exception>
		public void SendUdpTo(Player Player, byte[] Packet, int IncludeNrPreviousPackets)
		{
			if (this.state != MultiPlayerState.Ready)
				throw new Exception("The multiplayer environment is not ready to exchange data between players.");

			PeerConnection Connection = Player.Connection;
			if (!(Connection is null))
				Connection.SendUdp(Packet, IncludeNrPreviousPackets);
		}

		/// <summary>
		/// Sends a packet to a specific player using UDP. Can only be done if <see cref="State"/>=<see cref="MultiPlayerState.Ready"/>.
		/// </summary>
		/// <param name="PlayerId">ID of player to send the packet to.</param>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="IncludeNrPreviousPackets">Number of previous packets to include in the datagram. Note that the network limits
		/// total size of datagram packets.</param>
		/// <exception cref="Exception">If <see cref="State"/>!=<see cref="MultiPlayerState.Ready"/>.</exception>
		public void SendUdpTo(Guid PlayerId, byte[] Packet, int IncludeNrPreviousPackets)
		{
			Player Player;

			lock (this.remotePlayersByEndpoint)
			{
				if (!this.playersById.TryGetValue(PlayerId, out Player))
					throw new ArgumentException("No player with that ID.", nameof(PlayerId));
			}

			PeerConnection Connection = Player.Connection;
			if (!(Connection is null))
				Connection.SendUdp(Packet, IncludeNrPreviousPackets);
		}

		private void Peer_OnClosed(object sender, EventArgs e)
		{
			PeerConnection Connection = (PeerConnection)sender;
			Player Player = (Player)Connection.StateObject;
			if (Player is null)
				return;

			if (Player.Connection != Connection)
				return;

			lock (this.remotePlayersByEndpoint)
			{
				Player.Connection = null;
				this.connectionCount--;

				Connection.StateObject = null;
			}

			MultiPlayerEnvironmentPlayerInformationEventHandler h = this.OnPlayerDisconnected;
			if (!(h is null))
			{
				try
				{
					h(this, Player);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

		}

		/// <summary>
		/// Event raised when a new player is available.
		/// </summary>
		public event MultiPlayerEnvironmentPlayerInformationEventHandler OnPlayerAvailable = null;

		/// <summary>
		/// Event raised when a player has been connected to the local macine.
		/// </summary>
		public event MultiPlayerEnvironmentPlayerInformationEventHandler OnPlayerConnected = null;

		/// <summary>
		/// Event raised when a player has been disconnected from the local macine.
		/// </summary>
		public event MultiPlayerEnvironmentPlayerInformationEventHandler OnPlayerDisconnected = null;

		/// <summary>
		/// Creates inter-player peer-to-peer connections between known players.
		/// </summary>
		public async Task ConnectPlayers()
		{
			if (this.state != MultiPlayerState.FindingPlayers)
				throw new Exception("The multiplayer environment is not in the state of finding players.");

			await this.SetState(MultiPlayerState.ConnectingPlayers);

			int Index = 0;
			BinaryOutput Output = new BinaryOutput();
			Output.WriteByte(1);
			Output.WriteString(this.applicationName);
			this.localPlayer.Index = Index++;
			this.Serialize(this.localPlayer, Output);

#if LineListener
			Console.Out.Write("Tx: INTERCONNECT(" + this.localPlayer.ToString());
#endif
			lock (this.remotePlayersByEndpoint)
			{
				Output.WriteUInt((uint)this.remotePlayersByEndpoint.Count);

				foreach (Player Player in this.remotePlayersByEndpoint.Values)
				{
					Player.Index = Index++;
					this.Serialize(Player, Output);

#if LineListener
					Console.Out.Write("," + Player.ToString());
#endif
				}
			}

			this.mqttTerminatedPacketIdentifier = await this.mqttConnection.PUBLISH(this.mqttNegotiationTopic, MqttQualityOfService.AtLeastOnce, false, Output);
			this.mqttConnection.OnPublished += this.MqttConnection_OnPublished;

#if LineListener
			Console.Out.WriteLine(")");
#endif
			await this.StartConnecting();
		}

		private async Task StartConnecting()
		{
#if LineListener
			Console.Out.WriteLine("Current player has index " + this.localPlayer.Index.ToString());
#endif
			if (this.remotePlayers.Length == 0)
				await this.SetState(MultiPlayerState.Ready);
			else
			{
				foreach (Player Player in this.remotePlayers)
				{
					if (Player.Index < this.localPlayer.Index)
					{
#if LineListener
						Console.Out.WriteLine("Connecting to " + Player.ToString() + " (index " + Player.Index.ToString() + ")");
#endif
						PeerConnection Connection = await this.p2pNetwork.ConnectToPeer(Player.PublicEndpoint);

						Connection.StateObject = Player;
						Connection.OnClosed += this.Peer_OnClosed;
						Connection.OnReceived += this.Connection_OnReceived;

						Connection.Start();
					}
					else
					{
#if LineListener
						Console.Out.WriteLine("Waiting for connection from " + Player.ToString() + " (index " + Player.Index.ToString() + ")");
#endif
					}
				}
			}
		}

		private async Task<bool> Connection_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			PeerConnection Connection = (PeerConnection)Sender;
			Guid PlayerId;
			IPAddress PlayerRemoteAddress;
			IPEndPoint PlayerRemoteEndpoint;

			try
			{
				BinaryInput Input = new BinaryInput(BinaryTcpClient.ToArray(Buffer, Offset, Count));

				PlayerId = Input.ReadGuid();
				PlayerRemoteAddress = IPAddress.Parse(Input.ReadString());
				PlayerRemoteEndpoint = new IPEndPoint(PlayerRemoteAddress, Input.ReadUInt16());
			}
			catch (Exception)
			{
				Connection.Dispose();
				return true;
			}

			Player Player = (Player)Connection.StateObject;

			lock (this.remotePlayersByEndpoint)
			{
				if (!this.playersById.TryGetValue(PlayerId, out Player Player2) || Player2.PlayerId != Player.PlayerId)
				{
					Connection.Dispose();
					return true;
				}

				Player.Connection = Connection;
			}

			Connection.RemoteEndpoint = Player.GetExpectedEndpoint(this.p2pNetwork);

			Connection.OnReceived -= this.Connection_OnReceived;
			Connection.OnReceived += this.Peer_OnReceived;
			Connection.OnSent += this.Connection_OnSent;

			BinaryOutput Output = new BinaryOutput();

			Output.WriteGuid(this.localPlayer.PlayerId);
			Output.WriteString(this.ExternalAddress.ToString());
			Output.WriteUInt16((ushort)this.ExternalEndpoint.Port);

			await Connection.SendTcp(Output.GetPacket());

			MultiPlayerEnvironmentPlayerInformationEventHandler h = this.OnPlayerConnected;
			if (!(h is null))
			{
				try
				{
					await h(this, Player);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			return true;
		}

		private async Task<bool> Connection_OnSent(object Sender, byte[] Buffer, int Offset, int Count)
		{
			PeerConnection Connection = (PeerConnection)Sender;
			Player Player = (Player)Connection.StateObject;
			bool AllConnected;

			Connection.OnSent -= this.Connection_OnSent;

			lock (this.remotePlayersByEndpoint)
			{
				if (Player.Connection == Connection)
					this.connectionCount++;
				else
					Player.Connection.Dispose();

				AllConnected = this.connectionCount + 1 == this.playerCount;
			}

			if (AllConnected)
				await this.SetState(MultiPlayerState.Ready);

			return true;
		}

		private async Task MqttConnection_OnError(object Sender, Exception Exception)
		{
			this.exception = Exception;
			await this.SetState(MultiPlayerState.Error);
		}

		private async Task MqttConnection_OnConnectionError(object Sender, Exception Exception)
		{
			this.exception = Exception;
			await this.SetState(MultiPlayerState.Error);
		}

		/// <summary>
		/// Current state of the multi-player environment.
		/// </summary>
		public MultiPlayerState State
		{
			get { return this.state; }
		}

		internal async Task SetState(MultiPlayerState NewState)
		{
			if (this.state != NewState)
			{
				this.state = NewState;

				switch (NewState)
				{
					case MultiPlayerState.Ready:
						this.ready.Set();
						break;

					case MultiPlayerState.Error:
						this.error.Set();
						break;
				}

				MultiPlayerEnvironmentStateChangeEventHandler h = this.OnStateChange;
				if (!(h is null))
				{
					try
					{
						await h(this, NewState);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when the state of the peer-to-peer network changes.
		/// </summary>
		public event MultiPlayerEnvironmentStateChangeEventHandler OnStateChange = null;

		/// <summary>
		/// Application Name
		/// </summary>
		public string ApplicationName
		{
			get { return this.applicationName; }
		}

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
		/// In case <see cref="State"/>=<see cref="MultiPlayerState.Error"/>, this exception object contains details about the error.
		/// </summary>
		public Exception Exception
		{
			get { return this.exception; }
		}

		/// <summary>
		/// Waits for the multi-player environment object to be ready to play.
		/// </summary>
		/// <returns>true, if environment is ready to play, false if an error has occurred.</returns>
		public bool Wait()
		{
			return this.Wait(10000);
		}

		/// <summary>
		/// Waits for the multi-player environment object to be ready to play.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds. Default=10000.</param>
		/// <returns>true, if environment is ready to play, false if an error has occurred, or the environment could not be setup in the allotted time frame.</returns>
		public bool Wait(int TimeoutMilliseconds)
		{
			switch (WaitHandle.WaitAny(new WaitHandle[] { this.ready, this.error }, TimeoutMilliseconds))
			{
				case 0:
					return true;

				case 1:
				default:
					return false;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			Task _ = this.DisposeAsync();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public async Task DisposeAsync()
		{ 
			await this.CloseMqtt();

			await this.SetState(MultiPlayerState.Closed);

			this.p2pNetwork?.Dispose();
			this.p2pNetwork = null;

			this.ready?.Dispose();
			this.ready = null;

			this.error?.Dispose();
			this.error = null;

			if (this.remotePlayersByEndpoint != null)
			{
				lock (this.remotePlayersByEndpoint)
				{
					this.playersById.Clear();
					this.remotePlayersByIndex.Clear();

					foreach (Player Player in this.remotePlayersByEndpoint.Values)
					{
						if (Player.Connection != null)
							Player.Connection.Dispose();
					}

					this.remotePlayersByEndpoint.Clear();
					this.remotePlayers = null;
				}
			}
		}

		private async Task CloseMqtt()
		{
			if (this.mqttConnection != null)
			{
				if (this.mqttConnection.State == MqttState.Connected)
				{
					BinaryOutput Output = new BinaryOutput();
					Output.WriteByte(2);
					Output.WriteString(this.applicationName);
					Output.WriteGuid(this.localPlayer.PlayerId);

					this.mqttTerminatedPacketIdentifier = await this.mqttConnection.PUBLISH(this.mqttNegotiationTopic, MqttQualityOfService.AtLeastOnce, false, Output);
					this.mqttConnection.OnPublished += this.MqttConnection_OnPublished;

#if LineListener
					Console.Out.WriteLine("Tx: BYE(" + this.localPlayer.ToString() + ")");
#endif
				}
				else
				{
					this.mqttConnection.Dispose();
					this.mqttConnection = null;
				}
			}
		}

		private Task MqttConnection_OnPublished(object Sender, ushort PacketIdentifier)
		{
			if (this.mqttConnection != null && PacketIdentifier == this.mqttTerminatedPacketIdentifier)
			{
				this.mqttConnection.Dispose();
				this.mqttConnection = null;
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Number of players
		/// </summary>
		public int PlayerCount
		{
			get { return this.playerCount; }
		}

		/// <summary>
		/// If the local player is the first player in the list of players. Can be used to determine which machine controls game logic.
		/// </summary>
		public bool LocalPlayerIsFirst
		{
			get { return this.localPlayer.Index == 0; }
		}

	}
}
