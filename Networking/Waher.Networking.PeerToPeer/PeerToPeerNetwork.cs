using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.Networking.PeerToPeer
{
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
		public const int DefaultBacklog = BinaryTcpServer.DefaultC2CConnectionBacklog;

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

			Task.Run(async () =>
			{
				try
				{
					await this.Start();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			});
		}

		/// <summary>
		/// Starts searching for Internet Gateways. Once found, ports will be registered.
		/// </summary>
		public override async Task Start()
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

					await this.SetState(PeerToPeerNetworkState.Ready);

					this.AcceptTcpClients();
					this.BeginReceiveUdp();
				}
				catch (Exception ex)
				{
					this.exception = ex;
					await this.SetState(PeerToPeerNetworkState.Error);

					this.tcpListener?.Stop();
					this.tcpListener = null;

					this.udpClient?.Dispose();
					this.udpClient = null;
				}
			}

			await base.Start();
		}

		private async void AcceptTcpClients()
		{
			try
			{
				while (!this.disposed && !(this.tcpListener is null))
				{
					try
					{
						TcpClient TcpClient;

						try
						{
							TcpClient = await this.tcpListener.AcceptTcpClientAsync();
							if (this.disposed)
								return;
						}
						catch (InvalidOperationException)
						{
							await this.SetState(PeerToPeerNetworkState.Error);

							this.tcpListener?.Stop();
							this.tcpListener = null;

							this.udpClient?.Dispose();
							this.udpClient = null;

							return;
						}

						if (!(TcpClient is null))
						{
							PeerConnection Connection = null;

							try
							{
								BinaryTcpClient Client = new BinaryTcpClient(TcpClient, false);
								Client.Bind(true);

								Connection = new PeerConnection(Client, this,
									(IPEndPoint)TcpClient.Client.RemoteEndPoint, this.encapsulatePackets);

								await this.SetState(PeerToPeerNetworkState.Ready);

								await this.PeerConnected(Connection);

								Connection.Start();
							}
							catch (Exception)
							{
								if (!(Connection is null))
									await Connection.DisposeAsync();
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
						Log.Exception(ex);
					}
				}
			}
			catch (Exception ex)
			{
				if (this.disposed)
					return;

				Log.Exception(ex);
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
		public IPEndPoint ExternalEndpoint => this.externalEndpoint;

		/// <summary>
		/// Local IP Endpoint.
		/// </summary>
		public IPEndPoint LocalEndpoint => this.localEndpoint;

		/// <summary>
		/// If packets are to be encapsulated and delivered as ordered units (true), or if fragmentation in the 
		/// TCP case, or reordering of received datagrams in the UDP case, are allowed (false).
		/// </summary>
		public bool EncapsulatePackets
		{
			get => this.encapsulatePackets;
			set => this.encapsulatePackets = value;
		}

		/// <summary>
		/// is called before performing a registration.
		/// </summary>
		/// <param name="Registration">Registration to be performed.</param>
		/// <param name="TcpPortMapped">What TCP Ports are already mapped.</param>
		/// <param name="UdpPortMapped">What UDP Ports are already mapped.</param>
		protected override async Task BeforeRegistration(InternetGatewayRegistration Registration,
			Dictionary<ushort, bool> TcpPortMapped, Dictionary<ushort, bool> UdpPortMapped)
		{
			try
			{
				do
				{
					this.tcpListener = new TcpListener(this.localAddress, Registration.LocalPort);
					this.tcpListener.Start(this.backlog);

					int i = ((IPEndPoint)this.tcpListener.LocalEndpoint).Port;

					if (i < 0 || i > ushort.MaxValue ||
						TcpPortMapped.ContainsKey((ushort)i) ||
						UdpPortMapped.ContainsKey((ushort)i))
					{
						this.tcpListener.Stop();
						this.tcpListener = null;
					}
					else
					{
						try
						{
							this.udpClient = new UdpClient(this.tcpListener.LocalEndpoint.AddressFamily);
							this.udpClient.Client.Bind((IPEndPoint)this.tcpListener.LocalEndpoint);

							Registration.LocalPort = (ushort)i;
							if (Registration.ExternalPort == 0 ||
								TcpPortMapped.ContainsKey((ushort)Registration.ExternalPort) ||
								UdpPortMapped.ContainsKey((ushort)Registration.ExternalPort))
							{
								Registration.ExternalPort = Registration.LocalPort;
							}
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
				await this.SetState(PeerToPeerNetworkState.Error);
			}
		}

		/// <summary>
		/// Called when a new peer has connected.
		/// </summary>
		/// <param name="Connection">Peer connection</param>
		protected virtual Task PeerConnected(PeerConnection Connection)
		{
			return this.OnPeerConnected.Raise(this, Connection);
		}

		/// <summary>
		/// Event raised when a new peer has connected.
		/// </summary>
		public event EventHandlerAsync<PeerConnection> OnPeerConnected = null;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override Task DisposeAsync()
		{
			this.tcpListener?.Stop();
			this.tcpListener = null;

			this.udpClient?.Dispose();
			this.udpClient = null;

			return base.DisposeAsync();
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

			BinaryTcpClient Client = new BinaryTcpClient(false);
			IPEndPoint RemoteEndPoint2 = RemoteEndPoint;

			try
			{
				RemoteEndPoint2 = this.CheckLocalRemoteEndpoint(RemoteEndPoint);
				await Client.ConnectAsync(RemoteEndPoint2.Address, RemoteEndPoint2.Port, true);
			}
			catch (Exception ex)
			{
				Client.Dispose();
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
			}

			PeerConnection Result = new PeerConnection(Client, this, RemoteEndPoint2, this.encapsulatePackets);

			Result.StartIdleTimer();

			return Result;
		}

		private async void BeginReceiveUdp()    // Starts parallel task
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await this.udpClient.ReceiveAsync();
					if (!this.disposed)
						await this.OnUdpDatagramReceived.Raise(this, new UdpDatagramEventArgs(Data.RemoteEndPoint, Data.Buffer));
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Event raised when an incoming UDP datagram has been received.
		/// </summary>
		public event EventHandlerAsync<UdpDatagramEventArgs> OnUdpDatagramReceived = null;

		/// <summary>
		/// Sends an UDP datagram to a remote destination.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint of destination.</param>
		/// <param name="Datagram">UDP Datagram to send.</param>
		public async Task SendUdp(IPEndPoint RemoteEndpoint, byte[] Datagram)
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
				while (!this.disposed && !(Datagram is null))
				{
					await this.udpClient.SendAsync(Datagram, Datagram.Length, RemoteEndpoint);

					await this.OnUdpDatagramSent.Raise(this, new UdpDatagramEventArgs(RemoteEndpoint, Datagram));

					lock (this.writeQueue)
					{
						if (!(this.writeQueue.First is null))
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

				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Event raised when an outgoing UDP datagram has been sent.
		/// </summary>
		public event EventHandlerAsync<UdpDatagramEventArgs> OnUdpDatagramSent = null;
	}
}
