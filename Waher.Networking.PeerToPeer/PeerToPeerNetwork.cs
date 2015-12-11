using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
	public delegate void PeerConnectedEventHandler(PeerToPeerNetwork Listener, PeerConnection Peer);

	/// <summary>
	/// Manages a peer-to-peer network that can receive connections from outside of a NAT-enabled firewall.
	/// </summary>
	public class PeerToPeerNetwork : IDisposable
	{
		private const int defaultPort = 0;
		private const int defaultBacklog = 10;

		private Dictionary<IPAddress, bool> ipAddressesFound = new Dictionary<IPAddress, bool>();
		private TcpListener tcpListener;
		private UdpClient udpClient;
		private UPnPClient upnpClient;
		private WANIPConnectionV1 serviceWANIPConnectionV1;
		private IPAddress localAddress;
		private IPAddress externalAddress;
		private IPEndPoint localEndpoint;
		private IPEndPoint externalEndpoint;
		private PeerToPeerNetworkState state = PeerToPeerNetworkState.Created;
		private ManualResetEvent ready = new ManualResetEvent(false);
		private ManualResetEvent error = new ManualResetEvent(false);
		private Exception exception = null;
		private string applicationName;
		private int desiredPort;
		private int backlog;
		private bool tcpMappingAdded = false;
		private bool udpMappingAdded = false;

		/// <summary>
		/// Manages a peer-to-peer network that can receive connections from outside of a NAT-enabled firewall.
		/// </summary>
		/// <param name="ApplicationName">Name of Peer-to-Peer application. Any NAT port mappings in the firewall
		/// having the same name and pointing to the same machine will be removed. To allow multiple port mappings on
		/// the same machine, each mapping needs a unique name.</param>
		public PeerToPeerNetwork(string ApplicationName)
			: this(ApplicationName, defaultPort, defaultBacklog)
		{
		}

		/// <summary>
		/// Manages a peer-to-peer network that can receive connections from outside of a NAT-enabled firewall.
		/// </summary>
		/// <param name="ApplicationName">Name of Peer-to-Peer application. Any NAT port mappings in the firewall
		/// having the same name and pointing to the same machine will be removed. To allow multiple port mappings on
		/// the same machine, each mapping needs a unique name.</param>
		/// <param name="Port">Desired port number. If 0, a dynamic port number will be assigned.</param>
		public PeerToPeerNetwork(string ApplicationName, int Port)
			: this(ApplicationName, Port, defaultBacklog)
		{
		}

		/// <summary>
		/// Manages a peer-to-peer network that can receive connections from outside of a NAT-enabled firewall.
		/// </summary>
		/// <param name="ApplicationName">Name of Peer-to-Peer application. Any NAT port mappings in the firewall
		/// having the same name and pointing to the same machine will be removed. To allow multiple port mappings on
		/// the same machine, each mapping needs a unique name.</param>
		/// <param name="Port">Desired port number. If 0, a dynamic port number will be assigned.</param>
		/// <param name="Backlog">Connection backlog.</param>
		public PeerToPeerNetwork(string ApplicationName, int Port, int Backlog)
		{
			this.applicationName = ApplicationName;
			this.desiredPort = Port;
			this.backlog = Backlog;

			this.tcpListener = null;
			this.udpClient = null;

			if (this.OnPublicNetwork())
			{
				try
				{
					this.localAddress = this.externalAddress;
					ushort LocalPort;

					this.tcpListener = new TcpListener(this.localAddress, this.desiredPort);
					this.tcpListener.Start(this.backlog);

					LocalPort = (ushort)((IPEndPoint)this.tcpListener.LocalEndpoint).Port;

					this.localEndpoint = new IPEndPoint(this.localAddress, LocalPort);
					this.externalEndpoint = new IPEndPoint(this.externalAddress, LocalPort);

					this.udpClient = new UdpClient(this.localEndpoint.AddressFamily);
					this.udpClient.Client.Bind(this.localEndpoint);

					this.State = PeerToPeerNetworkState.Ready;

					this.tcpListener.BeginAcceptTcpClient(this.EndAcceptTcpClient, null);
					this.udpClient.BeginReceive(this.EndReceiveUdp, null);
				}
				catch (Exception ex)
				{
					this.exception = ex;
					this.State = PeerToPeerNetworkState.Error;

					if (this.tcpListener != null)
					{
						this.tcpListener.Stop();
						this.tcpListener = null;
					}

					if (this.udpClient != null)
					{
						this.udpClient.Close();
						this.udpClient = null;
					}
				}
			}
			else
				this.StartSearch();
		}

		private bool OnPublicNetwork()
		{
			foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				IPInterfaceProperties Properties = Interface.GetIPProperties();

				foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
				{
					if (UnicastAddress.Address.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4)
					{
						byte[] Addr = UnicastAddress.Address.GetAddressBytes();

						if (Addr[0] == 127)
							continue;	// Loopback address range: 127.0.0.0 - 127.255.255.55

						else if (Addr[0] == 10)
							continue;	// Private address range: 10.0.0.0 - 10.255.255.55

						else if (Addr[0] == 172 && Addr[1] >= 16 && Addr[1] <= 31)
							continue;	// Private address range: 172.16.0.0 - 172.31.255.255

						else if (Addr[0] == 192 && Addr[1] == 168)
							continue;	// Private address range: 192.168.0.0 - 192.168.255.255

						else if (Addr[0] == 169 && Addr[1] == 254)
							continue;	// Link-local address range: 169.254.0.0 - 169.254.255.255

						this.externalAddress = UnicastAddress.Address;
						return true;
					}
				}
			}

			return false;
		}

		private void StartSearch()
		{
			try
			{
				this.upnpClient = new UPnPClient();
				this.upnpClient.OnDeviceFound += new UPnPDeviceLocationEventHandler(upnpClient_OnDeviceFound);

				this.State = PeerToPeerNetworkState.SearchingForGateway;

				this.upnpClient.StartSearch("urn:schemas-upnp-org:service:WANIPConnection:1", 1);
				this.upnpClient.StartSearch("urn:schemas-upnp-org:service:WANIPConnection:2", 1);
			}
			catch (Exception ex)
			{
				this.exception = ex;
				this.State = PeerToPeerNetworkState.Error;
			}
		}

		private void upnpClient_OnDeviceFound(UPnPClient Sender, DeviceLocationEventArgs e)
		{
			try
			{
				lock (this.ipAddressesFound)
				{
					if (this.ipAddressesFound.ContainsKey(e.RemoteEndPoint.Address))
						return;

					this.ipAddressesFound[e.RemoteEndPoint.Address] = true;
				}

				e.Location.StartGetDevice(this.DeviceRetrieved, e);
			}
			catch (Exception ex)
			{
				this.exception = ex;
				this.State = PeerToPeerNetworkState.Error;
			}
		}

		private void DeviceRetrieved(object Sender, DeviceDescriptionEventArgs e)
		{
			try
			{
				if (e.DeviceDescriptionDocument != null)
				{
					UPnPService Service = e.DeviceDescriptionDocument.GetService("urn:schemas-upnp-org:service:WANIPConnection:1");
					if (Service == null)
					{
						Service = e.DeviceDescriptionDocument.GetService("urn:schemas-upnp-org:service:WANIPConnection:2");
						if (Service == null)
							return;
					}

					Service.StartGetService(this.ServiceRetrieved, e.State);
				}
			}
			catch (Exception ex)
			{
				this.exception = ex;
				this.State = PeerToPeerNetworkState.Error;
			}
		}

		private void ServiceRetrieved(object Sender, ServiceDescriptionEventArgs e)
		{
			try
			{
				DeviceLocationEventArgs e2 = (DeviceLocationEventArgs)e.State;
				Dictionary<ushort, bool> TcpPortMapped = new Dictionary<ushort, bool>();
				Dictionary<ushort, bool> UdpPortMapped = new Dictionary<ushort, bool>();
				string NewExternalIPAddress;
				ushort PortMappingIndex;
				string NewRemoteHost;
				ushort NewExternalPort;
				string NewProtocol;
				ushort NewInternalPort;
				string NewInternalClient;
				bool NewEnabled;
				string NewPortMappingDescription;
				uint NewLeaseDuration;

				this.serviceWANIPConnectionV1 = new WANIPConnectionV1(e.ServiceDescriptionDocument);
				this.State = PeerToPeerNetworkState.RegisteringApplicationInGateway;

				this.serviceWANIPConnectionV1.GetExternalIPAddress(out NewExternalIPAddress);
				this.externalAddress = IPAddress.Parse(NewExternalIPAddress);

				PortMappingIndex = 0;

				try
				{
					while (true)
					{
						this.serviceWANIPConnectionV1.GetGenericPortMappingEntry(PortMappingIndex, out NewRemoteHost,
							out NewExternalPort, out NewProtocol, out NewInternalPort, out NewInternalClient,
							out NewEnabled, out NewPortMappingDescription, out NewLeaseDuration);

						if (NewPortMappingDescription == this.applicationName && NewInternalClient == e2.LocalEndPoint.Address.ToString())
							this.serviceWANIPConnectionV1.DeletePortMapping(NewRemoteHost, NewExternalPort, NewProtocol);
						else
						{
							switch (NewProtocol)
							{
								case "TCP":
									TcpPortMapped[NewExternalPort] = true;
									break;

								case "UDP":
									UdpPortMapped[NewExternalPort] = true;
									break;
							}

							PortMappingIndex++;
						}
					}
				}
				catch (UPnPException)
				{
					// No more entries.
				}

				this.localAddress = e2.LocalEndPoint.Address;
				ushort LocalPort;
				int i;

				do
				{
					this.tcpListener = new TcpListener(this.localAddress, this.desiredPort);
					this.tcpListener.Start(this.backlog);

					i = ((IPEndPoint)this.tcpListener.LocalEndpoint).Port;
					LocalPort = (ushort)(i);
					if (i < 0 || i > ushort.MaxValue || TcpPortMapped.ContainsKey(LocalPort) || UdpPortMapped.ContainsKey(LocalPort))
					{
						this.tcpListener.Stop();
						this.tcpListener = null;

						if (this.desiredPort != 0)
							throw new ArgumentException("Port already assigned to another application in the network.", "Port");
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
				while (this.tcpListener == null);

				this.localEndpoint = new IPEndPoint(this.localAddress, LocalPort);

				this.serviceWANIPConnectionV1.AddPortMapping(string.Empty, LocalPort, "TCP", LocalPort, LocalAddress.ToString(), true, this.applicationName, 0);
				this.tcpMappingAdded = true;

				this.serviceWANIPConnectionV1.AddPortMapping(string.Empty, LocalPort, "UDP", LocalPort, LocalAddress.ToString(), true, this.applicationName, 0);
				this.udpMappingAdded = true;

				this.externalEndpoint = new IPEndPoint(this.externalAddress, LocalPort);
				this.State = PeerToPeerNetworkState.Ready;

				this.tcpListener.BeginAcceptTcpClient(this.EndAcceptTcpClient, null);
				this.udpClient.BeginReceive(this.EndReceiveUdp, null);
			}
			catch (Exception ex)
			{
				this.exception = ex;
				this.State = PeerToPeerNetworkState.Error;
			}
		}

		private void EndAcceptTcpClient(IAsyncResult ar)
		{
			if (this.tcpListener != null)
			{
				try
				{
					TcpClient Client = this.tcpListener.EndAcceptTcpClient(ar);
					PeerConnection Connection = new PeerConnection(Client, this, (IPEndPoint)Client.Client.RemoteEndPoint);

					this.tcpListener.BeginAcceptTcpClient(this.EndAcceptTcpClient, null);
					this.State = PeerToPeerNetworkState.Ready;

					this.PeerConnected(Connection);

					Connection.Start();
				}
				catch (Exception)
				{
					if (this.state != PeerToPeerNetworkState.Closed)
						this.State = PeerToPeerNetworkState.Error;
				}
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
					Debug.WriteLine(ex.Message);
					Debug.WriteLine(ex.StackTrace.ToString());
				}
			}
		}

		/// <summary>
		/// Event raised when a new peer has connected.
		/// </summary>
		public event PeerConnectedEventHandler OnPeerConnected = null;

		/// <summary>
		/// Application Name
		/// </summary>
		public string ApplicationName
		{
			get { return this.applicationName; }
		}

		/// <summary>
		/// Current state of the peer-to-peer network object.
		/// </summary>
		public PeerToPeerNetworkState State
		{
			get { return this.state; }
			internal set
			{
				if (this.state != value)
				{
					this.state = value;

					switch (value)
					{
						case PeerToPeerNetworkState.Ready:
							this.ready.Set();
							break;

						case PeerToPeerNetworkState.Error:
							this.error.Set();
							break;
					}

					PeerToPeerNetworkStateChangeEventHandler h = this.OnStateChange;
					if (h != null)
					{
						try
						{
							h(this, value);
						}
						catch (Exception ex)
						{
							Debug.WriteLine(ex.Message);
							Debug.WriteLine(ex.StackTrace.ToString());
						}
					}
				}
			}
		}

		/// <summary>
		/// Event raised when the state of the peer-to-peer network changes.
		/// </summary>
		public event PeerToPeerNetworkStateChangeEventHandler OnStateChange = null;

		/// <summary>
		/// External IP Address.
		/// </summary>
		public IPAddress ExternalAddress
		{
			get { return this.externalAddress; }
		}

		/// <summary>
		/// External IP Endpoint.
		/// </summary>
		public IPEndPoint ExternalEndpoint
		{
			get { return this.externalEndpoint; }
		}

		/// <summary>
		/// Local IP Address.
		/// </summary>
		public IPAddress LocalAddress
		{
			get { return this.localAddress; }
		}

		/// <summary>
		/// Local IP Endpoint.
		/// </summary>
		public IPEndPoint LocalEndpoint
		{
			get { return this.localEndpoint; }
		}

		/// <summary>
		/// In case <see cref="State"/>=<see cref="PeerToPeerNetworkState.Error"/>, this exception object contains details about the error.
		/// </summary>
		public Exception Exception
		{
			get { return this.exception; }
		}

		/// <summary>
		/// Waits for the peer-to-peer network object to be ready to receive connections.
		/// </summary>
		/// <returns>true, if connections can be received, false if a peer-to-peer listener cannot be created in the current network.</returns>
		public bool Wait()
		{
			return this.Wait(10000);
		}

		/// <summary>
		/// Waits for the peer-to-peer network object to be ready to receive connections.
		/// </summary>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds. Default=10000.</param>
		/// <returns>true, if connections can be received, false if a peer-to-peer listener could not be created in the allotted time.</returns>
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
			this.State = PeerToPeerNetworkState.Closed;

			if (this.tcpListener != null)
			{
				this.tcpListener.Stop();
				this.tcpListener = null;
			}

			if (this.tcpMappingAdded)
			{
				this.tcpMappingAdded = false;
				try
				{
					this.serviceWANIPConnectionV1.DeletePortMapping(string.Empty, (ushort)this.localEndpoint.Port, "TCP");
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			if (this.udpMappingAdded)
			{
				this.udpMappingAdded = false;
				try
				{
					this.serviceWANIPConnectionV1.DeletePortMapping(string.Empty, (ushort)this.localEndpoint.Port, "UDP");
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			this.serviceWANIPConnectionV1 = null;

			if (this.upnpClient != null)
			{
				this.upnpClient.Dispose();
				this.upnpClient = null;
			}

			if (this.ipAddressesFound != null)
			{
				this.ipAddressesFound.Clear();
				this.ipAddressesFound = null;
			}

			if (this.ready != null)
			{
				this.ready.Close();
				this.ready = null;
			}

			if (this.error != null)
			{
				this.error.Close();
				this.error = null;
			}
		}

		/// <summary>
		/// Connects to a peer in the peer-to-peer network. If the remote end point resides behind the same firewall as the current application,
		/// a direct connection to the local peer is made, for improved performance.
		/// </summary>
		/// <param name="RemoteEndPoint">Remote End-point.</param>
		/// <returns>Peer connection</returns>
		public PeerConnection ConnectToPeer(IPEndPoint RemoteEndPoint)
		{
			if (this.state != PeerToPeerNetworkState.Ready)
				throw new IOException("Peer-to-peer network not ready.");

			TcpClient Client = new TcpClient(new IPEndPoint(this.localAddress, 0));
			IPEndPoint RemoteEndPoint2;

			try
			{
				if (IPAddress.Equals(RemoteEndPoint.Address, this.externalAddress))
				{
					ushort InternalPort;
					string InternalClient;
					string PortMappingDescription;
					uint LeaseDuration;
					bool Enabled;

					this.serviceWANIPConnectionV1.GetSpecificPortMappingEntry(string.Empty, (ushort)RemoteEndPoint.Port, "TCP",
						out InternalPort, out InternalClient, out Enabled, out PortMappingDescription, out LeaseDuration);

					RemoteEndPoint2 = new IPEndPoint(IPAddress.Parse(InternalClient), InternalPort);
					Client.Connect(RemoteEndPoint2);
				}
				else
				{
					RemoteEndPoint2 = RemoteEndPoint;
					Client.Connect(RemoteEndPoint);
				}
			}
			catch (Exception)
			{
				Client.Close();
				throw;
			}

			PeerConnection Result = new PeerConnection(Client, this, RemoteEndPoint2);

			Result.StartIdleTimer();

			return Result;
		}

		private void EndReceiveUdp(IAsyncResult ar)
		{
			try
			{
				if (this.udpClient != null)
				{
					IPEndPoint RemoteEndpoint = null;

					byte[] Data = this.udpClient.EndReceive(ar, ref RemoteEndpoint);
					if (RemoteEndpoint != null && Data != null)
					{
						UdpDatagramEvent h = this.OnUdpDatagramReceived;
						if (h != null)
						{
							try
							{
								h(this, new UdpDatagramEventArgs(RemoteEndpoint, Data));
							}
							catch (Exception ex)
							{
								Debug.WriteLine(ex.Message);
								Debug.WriteLine(ex.StackTrace.ToString());
							}
						}
					}

					this.udpClient.BeginReceive(this.EndReceiveUdp, null);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Debug.WriteLine(ex.StackTrace.ToString());
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
		public void SendUdp(IPEndPoint RemoteEndpoint, byte[] Datagram)
		{
			lock (this.writeQueue)
			{
				if (this.isWriting)
					this.writeQueue.AddLast(new KeyValuePair<IPEndPoint, byte[]>(RemoteEndpoint, Datagram));
				else
				{
					this.isWriting = true;
					try
					{
						this.udpClient.BeginSend(Datagram, Datagram.Length, RemoteEndpoint, this.EndSend, new UdpDatagramEventArgs(RemoteEndpoint, Datagram));
					}
					catch (Exception)
					{
						this.isWriting = false;
						this.writeQueue.Clear();
						throw;
					}
				}
			}
		}

		private bool isWriting = false;
		private LinkedList<KeyValuePair<IPEndPoint, byte[]>> writeQueue = new LinkedList<KeyValuePair<IPEndPoint, byte[]>>();

		private void EndSend(IAsyncResult ar)
		{
			try
			{
				if (this.udpClient != null)
				{
					UdpDatagramEventArgs e = (UdpDatagramEventArgs)ar.AsyncState;
					this.udpClient.EndSend(ar);

					lock (this.writeQueue)
					{
						if (this.writeQueue.First != null)
						{
							KeyValuePair<IPEndPoint, byte[]> Rec = this.writeQueue.First.Value;
							this.writeQueue.RemoveFirst();

							this.udpClient.BeginSend(Rec.Value, Rec.Value.Length, Rec.Key, this.EndSend, new UdpDatagramEventArgs(Rec.Key, Rec.Value));
						}
						else
							this.isWriting = false;
					}

					UdpDatagramEvent h = this.OnUdpDatagramSent;
					if (h != null)
					{
						try
						{
							h(this, e);
						}
						catch (Exception ex)
						{
							Debug.WriteLine(ex.Message);
							Debug.WriteLine(ex.StackTrace.ToString());
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

				Debug.WriteLine(ex.Message);
				Debug.WriteLine(ex.StackTrace.ToString());
			}
		}

		/// <summary>
		/// Event raised when an outgoing UDP datagram has been sent.
		/// </summary>
		public event UdpDatagramEvent OnUdpDatagramSent = null;

	}
}
