using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Xml;

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
	public class UPnPClient : IDisposable
	{
		private const int ssdpPort = 1900;
		private const int defaultMaximumSearchTimeSeconds = 10;

		private LinkedList<KeyValuePair<UdpClient, IPEndPoint>> ssdpOutgoing = new LinkedList<KeyValuePair<UdpClient, IPEndPoint>>();
		private LinkedList<UdpClient> ssdpIncoming = new LinkedList<UdpClient>();

		/// <summary>
		/// Implements support for the UPnP protocol, as described in:
		/// http://upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0.pdf
		/// </summary>
		public UPnPClient()
		{
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
							Outgoing = new UdpClient(AddressFamily.InterNetworkV6);
							Outgoing.MulticastLoopback = false;
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

					Outgoing.BeginReceive(this.EndReceiveOutgoing, Outgoing);

					try
					{
						Incoming = new UdpClient(Outgoing.Client.AddressFamily);
						Incoming.ExclusiveAddressUse = false;
						Incoming.Client.Bind(new IPEndPoint(UnicastAddress.Address, ssdpPort));

						Incoming.BeginReceive(this.EndReceiveIncoming, Incoming);

						this.ssdpIncoming.AddLast(Incoming);
					}
					catch (Exception)
					{
						Incoming = null;
					}

					try
					{
						Incoming = new UdpClient(ssdpPort, Outgoing.Client.AddressFamily);
						Incoming.MulticastLoopback = false;
						Incoming.JoinMulticastGroup(MulticastAddress);

						Incoming.BeginReceive(this.EndReceiveIncoming, Incoming);

						this.ssdpIncoming.AddLast(Incoming);
					}
					catch (Exception)
					{
						Incoming = null;
					}
				}
			}
		}

		private void EndReceiveOutgoing(IAsyncResult ar)
		{
			try
			{
				UdpClient UdpClient = (UdpClient)ar.AsyncState;
				IPEndPoint RemoteIP = null;
				byte[] Packet = UdpClient.EndReceive(ar, ref RemoteIP);
				string Header = Encoding.ASCII.GetString(Packet);
				UPnPHeaders Headers = new UPnPHeaders(Header);

				if (RemoteIP != null && Headers.Direction == HttpDirection.Response && Headers.HttpVersion >= 1.0 && Headers.ResponseCode == 200)
				{
					if (!string.IsNullOrEmpty(Headers.Location))
					{
						UPnPDeviceLocationEventHandler h = this.OnDeviceFound;
						if (h != null)
						{
							DeviceLocation DeviceLocation = new DeviceLocation(this, Headers.SearchTarget, Headers.Server, Headers.Location,
								Headers.UniqueServiceName, Headers);
							DeviceLocationEventArgs e = new DeviceLocationEventArgs(DeviceLocation, (IPEndPoint)UdpClient.Client.LocalEndPoint, RemoteIP);
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
					this.HandleIncoming(UdpClient, RemoteIP, Headers);

				UdpClient.BeginReceive(this.EndReceiveOutgoing, UdpClient);
			}
			catch (Exception ex)
			{
				this.RaiseOnError(ex);
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

		private void EndReceiveIncoming(IAsyncResult ar)
		{
			try
			{
				UdpClient UdpClient = (UdpClient)ar.AsyncState;
				IPEndPoint RemoteIP = null;
				byte[] Packet = UdpClient.EndReceive(ar, ref RemoteIP);
				string Header = Encoding.ASCII.GetString(Packet);
				UPnPHeaders Headers = new UPnPHeaders(Header);

				if (RemoteIP != null && Headers.Direction == HttpDirection.Request && Headers.HttpVersion >= 1.0)
					this.HandleIncoming(UdpClient, RemoteIP, Headers);

				UdpClient.BeginReceive(this.EndReceiveOutgoing, UdpClient);
			}
			catch (Exception ex)
			{
				this.RaiseOnError(ex);
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

				this.SendPacket(P.Key, P.Value, Packet);
			}
		}

		private void SendPacket(UdpClient Client, IPEndPoint Destination, byte[] Packet)
		{
			Client.BeginSend(Packet, Packet.Length, Destination, this.EndSend, Client);
		}

		private void EndSend(IAsyncResult ar)
		{
			try
			{
				UdpClient UdpClient = (UdpClient)ar.AsyncState;
				UdpClient.EndSend(ar);
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
			foreach (KeyValuePair<UdpClient, IPEndPoint> P in this.ssdpOutgoing)
			{
				try
				{
					P.Key.Close();
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
					Client.Close();
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			this.ssdpIncoming.Clear();
		}

		/// <summary>
		/// Gets the device description document from a device in the network. 
		/// This method is the synchronous version of <see cref="StartGetDevice"/>.
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
		/// This method is the synchronous version of <see cref="StartGetDevice"/>.
		/// </summary>
		/// <param name="Location">URL of the Device Description Document.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Device Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public DeviceDescriptionDocument GetDevice(string Location, int Timeout)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			DeviceDescriptionEventArgs e = null;

			this.StartGetDevice(Location, (sender, e2) =>
				{
					e = e2;
					Done.Set();
				}, null);

			if (!Done.WaitOne(Timeout))
				throw new TimeoutException("Timeout.");

			if (e.Exception != null)
				throw e.Exception;

			return e.DeviceDescriptionDocument;
		}

		/// <summary>
		/// Starts the retrieval of a Device Description Document.
		/// </summary>
		/// <param name="Location">URL of the Device Description Document.</param>
		/// <param name="Callback">Callback method. Will be called when the document has been downloaded, or an error has occurred.</param>
		/// <param name="State">State object propagated to the callback method.</param>
		public void StartGetDevice(string Location, DeviceDescriptionEventHandler Callback, object State)
		{
			Uri LocationUri = new Uri(Location);
			WebClient Client = new WebClient();
			Client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadDeviceCompleted);
			Client.DownloadDataAsync(LocationUri, new object[] { Callback, Location, State });
		}

		private void DownloadDeviceCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			object[] P = (object[])e.UserState;
			DeviceDescriptionEventHandler Callback = (DeviceDescriptionEventHandler)P[0];
			string BaseUrl = (string)P[1];
			object State = P[2];
			DeviceDescriptionEventArgs e2;

			if (e.Error != null)
				e2 = new DeviceDescriptionEventArgs(e.Error, this, State);
			else
			{
				try
				{
					XmlDocument Xml = new XmlDocument();
					Xml.Load(new MemoryStream(e.Result));

					DeviceDescriptionDocument Device = new DeviceDescriptionDocument(Xml, this, BaseUrl);
					e2 = new DeviceDescriptionEventArgs(Device, this, State);
				}
				catch (Exception ex)
				{
					this.RaiseOnError(ex);
					e2 = new DeviceDescriptionEventArgs(e.Error, this, State);
				}
				finally
				{
					WebClient Client = sender as WebClient;
					if (Client != null)
						Client.Dispose();
				}
			}


			try
			{
				Callback(this, e2);
			}
			catch (Exception ex)
			{
				this.RaiseOnError(ex);
			}
		}

		/// <summary>
		/// Gets the service description document from a service in the network. 
		/// This method is the synchronous version of <see cref="StartGetService"/>.
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
		/// This method is the synchronous version of <see cref="StartGetService"/>.
		/// </summary>
		/// <param name="Service">Service to get.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Service Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public ServiceDescriptionDocument GetService(UPnPService Service, int Timeout)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ServiceDescriptionEventArgs e = null;

			this.StartGetService(Service, (sender, e2) =>
			{
				e = e2;
				Done.Set();
			}, null);

			if (!Done.WaitOne(Timeout))
				throw new TimeoutException("Timeout.");

			if (e.Exception != null)
				throw e.Exception;

			return e.ServiceDescriptionDocument;
		}

		/// <summary>
		/// Starts the retrieval of a Service Description Document.
		/// </summary>
		/// <param name="Service">Service object.</param>
		/// <param name="Callback">Callback method. Will be called when the document has been downloaded, or an error has occurred.</param>
		/// <param name="State">State object propagated to the callback method.</param>
		public void StartGetService(UPnPService Service, ServiceDescriptionEventHandler Callback, object State)
		{
			WebClient Client = new WebClient();
			Client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadServiceCompleted);
			Client.DownloadDataAsync(Service.SCPDURI, new object[] { Service, Callback, State });
		}

		private void DownloadServiceCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			object[] P = (object[])e.UserState;
			UPnPService Service = (UPnPService)P[0];
			ServiceDescriptionEventHandler Callback = (ServiceDescriptionEventHandler)P[1];
			object State = P[2];
			ServiceDescriptionEventArgs e2;

			if (e.Error != null)
				e2 = new ServiceDescriptionEventArgs(e.Error, this, State);
			else
			{
				try
				{
					XmlDocument Xml = new XmlDocument();
					Xml.Load(new MemoryStream(e.Result));

					ServiceDescriptionDocument ServiceDoc = new ServiceDescriptionDocument(Xml, this, Service);
					e2 = new ServiceDescriptionEventArgs(ServiceDoc, this, State);
				}
				catch (Exception ex)
				{
					this.RaiseOnError(ex);
					e2 = new ServiceDescriptionEventArgs(e.Error, this, State);
				}
				finally
				{
					WebClient Client = sender as WebClient;
					if (Client != null)
						Client.Dispose();
				}
			}

			try
			{
				Callback(this, e2);
			}
			catch (Exception ex)
			{
				this.RaiseOnError(ex);
			}
		}


	}
}
