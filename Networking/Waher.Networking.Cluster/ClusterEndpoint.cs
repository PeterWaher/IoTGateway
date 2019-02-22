using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Waher.Events;
using Waher.Networking.Cluster.Serialization;
using Waher.Networking.Cluster.Serialization.Properties;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
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
		/// <param name="AllowLoopback">If communication over the loopback interface should be permitted. (Default=false)</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public ClusterEndpoint(IPAddress MulticastAddress, int Port, byte[] SharedSecret,
			params ISniffer[] Sniffers)
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
#else
			foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				switch (Interface.NetworkInterfaceType)
				{
					case NetworkInterfaceType.Loopback:
						continue;
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

					if (!IsLoopback)
					{
						try
						{
							Client = new UdpClient(AddressFamily)
							{
								DontFragment = true,
								ExclusiveAddressUse = true,
								MulticastLoopback = false,
								EnableBroadcast = true,
								Ttl = 30
							};

							Client.Client.Bind(new IPEndPoint(Address, 0));

							try
							{
								Client.JoinMulticastGroup(MulticastAddress);
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
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

						ClusterUdpClient ClusterUdpClient = new ClusterUdpClient(this, Client, Address);
						ClusterUdpClient.BeginReceive();

						this.clients.AddLast(ClusterUdpClient);
					}
				}
			}
		}

		/// <summary>
		/// IP endpoints listening on.
		/// </summary>
		public IEnumerable<IPEndPoint> Endpoints
		{
			get
			{
				foreach (ClusterUdpClient Client in this.clients)
					yield return Client.EndPoint;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
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
			private readonly IPAddress localAddress;
			private readonly byte[] ivTx = new byte[16];
			private readonly byte[] ivRx = new byte[16];
			private HMACSHA1 hmac;
			private UdpClient client;
			private bool isWriting = false;
			private bool disposed = false;

			public ClusterUdpClient(ClusterEndpoint Endpoint, UdpClient Client, IPAddress LocalAddress)
			{
				this.endpoint = Endpoint;
				this.client = Client;
				this.localAddress = LocalAddress;

				byte[] A = LocalAddress.GetAddressBytes();
				Array.Copy(A, 0, this.ivTx, 8, 4);

				this.hmac = new HMACSHA1(A);
			}

			public IPAddress Address => this.localAddress;
			public IPEndPoint EndPoint => this.client.Client.LocalEndPoint as IPEndPoint;

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
							byte[] Datagram = Data.Buffer;
							int i, c = Datagram.Length - 12;

							if (c <= 0 || (c & 15) != 0)
								continue;

							Array.Copy(Datagram, 0, this.ivRx, 0, 8);
							Array.Copy(Datagram, 8, this.ivRx, 12, 4);

							byte[] A = Data.RemoteEndPoint.Address.GetAddressBytes();
							Array.Copy(A, 0, this.ivRx, 8, 4);

							int FragmentNr = this.ivRx[13];
							bool LastFragment = (FragmentNr & 0x80) != 0;
							FragmentNr &= 0x7f;
							FragmentNr <<= 8;
							FragmentNr |= this.ivRx[12];

							int Padding = this.ivRx[14] >> 4;

							// TODO: Check DateTime
							// TODO: Fragments

							using (ICryptoTransform Decryptor = this.endpoint.aes.CreateDecryptor(this.endpoint.key, this.ivRx))
							{
								byte[] Decrypted = Decryptor.TransformFinalBlock(Datagram, 12, Datagram.Length - 12);

								using (HMACSHA1 HMAC = new HMACSHA1(A))
								{
									c = Decrypted.Length - 16 - Padding;

									byte[] MAC = HMAC.ComputeHash(Decrypted, 20, c);

									for (i = 0; i < 20; i++)
									{
										if (MAC[i] != Decrypted[i])
											break;
									}

									if (i < 20)
										continue;

									byte[] Received = new byte[c];
									Array.Copy(Decrypted, 20, Received, 0, c);

									this.endpoint.DataReceived(Received, Data.RemoteEndPoint);
								}
							}
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
						Array.Copy(TP, 0, this.ivTx, 0, 8);

						for (FragmentNr = 0; FragmentNr < NrFragments; FragmentNr++, Pos += 32768)
						{
							int FragmentSize = Math.Min(32768, Len - (FragmentNr << 15));
							int Padding = (-FragmentSize) & 15;
							byte[] Datagram = new byte[28 + FragmentSize + Padding];

							this.ivTx[12] = (byte)FragmentNr;
							this.ivTx[13] = (byte)(FragmentNr >> 8);

							if (FragmentNr == NrFragments - 1)
								this.ivTx[13] |= 0x80;

							this.ivTx[14] = (byte)((this.ivTx[14] & 0x0f) | (Padding << 4));

							Array.Copy(this.ivTx, 0, Datagram, 0, 8);
							Array.Copy(this.ivTx, 12, Datagram, 8, 4);

							byte[] MAC = this.hmac.ComputeHash(Message, Pos, FragmentSize);

							Array.Copy(MAC, 0, Datagram, 12, 20);
							Array.Copy(Message, Pos, Datagram, 32, FragmentSize);

							using (ICryptoTransform Encryptor = this.endpoint.aes.CreateEncryptor(this.endpoint.key, this.ivTx))
							{
								byte[] Encrypted = Encryptor.TransformFinalBlock(Datagram, 12, Datagram.Length - 12);
								Array.Copy(Encrypted, 0, Datagram, 12, Encrypted.Length);
							}

							if (++this.ivTx[15] == 0)
								++this.ivTx[14];

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

			this.OnDataReceived?.Invoke(this, new EventArgs());
		}

		public event EventHandler OnDataReceived = null;

		private void Transmit(byte[] Message)
		{
			this.TransmitBinary(Message);

			foreach (ClusterUdpClient Client in this.clients)
				Client.BeginTransmit(Message);
		}

		/// <summary>
		/// Sends an unacknowledged message
		/// </summary>
		/// <param name="Message">Message object</param>
		public void SendMessageUnacknowledged(IClusterMessage Message)
		{
			Serializer Output = new Serializer();

			try
			{
				ObjectInfo Info = GetObjectInfo(Message.GetType());

				Output.WriteByte(0);    // Unacknowledged message.
				Info.Serialize(Output, Message);
				this.Transmit(Output.ToArray());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				Output.Dispose();
			}
		}

		private static ObjectInfo GetObjectInfo(Type T)
		{
			lock (objectInfo)
			{
				if (objectInfo.TryGetValue(T, out ObjectInfo Result))
					return Result;
			}

			if (propertyTypes is null)
				Init();

			List<IProperty> Properties = null;
			TypeInfo TI = T.GetTypeInfo();

			foreach (PropertyInfo PI in TI.DeclaredProperties)
			{
				if (PI.GetMethod is null || PI.SetMethod is null)
					continue;

				if (!propertyTypes.TryGetValue(PI.PropertyType, out IProperty Property))
					continue;

				if (Properties is null)
					Properties = new List<IProperty>();

				Properties.Add(Property);
			}

			return new ObjectInfo()
			{
				Name = T.FullName,
				Properties = Properties?.ToArray()
			};
		}

		private class ObjectInfo
		{
			public IProperty[] Properties;
			public string Name;

			public void Serialize(Serializer Output, object Object)
			{
				Output.WriteName(this.Name);

				if (!(this.Properties is null))
				{
					foreach (IProperty Property in this.Properties)
					{
						Output.WriteName(Property.Name);
						Property.Serialize(Output, Object);
					}
				}
			}
		}

		private static Dictionary<Type, IProperty> propertyTypes = null;
		private static Dictionary<Type, ObjectInfo> objectInfo = new Dictionary<Type, ObjectInfo>();

		public static void Init()
		{
			Dictionary<Type, IProperty> PropertyTypes = new Dictionary<Type, IProperty>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IProperty)))
			{
				TypeInfo TI = T.GetTypeInfo();
				if (TI.IsAbstract)
					continue;

				try
				{
					IProperty Property = (IProperty)Activator.CreateInstance(T, Types.NoParameters);
					PropertyTypes[T] = Property;
				}
				catch (Exception)
				{
					continue;
				}
			}

			if (propertyTypes is null)
				Types.OnInvalidated += (sender, e) => Init();

			propertyTypes = PropertyTypes;
		}

	}
}
