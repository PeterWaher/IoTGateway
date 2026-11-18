using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking;

namespace Waher.Things.Matter.Discovery
{
	/// <summary>
	/// Discovered DNS-SD service instance.
	/// </summary>
	public class DnsSdInstance
	{
		private readonly Dictionary<string, string> txt = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private readonly List<IPAddress> addresses = new List<IPAddress>();

		/// <summary>
		/// Discovered DNS-SD service instance.
		/// </summary>
		/// <param name="FullName">Full instance name (instance.service.domain).</param>
		public DnsSdInstance(string FullName)
		{
			this.FullName = FullName;

			int i = FullName.IndexOf('.');
			this.InstanceName = i < 0 ? FullName : FullName.Substring(0, i);
			this.ServiceType = i < 0 ? string.Empty : FullName.Substring(i + 1);
		}

		/// <summary>
		/// Full instance name.
		/// </summary>
		public string FullName { get; }

		/// <summary>
		/// Instance name (first label).
		/// </summary>
		public string InstanceName { get; }

		/// <summary>
		/// Service type (e.g. _matter._tcp.local).
		/// </summary>
		public string ServiceType { get; }

		/// <summary>
		/// Host name.
		/// </summary>
		public string HostName { get; internal set; }

		/// <summary>
		/// Port.
		/// </summary>
		public ushort Port { get; internal set; }

		/// <summary>
		/// Addresses of host.
		/// </summary>
		public IReadOnlyList<IPAddress> Addresses => this.addresses;

		/// <summary>
		/// TXT key/value pairs.
		/// </summary>
		public IReadOnlyDictionary<string, string> Txt => this.txt;

		/// <summary>
		/// When the information was last updated.
		/// </summary>
		public DateTime Updated { get; internal set; } = DateTime.UtcNow;

		/// <summary>
		/// If the instance has been resolved (host, port and at least one address known).
		/// </summary>
		public bool IsResolved => !string.IsNullOrEmpty(this.HostName) && this.Port != 0 && this.addresses.Count > 0;

		internal void SetTxt(string[] Strings)
		{
			this.txt.Clear();

			foreach (string s in Strings)
			{
				int i = s.IndexOf('=');

				if (i < 0)
					this.txt[s] = string.Empty;
				else
					this.txt[s.Substring(0, i)] = s.Substring(i + 1);
			}
		}

		internal void AddAddress(IPAddress Address)
		{
			lock (this.addresses)
			{
				foreach (IPAddress A in this.addresses)
				{
					if (A.Equals(Address))
						return;
				}

				this.addresses.Add(Address);
			}
		}

		/// <summary>
		/// Gets a TXT value.
		/// </summary>
		/// <param name="Key">Key.</param>
		/// <returns>Value, or null if not found.</returns>
		public string GetTxt(string Key)
		{
			return this.txt.TryGetValue(Key, out string Value) ? Value : null;
		}

		/// <summary>
		/// Gets the addresses, ordered by preference.
		/// </summary>
		/// <param name="IPv4">If IPv4 addresses are supported.</param>
		/// <param name="IPv6">If IPv6 addresses are supported.</param>
		/// <returns>Ordered addresses.</returns>
		public IPAddress[] GetPreferredAddresses(bool IPv4, bool IPv6)
		{
			List<IPAddress> Result = new List<IPAddress>();

			lock (this.addresses)
			{
				if (IPv6)
				{
					foreach (IPAddress A in this.addresses)
					{
						if (A.AddressFamily == AddressFamily.InterNetworkV6 && !A.IsIPv6LinkLocal)
							Result.Add(A);
					}

					foreach (IPAddress A in this.addresses)
					{
						if (A.AddressFamily == AddressFamily.InterNetworkV6 && A.IsIPv6LinkLocal)
							Result.Add(A);
					}
				}

				if (IPv4)
				{
					foreach (IPAddress A in this.addresses)
					{
						if (A.AddressFamily == AddressFamily.InterNetwork)
							Result.Add(A);
					}
				}
			}

			return Result.ToArray();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.FullName + " @ " + this.HostName + ":" + this.Port.ToString() + " [" + string.Join(", ", this.addresses) + "]";
		}
	}

	/// <summary>
	/// Multicast DNS (RFC 6762) client, supporting DNS-SD (RFC 6763) browsing and resolution over IPv4 and IPv6.
	/// </summary>
	public class MdnsClient : IDisposable
	{
		/// <summary>
		/// mDNS port.
		/// </summary>
		public const int MdnsPort = 5353;

		/// <summary>
		/// IPv4 mDNS multicast address.
		/// </summary>
		public static readonly IPAddress MulticastIPv4 = IPAddress.Parse("224.0.0.251");

		/// <summary>
		/// IPv6 mDNS multicast address.
		/// </summary>
		public static readonly IPAddress MulticastIPv6 = IPAddress.Parse("ff02::fb");

		private readonly Dictionary<string, DnsSdInstance> instances = new Dictionary<string, DnsSdInstance>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, List<string>> ptr = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, Dictionary<string, KeyValuePair<IPAddress, DateTime>>> hostAddresses =
			new Dictionary<string, Dictionary<string, KeyValuePair<IPAddress, DateTime>>>(StringComparer.OrdinalIgnoreCase);
		private readonly ICommunicationLayer sniffers;
		private Socket socket4;
		private Socket socket6;
		private bool unicastFallback4 = false;
		private bool unicastFallback6 = false;
		private bool disposed = false;

		/// <summary>
		/// Multicast DNS client.
		/// </summary>
		/// <param name="Sniffers">Optional communication layer for sniffers.</param>
		public MdnsClient(ICommunicationLayer Sniffers)
		{
			this.sniffers = Sniffers;
		}

		/// <summary>
		/// Event raised when a DNS-SD instance has been updated.
		/// </summary>
		public event EventHandlerAsync<DnsSdInstance> OnInstanceUpdated = null;

		/// <summary>
		/// Starts the client, opening sockets.
		/// </summary>
		public void Start()
		{
			if (!(this.socket4 is null) || !(this.socket6 is null))
				return;

			try
			{
				this.socket4 = this.OpenSocket(AddressFamily.InterNetwork, out this.unicastFallback4);
			}
			catch (Exception ex)
			{
				this.sniffers?.Information("IPv4 mDNS not available: " + ex.Message);
				this.socket4 = null;
			}

			try
			{
				if (Socket.OSSupportsIPv6)
					this.socket6 = this.OpenSocket(AddressFamily.InterNetworkV6, out this.unicastFallback6);
			}
			catch (Exception ex)
			{
				this.sniffers?.Information("IPv6 mDNS not available: " + ex.Message);
				this.socket6 = null;
			}

			if (this.socket4 is null && this.socket6 is null)
				throw new MatterException("Unable to open mDNS sockets.");

			if (!(this.socket4 is null))
				this.Receive(this.socket4);

			if (!(this.socket6 is null))
				this.Receive(this.socket6);
		}

		private Socket OpenSocket(AddressFamily Family, out bool UnicastFallback)
		{
			Socket s = new Socket(Family, SocketType.Dgram, ProtocolType.Udp);
			bool V6 = Family == AddressFamily.InterNetworkV6;

			UnicastFallback = false;

			try
			{
				try
				{
					s.ExclusiveAddressUse = false;
				}
				catch (Exception)
				{
					// Not supported on all platforms.
				}

				s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				if (V6)
					s.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, true);

				try
				{
					s.Bind(new IPEndPoint(V6 ? IPAddress.IPv6Any : IPAddress.Any, MdnsPort));
				}
				catch (SocketException)
				{
					s.Bind(new IPEndPoint(V6 ? IPAddress.IPv6Any : IPAddress.Any, 0));
					UnicastFallback = true;
				}

				if (V6)
				{
					s.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true);
					s.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, 255);
					s.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback, true);
				}
				else
				{
					s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
					s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
					s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
				}

				int NrJoined = 0;

				foreach (KeyValuePair<NetworkInterface, IPAddress> P in GetInterfaces(Family))
				{
					try
					{
						if (V6)
						{
							int Index = P.Key.GetIPProperties().GetIPv6Properties().Index;
							s.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(MulticastIPv6, Index));
						}
						else
							s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastIPv4, P.Value));

						NrJoined++;
					}
					catch (Exception)
					{
						// Interface may not support multicast, or membership already joined.
					}
				}

				if (NrJoined == 0)
				{
					if (V6)
						s.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(MulticastIPv6));
					else
						s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastIPv4));
				}

				return s;
			}
			catch (Exception)
			{
				s.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Gets operational, multicast-capable network interfaces and their addresses, for a given address family.
		/// </summary>
		/// <param name="Family">Address family.</param>
		/// <returns>Interfaces and an address of each.</returns>
		public static IEnumerable<KeyValuePair<NetworkInterface, IPAddress>> GetInterfaces(AddressFamily Family)
		{
			NetworkInterface[] Interfaces;

			try
			{
				Interfaces = NetworkInterface.GetAllNetworkInterfaces();
			}
			catch (Exception)
			{
				yield break;
			}

			foreach (NetworkInterface Interface in Interfaces)
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				bool Multicast;

				try
				{
					Multicast = Interface.SupportsMulticast;
				}
				catch (Exception)
				{
					Multicast = true;
				}

				if (!Multicast)
					continue;

				IPInterfaceProperties Properties;

				try
				{
					Properties = Interface.GetIPProperties();
				}
				catch (Exception)
				{
					continue;
				}

				foreach (UnicastIPAddressInformation Info in Properties.UnicastAddresses)
				{
					if (Info.Address.AddressFamily == Family)
					{
						yield return new KeyValuePair<NetworkInterface, IPAddress>(Interface, Info.Address);
						break;
					}
				}
			}
		}

		private async void Receive(Socket Socket)
		{
			byte[] Buffer = new byte[9000];
			bool V6 = Socket.AddressFamily == AddressFamily.InterNetworkV6;
			EndPoint Any = new IPEndPoint(V6 ? IPAddress.IPv6Any : IPAddress.Any, 0);

			try
			{
				while (!this.disposed)
				{
					SocketReceiveMessageFromResult Result;

					try
					{
						Result = await Socket.ReceiveMessageFromAsync(new ArraySegment<byte>(Buffer), SocketFlags.None, Any);
					}
					catch (ObjectDisposedException)
					{
						break;
					}
					catch (SocketException)
					{
						if (this.disposed)
							break;

						continue;
					}

					if (Result.ReceivedBytes <= 0)
						continue;

					byte[] Data = new byte[Result.ReceivedBytes];
					Array.Copy(Buffer, 0, Data, 0, Data.Length);

					try
					{
						await this.Process(Data, (IPEndPoint)Result.RemoteEndPoint, Result.PacketInformation.Interface);
					}
					catch (Exception ex)
					{
						this.sniffers?.Error("Invalid mDNS message: " + ex.Message);
					}
				}
			}
			catch (Exception ex)
			{
				if (!this.disposed)
					Log.Exception(ex);
			}
		}

		private async Task Process(byte[] Data, IPEndPoint From, int InterfaceIndex)
		{
			DnsMessage Message = DnsMessage.Decode(Data);

			if (!Message.IsResponse)
				return;

			List<DnsSdInstance> Updated = null;
			DateTime Now = DateTime.UtcNow;

			lock (this.instances)
			{
				foreach (DnsRecord R in Message.AllRecords)
				{
					switch (R.Type)
					{
						case DnsType.PTR:
							if (!this.ptr.TryGetValue(R.Name, out List<string> Targets))
							{
								Targets = new List<string>();
								this.ptr[R.Name] = Targets;
							}

							if (R.Ttl == 0)
							{
								Targets.Remove(R.Target);
								this.instances.Remove(R.Target);
							}
							else if (!Targets.Contains(R.Target))
								Targets.Add(R.Target);
							break;

						case DnsType.SRV:
							DnsSdInstance Instance = this.GetInstanceLocked(R.Name);
							if (R.Ttl == 0)
								break;

							Instance.HostName = R.Target;
							Instance.Port = R.Port;
							Instance.Updated = Now;
							AddUpdated(ref Updated, Instance);
							break;

						case DnsType.TXT:
							Instance = this.GetInstanceLocked(R.Name);
							Instance.SetTxt(R.Text ?? Array.Empty<string>());
							Instance.Updated = Now;
							AddUpdated(ref Updated, Instance);
							break;

						case DnsType.A:
						case DnsType.AAAA:
							if (R.Address is null || R.Ttl == 0)
								break;

							IPAddress Address = R.Address;

							if (Address.IsIPv6LinkLocal && InterfaceIndex > 0)
								Address = new IPAddress(Address.GetAddressBytes(), InterfaceIndex);

							if (!this.hostAddresses.TryGetValue(R.Name, out Dictionary<string, KeyValuePair<IPAddress, DateTime>> Addresses))
							{
								Addresses = new Dictionary<string, KeyValuePair<IPAddress, DateTime>>();
								this.hostAddresses[R.Name] = Addresses;
							}

							Addresses[Address.ToString()] = new KeyValuePair<IPAddress, DateTime>(Address, Now.AddSeconds(R.Ttl));
							break;
					}
				}

				foreach (DnsSdInstance Instance in this.instances.Values)
				{
					if (!string.IsNullOrEmpty(Instance.HostName) &&
						this.hostAddresses.TryGetValue(Instance.HostName, out Dictionary<string, KeyValuePair<IPAddress, DateTime>> Addresses))
					{
						int Before = Instance.Addresses.Count;

						foreach (KeyValuePair<IPAddress, DateTime> P in Addresses.Values)
						{
							if (P.Value >= Now)
								Instance.AddAddress(P.Key);
						}

						if (Instance.Addresses.Count != Before)
							AddUpdated(ref Updated, Instance);
					}
				}
			}

			if (!(Updated is null))
			{
				foreach (DnsSdInstance Instance in Updated)
					await this.OnInstanceUpdated.Raise(this, Instance);
			}
		}

		private static void AddUpdated(ref List<DnsSdInstance> Updated, DnsSdInstance Instance)
		{
			Updated ??= new List<DnsSdInstance>();

			if (!Updated.Contains(Instance))
				Updated.Add(Instance);
		}

		private DnsSdInstance GetInstanceLocked(string FullName)
		{
			if (!this.instances.TryGetValue(FullName, out DnsSdInstance Instance))
			{
				Instance = new DnsSdInstance(FullName);
				this.instances[FullName] = Instance;
			}

			return Instance;
		}

		/// <summary>
		/// Sends a query.
		/// </summary>
		/// <param name="Questions">Questions.</param>
		public async Task QueryAsync(params DnsQuestion[] Questions)
		{
			DnsMessage Message = new DnsMessage()
			{
				Id = 0,
				IsResponse = false
			};

			Message.Questions.AddRange(Questions);
			byte[] Data = Message.Encode();

			if (!(this.socket4 is null))
				await this.SendMulticast(this.socket4, Data, false);

			if (!(this.socket6 is null))
				await this.SendMulticast(this.socket6, Data, true);
		}

		private async Task SendMulticast(Socket Socket, byte[] Data, bool V6)
		{
			IPEndPoint Destination = new IPEndPoint(V6 ? MulticastIPv6 : MulticastIPv4, MdnsPort);
			bool Sent = false;

			foreach (KeyValuePair<NetworkInterface, IPAddress> P in GetInterfaces(V6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork))
			{
				try
				{
					lock (Socket)
					{
						if (V6)
						{
							int Index = P.Key.GetIPProperties().GetIPv6Properties().Index;
							Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastInterface, Index);
						}
						else
						{
							Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface,
								BitConverter.ToInt32(P.Value.GetAddressBytes(), 0));
						}
					}

					await Socket.SendToAsync(new ArraySegment<byte>(Data), SocketFlags.None, Destination);
					Sent = true;
				}
				catch (Exception)
				{
					// Ignore interface errors.
				}
			}

			if (!Sent)
			{
				try
				{
					await Socket.SendToAsync(new ArraySegment<byte>(Data), SocketFlags.None, Destination);
				}
				catch (Exception ex)
				{
					this.sniffers?.Warning("Unable to send mDNS query: " + ex.Message);
				}
			}
		}

		/// <summary>
		/// Browses for instances of a service type.
		/// </summary>
		/// <param name="ServiceType">Service type, or subtype (e.g. "_L3840._sub._matterc._udp.local").</param>
		/// <param name="Timeout">Time to wait for responses.</param>
		/// <param name="StopWhen">Optional condition: browsing stops when a resolved instance matches.</param>
		/// <returns>Resolved instances.</returns>
		public async Task<DnsSdInstance[]> BrowseAsync(string ServiceType, TimeSpan Timeout, Func<DnsSdInstance, bool> StopWhen = null)
		{
			DateTime Limit = DateTime.UtcNow + Timeout;
			bool Unicast = this.unicastFallback4 || this.unicastFallback6;
			int Round = 0;

			while (true)
			{
				if (Round == 0 || Round == 2 || Round == 6 || Round == 14)
					await this.QueryAsync(new DnsQuestion(ServiceType, DnsType.PTR, Unicast));

				DnsSdInstance[] Found = this.GetInstances(ServiceType);
				List<DnsQuestion> Missing = new List<DnsQuestion>();

				foreach (DnsSdInstance Instance in Found)
				{
					if (!(StopWhen is null) && Instance.IsResolved && StopWhen(Instance))
						return new DnsSdInstance[] { Instance };

					if (string.IsNullOrEmpty(Instance.HostName))
					{
						Missing.Add(new DnsQuestion(Instance.FullName, DnsType.SRV, Unicast));
						Missing.Add(new DnsQuestion(Instance.FullName, DnsType.TXT, Unicast));
					}
					else if (Instance.Addresses.Count == 0)
					{
						Missing.Add(new DnsQuestion(Instance.HostName, DnsType.AAAA, Unicast));
						Missing.Add(new DnsQuestion(Instance.HostName, DnsType.A, Unicast));
					}
				}

				if (Missing.Count > 0 && (Round % 2) == 1)
					await this.QueryAsync(Missing.ToArray());

				if (DateTime.UtcNow >= Limit)
				{
					List<DnsSdInstance> Result = new List<DnsSdInstance>();

					foreach (DnsSdInstance Instance in Found)
					{
						if (Instance.IsResolved)
							Result.Add(Instance);
					}

					return Result.ToArray();
				}

				await Task.Delay(100);
				Round++;
			}
		}

		/// <summary>
		/// Resolves a specific service instance.
		/// </summary>
		/// <param name="FullName">Full instance name (instance.service.domain).</param>
		/// <param name="Timeout">Time to wait for responses.</param>
		/// <returns>Resolved instance, or null if not found.</returns>
		public async Task<DnsSdInstance> ResolveAsync(string FullName, TimeSpan Timeout)
		{
			DateTime Limit = DateTime.UtcNow + Timeout;
			bool Unicast = this.unicastFallback4 || this.unicastFallback6;
			int Round = 0;

			while (true)
			{
				DnsSdInstance Instance;

				lock (this.instances)
				{
					if (!this.instances.TryGetValue(FullName, out Instance))
						Instance = null;
				}

				if (!(Instance is null) && Instance.IsResolved)
					return Instance;

				if (Round == 0 || Round == 3 || Round == 8 || Round == 18)
				{
					List<DnsQuestion> Questions = new List<DnsQuestion>()
					{
						new DnsQuestion(FullName, DnsType.SRV, Unicast),
						new DnsQuestion(FullName, DnsType.TXT, Unicast)
					};

					if (!string.IsNullOrEmpty(Instance?.HostName))
					{
						Questions.Add(new DnsQuestion(Instance.HostName, DnsType.AAAA, Unicast));
						Questions.Add(new DnsQuestion(Instance.HostName, DnsType.A, Unicast));
					}

					await this.QueryAsync(Questions.ToArray());
				}
				else if (!(Instance is null) && !string.IsNullOrEmpty(Instance.HostName) && Instance.Addresses.Count == 0 && (Round % 3) == 1)
				{
					await this.QueryAsync(
						new DnsQuestion(Instance.HostName, DnsType.AAAA, Unicast),
						new DnsQuestion(Instance.HostName, DnsType.A, Unicast));
				}

				if (DateTime.UtcNow >= Limit)
					return null;

				await Task.Delay(100);
				Round++;
			}
		}

		/// <summary>
		/// Gets cached instances of a service type.
		/// </summary>
		/// <param name="ServiceType">Service type.</param>
		/// <returns>Instances.</returns>
		public DnsSdInstance[] GetInstances(string ServiceType)
		{
			List<DnsSdInstance> Result = new List<DnsSdInstance>();

			lock (this.instances)
			{
				if (this.ptr.TryGetValue(ServiceType, out List<string> Targets))
				{
					foreach (string Target in Targets)
						Result.Add(this.GetInstanceLocked(Target));
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Gets a cached instance.
		/// </summary>
		/// <param name="FullName">Full instance name.</param>
		/// <returns>Instance, or null if not found.</returns>
		public DnsSdInstance GetInstance(string FullName)
		{
			lock (this.instances)
			{
				return this.instances.TryGetValue(FullName, out DnsSdInstance Instance) ? Instance : null;
			}
		}

		/// <summary>
		/// Removes a cached instance (forcing a new resolution).
		/// </summary>
		/// <param name="FullName">Full instance name.</param>
		public void Forget(string FullName)
		{
			lock (this.instances)
			{
				if (this.instances.TryGetValue(FullName, out DnsSdInstance Instance))
				{
					this.instances.Remove(FullName);

					if (!string.IsNullOrEmpty(Instance.HostName))
						this.hostAddresses.Remove(Instance.HostName);
				}
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.disposed = true;

			this.socket4?.Dispose();
			this.socket4 = null;

			this.socket6?.Dispose();
			this.socket6 = null;
		}
	}
}
