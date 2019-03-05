using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Cluster.Messages;
using Waher.Networking.Cluster.Serialization;
using Waher.Networking.Cluster.Serialization.Properties;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;
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
		private readonly IPEndPoint destination;
		internal readonly Aes aes;
		internal readonly byte[] key;
		internal Cache<string, object> currentStatus;
		private Scheduler scheduler;
		private readonly Dictionary<IPEndPoint, object> remoteStatus;
		private object localStatus;
		private Timer aliveTimer;
		internal bool shuttingDown = false;
		private readonly bool internalScheduler;

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

			this.localStatus = null;
			this.remoteStatus = new Dictionary<IPEndPoint, object>();
			this.currentStatus = new Cache<string, object>(int.MaxValue, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
			this.currentStatus.Removed += CurrentStatus_Removed;

			if (Types.TryGetModuleParameter("Scheduler", out object Obj) &&
				Obj is Scheduler Scheduler)
			{
				this.scheduler = Scheduler;
				this.internalScheduler = false;
			}
			else
			{
				this.scheduler = new Scheduler();
				this.internalScheduler = true;
			}

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

			this.aliveTimer = new Timer(this.SendAlive, null, 0, 5000);
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
			if (!this.shuttingDown)
			{
				this.shuttingDown = true;
				this.SendMessageUnacknowledged(new ShuttingDown());
			}
		}

		internal void Dispose2()
		{
			if (this.internalScheduler)
				this.scheduler?.Dispose();

			this.scheduler = null;

			this.aliveTimer?.Dispose();
			this.aliveTimer = null;

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

			this.currentStatus?.Dispose();
			this.currentStatus = null;

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

		internal async void DataReceived(byte[] Data, IPEndPoint From)
		{
			try
			{
				if (Data.Length == 0)
					return;

				this.ReceiveBinary(Data);

				using (Deserializer Input = new Deserializer(Data))
				{
					byte Command = Input.ReadByte();

					switch (Command)
					{
						case 0: // Unacknowledged message
							object Object = rootObject.Deserialize(Input, typeof(object));
							if (Object is IClusterMessage Message)
							{
								await Message.MessageReceived(this, From);
								this.OnMessageReceived?.Invoke(this, new ClusterMessageEventArgs(Message));
							}
							else
								this.Error("Non-message object received in message: " + Object?.GetType()?.FullName);
							break;

						case 1: // Acknowledged message
							Guid Id = Input.ReadGuid();
							string s = Id.ToString();
							ulong Len = Input.ReadVarUInt64();
							bool Skip = false;

							while (Len > 0)
							{
								string Address = new IPAddress(Input.ReadBinary()).ToString();
								int Port = Input.ReadUInt16();

								foreach (ClusterUdpClient Client in this.clients)
								{
									if (Client.IsEndpoint(Address, Port))
									{
										Skip = true;
										break;
									}
								}

								if (Skip)
									break;

								Len--;
							}

							if (Skip)
								break;

							if (!this.currentStatus.TryGetValue(s, out object Obj) || 
								!(Obj is bool Ack))
							{
								Object = rootObject.Deserialize(Input, typeof(object));
								if (!((Message = Object as IClusterMessage) is null))
								{
									Ack = await Message.MessageReceived(this, From);
									this.OnMessageReceived?.Invoke(this, new ClusterMessageEventArgs(Message));
								}
								else
								{
									this.Error("Non-message object received in message: " + Object?.GetType()?.FullName);
									Ack = false;
								}
							}

							using (Serializer Output = new Serializer())
							{
								Output.WriteByte(Ack ? (byte)2 : (byte)3);
								Output.WriteGuid(Id);

								this.Transmit(Output.ToArray(), From);
							}
							break;

						case 2: // ACK
						case 3: // NACK
							Id = Input.ReadGuid();
							s = Id.ToString();

							if (this.currentStatus.TryGetValue(s, out Obj) &&
								Obj is MessageStatus MessageStatus)
							{
								lock (MessageStatus.Acknowledged)
								{
									MessageStatus.Acknowledged[From] = (Command == 2);
								}

								EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

								if (MessageStatus.IsComplete(CurrentStatus))
								{
									this.currentStatus.Remove(s);
									this.scheduler.Remove(MessageStatus.Timeout);

									MessageStatus.Callback?.Invoke(this, new ClusterMessageAckEventArgs(
										MessageStatus.Message, MessageStatus.GetResponses(CurrentStatus),
										MessageStatus.State));
								}
							}
							break;
					}
				}
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a cluster message has been received.
		/// </summary>
		public event ClusterMessageEventHandler OnMessageReceived = null;

		private void Transmit(byte[] Message, IPEndPoint Destination)
		{
			this.TransmitBinary(Message);

			foreach (ClusterUdpClient Client in this.clients)
				Client.BeginTransmit(Message, Destination);
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
				this.Transmit(Output.ToArray(), this.destination);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				if (this.shuttingDown)
					this.Dispose2();
			}
			finally
			{
				Output.Dispose();
			}
		}

		/// <summary>
		/// Sends a message using acknowledged service.
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SendMessageAcknowledged(IClusterMessage Message,
			ClusterMessageAckEventHandler Callback, object State)
		{
			Serializer Output = new Serializer();
			Guid Id = Guid.NewGuid();
			string s = Id.ToString();
			MessageStatus Rec = null;

			try
			{
				Output.WriteByte(1);		// Acknowledged message.
				Output.WriteGuid(Id);
				Output.WriteVarUInt64(0);	// Endpoints to skip
				rootObject.Serialize(Output, Message);

				byte[] Bin = Output.ToArray();

				Rec = new MessageStatus()
				{
					Id = Id,
					Message = Message,
					MessageBinary = Bin,
					Callback = Callback,
					State = State
				};

				this.currentStatus[s] = Rec;

				Rec.Timeout = this.scheduler.Add(DateTime.Now.AddSeconds(2), this.ResendAcknowledgedMessage, Rec);

				this.Transmit(Bin, this.destination);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				if (this.shuttingDown)
					this.Dispose2();
			}
			finally
			{
				Output.Dispose();
			}
		}

		private void ResendAcknowledgedMessage(Object P)
		{
			MessageStatus Rec = (MessageStatus)P;

			try
			{
				EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

				if (Rec.IsComplete(CurrentStatus))
				{
					this.currentStatus.Remove(Rec.Id.ToString());
					this.scheduler.Remove(Rec.Timeout);

					Rec.Callback?.Invoke(this, new ClusterMessageAckEventArgs(
						Rec.Message, Rec.GetResponses(CurrentStatus), Rec.State));
				}
				else
				{
					using (Serializer Output = new Serializer())
					{
						Output.WriteByte(1);        // Acknowledged message.
						Output.WriteGuid(Rec.Id);

						lock (Rec.Acknowledged)
						{
							Output.WriteVarUInt64((uint)Rec.Acknowledged.Count);   // Endpoints to skip

							foreach (IPEndPoint Endpoint in Rec.Acknowledged.Keys)
							{
								Output.WriteBinary(Endpoint.Address.GetAddressBytes());
								Output.WriteUInt16((ushort)Endpoint.Port);
							}
						}

						Output.WriteRaw(Rec.MessageBinary, 18, Rec.MessageBinary.Length - 18);

						Rec.Timeout = this.scheduler.Add(DateTime.Now.AddSeconds(2), this.ResendAcknowledgedMessage, Rec);
						this.Transmit(Output.ToArray(), this.destination);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Sends a message using acknowledged service.
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		public Task<EndpointAcknowledgement[]> SendMessageAcknowledgedAsync(IClusterMessage Message)
		{
			TaskCompletionSource<EndpointAcknowledgement[]> Result = new TaskCompletionSource<EndpointAcknowledgement[]>();

			this.SendMessageAcknowledged(Message, (sender, e) =>
			{
				Result.TrySetResult(e.Responses);
			}, null);

			return Result.Task;
		}

		private void SendAlive(object State)
		{
			try
			{
				ClusterGetStatusEventArgs e = new ClusterGetStatusEventArgs();

				this.GetStatus?.Invoke(this, e);
				this.localStatus = e.Status;

				this.SendMessageUnacknowledged(new Alive()
				{
					Status = this.localStatus
				});
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when current status is needed.
		/// </summary>
		public event ClusterGetStatusEventHandler GetStatus = null;

		/// <summary>
		/// Local status. For remote endpoint statuses, see <see cref="GetRemoteStatuses"/>
		/// </summary>
		public object LocalStatus => this.localStatus;

		/// <summary>
		/// Gets current remote statuses. For the current local status, see
		/// <see cref="LocalStatus"/>
		/// </summary>
		/// <returns>Current set of remote statuses</returns>
		public EndpointStatus[] GetRemoteStatuses()
		{
			EndpointStatus[] Result;
			int i, c;

			lock (this.remoteStatus)
			{
				Result = new EndpointStatus[c = this.remoteStatus.Count];

				i = 0;
				foreach (KeyValuePair<IPEndPoint, object> P in this.remoteStatus)
					Result[i++] = new EndpointStatus(P.Key, P.Value);
			}

			return Result;
		}

		internal void StatusReported(object Status, IPEndPoint RemoteEndpoint)
		{
			lock (this.remoteStatus)
			{
				this.remoteStatus[RemoteEndpoint] = Status;
			}

			string s = RemoteEndpoint.ToString();

			if (!this.currentStatus.ContainsKey(s))
				this.currentStatus[s] = RemoteEndpoint;
		}

		internal void EndpointShutDown(IPEndPoint RemoteEndpoint)
		{
			lock (this.remoteStatus)
			{
				this.remoteStatus.Remove(RemoteEndpoint);
			}
		}

		private void CurrentStatus_Removed(object Sender, CacheItemEventArgs<string, object> e)
		{
			if (e.Value is IPEndPoint RemoteEndpoint)
			{
				lock (this.remoteStatus)
				{
					this.remoteStatus.Remove(RemoteEndpoint);
				}
			}
			else if (e.Value is MessageStatus MessageStatus)
			{
				if (e.Reason != RemovedReason.Manual)
				{
					this.scheduler.Remove(MessageStatus.Timeout);

					try
					{
						MessageStatus.Callback?.Invoke(this, new ClusterMessageAckEventArgs(
							MessageStatus.Message, MessageStatus.GetResponses(this.GetRemoteStatuses()),
							MessageStatus.State));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Sends an acknowledged ping message to the other servers in the cluster.
		/// </summary>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Ping(ClusterMessageAckEventHandler Callback, object State)
		{
			this.SendMessageAcknowledged(new Ping(), Callback, State);
		}

		/// <summary>
		/// Sends an acknowledged ping message to the other servers in the cluster.
		/// </summary>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task<EndpointAcknowledgement[]> PingAsync()
		{
			TaskCompletionSource<EndpointAcknowledgement[]> Result = new TaskCompletionSource<EndpointAcknowledgement[]>();

			this.Ping((sender, e) =>
			{
				Result.TrySetResult(e.Responses);
			}, null);

			return Result.Task;
		}

	}
}
