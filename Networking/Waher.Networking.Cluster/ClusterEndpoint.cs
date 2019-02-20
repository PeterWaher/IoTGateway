using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Security;
#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Connectivity;
#endif

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Represents one endpoint (or participant) in the network cluster.
	/// </summary>
	public class ClusterEndpoint : Sniffable, IDisposable
	{
		private readonly LinkedList<ClusterUdpClient> clients = new LinkedList<ClusterUdpClient>();
		private readonly IPEndPoint destination;
		private readonly Aes aes;
		private readonly byte[] key;
		private bool disposed = false;

		/// <summary>
		/// Represents one endpoint (or participant) in the network cluster.
		/// </summary>
		/// <param name="MulticastAddress">UDP Multicast address used for cluster communication.</param>
		/// <param name="Port">Port used in cluster communication</param>
		/// <param name="SharedSecret">Shared secret. Used to encrypt and decrypt communication. Secret is UTF-8 encoded and then hashed before being fed to AES.</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public ClusterEndpoint(IPAddress MulticastAddress, int Port, string SharedSecret,
			params ISniffer[] Sniffers)
			: this(MulticastAddress, Port, Encoding.UTF8.GetBytes(SharedSecret), Sniffers)
		{
		}

		/// <summary>
		/// Represents one endpoint (or participant) in the network cluster.
		/// </summary>
		/// <param name="MulticastAddress">UDP Multicast address used for cluster communication.</param>
		/// <param name="Port">Port used in cluster communication</param>
		/// <param name="SharedSecret">Shared secret. Used to encrypt and decrypt communication. Secret is hashed before being fed to AES.</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public ClusterEndpoint(IPAddress MulticastAddress, int Port, byte[] SharedSecret,
			params ISniffer[] Sniffers)
			: this(MulticastAddress, Port, SharedSecret, false, Sniffers)
		{
		}

		/// <summary>
		/// Represents one endpoint (or participant) in the network cluster.
		/// </summary>
		/// <param name="MulticastAddress">UDP Multicast address used for cluster communication.</param>
		/// <param name="Port">Port used in cluster communication</param>
		/// <param name="SharedSecret">Shared secret. Used to encrypt and decrypt communication. Secret is hashed before being fed to AES.</param>
		/// <param name="AllowLoopback">If communication over the loopback interface should be permitted. (Default=false)</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public ClusterEndpoint(IPAddress MulticastAddress, int Port, byte[] SharedSecret,
			bool AllowLoopback, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			UdpClient Client;

			if (MulticastAddress.AddressFamily != AddressFamily.InterNetwork)
				throw new ArgumentException("Cluster communication must be done using IPv4.", nameof(MulticastAddress));

			this.aes = Aes.Create();
			this.aes.BlockSize = 128;
			this.aes.KeySize = 256;
			this.aes.Mode = CipherMode.CBC;
			this.aes.Padding = PaddingMode.None;

			this.key = Hashes.ComputeSHA256Hash(SharedSecret);
			this.destination = new IPEndPoint(MulticastAddress, Port);

#if WINDOWS_UWP
			foreach (HostName HostName in NetworkInformation.GetHostNames())
			{
				if (HostName.IPInformation is null)
					continue;

				foreach (ConnectionProfile Profile in NetworkInformation.GetConnectionProfiles())
				{
					if (Profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None)
						continue;

					if (Profile.NetworkAdapter.NetworkAdapterId != HostName.IPInformation.NetworkAdapter.NetworkAdapterId)
						continue;

					if (!IPAddress.TryParse(HostName.CanonicalName, out IPAddress Address))
						continue;

					AddressFamily AddressFamily = Address.AddressFamily;
					bool IsLoopback = IPAddress.IsLoopback(Address);
					IPAddress MulticastAddress;
#else
			foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				switch (Interface.NetworkInterfaceType)
				{
					case NetworkInterfaceType.Loopback:
						if (!AllowLoopback)
							continue;
						break;
				}

				IPInterfaceProperties Properties = Interface.GetIPProperties();

				foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
				{
					IPAddress Address = UnicastAddress.Address;
					AddressFamily AddressFamily = Address.AddressFamily;
					bool IsLoopback = Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback;
#endif
					if (Address.AddressFamily != MulticastAddress.AddressFamily)
						continue;

					if (!IsLoopback || AllowLoopback)
					{
						try
						{
							Client = new UdpClient(AddressFamily)
							{
								DontFragment = true,
								MulticastLoopback = AllowLoopback
							};
						}
						catch (NotSupportedException)
						{
							continue;
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
							continue;
						}

						Client.EnableBroadcast = true;
						Client.Ttl = 30;
						Client.Client.Bind(new IPEndPoint(Address, Port));
						Client.JoinMulticastGroup(MulticastAddress);

						ClusterUdpClient ClusterUdpClient = new ClusterUdpClient(this, Client);
						ClusterUdpClient.BeginReceive();

						this.clients.AddLast(ClusterUdpClient);
					}
				}
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;

			foreach (ClusterUdpClient Client in this.clients)
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

			this.clients.Clear();

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

		private class ClusterUdpClient : IDisposable
		{
			private readonly ClusterEndpoint endpoint;
			private readonly LinkedList<byte[]> outputQueue = new LinkedList<byte[]>();
			private readonly byte[] iv = new byte[16];
			private HMACSHA1 hmac;
			private UdpClient client;
			private bool isWriting = false;
			private bool disposed = false;

			public ClusterUdpClient(ClusterEndpoint Endpoint, UdpClient Client)
			{
				this.endpoint = Endpoint;
				this.client = Client;

				byte[] A = ((IPEndPoint)Client.Client.LocalEndPoint).Address.GetAddressBytes();
				Array.Copy(A, 0, this.iv, 8, 4);

				this.hmac = new HMACSHA1(A);
			}

			public void Dispose()
			{
				this.disposed = true;

				this.client?.Dispose();
				this.client = null;

				this.hmac?.Dispose();
				this.hmac = null;
			}

			public async void BeginReceive()
			{
				try
				{
					while (!this.disposed)
					{
						UdpReceiveResult Data = await client.ReceiveAsync();
						if (this.disposed)
							return;

						try
						{
							this.endpoint.DataReceived(Data.Buffer, Data.RemoteEndPoint);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}
				catch (ObjectDisposedException)
				{
					// Closed.
				}
				catch (Exception ex)
				{
					this.endpoint.Error(ex.Message);
				}
			}

			public async void BeginTransmit(byte[] Message)
			{
				if (this.disposed)
					return;

				lock (this.outputQueue)
				{
					if (this.isWriting)
					{
						this.outputQueue.AddLast(Message);
						return;
					}
					else
						this.isWriting = true;
				}

				try
				{
					while (Message != null)
					{
						int Len = Message.Length;
						int NrFragments = (Len + 32767) >> 15;
						int FragmentNr;
						int Pos = 0;

						if (NrFragments == 0)
							return;

						if (NrFragments >= 32768)
							throw new ArgumentOutOfRangeException("Message too big.", nameof(Message));

						byte[] TP = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
						Array.Copy(TP, 0, this.iv, 0, 8);

						for (FragmentNr = 0; FragmentNr < NrFragments; FragmentNr++, Pos += 32768)
						{
							int FragmentSize = Math.Min(32768, Len - (FragmentNr << 15));
							int Padding = (16 - FragmentSize) & 15;
							byte[] Datagram = new byte[28 + FragmentSize + Padding];

							this.iv[12] = (byte)FragmentNr;
							this.iv[13] = (byte)(FragmentNr >> 8);

							if (FragmentNr == NrFragments - 1)
								this.iv[13] |= 0x80;

							this.iv[14] = (byte)((this.iv[14] & 0x0f) | (Padding << 4));

							Array.Copy(this.iv, 0, Datagram, 0, 8);
							Array.Copy(this.iv, 12, Datagram, 8, 4);

							byte[] MAC = this.hmac.ComputeHash(Message, Pos, FragmentSize);

							Array.Copy(MAC, 0, Datagram, 12, 16);
							Array.Copy(Message, Pos, Datagram, 28, FragmentSize);

							using (ICryptoTransform Encryptor = this.endpoint.aes.CreateEncryptor(this.endpoint.key, this.iv))
							{
								byte[] Encrypted = Encryptor.TransformFinalBlock(Datagram, 16, FragmentSize + 16);
								Array.Copy(Encrypted, 0, Datagram, 16, Encrypted.Length);
							}

							if (++this.iv[15] == 0)
								++this.iv[14];

							await this.client.SendAsync(Datagram, Datagram.Length, this.endpoint.destination);

							if (this.disposed)
								return;
						}

						lock (this.outputQueue)
						{
							if (this.outputQueue.First is null)
							{
								this.isWriting = false;
								Message = null;
							}
							else
							{
								Message = this.outputQueue.First.Value;
								this.outputQueue.RemoveFirst();
							}
						}
					}
				}
				catch (Exception ex)
				{
					this.endpoint.Error(ex.Message);
				}
			}
		}

		private void DataReceived(byte[] Data, IPEndPoint From)
		{
			this.ReceiveBinary(Data);
		}

		private void Transmit(byte[] Message)
		{
			this.TransmitBinary(Message);

			foreach (ClusterUdpClient Client in this.clients)
				Client.BeginTransmit(Message);
		}

	}
}
