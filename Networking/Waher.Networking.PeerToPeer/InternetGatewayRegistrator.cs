using System;
using System.Threading;
using System.Collections.Generic;
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
	/// Manages registration of TCP and UDP ports in an Internet Gateway
	/// </summary>
	public class InternetGatewayRegistrator : IDisposable
	{
		internal readonly InternetGatewayRegistration[] ports;
		internal IPAddress localAddress;
		internal IPAddress externalAddress;
		internal Exception exception = null;
		private readonly ISniffer[] sniffers;
		private Dictionary<IPAddress, bool> ipAddressesFound = new Dictionary<IPAddress, bool>();
		private UPnPClient upnpClient = null;
		private WANIPConnectionV1 serviceWANIPConnectionV1;
		private PeerToPeerNetworkState state = PeerToPeerNetworkState.Created;
		private ManualResetEvent ready = new ManualResetEvent(false);
		private ManualResetEvent error = new ManualResetEvent(false);
		private Timer searchTimer = null;
		internal bool disposed = false;

		/// <summary>
		/// Manages registration of TCP and UDP ports in an Internet Gateway
		/// </summary>
		/// <param name="Ports">Ports to register in the Internet Gateway.</param>
		/// <param name="Sniffers">Sniffers</param>
		public InternetGatewayRegistrator(InternetGatewayRegistration[] Ports, params ISniffer[] Sniffers)
		{
			this.ports = Ports;
			this.sniffers = Sniffers;

			NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
		}

		private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
		{
			if (State != PeerToPeerNetworkState.SearchingForGateway)    // Multiple events might get fired one after the other. Just start one search.
			{
				this.State = PeerToPeerNetworkState.Reinitializing;

				this.ready?.Reset();
				this.error?.Reset();

				this.Start();
			}
		}

		/// <summary>
		/// Starts the registration.
		/// </summary>
		public virtual void Start()
		{
			if (this.OnPublicNetwork())
				this.localAddress = this.externalAddress;
			else
				this.SearchGateways();
		}

		/// <summary>
		/// If the machine is on a public network.
		/// </summary>
		/// <returns>If on a public network.</returns>
		public bool OnPublicNetwork()
		{
			foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				IPInterfaceProperties Properties = Interface.GetIPProperties();

				foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
				{
					if (!IsPublicAddress(UnicastAddress.Address))
						continue;

					this.externalAddress = UnicastAddress.Address;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if an IPv4 address is public.
		/// </summary>
		/// <param name="Address">IPv4 address.</param>
		/// <returns>If address is public.</returns>
		public static bool IsPublicAddress(IPAddress Address)
		{
			if (Address.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4)
			{
				byte[] Addr = Address.GetAddressBytes();

				// https://www.iana.org/assignments/ipv4-address-space/ipv4-address-space.xhtml

				switch (Addr[0])
				{
					case 0:
						return false;   // 000.X.X.X/8: Reserved for self-identification [RFC1122]

					case 10:
						return false;   // 010.X.X.X/8: Reserved for Private-Use Networks [RFC1918]

					case 100:           // 100.64.X.X/10 reserved for Shared Address Space [RFC6598].
						return (Addr[1] & 0xc0) != 64;

					case 127:           // 127.X.X.X/8 reserved for Loopback [RFC1122]
						return false;

					case 169:           // 169.254.X.X/16 reserved for Link Local
						return Addr[1] != 254;

					case 172:           // 172.16.0.0/12 reserved for Private-Use Networks
						return (Addr[1] & 0xf0) != 16;

					case 192:
						switch (Addr[1])
						{
							case 0:
								switch (Addr[2])
								{
									case 0:				// 192.0.0.0/24 reserved for IANA IPv4 Special Purpose Address Registry
										return false;

									case 2:				// 192.0.2.X/24  reserved for TEST-NET-1
										return false;

									default:
										return true;
								}

							case 88:
								return Addr[2] != 99;   // 192.88.99.X/24 reserved for 6to4 Relay Anycast

							case 168:
								return false;           // 192.168.0.0/16 reserved for Private-Use Networks
						}
						break;
				}

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Searches for Internet Gateways in the network.
		/// </summary>
		public void SearchGateways()
		{
			try
			{
				this.searchTimer?.Dispose();
				this.searchTimer = null;

				if (this.upnpClient is null)
				{
					this.upnpClient = new UPnPClient(this.sniffers);
					this.upnpClient.OnDeviceFound += this.UpnpClient_OnDeviceFound;
				}

				lock (this.ipAddressesFound)
				{
					this.ipAddressesFound.Clear();
				}

				this.State = PeerToPeerNetworkState.SearchingForGateway;

				this.upnpClient.StartSearch("urn:schemas-upnp-org:service:WANIPConnection:1", 1);
				this.upnpClient.StartSearch("urn:schemas-upnp-org:service:WANIPConnection:2", 1);

				this.searchTimer = new Timer(this.SearchTimeout, null, 10000, Timeout.Infinite);
			}
			catch (Exception ex)
			{
				this.exception = ex;
				this.State = PeerToPeerNetworkState.Error;
			}
		}

		private void SearchTimeout(object State)
		{
			this.searchTimer?.Dispose();
			this.searchTimer = null;

			this.State = PeerToPeerNetworkState.Error;
		}

		private void Reinitialize(object State)
		{
			this.searchTimer?.Dispose();
			this.searchTimer = null;

			this.NetworkChange_NetworkAddressChanged(this, new EventArgs());
		}

		private async void UpnpClient_OnDeviceFound(object Sender, DeviceLocationEventArgs e)
		{
			try
			{
				lock (this.ipAddressesFound)
				{
					if (this.ipAddressesFound.ContainsKey(e.RemoteEndPoint.Address))
						return;

					this.ipAddressesFound[e.RemoteEndPoint.Address] = true;
				}

				DeviceDescriptionDocument Doc = await e.Location.GetDeviceAsync();
				if (Doc != null)
				{
					UPnPService Service = Doc.GetService("urn:schemas-upnp-org:service:WANIPConnection:1");
					if (Service is null)
					{
						Service = Doc.GetService("urn:schemas-upnp-org:service:WANIPConnection:2");
						if (Service is null)
							return;
					}

					ServiceDescriptionDocument Scpd = await Service.GetServiceAsync();
					this.ServiceRetrieved(Scpd, e.LocalEndPoint);
				}
			}
			catch (Exception ex)
			{
				this.exception = ex;
				this.State = PeerToPeerNetworkState.Error;
			}
		}

		private void ServiceRetrieved(ServiceDescriptionDocument Scpd, IPEndPoint LocalEndPoint)
		{
			try
			{
				Dictionary<ushort, bool> TcpPortMapped = new Dictionary<ushort, bool>();
				Dictionary<ushort, bool> UdpPortMapped = new Dictionary<ushort, bool>();
				ushort PortMappingIndex;

				this.serviceWANIPConnectionV1 = new WANIPConnectionV1(Scpd);
				this.State = PeerToPeerNetworkState.RegisteringApplicationInGateway;

				this.serviceWANIPConnectionV1.GetExternalIPAddress(out string NewExternalIPAddress);
				this.externalAddress = IPAddress.Parse(NewExternalIPAddress);

				Log.Informational("External IP Address: " + NewExternalIPAddress);

				if (!IsPublicAddress(this.externalAddress))
				{
					Log.Warning("External IP Address not a public IP address.");
					return;     // TODO: Handle multiple layers of gateways.
				}

				PortMappingIndex = 0;

				try
				{
					string LocalAddress = LocalEndPoint.Address.ToString();

					while (true)
					{
						this.serviceWANIPConnectionV1.GetGenericPortMappingEntry(PortMappingIndex, out string NewRemoteHost,
							out ushort NewExternalPort, out string NewProtocol, out ushort NewInternalPort, out string NewInternalClient,
							out bool NewEnabled, out string NewPortMappingDescription, out uint NewLeaseDuration);

						if (NewInternalClient != LocalAddress)
						{
							PortMappingIndex++;
							continue;
						}

						bool Found = false;

						foreach (InternetGatewayRegistration Registration in this.ports)
						{
							if ((Registration.ExternalPort != 0 && NewExternalPort == Registration.ExternalPort) ||
								(Registration.ExternalPort == 0 && NewPortMappingDescription == Registration.ApplicationName))
							{
								if (NewProtocol == "TCP")
								{
									Found = true;
									Registration.TcpRegistered = true;
									break;
								}
								else if (NewProtocol == "UDP")
								{
									Found = true;
									Registration.UdpRegistered = true;
									break;
								}

								Log.Notice("Deleting Internet Gateway port mapping.",
									new KeyValuePair<string, object>("Host", NewRemoteHost),
									new KeyValuePair<string, object>("External Port", NewExternalPort),
									new KeyValuePair<string, object>("Protocol", NewProtocol),
									new KeyValuePair<string, object>("Local Port", NewInternalPort),
									new KeyValuePair<string, object>("Local Address", NewInternalClient),
									new KeyValuePair<string, object>("Application", NewPortMappingDescription));

								this.serviceWANIPConnectionV1.DeletePortMapping(NewRemoteHost, NewExternalPort, NewProtocol);
							}
						}

						if (Found)
						{
							PortMappingIndex++;
							continue;
						}
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
				catch (AggregateException ex)
				{
					if (!(ex.InnerException is UPnPException))
						System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
				}
				catch (UPnPException)
				{
					// No more entries.
				}

				this.localAddress = LocalEndPoint.Address;

				foreach (InternetGatewayRegistration Registration in this.ports)
				{
					this.BeforeRegistration(Registration, TcpPortMapped, UdpPortMapped);

					if ((Registration.TcpRegistered || !Registration.Tcp) &&
						(Registration.UdpRegistered || !Registration.Udp))
					{
						continue;
					}

					if (Registration.Tcp && !Registration.TcpRegistered)
					{
						Log.Notice("Adding Internet Gateway port mapping.",
							new KeyValuePair<string, object>("Host", string.Empty),
							new KeyValuePair<string, object>("External Port", Registration.ExternalPort),
							new KeyValuePair<string, object>("Protocol", "TCP"),
							new KeyValuePair<string, object>("Local Port", Registration.LocalPort),
							new KeyValuePair<string, object>("Local Address", LocalAddress.ToString()),
							new KeyValuePair<string, object>("Application", Registration.ApplicationName));

						try
						{
							this.serviceWANIPConnectionV1.AddPortMapping(string.Empty, Registration.ExternalPort,
								"TCP", Registration.LocalPort, LocalAddress.ToString(), true, Registration.ApplicationName, 0);

							Registration.TcpRegistered = true;
						}
						catch (Exception ex)
						{
							Log.Error("Unable to register port in Internet Gateway: " + ex.Message,
								new KeyValuePair<string, object>("External Port", Registration.ExternalPort),
								new KeyValuePair<string, object>("Protocol", "TCP"),
								new KeyValuePair<string, object>("Local Port", Registration.LocalPort),
								new KeyValuePair<string, object>("Local Address", LocalAddress.ToString()),
								new KeyValuePair<string, object>("Application", Registration.ApplicationName));
						}
					}

					if (Registration.Udp && !Registration.UdpRegistered)
					{
						Log.Notice("Adding Internet Gateway port mapping.",
							new KeyValuePair<string, object>("Host", string.Empty),
							new KeyValuePair<string, object>("External Port", Registration.ExternalPort),
							new KeyValuePair<string, object>("Protocol", "UDP"),
							new KeyValuePair<string, object>("Local Port", Registration.LocalPort),
							new KeyValuePair<string, object>("Local Address", LocalAddress.ToString()),
							new KeyValuePair<string, object>("Application", Registration.ApplicationName));

						try
						{
							this.serviceWANIPConnectionV1.AddPortMapping(string.Empty, Registration.ExternalPort,
								"UDP", Registration.LocalPort, LocalAddress.ToString(), true, Registration.ApplicationName, 0);

							Registration.UdpRegistered = true;
						}
						catch (Exception ex)
						{
							Log.Error("Unable to register port in Internet Gateway: " + ex.Message,
								new KeyValuePair<string, object>("External Port", Registration.ExternalPort),
								new KeyValuePair<string, object>("Protocol", "UDP"),
								new KeyValuePair<string, object>("Local Port", Registration.LocalPort),
								new KeyValuePair<string, object>("Local Address", LocalAddress.ToString()),
								new KeyValuePair<string, object>("Application", Registration.ApplicationName));
						}
					}
				}

				this.State = PeerToPeerNetworkState.Ready;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message);

				this.exception = ex;
				this.State = PeerToPeerNetworkState.Error;
			}
		}

		/// <summary>
		/// is called before performing a registration.
		/// </summary>
		/// <param name="Registration">Registration to be performed.</param>
		/// <param name="TcpPortMapped">What TCP ports have already been mapped.</param>
		/// <param name="UdpPortMapped">What UDP ports have already been mapped.</param>
		protected virtual void BeforeRegistration(InternetGatewayRegistration Registration,
			Dictionary<ushort, bool> TcpPortMapped, Dictionary<ushort, bool> UdpPortMapped)
		{
			// Do nothing by default.
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
							this.searchTimer?.Dispose();
							this.searchTimer = null;
							this.ready?.Set();
							break;

						case PeerToPeerNetworkState.Error:
							this.searchTimer?.Dispose();
							this.searchTimer = null;
							this.error?.Set();

							this.searchTimer = new Timer(this.Reinitialize, null, 60000, Timeout.Infinite);
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
							Log.Critical(ex);
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
		/// Local IP Address.
		/// </summary>
		public IPAddress LocalAddress
		{
			get { return this.localAddress; }
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
		public virtual void Dispose()
		{
			this.disposed = true;
			this.State = PeerToPeerNetworkState.Closed;

			NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;

			this.searchTimer?.Dispose();
			this.searchTimer = null;

			foreach (InternetGatewayRegistration Registration in this.ports)
			{
				if (Registration.TcpRegistered)
				{
					Registration.TcpRegistered = false;
					try
					{
						Log.Notice("Deleting Internet Gateway port mapping.",
							new KeyValuePair<string, object>("Host", string.Empty),
							new KeyValuePair<string, object>("External Port", Registration.ExternalPort),
							new KeyValuePair<string, object>("Protocol", "TCP"),
							new KeyValuePair<string, object>("Local Port", Registration.LocalPort),
							new KeyValuePair<string, object>("Application", Registration.ApplicationName));

						this.serviceWANIPConnectionV1.DeletePortMapping(string.Empty, Registration.LocalPort, "TCP");
					}
					catch (Exception)
					{
						// Ignore
					}
				}

				if (Registration.UdpRegistered)
				{
					Registration.UdpRegistered = false;
					try
					{
						Log.Notice("Deleting Internet Gateway port mapping.",
							new KeyValuePair<string, object>("Host", string.Empty),
							new KeyValuePair<string, object>("External Port", Registration.ExternalPort),
							new KeyValuePair<string, object>("Protocol", "UDP"),
							new KeyValuePair<string, object>("Local Port", Registration.LocalPort),
							new KeyValuePair<string, object>("Application", Registration.ApplicationName));

						this.serviceWANIPConnectionV1.DeletePortMapping(string.Empty, Registration.LocalPort, "UDP");
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}

			this.serviceWANIPConnectionV1 = null;

			this.upnpClient?.Dispose();
			this.upnpClient = null;

			this.ipAddressesFound?.Clear();
			this.ipAddressesFound = null;

			this.ready?.Dispose();
			this.ready = null;

			this.error?.Dispose();
			this.error = null;
		}

		/// <summary>
		/// Checks if a remote endpoint resides in the internal network, and if so, replaces it with the corresponding
		/// local endpoint.
		/// </summary>
		/// <param name="RemoteEndPoint">Remote endpoint.</param>
		/// <returns>Optimized remote endpoint.</returns>
		public IPEndPoint CheckLocalRemoteEndpoint(IPEndPoint RemoteEndPoint)
		{
			if (IPAddress.Equals(RemoteEndPoint.Address, this.externalAddress))
			{
				this.serviceWANIPConnectionV1.GetSpecificPortMappingEntry(string.Empty, (ushort)RemoteEndPoint.Port, "TCP",
					out ushort InternalPort, out string InternalClient, out bool _, out string _, out uint _);

				return new IPEndPoint(IPAddress.Parse(InternalClient), InternalPort);
			}
			else
				return RemoteEndPoint;
		}

	}
}
