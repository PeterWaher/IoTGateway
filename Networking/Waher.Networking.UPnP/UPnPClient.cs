using System;
using System.Diagnostics;
using System.Threading;
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
	/// UPnP error event handler.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Exception">Information about error received.</param>
	public delegate void UPnPExceptionEventHandler(object Sender, Exception Exception);

	/// <summary>
	/// Implements support for the UPnP protocol, as described in:
	/// http://upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0.pdf
	/// </summary>
	public class UPnPClient : Sniffable, IDisposable
	{
		private const int ssdpPort = 1900;
		private const int defaultMaximumSearchTimeSeconds = 10;

		private LinkedList<KeyValuePair<UdpClient, IPEndPoint>> ssdpOutgoing = new LinkedList<KeyValuePair<UdpClient, IPEndPoint>>();
		private LinkedList<UdpClient> ssdpIncoming = new LinkedList<UdpClient>();
		private bool disposed = false;

		/// <summary>
		/// Implements support for the UPnP protocol, as described in:
		/// http://upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0.pdf
		/// </summary>
		/// <param name="Sniffers">Sniffers.</param>
		public UPnPClient(params ISniffer[] Sniffers)
			: base(Sniffers)
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
							Outgoing.DontFragment = true;
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
					Outgoing.JoinMulticastGroup(MulticastAddress);

					IPEndPoint EP = new IPEndPoint(MulticastAddress, ssdpPort);
					this.ssdpOutgoing.AddLast(new KeyValuePair<UdpClient, IPEndPoint>(Outgoing, EP));

					this.BeginReceiveOutgoing(Outgoing);

					try
					{
						Incoming = new UdpClient(Outgoing.Client.AddressFamily)
						{
							ExclusiveAddressUse = false
						};

						Incoming.Client.Bind(new IPEndPoint(UnicastAddress.Address, ssdpPort));
						this.BeginReceiveIncoming(Incoming);

						this.ssdpIncoming.AddLast(Incoming);
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

							Incoming.JoinMulticastGroup(MulticastAddress);
							this.BeginReceiveIncoming(Incoming);

							this.ssdpIncoming.AddLast(Incoming);
						}
						catch (Exception)
						{
							Incoming = null;
						}
					}
				}
			}
		}

		private async void BeginReceiveOutgoing(UdpClient Client)
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await Client.ReceiveAsync();
					if (this.disposed)
						return;

					byte[] Packet = Data.Buffer;
					this.ReceiveBinary(Packet);

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
								UPnPDeviceLocationEventHandler h = this.OnDeviceFound;
								if (h != null)
								{
									DeviceLocation DeviceLocation = new DeviceLocation(this, Headers.SearchTarget, Headers.Server, Headers.Location,
										Headers.UniqueServiceName, Headers);
									DeviceLocationEventArgs e = new DeviceLocationEventArgs(DeviceLocation, (IPEndPoint)Client.Client.LocalEndPoint, Data.RemoteEndPoint);
									try
									{
										h(this, e);
									}
									catch (Exception ex)
									{
										this.RaiseOnError(ex);
									}
								}
							}
						}
						else if (Headers.Direction == HttpDirection.Request && Headers.HttpVersion >= 1.0)
							this.HandleIncoming(Client, Data.RemoteEndPoint, Headers);
					}
					catch (Exception ex)
					{
						this.RaiseOnError(ex);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Closed.
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
			}
		}

		/// <summary>
		/// Event raised when a device has been found as a result of a search made by the client.
		/// </summary>
		public event UPnPDeviceLocationEventHandler OnDeviceFound = null;

		private void HandleIncoming(UdpClient UdpClient, IPEndPoint RemoteIP, UPnPHeaders Headers)
		{
			switch (Headers.Verb)
			{
				case "M-SEARCH":
					NotificationEventHandler h = this.OnSearch;
					if (h != null)
					{
						try
						{
							h(this, new NotificationEventArgs(this, Headers, (IPEndPoint)UdpClient.Client.LocalEndPoint, RemoteIP));
						}
						catch (Exception ex)
						{
							this.RaiseOnError(ex);
						}
					}
					break;

				case "NOTIFY":
					h = this.OnNotification;
					if (h != null)
					{
						try
						{
							h(this, new NotificationEventArgs(this, Headers, (IPEndPoint)UdpClient.Client.LocalEndPoint, RemoteIP));
						}
						catch (Exception ex)
						{
							this.RaiseOnError(ex);
						}
					}
					break;
			}
		}

		/// <summary>
		/// Event raised when the client is notified of a device or service in the network.
		/// </summary>
		public event NotificationEventHandler OnNotification = null;

		/// <summary>
		/// Event raised when the client receives a request searching for devices or services in the network.
		/// </summary>
		public event NotificationEventHandler OnSearch = null;

		private async void BeginReceiveIncoming(UdpClient Client)
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await Client.ReceiveAsync();
					if (this.disposed)
						return;

					byte[] Packet = Data.Buffer;
					this.ReceiveBinary(Packet);

					if (this.disposed)
						return;

					try
					{
						string Header = Encoding.ASCII.GetString(Packet);
						UPnPHeaders Headers = new UPnPHeaders(Header);

						this.ReceiveText(Header);

						if (Data.RemoteEndPoint != null &&
							Headers.Direction == HttpDirection.Request &&
							Headers.HttpVersion >= 1.0)
						{
							this.HandleIncoming(Client, Data.RemoteEndPoint, Headers);
						}
					}
					catch (Exception ex)
					{
						this.RaiseOnError(ex);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Closed.
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
			}
		}

		/// <summary>
		/// Starts a search for devices on the network.
		/// </summary>
		public void StartSearch()
		{
			this.StartSearch("upnp:rootdevice", defaultMaximumSearchTimeSeconds);
			//this.StartSearch("ssdp:all", defaultMaximumSearchTimeSeconds);
		}

		/// <summary>
		/// Starts a search for devices on the network.
		/// </summary>
		/// <param name="MaximumWaitTimeSeconds">Maximum Wait Time, in seconds. Default=10 seconds.</param>
		public void StartSearch(int MaximumWaitTimeSeconds)
		{
			this.StartSearch("upnp:rootdevice", MaximumWaitTimeSeconds);
			//this.StartSearch("ssdp:all", MaximumWaitTimeSeconds);
		}

		/// <summary>
		/// Starts a search for devices on the network.
		/// </summary>
		/// <param name="SearchTarget">Search target. (Default="upnp:rootdevice", which searches for all types of root devices.)</param>
		public void StartSearch(string SearchTarget)
		{
			this.StartSearch(SearchTarget, defaultMaximumSearchTimeSeconds);
		}

		/// <summary>
		/// Starts a search for devices on the network.
		/// </summary>
		/// <param name="SearchTarget">Search target. (Default="upnp:rootdevice", which searches for all types of root devices.)</param>
		/// <param name="MaximumWaitTimeSeconds">Maximum Wait Time, in seconds. Default=10 seconds.</param>
		public void StartSearch(string SearchTarget, int MaximumWaitTimeSeconds)
		{
			foreach (KeyValuePair<UdpClient, IPEndPoint> P in this.ssdpOutgoing)
			{
				string MSearch = "M-SEARCH * HTTP/1.1\r\n" +
					"HOST: " + P.Value.ToString() + "\r\n" +
					"MAN:\"ssdp:discover\"\r\n" +
					"ST: " + SearchTarget + "\r\n" +
					"MX:" + MaximumWaitTimeSeconds.ToString() + "\r\n\r\n";
				byte[] Packet = Encoding.ASCII.GetBytes(MSearch);

				this.SendPacket(P.Key, P.Value, Packet, MSearch);
			}
		}

		private async void SendPacket(UdpClient Client, IPEndPoint Destination, byte[] Packet, string Text)
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
				this.RaiseOnError(ex);
			}
		}

		private void RaiseOnError(Exception ex)
		{
			UPnPExceptionEventHandler h = this.OnError;
			if (h != null)
			{
				try
				{
					h(this, ex);
				}
				catch (Exception ex2)
				{
					Events.Log.Critical(ex2);
				}
			}
		}

		/// <summary>
		/// Event raised when an error occurs.
		/// </summary>
		public event UPnPExceptionEventHandler OnError = null;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;

			foreach (KeyValuePair<UdpClient, IPEndPoint> P in this.ssdpOutgoing)
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

			this.ssdpOutgoing.Clear();

			foreach (UdpClient Client in this.ssdpIncoming)
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

			this.ssdpIncoming.Clear();

			foreach (ISniffer Sniffer in this.Sniffers)
			{
				if (Sniffer is IDisposable Disposable)
				{
					try
					{
						Disposable.Dispose();
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
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

					XmlDocument Xml = new XmlDocument();
					Xml.Load(Stream);

					return new DeviceDescriptionDocument(Xml, this, Location);
				}
				catch (Exception ex)
				{
					this.RaiseOnError(ex);
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

					XmlDocument Xml = new XmlDocument();
					Xml.Load(Stream);

					return new ServiceDescriptionDocument(Xml, this, Service);
				}
				catch (Exception ex)
				{
					this.RaiseOnError(ex);
					return null;
				}
			}
		}

	}
}
