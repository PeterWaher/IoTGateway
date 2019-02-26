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
		private static Dictionary<Type, IProperty> propertyTypes = null;
		private static Dictionary<Type, ObjectInfo> objectInfo = new Dictionary<Type, ObjectInfo>();
		private static ObjectInfo rootObject;

		private readonly LinkedList<ClusterUdpClient> clients = new LinkedList<ClusterUdpClient>();
		internal readonly IPEndPoint destination;
		internal readonly Aes aes;
		internal readonly byte[] key;

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

			if (propertyTypes is null)
			{
				Init();
				rootObject = GetObjectInfo(typeof(object));
			}

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

		internal void DataReceived(byte[] Data, IPEndPoint From)
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
		/// Serializes an object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Binary representation</returns>
		public byte[] Serialize(object Object)
		{
			using (Serializer Output = new Serializer())
			{
				rootObject.Serialize(Output, Object);
				return Output.ToArray();
			}
		}

		/// <summary>
		/// Deserializes an object.
		/// </summary>
		/// <param name="Data">Binary representation of object.</param>
		/// <returns>Deserialized object</returns>
		/// <exception cref="KeyNotFoundException">If the corresponding type, or any of the embedded properties, could not be found.</exception>
		public object Deserialize(byte[] Data)
		{
			using (Deserializer Input = new Deserializer(Data))
			{
				return rootObject.Deserialize(Input, typeof(object));
			}
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
				Output.WriteByte(0);    // Unacknowledged message.
				rootObject.Serialize(Output, Message);
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

		internal static ObjectInfo GetObjectInfo(Type T)
		{
			lock (objectInfo)
			{
				if (objectInfo.TryGetValue(T, out ObjectInfo Result))
					return Result;
			}

			List<PropertyReference> Properties = new List<PropertyReference>();
			TypeInfo TI = T.GetTypeInfo();

			foreach (PropertyInfo PI in T.GetRuntimeProperties())
			{
				if (PI.GetMethod is null || PI.SetMethod is null)
					continue;

				Type PT = PI.PropertyType;
				IProperty Property = GetProperty(PT);

				if (Property is null)
					continue;

				Properties.Add(new PropertyReference()
				{
					Name = PI.Name,
					Info = PI,
					Property = Property
				});
			}

			return new ObjectInfo()
			{
				Type = T,
				Properties = Properties.ToArray()
			};
		}

		internal static IProperty GetProperty(Type PT)
		{
			if (PT.IsArray)
			{
				Type ElementType = PT.GetElementType();
				return new ArrayProperty(PT, ElementType, GetProperty(ElementType));
			}

			TypeInfo PTI = PT.GetTypeInfo();

			if (PTI.IsEnum)
				return new EnumProperty(PT);
			else if (PTI.IsGenericType && PT.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				Type ElementType = PT.GenericTypeArguments[0];
				IProperty Element = GetProperty(ElementType);

				return new NullableProperty(PT, ElementType, Element);
			}
			else if (propertyTypes.TryGetValue(PT, out IProperty Property))
				return Property;
			else if (!PTI.IsValueType)
				return new ObjectProperty(PT, GetObjectInfo(PT));
			else
				return null;
		}

		private static void Init()
		{
			Dictionary<Type, IProperty> PropertyTypes = new Dictionary<Type, IProperty>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IProperty)))
			{
				TypeInfo TI = T.GetTypeInfo();
				if (TI.IsAbstract)
					continue;

				ConstructorInfo DefaultConstructor = null;

				foreach (ConstructorInfo CI in TI.DeclaredConstructors)
				{
					if (CI.GetParameters().Length == 0)
					{
						DefaultConstructor = CI;
						break;
					}
				}

				if (DefaultConstructor is null)
					continue;

				try
				{
					IProperty Property = (IProperty)DefaultConstructor.Invoke(Types.NoParameters);
					Type PT = Property.PropertyType;
					if (PT is null)
						continue;

					if (PropertyTypes.ContainsKey(PT))
					{
						Log.Error("Multiple classes available for property type " + PT.FullName + ".");
						continue;
					}

					PropertyTypes[PT] = Property;
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
