using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
#else
using System.Security.Authentication;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
#endif

namespace Waher.Networking
{
	/// <summary>
	/// Implements a binary TCP Server. The server adapts to network changes,
	/// maintains a list of current connection, and removes unused connections
	/// automatically.
	/// </summary>
	public class BinaryTcpServer : Sniffable, IDisposable
	{
		/// <summary>
		/// Default Connection backlog (10).
		/// </summary>
		public const int DefaultConnectionBacklog = 10;

		/// <summary>
		/// Default buffer size (16384).
		/// </summary>
		public const int DefaultBufferSize = 16384;

#if WINDOWS_UWP
		private LinkedList<KeyValuePair<StreamSocketListener, Guid>> listeners = new LinkedList<KeyValuePair<StreamSocketListener, Guid>>();
#else
		private LinkedList<TcpListener> listeners = new LinkedList<TcpListener>();
		private X509Certificate serverCertificate;
		private ClientCertificates clientCertificates = ClientCertificates.NotUsed;
		private bool trustClientCertificates = false;
		private bool clientCertificateSettingsLocked = false;
		private bool closed = false;
		private readonly bool tls;
#endif
		private Cache<Guid, ServerTcpConnection> connections;
		private readonly object synchObj = new object();
		private long nrBytesRx = 0;
		private long nrBytesTx = 0;
		private int port;

		/// <summary>
		/// Creates a TCP server, waiting for incoming connections. Encryption is not
		/// initiated.
		/// </summary>
		/// <param name="Port">Port number.</param>
		/// <param name="ActivityTimeout">Time before closing unused client connections.</param>
		/// <param name="Sniffers">Sniffers</param>
		public BinaryTcpServer(int Port, TimeSpan ActivityTimeout, ISniffer[] Sniffers)
			: base(Sniffers)
		{
#if !WINDOWS_UWP
			this.tls = false;
#endif
			this.Init(Port, ActivityTimeout);
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Creates a TCP server, waiting for incoming connections. Encryption is
		/// initiated for each connection request.
		/// </summary>
		/// <param name="Port">Port number.</param>
		/// <param name="ActivityTimeout">Time before closing unused client connections.</param>
		/// <param name="ServerCertificate">Server certificate.</param>
		/// <param name="Sniffers">Sniffers</param>
		public BinaryTcpServer(int Port, TimeSpan ActivityTimeout,
			X509Certificate ServerCertificate, ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.tls = true;
			this.serverCertificate = ServerCertificate;

			this.Init(Port, ActivityTimeout);
		}
#endif
		private void Init(int Port, TimeSpan ActivityTimeout)
		{
			if (ActivityTimeout <= TimeSpan.Zero)
				throw new ArgumentException("Activity timeout must be positive.", nameof(ActivityTimeout));

			this.port = Port;
			this.connections = new Cache<Guid, ServerTcpConnection>(int.MaxValue, TimeSpan.MaxValue,
				ActivityTimeout);

			this.connections.Removed += this.Connections_Removed;

#if WINDOWS_UWP
			NetworkInformation.NetworkStatusChanged += this.NetworkChange_NetworkAddressChanged;
#else
			NetworkChange.NetworkAddressChanged += this.NetworkChange_NetworkAddressChanged;
#endif
		}

#if WINDOWS_UWP
		private void NetworkChange_NetworkAddressChanged(object sender)
		{
			Task _ = this.NetworkChanged();
		}

		/// <summary>
		/// Adapts the server to changes in the network. This method can be called automatically by calling the constructor accordingly.
		/// </summary>
		public async Task NetworkChanged()
#else
		private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
		{
			this.NetworkChanged();
		}

		/// <summary>
		/// Adapts the server to changes in the network. This method can be called automatically by calling the constructor accordingly.
		/// </summary>
		public void NetworkChanged()
#endif
		{
			try
			{
#if WINDOWS_UWP
				LinkedList<KeyValuePair<StreamSocketListener, Guid>> Listeners = this.listeners;
				this.listeners = new LinkedList<KeyValuePair<StreamSocketListener, Guid>>();

				await this.Open(Listeners);

				foreach (KeyValuePair<StreamSocketListener, Guid> P in Listeners)
					P.Key.Dispose();
#else
				LinkedList<TcpListener> Listeners = this.listeners;
				this.listeners = new LinkedList<TcpListener>();

				this.Open(Listeners);

				foreach (TcpListener L in Listeners)
					L.Stop();
#endif
				this.OnNetworkChanged?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when the network has been changed.
		/// </summary>
		public event EventHandler OnNetworkChanged = null;

#if WINDOWS_UWP
		/// <summary>
		/// Opens the server for incoming connection requests.
		/// </summary>
		/// <param name="Listeners">Optional list of existing listeners to reuse.</param>
		/// <return>Number of network nterfaces where port was successfully opened, vs failed.</return>
		public async Task<KeyValuePair<int, int>> Open(LinkedList<KeyValuePair<StreamSocketListener, Guid>> Listeners)
		{
			int NrOpened = 0;
			int NrFailed = 0;

			try
			{
				StreamSocketListener Listener;

				foreach (ConnectionProfile Profile in NetworkInformation.GetConnectionProfiles())
				{
					if (Profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None)
						continue;

					Listener = null;

					LinkedListNode<KeyValuePair<StreamSocketListener, Guid>> Node;

					Node = Listeners?.First;
					while (!(Node is null))
					{
						StreamSocketListener L = Node.Value.Key;
						Guid AdapterId = Node.Value.Value;

						if (AdapterId == Profile.NetworkAdapter.NetworkAdapterId)
						{
							Listener = L;
							Listeners.Remove(Node);
							break;
						}

						Node = Node.Next;
					}

					if (Listener is null)
					{
						try
						{
							Listener = new StreamSocketListener();
							await Listener.BindServiceNameAsync(this.port.ToString(), SocketProtectionLevel.PlainSocket, Profile.NetworkAdapter);
							Listener.ConnectionReceived += this.Listener_ConnectionReceived;

							NrOpened++;
							this.listeners.AddLast(new KeyValuePair<StreamSocketListener, Guid>(Listener, Profile.NetworkAdapter.NetworkAdapterId));
						}
						catch (Exception ex)
						{
							NrFailed++;
							Log.Critical(ex, Profile.ProfileName);
						}
					}
					else
					{
						NrOpened++;
						this.listeners.AddLast(new KeyValuePair<StreamSocketListener, Guid>(Listener, Profile.NetworkAdapter.NetworkAdapterId));
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return new KeyValuePair<int, int>(NrOpened, NrFailed);
		}

		/// <summary>
		/// Closes the server from incoming connection requests.
		/// </summary>
		/// <param name="CloseConnectedClients">If connected clients should be
		/// closed as well.</param>
		public void Close(bool CloseConnectedClients)
		{
			LinkedList<KeyValuePair<StreamSocketListener, Guid>> Listeners = this.listeners;
			this.listeners = new LinkedList<KeyValuePair<StreamSocketListener, Guid>>();

			foreach (KeyValuePair<StreamSocketListener, Guid> P in Listeners)
				P.Key.Dispose();

			if (CloseConnectedClients)
				this.connections.Clear();
		}
#else
		/// <summary>
		/// Opens the server for incoming connection requests.
		/// </summary>
		/// <param name="Listeners">Optional list of existing listeners to reuse.</param>
		public Task Open(LinkedList<TcpListener> Listeners)
		{
			return this.Open(Listeners, out _, out _);
		}

		/// <summary>
		/// Opens the server for incoming connection requests.
		/// </summary>
		/// <param name="Listeners">Optional list of existing listeners to reuse.</param>
		/// <param name="NrOpened">Number of network interfaces where the port was successfully opened.</param>
		/// <param name="NrFailed">Number of network interfaces where the port was not possible to be opened.</param>
		public Task Open(LinkedList<TcpListener> Listeners, out int NrOpened, out int NrFailed)
		{
			NrOpened = 0;
			NrFailed = 0;

			try
			{
				TcpListener Listener;

				foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (Interface.OperationalStatus != OperationalStatus.Up)
						continue;

					IPInterfaceProperties Properties = Interface.GetIPProperties();

					foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
					{
						if ((UnicastAddress.Address.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4) ||
							(UnicastAddress.Address.AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6))
						{
							Listener = null;

							LinkedListNode<TcpListener> Node;
							IPEndPoint DesiredEndpoint = new IPEndPoint(UnicastAddress.Address, this.port);

							Node = Listeners?.First;
							while (!(Node is null))
							{
								TcpListener L = Node.Value;

								if ((!this.tls) && L.LocalEndpoint == DesiredEndpoint)
								{
									Listener = L;
									Listeners.Remove(Node);
									break;
								}

								Node = Node.Next;
							}

							if (Listener is null)
							{
								try
								{
									Listener = new TcpListener(UnicastAddress.Address, this.port);
									Listener.Start(DefaultConnectionBacklog);
									Task T = this.ListenForIncomingConnections(Listener);

									NrOpened++;
									this.listeners.AddLast(Listener);
								}
								catch (SocketException)
								{
									NrFailed++;
									Log.Error("Unable to open port for listening.",
										new KeyValuePair<string, object>("Address", UnicastAddress.Address.ToString()),
										new KeyValuePair<string, object>("Port", this.port));
								}
								catch (Exception ex)
								{
									NrFailed++;
									Log.Critical(ex, UnicastAddress.Address.ToString() + ":" + this.port);
								}
							}
							else
							{
								NrOpened++;
								this.listeners.AddLast(Listener);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Closes the server from incoming connection requests.
		/// </summary>
		/// <param name="CloseConnectedClients">If connected clients should be
		/// closed as well.</param>
		public void Close(bool CloseConnectedClients)
		{
			LinkedList<TcpListener> Listeners = this.listeners;
			this.listeners = new LinkedList<TcpListener>();

			foreach (TcpListener Listener in Listeners)
				Listener.Stop();

			if (CloseConnectedClients)
				this.connections.Clear();
		}
#endif

		/// <summary>
		/// Opens the server for incoming connection requests.
		/// </summary>
		public Task Open()
		{
			return this.Open(null);
		}

		/// <summary>
		/// Closes the server from incoming connection requests. Connected clients
		/// are also closed.
		/// </summary>
		public void Close()
		{
			this.Close(true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
#if WINDOWS_UWP
			NetworkInformation.NetworkStatusChanged -= this.NetworkChange_NetworkAddressChanged;
#else
			this.closed = true;
			NetworkChange.NetworkAddressChanged -= this.NetworkChange_NetworkAddressChanged;
#endif
			this.Close(true);

			this.connections.Dispose();
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Updates the server certificate
		/// </summary>
		/// <param name="ServerCertificate">Server Certificate.</param>
		public void UpdateCertificate(X509Certificate ServerCertificate)
		{
			this.serverCertificate = ServerCertificate;
		}

		/// <summary>
		/// Configures Mutual-TLS capabilities of the server. Affects all connections, all resources.
		/// </summary>
		/// <param name="ClientCertificates">If client certificates are not used, optional or required.</param>
		/// <param name="TrustClientCertificates">If client certificates should be trusted, even if they do not validate.</param>
		/// <param name="LockSettings">If client certificate settings should be locked.</param>
		public void ConfigureMutualTls(ClientCertificates ClientCertificates, bool TrustClientCertificates, bool LockSettings)
		{
			if (this.clientCertificateSettingsLocked)
				throw new InvalidOperationException("Mutual TLS settings locked.");

			this.clientCertificates = ClientCertificates;
			this.trustClientCertificates = TrustClientCertificates;
			this.clientCertificateSettingsLocked = LockSettings;
		}

		/// <summary>
		/// If client certificates are not used, optional or required.
		/// </summary>
		public ClientCertificates ClientCertificates => this.clientCertificates;

		/// <summary>
		/// If client certificates should be trusted, even if they do not validate.
		/// </summary>
		public bool TrustClientCertificates => this.trustClientCertificates;
#endif

		private async Task<bool> AcceptConnection(ServerTcpConnection Connection)
		{
			ServerConnectionAcceptEventArgs e = new ServerConnectionAcceptEventArgs(Connection);
			EventHandlerAsync<ServerConnectionAcceptEventArgs> h = this.OnAccept;

			if (!(h is null))
			{
				try
				{
					await h(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					e.Accept = false;
				}
			}

			return e.Accept;
		}

		/// <summary>
		/// Event raised when a client tries to connect to the server. An event handler
		/// can set the <see cref="ServerConnectionAcceptEventArgs.Accept"/> property
		/// to control if the server should accept the connection or not.
		/// </summary>
		public event EventHandlerAsync<ServerConnectionAcceptEventArgs> OnAccept;

#if WINDOWS_UWP
		private void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
		{
			try
			{
				StreamSocket Client = args.Socket;

				BinaryTcpClient BinaryTcpClient = new BinaryTcpClient(Client);
				BinaryTcpClient.Bind(true);
				ServerTcpConnection Connection = new ServerTcpConnection(this, BinaryTcpClient);

				if (this.AcceptConnection(Connection).Result)
				{
					this.Information("Connection accepted from " + Client.Information.RemoteAddress.ToString() + ":" + Client.Information.RemotePort + ".");
					BinaryTcpClient.Continue();
					Task.Run(() => this.Added(Connection));
				}
				else
				{
					this.Warning("Connection rejected from " + Client.Information.RemoteAddress.ToString() + ":" + Client.Information.RemotePort + ".");
					Connection.Client.Dispose();
				}
			}
			catch (SocketException)
			{
				// Ignore
			}
			catch (Exception ex)
			{
				if (this.listeners is null)
					return;

				Log.Critical(ex);
			}
		}
#else
		private async Task ListenForIncomingConnections(TcpListener Listener)
		{
			try
			{
				while (!this.closed)
				{
					try
					{
						TcpClient Client;

						try
						{
							Client = await Listener.AcceptTcpClientAsync();
							if (this.closed)
								return;
						}
						catch (InvalidOperationException)
						{
							LinkedListNode<TcpListener> Node = this.listeners?.First;

							while (!(Node is null))
							{
								if (Node.Value == Listener)
								{
									this.listeners.Remove(Node);
									break;
								}

								Node = Node.Next;
							}

							return;
						}

						if (!(Client is null))
						{
							BinaryTcpClient BinaryTcpClient = new BinaryTcpClient(Client);
							BinaryTcpClient.Bind(true);

							ServerTcpConnection Connection = new ServerTcpConnection(this, BinaryTcpClient);

							if (await this.AcceptConnection(Connection))
							{
								this.Information("Connection accepted from " + Client.Client.RemoteEndPoint.ToString() + ".");

								if (this.tls)
								{
									Task T = this.SwitchToTls(Connection);
								}
								else
								{
									BinaryTcpClient.Continue();
									await this.Added(Connection);
								}
							}
							else
							{
								this.Warning("Connection rejected from " + Client.Client.RemoteEndPoint.ToString() + ".");
								Connection.Client.Dispose();
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
						if (this.closed || this.listeners is null)
							break;

						bool Found = false;

						foreach (TcpListener L in this.listeners)
						{
							if (L == Listener)
							{
								Found = true;
								break;
							}
						}

						if (Found)
							Log.Critical(ex);
						else
							break;  // Removed, for instance due to network change
					}
				}
			}
			catch (Exception ex)
			{
				if (this.closed || this.listeners is null)
					return;

				Log.Critical(ex);
			}
		}

		private async Task SwitchToTls(ServerTcpConnection Connection)
		{
			try
			{
				this.Information("Switching to TLS.");

				await Connection.Client.UpgradeToTlsAsServer(this.serverCertificate, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12,
					this.clientCertificates, null, this.trustClientCertificates);

				if (this.HasSniffers)
				{
					this.Information("TLS established" +
						". Cipher Strength: " + Connection.Client.CipherStrength.ToString() +
						", Hash Strength: " + Connection.Client.HashStrength.ToString() +
						", Key Exchange Strength: " + Connection.Client.KeyExchangeStrength.ToString());

					if (!(Connection.Client.RemoteCertificate is null))
					{
						if (this.HasSniffers)
						{
							StringBuilder sb = new StringBuilder();

							sb.Append("Remote Certificate received. Valid: ");
							sb.Append(Connection.Client.RemoteCertificateValid.ToString());
							sb.Append(", Subject: ");
							sb.Append(Connection.Client.RemoteCertificate.Subject);
							sb.Append(", Issuer: ");
							sb.Append(Connection.Client.RemoteCertificate.Issuer);
							sb.Append(", S/N: ");
							sb.Append(Convert.ToBase64String(Connection.Client.RemoteCertificate.GetSerialNumber()));
							sb.Append(", Hash: ");
							sb.Append(Convert.ToBase64String(Connection.Client.RemoteCertificate.GetCertHash()));

							this.Information(sb.ToString());
						}
					}
				}

				Connection.Client.Continue();
				await this.Added(Connection);
			}
			catch (AuthenticationException ex)
			{
				await this.LoginFailure(ex, Connection);
			}
			catch (Win32Exception ex)
			{
				await this.LoginFailure(ex, Connection);
			}
			catch (SocketException)
			{
				Connection.Client.Dispose();
			}
			catch (IOException)
			{
				Connection.Client.Dispose();
			}
			catch (Exception ex)
			{
				Connection.Client.Dispose();
				Log.Critical(ex);
			}
		}

		private async Task LoginFailure(Exception ex, ServerTcpConnection Connection)
		{
			EventHandlerAsync<ServerTlsErrorEventArgs> h = this.OnTlsUpgradeError;

			if (!(h is null))
			{
				Exception ex2 = Log.UnnestException(ex);
				try
				{
					await h(this, new ServerTlsErrorEventArgs(Connection, ex2));
				}
				catch (Exception ex3)
				{
					Log.Critical(ex3);
				}
			}

			Connection.Client.Dispose();
		}

		/// <summary>
		/// Event raised when a client is unable to switch to TLS.
		/// </summary>
		public event EventHandlerAsync<ServerTlsErrorEventArgs> OnTlsUpgradeError;
#endif
		private async Task Added(ServerTcpConnection Connection)
		{
			lock (this.connections)
			{
				this.connections[Connection.Id] = Connection;
			}

			EventHandlerAsync<ServerConnectionEventArgs> h = this.OnClientConnected;
			if (!(h is null))
			{
				try
				{
					await h(this, new ServerConnectionEventArgs(Connection));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a client has connected.
		/// </summary>
		public event EventHandlerAsync<ServerConnectionEventArgs> OnClientConnected;

		private async void Connections_Removed(object Sender, CacheItemEventArgs<Guid, ServerTcpConnection> e)
		{
			try
			{
				e.Value.Client?.Dispose();

				EventHandlerAsync<ServerConnectionEventArgs> h = this.OnClientDisconnected;
				if (!(h is null))
					await h(this, new ServerConnectionEventArgs(e.Value));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a client has been disconnected.
		/// </summary>
		public event EventHandlerAsync<ServerConnectionEventArgs> OnClientDisconnected;

		internal void Remove(ServerTcpConnection Connection)
		{
			this.connections.Remove(Connection.Id);
		}

		internal async Task DataReceived(ServerTcpConnection Connection, byte[] Buffer,
			int Offset, int Count)
		{
			this.connections?.ContainsKey(Connection.Id);   // Refreshes timer for connection.

			lock (this.synchObj)
			{
				this.nrBytesRx += Count;
			}

			if (this.HasSniffers)
				this.ReceiveBinary(BinaryTcpClient.ToArray(Buffer, Offset, Count));

			EventHandlerAsync<ServerConnectionDataEventArgs> h = this.OnDataReceived;
			if (!(h is null))
			{
				try
				{
					await h(this, new ServerConnectionDataEventArgs(Connection, Buffer, Offset, Count));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raisde when data has been received from a client.
		/// </summary>
		public event EventHandlerAsync<ServerConnectionDataEventArgs> OnDataReceived;

		internal void DataSent(byte[] Data)
		{
			lock (this.synchObj)
			{
				this.nrBytesTx += Data.Length;
			}

			this.TransmitBinary(Data);
		}

		/// <summary>
		/// Number of bytes received
		/// </summary>
		public long NrBytesRx
		{
			get
			{
				lock (this.synchObj)
				{
					return this.nrBytesRx;
				}
			}
		}

		/// <summary>
		/// Number of bytes transmitted
		/// </summary>
		public long NrBytesTx
		{
			get
			{
				lock (this.synchObj)
				{
					return this.nrBytesTx;
				}
			}
		}

	}
}
