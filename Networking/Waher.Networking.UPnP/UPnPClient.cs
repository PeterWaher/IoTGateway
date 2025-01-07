using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Implements support for the UPnP protocol, as described in:
	/// http://upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0.pdf
	/// </summary>
	public class UPnPClient : CommunicationLayer, IDisposableAsync
	{
		private const int ssdpPort = 1900;
		private const int defaultMaximumSearchTimeSeconds = 10;

		private readonly List<KeyValuePair<UdpClient, IPEndPoint>> ssdpOutgoing = new List<KeyValuePair<UdpClient, IPEndPoint>>();
		private readonly List<UdpClient> ssdpIncoming = new List<UdpClient>();
		private bool disposed = false;

		/// <summary>
		/// Implements support for the UPnP protocol, as described in:
		/// http://upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0.pdf
		/// </summary>
		/// <param name="Sniffers">Sniffers.</param>
		public UPnPClient(params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			Dictionary<AddressFamily, bool> GenIncoming = new Dictionary<AddressFamily, bool>();
			UdpClient Outgoing;
			UdpClient Incoming;

			foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				IPInterfaceProperties Properties = Interface.GetIPProperties();
				IPAddress MulticastAddress;

				foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
				{
					if (UnicastAddress.Address.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4)
					{
						try
						{
							Outgoing = new UdpClient(AddressFamily.InterNetwork);
							MulticastAddress = IPAddress.Parse("239.255.255.250");
							//Outgoing.DontFragment = true;
							Outgoing.MulticastLoopback = false;
						}
						catch (Exception)
						{
							continue;
						}
					}
					else if (UnicastAddress.Address.AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6)
					{
						try
						{
							Outgoing = new UdpClient(AddressFamily.InterNetworkV6)
							{
								MulticastLoopback = false
							};

							MulticastAddress = IPAddress.Parse("[FF02::C]");
						}
						catch (Exception)
						{
							continue;
						}
					}
					else
						continue;

					Outgoing.EnableBroadcast = true;
					Outgoing.MulticastLoopback = false;
					Outgoing.Ttl = 30;
					Outgoing.Client.Bind(new IPEndPoint(UnicastAddress.Address, 0));

					if (IsMulticastAddress(MulticastAddress))
						Outgoing.JoinMulticastGroup(MulticastAddress);
					else
						this.Warning("Address provided is not a multi-cast address.");

					IPEndPoint EP = new IPEndPoint(MulticastAddress, ssdpPort);
					lock (this.ssdpOutgoing)
					{
						this.ssdpOutgoing.Add(new KeyValuePair<UdpClient, IPEndPoint>(Outgoing, EP));
					}

					this.BeginReceiveOutgoing(Outgoing);

					try
					{
						Incoming = new UdpClient(Outgoing.Client.AddressFamily)
						{
							ExclusiveAddressUse = false
						};

						Incoming.Client.Bind(new IPEndPoint(UnicastAddress.Address, ssdpPort));
						this.BeginReceiveIncoming(Incoming);

						lock (this.ssdpIncoming)
						{
							this.ssdpIncoming.Add(Incoming);
						}
					}
					catch (Exception)
					{
						Incoming = null;
					}

					if (!GenIncoming.ContainsKey(Outgoing.Client.AddressFamily))
					{
						GenIncoming[Outgoing.Client.AddressFamily] = true;

						try
						{
							Incoming = new UdpClient(ssdpPort, Outgoing.Client.AddressFamily)
							{
								MulticastLoopback = false
							};

							if (IsMulticastAddress(MulticastAddress))
								Incoming.JoinMulticastGroup(MulticastAddress);
							else
								this.Warning("Address provided is not a multi-cast address.");

							this.BeginReceiveIncoming(Incoming);

							lock (this.ssdpIncoming)
							{
								this.ssdpIncoming.Add(Incoming);
							}
						}
						catch (Exception)
						{
							Incoming = null;
						}
					}
				}
			}
		}

		private async void BeginReceiveOutgoing(UdpClient Client)   // Starts parallel task
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await Client.ReceiveAsync();
					if (this.disposed)
						return;

					byte[] Packet = Data.Buffer;
					this.ReceiveBinary(true, Packet);

					try
					{
						string Header = Encoding.ASCII.GetString(Packet);
						UPnPHeaders Headers = new UPnPHeaders(Header);

						this.ReceiveText(Header);

						if (Headers.Direction == HttpDirection.Response &&
							Headers.HttpVersion >= 1.0 &&
							Headers.ResponseCode == 200)
						{
							if (!string.IsNullOrEmpty(Headers.Location))
							{
								DeviceLocation DeviceLocation = new DeviceLocation(this, Headers.SearchTarget, Headers.Server, Headers.Location,
									Headers.UniqueServiceName, Headers);
								DeviceLocationEventArgs e = new DeviceLocationEventArgs(DeviceLocation, (IPEndPoint)Client.Client.LocalEndPoint, Data.RemoteEndPoint);

								await this.OnDeviceFound.Raise(this, e);
							}
						}
						else if (Headers.Direction == HttpDirection.Request && Headers.HttpVersion >= 1.0)
							await this.HandleIncoming(Client, Data.RemoteEndPoint, Headers);
					}
					catch (Exception ex)
					{
						await this.RaiseOnError(ex);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Closed.
			}
			catch (Exception ex)
			{
				this.Exception(ex);
			}
		}

		/// <summary>
		/// Event raised when a device has been found as a result of a search made by the client.
		/// </summary>
		public event EventHandlerAsync<DeviceLocationEventArgs> OnDeviceFound = null;

		private async Task HandleIncoming(UdpClient UdpClient, IPEndPoint RemoteIP, UPnPHeaders Headers)
		{
			switch (Headers.Verb)
			{
				case "M-SEARCH":
					await this.OnSearch.Raise(this, new NotificationEventArgs(this, Headers, (IPEndPoint)UdpClient.Client.LocalEndPoint, RemoteIP));
					break;

				case "NOTIFY":
					await this.OnNotification.Raise(this, new NotificationEventArgs(this, Headers, (IPEndPoint)UdpClient.Client.LocalEndPoint, RemoteIP));
					break;
			}
		}

		/// <summary>
		/// Event raised when the client is notified of a device or service in the network.
		/// </summary>
		public event EventHandlerAsync<NotificationEventArgs> OnNotification = null;

		/// <summary>
		/// Event raised when the client receives a request searching for devices or services in the network.
		/// </summary>
		public event EventHandlerAsync<NotificationEventArgs> OnSearch = null;

		private async void BeginReceiveIncoming(UdpClient Client)   // Starts parallel task
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await Client.ReceiveAsync();
					if (this.disposed)
						return;

					byte[] Packet = Data.Buffer;
					this.ReceiveBinary(true, Packet);

					if (this.disposed)
						return;

					try
					{
						string Header = Encoding.ASCII.GetString(Packet);
						UPnPHeaders Headers = new UPnPHeaders(Header);

						this.ReceiveText(Header);

						if (!(Data.RemoteEndPoint is null) &&
							Headers.Direction == HttpDirection.Request &&
							Headers.HttpVersion >= 1.0)
						{
							await this.HandleIncoming(Client, Data.RemoteEndPoint, Headers);
						}
					}
					catch (Exception ex)
					{
						await this.RaiseOnError(ex);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Closed.
			}
			catch (Exception ex)
			{
				this.Exception(ex);
			}
		}

		/// <summary>
		/// Starts a search for devices on the network.
		/// </summary>
		public Task StartSearch()
		{
			return this.StartSearch("upnp:rootdevice", defaultMaximumSearchTimeSeconds);
			//this.StartSearch("ssdp:all", defaultMaximumSearchTimeSeconds);
		}

		/// <summary>
		/// Starts a search for devices on the network.
		/// </summary>
		/// <param name="MaximumWaitTimeSeconds">Maximum Wait Time, in seconds. Default=10 seconds.</param>
		public Task StartSearch(int MaximumWaitTimeSeconds)
		{
			return this.StartSearch("upnp:rootdevice", MaximumWaitTimeSeconds);
			//this.StartSearch("ssdp:all", MaximumWaitTimeSeconds);
		}

		/// <summary>
		/// Starts a search for devices on the network.
		/// </summary>
		/// <param name="SearchTarget">Search target. (Default="upnp:rootdevice", which searches for all types of root devices.)</param>
		public Task StartSearch(string SearchTarget)
		{
			return this.StartSearch(SearchTarget, defaultMaximumSearchTimeSeconds);
		}

		/// <summary>
		/// Starts a search for devices on the network.
		/// </summary>
		/// <param name="SearchTarget">Search target. (Default="upnp:rootdevice", which searches for all types of root devices.)</param>
		/// <param name="MaximumWaitTimeSeconds">Maximum Wait Time, in seconds. Default=10 seconds.</param>
		public async Task StartSearch(string SearchTarget, int MaximumWaitTimeSeconds)
		{
			foreach (KeyValuePair<UdpClient, IPEndPoint> P in this.GetOutgoing())
			{
				string MSearch = "M-SEARCH * HTTP/1.1\r\n" +
					"HOST: " + P.Value.ToString() + "\r\n" +
					"MAN:\"ssdp:discover\"\r\n" +
					"ST: " + SearchTarget + "\r\n" +
					"MX:" + MaximumWaitTimeSeconds.ToString() + "\r\n\r\n";
				byte[] Packet = Encoding.ASCII.GetBytes(MSearch);

				await this.SendPacket(P.Key, P.Value, Packet, MSearch);
			}
		}

		private KeyValuePair<UdpClient, IPEndPoint>[] GetOutgoing()
		{
			return this.GetOutgoing(false);
		}

		private KeyValuePair<UdpClient, IPEndPoint>[] GetOutgoing(bool Clear)
		{
			lock (this.ssdpOutgoing)
			{
				KeyValuePair<UdpClient, IPEndPoint>[] Result = this.ssdpOutgoing.ToArray();

				if (Clear)
					this.ssdpOutgoing.Clear();

				return Result;
			}
		}

		private UdpClient[] GetIncoming()
		{
			return this.GetIncoming(false);
		}

		private UdpClient[] GetIncoming(bool Clear)
		{
			lock (this.ssdpIncoming)
			{
				UdpClient[] Result = this.ssdpIncoming.ToArray();

				if (Clear)
					this.ssdpIncoming.Clear();

				return Result;
			}
		}

		private async Task SendPacket(UdpClient Client, IPEndPoint Destination, byte[] Packet, string Text)
		{
			if (this.disposed)
				return;

			try
			{
				this.TransmitText(Text);
				await Client.SendAsync(Packet, Packet.Length, Destination);
			}
			catch (Exception ex)
			{
				await this.RaiseOnError(ex);
			}
		}

		private Task RaiseOnError(Exception ex)
		{
			return this.OnError.Raise(this, ex);
		}

		/// <summary>
		/// Event raised when an error occurs.
		/// </summary>
		public event EventHandlerAsync<Exception> OnError = null;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use DisposeAsync() instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync"/>
		/// </summary>
		public async Task DisposeAsync()
		{
			this.disposed = true;

			foreach (KeyValuePair<UdpClient, IPEndPoint> P in this.GetOutgoing(true))
			{
				try
				{
					P.Key.Dispose();
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			foreach (UdpClient Client in this.GetIncoming(true))
			{
				try
				{
					Client.Dispose();
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			foreach (ISniffer Sniffer in this.Sniffers)
			{
				try
				{
					if (Sniffer is IDisposableAsync DisposableAsync)
						await DisposableAsync.DisposeAsync();
					else if (Sniffer is IDisposable Disposable)
						Disposable.Dispose();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		/// <summary>
		/// Gets the device description document from a device in the network. 
		/// This method is the synchronous version of <see cref="GetDeviceAsync(string, int)"/>.
		/// </summary>
		/// <param name="Location">URL of the Device Description Document.</param>
		/// <returns>Device Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public DeviceDescriptionDocument GetDevice(string Location)
		{
			return this.GetDevice(Location, 10000);
		}

		/// <summary>
		/// Gets the device description document from a device in the network. 
		/// This method is the synchronous version of <see cref="GetDeviceAsync(string, int)"/>.
		/// </summary>
		/// <param name="Location">URL of the Device Description Document.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Device Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public DeviceDescriptionDocument GetDevice(string Location, int Timeout)
		{
			return this.GetDeviceAsync(Location, Timeout).Result;
		}

		/// <summary>
		/// Gets a Device Description Document from a device.
		/// </summary>
		/// <param name="Location">URL of the Device Description Document.</param>
		/// <returns>Device description document, if found, or null otherwise.</returns>
		public Task<DeviceDescriptionDocument> GetDeviceAsync(string Location)
		{
			return this.GetDeviceAsync(Location, 10000);
		}

		/// <summary>
		/// Gets a Device Description Document from a device.
		/// </summary>
		/// <param name="Location">URL of the Device Description Document.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Device description document, if found, or null otherwise.</returns>
		public async Task<DeviceDescriptionDocument> GetDeviceAsync(string Location, int Timeout)
		{
			Uri LocationUri = new Uri(Location);
			using (HttpClient Client = new HttpClient())
			{
				try
				{
					Client.Timeout = TimeSpan.FromMilliseconds(Timeout);
					Stream Stream = await Client.GetStreamAsync(LocationUri);

					XmlDocument Xml = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					Xml.Load(Stream);

					return new DeviceDescriptionDocument(Xml, this, Location);
				}
				catch (Exception ex)
				{
					await this.RaiseOnError(ex);
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the service description document from a service in the network. 
		/// This method is the synchronous version of <see cref="GetServiceAsync(UPnPService)"/>.
		/// </summary>
		/// <param name="Service">Service to get.</param>
		/// <returns>Service Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public ServiceDescriptionDocument GetService(UPnPService Service)
		{
			return this.GetService(Service, 10000);
		}

		/// <summary>
		/// Gets the service description document from a service in the network. 
		/// This method is the synchronous version of <see cref="GetServiceAsync(UPnPService, int)"/>.
		/// </summary>
		/// <param name="Service">Service to get.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Service Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public ServiceDescriptionDocument GetService(UPnPService Service, int Timeout)
		{
			return this.GetServiceAsync(Service, Timeout).Result;
		}

		/// <summary>
		/// Gets a Service Description Document from a device.
		/// </summary>
		/// <param name="Service">Service object.</param>
		/// <returns>Service description document, if found, or null otherwise.</returns>
		public Task<ServiceDescriptionDocument> GetServiceAsync(UPnPService Service)
		{
			return this.GetServiceAsync(Service, 10000);
		}

		/// <summary>
		/// Gets a Service Description Document from a device.
		/// </summary>
		/// <param name="Service">Service object.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Service description document, if found, or null otherwise.</returns>
		public async Task<ServiceDescriptionDocument> GetServiceAsync(UPnPService Service, int Timeout)
		{
			using (HttpClient Client = new HttpClient())
			{
				try
				{
					Client.Timeout = TimeSpan.FromMilliseconds(Timeout);
					Stream Stream = await Client.GetStreamAsync(Service.SCPDURI);

					XmlDocument Xml = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					Xml.Load(Stream);

					return new ServiceDescriptionDocument(Xml, this, Service);
				}
				catch (Exception ex)
				{
					await this.RaiseOnError(ex);
					return null;
				}
			}
		}

	}
}
