using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.UPnP;
using Waher.Networking.UPnP.Services;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// State of Peer-to-peer network.
	/// </summary>
	public enum PeerToPeerNetworkState
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
		/// Ready to receive connections.
		/// </summary>
		Ready,

		/// <summary>
		/// Unable to create a peer-to-peer network that receives connections from the Internet.
		/// </summary>
		Error,

		/// <summary>
		/// Network is closed
		/// </summary>
		Closed
	}

	/// <summary>
	/// Event handler for peer-to-peer network state change events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewState">New state.</param>
	public delegate void PeerToPeerNetworkStateChangeEventHandler(object Sender, PeerToPeerNetworkState NewState);

	/// <summary>
	/// Event handler whenever a peer has connected.
	/// </summary>
	/// <param name="Listener">Sender of event.</param>
	/// <param name="Peer">Peer connection.</param>
	public delegate void PeerConnectedEventHandler(object Listener, PeerConnection Peer);

	/// <summary>
	/// Manages a peer-to-peer network that can receive connections from outside of a NAT-enabled firewall.
	/// </summary>
	public class PeerToPeerNetwork : InternetGatewayRegistrator
	{
		/// <summary>
		/// Default desired port number. (0 = any port number.)
		/// </summary>
		public const ushort DefaultPort = 0;

		/// <summary>
		/// Default connection backlog (10).
		/// </summary>
		public const int DefaultBacklog = 10;

		private readonly LinkedList<KeyValuePair<IPEndPoint, byte[]>> writeQueue = new LinkedList<KeyValuePair<IPEndPoint, byte[]>>();
		private TcpListener tcpListener;
		private UdpClient udpClient;
		private IPEndPoint localEndpoint;
		private IPEndPoint externalEndpoint;
		private readonly int backlog;
		private bool encapsulatePackets = true;
		private bool isWriting = false;

		/// <summary>
		/// Manages a peer-to-peer network that can receive connections from outside of a NAT-enabled firewall.
		/// </summary>
		/// <param name="ApplicationName">Name of Peer-to-Peer application. Any NAT port mappings in the firewall
		/// having the same name and pointing to the same machine will be removed. To allow multiple port mappings on
		/// the same machine, each mapping needs a unique name.</param>
		/// <param name="Sniffers">Sniffers</param>
		public PeerToPeerNetwork(string ApplicationName, params ISniffer[] Sniffers)
			: this(ApplicationName, DefaultPort, DefaultPort, DefaultBacklog, Sniffers)
		{
		}

		/// <summary>
		/// Manages a peer-to-peer network that can receive connections from outside of a NAT-enabled firewall.
		/// </summary>
		/// <param name="ApplicationName">Name of Peer-to-Peer application. Any NAT port mappings in the firewall
		/// having the same name and pointing to the same machine will be removed. To allow multiple port mappings on
		/// the same machine, each mapping needs a unique name.</param>
		/// <param name="LocalPort">Desired local port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="ExternalPort">Desired external port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="Sniffers">Sniffers</param>
		public PeerToPeerNetwork(string ApplicationName, ushort LocalPort, ushort ExternalPort, params ISniffer[] Sniffers)
			: this(ApplicationName, LocalPort, ExternalPort, DefaultBacklog, Sniffers)
		{
		}

		/// <summary>
		/// Manages a peer-to-peer network that can receive connections from outside of a NAT-enabled firewall.
		/// </summary>
		/// <param name="ApplicationName">Name of Peer-to-Peer application. Any NAT port mappings in the firewall
		/// having the same name and pointing to the same machine will be removed. To allow multiple port mappings on
		/// the same machine, each mapping needs a unique name.</param>
		/// <param name="LocalPort">Desired local port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="ExternalPort">Desired external port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="Backlog">Connection backlog.</param>
		/// <param name="Sniffers">Sniffers</param>
		public PeerToPeerNetwork(string ApplicationName, ushort LocalPort, ushort ExternalPort, int Backlog, params ISniffer[] Sniffers)
			: base(new InternetGatewayRegistration[] { new InternetGatewayRegistration()
			{
				ApplicationName = ApplicationName,
				LocalPort = LocalPort,
				ExternalPort = ExternalPort,
				Tcp = true,
				Udp = true
			} }, Sniffers)
		{
			this.backlog = Backlog;

			this.tcpListener = null;
			this.udpClient = null;

			this.Start();
		}

		/// <summary>
		/// Starts searching for Internet Gateways. Once found, ports will be registered.
		/// </summary>
		public override void Start()
		{
			if (this.OnPublicNetwork())
			{
				try
				{
					ushort PublicPort;

					this.tcpListener = new TcpListener(this.localAddress, this.ports[0].ExternalPort);
					this.tcpListener.Start(this.backlog);

					PublicPort = (ushort)((IPEndPoint)this.tcpListener.LocalEndpoint).Port;

					this.localEndpoint = new IPEndPoint(this.localAddress, PublicPort);
					this.externalEndpoint = new IPEndPoint(this.externalAddress, PublicPort);

					this.udpClient = new UdpClient(this.localEndpoint.AddressFamily);
					this.udpClient.Client.Bind(this.localEndpoint);

					this.State = PeerToPeerNetworkState.Ready;

					this.AcceptTcpClients();
					this.BeginReceiveUdp();
				}
				catch (Exception ex)
				{
					this.exception = ex;
					this.State = PeerToPeerNetworkState.Error;

					this.tcpListener?.Stop();
					this.tcpListener = null;

					this.udpClient?.Dispose();
					this.udpClient = null;
				}
			}

			base.Start();
		}

		private async void AcceptTcpClients()
		{
			try
			{
				while (!this.disposed)
				{
					try
					{
						TcpClient Client = await this.tcpListener.AcceptTcpClientAsync();
						if (this.disposed)
							return;

						if (!(Client is null))
						{
							PeerConnection Connection = null;

							try
							{
								Connection = new PeerConnection(Client, this,
									(IPEndPoint)Client.Client.RemoteEndPoint, this.encapsulatePackets);

								this.State = PeerToPeerNetworkState.Ready;

								this.PeerConnected(Connection);

								Connection.Start();
							}
							catch (Exception)
							{
								Connection?.Dispose();
							}
						}
					}
					catch (SocketException)
					{
						// Ignore
					}
					catch (ObjectDisposedException)
					{
						// Ignore
					}
					catch (NullReferenceException)
					{
						// Ignore
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			catch (Exception ex)
			{
				if (this.disposed)
					return;

				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Desired local port number. If 0, a dynamic port number will be assigned.
		/// </summary>
		public ushort DesiredLocalPort
		{
			get { return this.ports[0].LocalPort; }
			set { this.ports[0].LocalPort = value; }
		}

		/// <summary>
		/// Desired external port number. If 0, a dynamic port number will be assigned.
		/// </summary>
		public ushort DesiredExternalPort
		{
			get { return this.ports[0].ExternalPort; }
			set { this.ports[0].ExternalPort = value; }
		}

		/// <summary>
		/// Application Name
		/// </summary>
		public string ApplicationName
		{
			get { return this.ports[0].ApplicationName; }
		}

		/// <summary>
		/// External IP Endpoint.
		/// </summary>
		public IPEndPoint ExternalEndpoint
		{
			get { return this.externalEndpoint; }
		}

		/// <summary>
		/// Local IP Endpoint.
		/// </summary>
		public IPEndPoint LocalEndpoint
		{
			get { return this.localEndpoint; }
		}

		/// <summary>
		/// If packets are to be encapsulated and delivered as ordered units (true), or if fragmentation in the 
		/// TCP case, or reordering of received datagrams in the UDP case, are allowed (false).
		/// </summary>
		public bool EncapsulatePackets
		{
			get { return this.encapsulatePackets; }
			set { this.encapsulatePackets = value; }
		}

		/// <summary>
		/// is called before performing a registration.
		/// </summary>
		/// <param name="Registration">Registration to be performed.</param>
		protected override void BeforeRegistration(InternetGatewayRegistration Registration, Dictionary<ushort, bool> TcpPortMapped, 
			Dictionary<ushort, bool> UdpPortMapped)
		{
			try
			{
				do
				{
					this.tcpListener = new TcpListener(this.localAddress, Registration.LocalPort);
					this.tcpListener.Start(this.backlog);

					int i = ((IPEndPoint)this.tcpListener.LocalEndpoint).Port;
					Registration.LocalPort = (ushort)i;
					if (Registration.ExternalPort == 0)
						Registration.ExternalPort = Registration.LocalPort;

					if (i < 0 || i > ushort.MaxValue ||
						TcpPortMapped.ContainsKey((ushort)Registration.ExternalPort) || 
						UdpPortMapped.ContainsKey((ushort)Registration.ExternalPort))
					{
						this.tcpListener.Stop();
						this.tcpListener = null;

						throw new ArgumentException("External port already assigned to another application in the network.", nameof(Registration));
					}
					else
					{
						try
						{
							this.udpClient = new UdpClient(this.tcpListener.LocalEndpoint.AddressFamily);
							this.udpClient.Client.Bind((IPEndPoint)this.tcpListener.LocalEndpoint);
						}
						catch (Exception)
						{
							this.tcpListener.Stop();
							this.tcpListener = null;
						}
					}
				}
				while (this.tcpListener is null);

				this.localEndpoint = new IPEndPoint(this.localAddress, Registration.LocalPort);
				this.externalEndpoint = new IPEndPoint(this.externalAddress, Registration.ExternalPort);

				this.AcceptTcpClients();
				this.BeginReceiveUdp();
			}
			catch (Exception ex)
			{
				this.exception = ex;
				this.State = PeerToPeerNetworkState.Error;
			}
		}

		/// <summary>
		/// Called when a new peer has connected.
		/// </summary>
		/// <param name="Connection">Peer connection</param>
		protected virtual void PeerConnected(PeerConnection Connection)
		{
			PeerConnectedEventHandler h = this.OnPeerConnected;
			if (h != null)
			{
				try
				{
					h(this, Connection);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a new peer has connected.
		/// </summary>
		public event PeerConnectedEventHandler OnPeerConnected = null;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			this.tcpListener?.Stop();
			this.tcpListener = null;

			this.udpClient?.Dispose();
			this.udpClient = null;

			base.Dispose();
		}

		/// <summary>
		/// Connects to a peer in the peer-to-peer network. If the remote end point resides behind the same firewall as the current application,
		/// a direct connection to the local peer is made, for improved performance.
		/// </summary>
		/// <param name="RemoteEndPoint">Remote End-point.</param>
		/// <returns>Peer connection</returns>
		public async Task<PeerConnection> ConnectToPeer(IPEndPoint RemoteEndPoint)
		{
			if (this.State != PeerToPeerNetworkState.Ready)
				throw new IOException("Peer-to-peer network not ready.");

			TcpClient Client = new TcpClient();
			IPEndPoint RemoteEndPoint2;

			try
			{
				RemoteEndPoint2 = this.CheckLocalRemoteEndpoint(RemoteEndPoint);
				await Client.ConnectAsync(RemoteEndPoint2.Address, RemoteEndPoint2.Port);
			}
			catch (Exception)
			{
				Client.Dispose();
				throw;
			}

			PeerConnection Result = new PeerConnection(Client, this, RemoteEndPoint2, this.encapsulatePackets);

			Result.StartIdleTimer();

			return Result;
		}

		private async void BeginReceiveUdp()
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await this.udpClient.ReceiveAsync();
					if (!this.disposed)
					{
						UdpDatagramEvent h = this.OnUdpDatagramReceived;
						if (h != null)
						{
							try
							{
								h(this, new UdpDatagramEventArgs(Data.RemoteEndPoint, Data.Buffer));
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when an incoming UDP datagram has been received.
		/// </summary>
		public event UdpDatagramEvent OnUdpDatagramReceived = null;

		/// <summary>
		/// Sends an UDP datagram to a remote destination.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint of destination.</param>
		/// <param name="Datagram">UDP Datagram to send.</param>
		public async void SendUdp(IPEndPoint RemoteEndpoint, byte[] Datagram)
		{
			lock (this.writeQueue)
			{
				if (this.isWriting)
				{
					this.writeQueue.AddLast(new KeyValuePair<IPEndPoint, byte[]>(RemoteEndpoint, Datagram));
					return;
				}
				else
					this.isWriting = true;
			}

			try
			{
				while (!this.disposed && Datagram != null)
				{
					await this.udpClient.SendAsync(Datagram, Datagram.Length, RemoteEndpoint);

					UdpDatagramEvent h = this.OnUdpDatagramSent;
					if (h != null)
					{
						try
						{
							h(this, new UdpDatagramEventArgs(RemoteEndpoint, Datagram));
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					lock (this.writeQueue)
					{
						if (this.writeQueue.First != null)
						{
							KeyValuePair<IPEndPoint, byte[]> Rec = this.writeQueue.First.Value;
							this.writeQueue.RemoveFirst();

							RemoteEndpoint = Rec.Key;
							Datagram = Rec.Value;
						}
						else
						{
							this.isWriting = false;
							Datagram = null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				lock (this.writeQueue)
				{
					this.isWriting = false;
					this.writeQueue.Clear();
				}

				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when an outgoing UDP datagram has been sent.
		/// </summary>
		public event UdpDatagramEvent OnUdpDatagramSent = null;

	}
}
