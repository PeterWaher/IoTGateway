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
using Waher.Networking.Cluster.Commands;
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

		private readonly LinkedList<ClusterUdpClient> outgoing = new LinkedList<ClusterUdpClient>();
		private readonly LinkedList<ClusterUdpClient> incoming = new LinkedList<ClusterUdpClient>();
		private readonly IPEndPoint destination;
		internal readonly Aes aes;
		internal readonly byte[] key;
		internal Cache<string, object> currentStatus;
		private Scheduler scheduler;
		private readonly Random rnd = new Random();
		private readonly Dictionary<IPEndPoint, object> remoteStatus;
		internal readonly Dictionary<string, LockInfo> lockedResources = new Dictionary<string, LockInfo>();
		private object localStatus;
		private Timer aliveTimer;
		internal bool shuttingDown = false;
		private readonly bool internalScheduler;
		private ManualResetEvent shutDown = new ManualResetEvent(false);

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
			ClusterUdpClient ClusterUdpClient;
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
			this.currentStatus = new Cache<string, object>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromSeconds(30));
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

						ClusterUdpClient = new ClusterUdpClient(this, Client, Address);
						ClusterUdpClient.BeginReceive();

						this.outgoing.AddLast(ClusterUdpClient);

						try
						{
							Client = new UdpClient(AddressFamily)
							{
								ExclusiveAddressUse = false,
							};

							Client.Client.Bind(new IPEndPoint(Address, Port));
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

						ClusterUdpClient = new ClusterUdpClient(this, Client, Address);
						ClusterUdpClient.BeginReceive();

						this.incoming.AddLast(ClusterUdpClient);
					}
				}
			}

			try
			{
				Client = new UdpClient(Port, MulticastAddress.AddressFamily)
				{
					MulticastLoopback = false
				};

				Client.JoinMulticastGroup(MulticastAddress);

				ClusterUdpClient = new ClusterUdpClient(this, Client, null);
				ClusterUdpClient.BeginReceive();

				this.incoming.AddLast(ClusterUdpClient);
			}
			catch (Exception)
			{
				// Ignore
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
				foreach (ClusterUdpClient Client in this.incoming)
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
				LinkedList<string> ToRelease = null;

				lock (this.lockedResources)
				{
					foreach (LockInfo Info in this.lockedResources.Values)
					{
						if (Info.Locked)
						{
							if (ToRelease is null)
								ToRelease = new LinkedList<string>();

							ToRelease.AddLast(Info.Resource);
						}
					}

					this.lockedResources.Clear();
				}

				if (!(ToRelease is null))
				{
					foreach (string Resource in ToRelease)
					{
						this.SendMessageAcknowledged(new Release()
						{
							Resource = Resource
						}, null, null);
					}
				}

				this.shuttingDown = true;
				this.SendMessageUnacknowledged(new ShuttingDown());

				this.shutDown?.WaitOne(2000);
				this.shutDown?.Dispose();
				this.shutDown = null;
			}
		}

		internal void Dispose2()
		{
			if (this.internalScheduler)
				this.scheduler?.Dispose();

			this.scheduler = null;

			this.aliveTimer?.Dispose();
			this.aliveTimer = null;

			this.Clear(this.outgoing);
			this.Clear(this.incoming);

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

			this.shutDown?.Set();
		}

		private void Clear(LinkedList<ClusterUdpClient> Clients)
		{
			foreach (ClusterUdpClient Client in Clients)
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

			Clients.Clear();
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

								if (this.IsEndpoint(this.outgoing, Address, Port) ||
									this.IsEndpoint(this.incoming, Address, Port))
								{
									Skip = true;
									break;
								}

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
									try
									{
										Ack = await Message.MessageReceived(this, From);
										this.OnMessageReceived?.Invoke(this, new ClusterMessageEventArgs(Message));
									}
									catch (Exception ex)
									{
										this.Error(ex.Message);
										Log.Critical(ex);
										Ack = false;
									}

									this.currentStatus[s] = Ack;
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

						case 4: // Command
							Id = Input.ReadGuid();
							s = Id.ToString();
							Len = Input.ReadVarUInt64();
							Skip = false;

							while (Len > 0)
							{
								string Address = new IPAddress(Input.ReadBinary()).ToString();
								int Port = Input.ReadUInt16();

								if (this.IsEndpoint(this.outgoing, Address, Port) ||
									this.IsEndpoint(this.incoming, Address, Port))
								{
									Skip = true;
									break;
								}

								Len--;
							}

							if (Skip)
								break;

							if (!this.currentStatus.TryGetValue(s, out Obj))
							{
								Object = rootObject.Deserialize(Input, typeof(object));
								if (Object is IClusterCommand ClusterCommand)
								{
									try
									{
										Obj = await ClusterCommand.Execute(this, From);
										this.currentStatus[s] = Obj;
									}
									catch (Exception ex)
									{
										Obj = ex;
									}
								}
								else
									Obj = new Exception("Non-command object received in command: " + Object?.GetType()?.FullName);
							}

							using (Serializer Output = new Serializer())
							{
								if (Obj is Exception ex)
								{
									ex = Log.UnnestException(ex);

									Output.WriteByte(6);
									Output.WriteGuid(Id);
									Output.WriteString(ex.Message);
									Output.WriteString(ex.GetType().FullName);
								}
								else
								{
									Output.WriteByte(5);
									Output.WriteGuid(Id);
									rootObject.Serialize(Output, Obj);
								}

								this.Transmit(Output.ToArray(), From);
							}
							break;

						case 5: // Command Response
							Id = Input.ReadGuid();
							s = Id.ToString();

							if (this.currentStatus.TryGetValue(s, out Obj) &&
								Obj is CommandStatusBase CommandStatus)
							{
								Object = rootObject.Deserialize(Input, typeof(object));

								CommandStatus.AddResponse(From, Object);

								EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

								if (CommandStatus.IsComplete(CurrentStatus))
								{
									this.currentStatus.Remove(s);
									this.scheduler.Remove(CommandStatus.Timeout);

									CommandStatus.RaiseResponseEvent(CurrentStatus);
								}
							}
							break;

						case 6: // Command Exception
							Id = Input.ReadGuid();
							s = Id.ToString();

							if (this.currentStatus.TryGetValue(s, out Obj) &&
								Obj is CommandStatusBase CommandStatus2)
							{
								string ExceptionMessage = Input.ReadString();
								string ExceptionType = Input.ReadString();
								Exception ex;

								try
								{
									Type T = Types.GetType(ExceptionType);
									if (T is null)
										ex = new Exception(ExceptionMessage);
									else
										ex = (Exception)Activator.CreateInstance(T, ExceptionMessage);
								}
								catch (Exception)
								{
									ex = new Exception(ExceptionMessage);
								}

								CommandStatus2.AddError(From, ex);

								EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

								if (CommandStatus2.IsComplete(CurrentStatus))
								{
									this.currentStatus.Remove(s);
									this.scheduler.Remove(CommandStatus2.Timeout);

									CommandStatus2.RaiseResponseEvent(CurrentStatus);
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

		private bool IsEndpoint(LinkedList<ClusterUdpClient> Clients, string Address, int Port)
		{
			foreach (ClusterUdpClient Client in Clients)
			{
				if (Client.IsEndpoint(Address, Port))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Event raised when a cluster message has been received.
		/// </summary>
		public event ClusterMessageEventHandler OnMessageReceived = null;

		private void Transmit(byte[] Message, IPEndPoint Destination)
		{
			this.TransmitBinary(Message);

			foreach (ClusterUdpClient Client in this.outgoing)
				Client.BeginTransmit(Message, Destination);
		}

		/// <summary>
		/// Sends an unacknowledged message ("at most once")
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
		/// Sends a message using acknowledged service. ("at least once")
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
				Output.WriteByte(1);        // Acknowledged message.
				Output.WriteGuid(Id);
				Output.WriteVarUInt64(0);   // Endpoints to skip
				rootObject.Serialize(Output, Message);

				byte[] Bin = Output.ToArray();
				DateTime Now = DateTime.Now;

				Rec = new MessageStatus()
				{
					Id = Id,
					Message = Message,
					MessageBinary = Bin,
					Callback = Callback,
					State = State,
					TimeLimit = Now.AddSeconds(30)
				};

				this.currentStatus[s] = Rec;

				Rec.Timeout = this.scheduler.Add(Now.AddSeconds(2), this.ResendAcknowledgedMessage, Rec);

				this.Transmit(Bin, this.destination);

				EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

				if (Rec.IsComplete(CurrentStatus))
				{
					this.currentStatus.Remove(Rec.Id.ToString());
					this.scheduler.Remove(Rec.Timeout);

					Rec.Callback?.Invoke(this, new ClusterMessageAckEventArgs(
						Rec.Message, Rec.GetResponses(CurrentStatus), Rec.State));
				}
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
				DateTime Now = DateTime.Now;

				if (Rec.IsComplete(CurrentStatus) || Now >= Rec.TimeLimit)
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

						Rec.Timeout = this.scheduler.Add(Now.AddSeconds(2), this.ResendAcknowledgedMessage, Rec);
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

		/// <summary>
		/// Sends a message using assured service. ("exactly once")
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void SendMessageAssured(IClusterMessage Message,
			ClusterMessageAckEventHandler Callback, object State)
		{
			Guid MessageId = Guid.NewGuid();

			this.SendMessageAcknowledged(new Transport()
			{
				MessageID = MessageId,
				Message = Message
			}, (sender, e) =>
			{
				this.SendMessageAcknowledged(new Deliver()
				{
					MessageID = MessageId
				}, (sender2, e2) =>
				{
					Callback?.Invoke(this, new ClusterMessageAckEventArgs(Message, e2.Responses, State));
				}, null);
			}, null);
		}

		/// <summary>
		/// Sends a message using assured service. ("exactly once")
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		public Task<EndpointAcknowledgement[]> SendMessageAssuredAsync(IClusterMessage Message)
		{
			TaskCompletionSource<EndpointAcknowledgement[]> Result = new TaskCompletionSource<EndpointAcknowledgement[]>();

			this.SendMessageAssured(Message, (sender, e) =>
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

		/// <summary>
		/// Explicitly adds a remote endpoint status object.
		/// In normal operation, calling this method is not necessary, as 
		/// remote endpoint status is reported over the cluster.
		/// </summary>
		/// <param name="Endpoint">Cluster endpoint.</param>
		/// <param name="Status">Status object.</param>
		public void AddRemoteStatus(IPEndPoint Endpoint, object Status)
		{
			ClusterEndpointEventHandler h;
			ClusterEndpointStatusEventHandler h2;
			bool New;

			lock (this.remoteStatus)
			{
				New = !this.remoteStatus.ContainsKey(Endpoint);
				this.remoteStatus[Endpoint] = Status;
			}

			if (New && !((h = this.EndpointOnline) is null))
			{
				try
				{
					h(this, new ClusterEndpointEventArgs(Endpoint));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			if (!((h2 = this.EndpointStatus) is null))
			{
				try
				{
					h2(this, new ClusterEndpointStatusEventArgs(Endpoint, Status));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Explicitly removes a remote cluster endpoint status object.
		/// </summary>
		/// <param name="Endpoint">Cluster endpoint.</param>
		/// <returns>If the corresponding endpoint status object was found and removed.</returns>
		public bool RemoveRemoteStatus(IPEndPoint Endpoint)
		{
			ClusterEndpointEventHandler h;
			bool Removed;

			lock (this.remoteStatus)
			{
				Removed = this.remoteStatus.Remove(Endpoint);
			}

			if (Removed && !((h = this.EndpointOffline) is null))
			{
				try
				{
					h(this, new ClusterEndpointEventArgs(Endpoint));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			return Removed;
		}

		/// <summary>
		/// Event raised when a new endpoint is available in the cluster.
		/// </summary>
		public event ClusterEndpointEventHandler EndpointOnline = null;

		/// <summary>
		/// Event raised when an endpoint goes offline.
		/// </summary>
		public event ClusterEndpointEventHandler EndpointOffline = null;

		/// <summary>
		/// Event raised when status has been reported by an endpoint.
		/// </summary>
		public event ClusterEndpointStatusEventHandler EndpointStatus = null;

		internal void StatusReported(object Status, IPEndPoint RemoteEndpoint)
		{
			this.AddRemoteStatus(RemoteEndpoint, Status);

			string s = RemoteEndpoint.ToString();

			if (!this.currentStatus.ContainsKey(s))
				this.currentStatus[s] = RemoteEndpoint;
		}

		internal void EndpointShutDown(IPEndPoint RemoteEndpoint)
		{
			this.RemoveRemoteStatus(RemoteEndpoint);
		}

		internal void AssuredTransport(Guid MessageId, IClusterMessage Message)
		{
			this.currentStatus[MessageId.ToString()] = Message;
		}

		internal async Task<bool> AssuredDelivery(Guid MessageId, ClusterEndpoint Endpoint, IPEndPoint RemoteEndpoint)
		{
			string s = MessageId.ToString();

			if (this.currentStatus.TryGetValue(s, out object Obj))
			{
				if (Obj is bool b)
					return b;
				else if (Obj is IClusterMessage Message)
				{
					this.currentStatus[s] = false;
					b = await Message.MessageReceived(Endpoint, RemoteEndpoint);
					this.currentStatus[s] = b;

					this.OnMessageReceived?.Invoke(this, new ClusterMessageEventArgs(Message));

					return b;
				}
				else
					return false;
			}
			else
				return false;
		}

		private void CurrentStatus_Removed(object Sender, CacheItemEventArgs<string, object> e)
		{
			if (e.Value is IPEndPoint RemoteEndpoint)
				this.RemoveRemoteStatus(RemoteEndpoint);
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
		public Task<EndpointAcknowledgement[]> PingAsync()
		{
			TaskCompletionSource<EndpointAcknowledgement[]> Result = new TaskCompletionSource<EndpointAcknowledgement[]>();

			this.Ping((sender, e) =>
			{
				Result.TrySetResult(e.Responses);
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Execute a command on the other members of the cluster, and waits for 
		/// responses to be returned. Retries are performed in cases responses are 
		/// not received.
		/// </summary>
		/// <typeparam name="ResponseType">Type of response expected.</typeparam>
		/// <param name="Command">Command to send.</param>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ExecuteCommand<ResponseType>(IClusterCommand Command,
			ClusterResponseEventHandler<ResponseType> Callback, object State)
		{
			Serializer Output = new Serializer();
			Guid Id = Guid.NewGuid();
			string s = Id.ToString();
			CommandStatus<ResponseType> Rec = null;

			try
			{
				Output.WriteByte(4);        // Command.
				Output.WriteGuid(Id);
				Output.WriteVarUInt64(0);   // Endpoints to skip
				rootObject.Serialize(Output, Command);

				byte[] Bin = Output.ToArray();
				DateTime Now = DateTime.Now;

				Rec = new CommandStatus<ResponseType>()
				{
					Id = Id,
					Command = Command,
					CommandBinary = Bin,
					TimeLimit = Now.AddSeconds(30),
					Callback = Callback,
					State = State
				};

				this.currentStatus[s] = Rec;

				Rec.Timeout = this.scheduler.Add(Now.AddSeconds(2), this.ResendCommand<ResponseType>, Rec);

				this.Transmit(Bin, this.destination);

				EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

				if (Rec.IsComplete(CurrentStatus))
				{
					this.currentStatus.Remove(Rec.Id.ToString());
					this.scheduler.Remove(Rec.Timeout);

					Rec.Callback?.Invoke(this, new ClusterResponseEventArgs<ResponseType>(
						Rec.Command, Rec.GetResponses(CurrentStatus), Rec.State));
				}
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

		private void ResendCommand<ResponseType>(Object P)
		{
			CommandStatus<ResponseType> Rec = (CommandStatus<ResponseType>)P;
			DateTime Now = DateTime.Now;

			try
			{
				EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

				if (Rec.IsComplete(CurrentStatus) || Now >= Rec.TimeLimit)
				{
					this.currentStatus.Remove(Rec.Id.ToString());
					this.scheduler.Remove(Rec.Timeout);

					Rec.Callback?.Invoke(this, new ClusterResponseEventArgs<ResponseType>(
						Rec.Command, Rec.GetResponses(CurrentStatus), Rec.State));
				}
				else
				{
					using (Serializer Output = new Serializer())
					{
						Output.WriteByte(4);        // Command.
						Output.WriteGuid(Rec.Id);

						lock (Rec.Responses)
						{
							Output.WriteVarUInt64((uint)Rec.Responses.Count);   // Endpoints to skip

							foreach (IPEndPoint Endpoint in Rec.Responses.Keys)
							{
								Output.WriteBinary(Endpoint.Address.GetAddressBytes());
								Output.WriteUInt16((ushort)Endpoint.Port);
							}
						}

						Output.WriteRaw(Rec.CommandBinary, 18, Rec.CommandBinary.Length - 18);

						Rec.Timeout = this.scheduler.Add(Now.AddSeconds(2), this.ResendCommand<ResponseType>, Rec);
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
		/// Execute a command on the other members of the cluster, and waits for 
		/// responses to be returned. Retries are performed in cases responses are 
		/// not received.
		/// </summary>
		/// <typeparam name="ResponseType">Type of response expected.</typeparam>
		/// <param name="Command">Command to send.</param>
		/// <returns>Responses returned from available endpoints.</returns>
		public Task<EndpointResponse<ResponseType>[]> ExecuteCommandAsync<ResponseType>(IClusterCommand Command)
		{
			TaskCompletionSource<EndpointResponse<ResponseType>[]> Result =
				new TaskCompletionSource<EndpointResponse<ResponseType>[]>();

			this.ExecuteCommand<ResponseType>(Command, (sender, e) =>
			{
				Result.TrySetResult(e.Responses);
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Asks endpoints in the cluster to echo a text string back to the sender.
		/// </summary>
		/// <param name="Text">Text to echo.</param>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Echo(string Text, ClusterResponseEventHandler<string> Callback, object State)
		{
			this.ExecuteCommand<string>(new Echo()
			{
				Text = Text
			}, Callback, State);
		}

		/// <summary>
		/// Asks endpoints in the cluster to echo a text string back to the sender.
		/// </summary>
		/// <param name="Text">Text to echo.</param>
		public Task<EndpointResponse<string>[]> EchoAsync(string Text)
		{
			TaskCompletionSource<EndpointResponse<string>[]> Result = new TaskCompletionSource<EndpointResponse<string>[]>();

			this.Echo(Text, (sender, e) =>
			{
				Result.TrySetResult(e.Responses);
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Asks endpoints in the cluster to return assemblies available in their runtime environment.
		/// </summary>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAssemblies(ClusterResponseEventHandler<string[]> Callback, object State)
		{
			this.ExecuteCommand<string[]>(new Assemblies(), Callback, State);
		}

		/// <summary>
		/// Asks endpoints in the cluster to return assemblies available in their runtime environment.
		/// </summary>
		public Task<EndpointResponse<string[]>[]> GetAssembliesAsync()
		{
			TaskCompletionSource<EndpointResponse<string[]>[]> Result = new TaskCompletionSource<EndpointResponse<string[]>[]>();

			this.GetAssemblies((sender, e) =>
			{
				Result.TrySetResult(e.Responses);
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Locks a singleton resource in the cluster.
		/// </summary>
		/// <param name="ResourceName">Name of the resource</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <param name="Callback">Method to call when operation completes or fails.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Lock(string ResourceName, int TimeoutMilliseconds,
			ClusterResourceLockEventHandler Callback, object State)
		{
			LockInfo Info;
			LockInfoRec InfoRec;

			lock (this.lockedResources)
			{
				if (!this.lockedResources.TryGetValue(ResourceName, out Info))
				{
					Info = new LockInfo()
					{
						Resource = ResourceName,
						Locked = false
					};

					this.lockedResources[ResourceName] = Info;
				}

				Info.Queue.AddLast(InfoRec = new LockInfoRec()
				{
					Info = Info,
					Timeout = DateTime.Now.AddMilliseconds(TimeoutMilliseconds),
					Callback = Callback,
					State = State
				});
			}

			if (Info.Locked)
			{
				this.LockResult(false, null, InfoRec);
				return;
			}

			this.Lock(Info, InfoRec);
		}

		private void Lock(LockInfo Info, LockInfoRec InfoRec)
		{
			this.SendMessageAcknowledged(new Lock()
			{
				Resource = Info.Resource
			}, (sender, e) =>
			{
				IPEndPoint LockedBy = null;

				foreach (EndpointAcknowledgement Response in e.Responses)
				{
					if (Response.ACK.HasValue && !Response.ACK.Value)
					{
						LockedBy = Response.Endpoint;
						break;
					}
				}

				bool Ok;

				lock (this.lockedResources)
				{
					if (Info.Locked)
						Ok = false;
					else
						Ok = LockedBy is null;

					if (Ok)
					{
						Info.Locked = true;
						Info.Queue.Remove(InfoRec);
					}
					else if (InfoRec.Timeout <= DateTime.Now)
					{
						Info.Queue.Remove(InfoRec);

						if (!Info.Locked && Info.Queue.First is null)
							this.lockedResources.Remove(Info.Resource);
					}
				}

				this.LockResult(Ok, LockedBy, InfoRec);

			}, null);
		}

		private void LockResult(bool Ok, IPEndPoint LockedBy, LockInfoRec InfoRec)
		{
			if (Ok)
				this.Raise(InfoRec.Info.Resource, true, null, InfoRec);
			else if (DateTime.Now >= InfoRec.Timeout)
				this.Raise(InfoRec.Info.Resource, false, LockedBy, InfoRec);
			else if (!InfoRec.TimeoutScheduled)
			{
				InfoRec.LockedBy = LockedBy;
				InfoRec.Timeout = scheduler.Add(InfoRec.Timeout, this.LockTimeout, InfoRec);
				InfoRec.TimeoutScheduled = true;
			}
		}

		private void LockTimeout(object P)
		{
			LockInfoRec InfoRec = (LockInfoRec)P;
			LockInfo Info = InfoRec.Info;

			InfoRec.TimeoutScheduled = false;

			lock (this.lockedResources)
			{
				Info.Queue.Remove(InfoRec);

				if (!Info.Locked && Info.Queue.First is null)
					this.lockedResources.Remove(Info.Resource);
			}

			this.Raise(Info.Resource, false, InfoRec.LockedBy, InfoRec);
		}

		private void Raise(string ResourceName, bool LockSuccessful, IPEndPoint LockedBy,
			LockInfoRec InfoRec)
		{
			try
			{
				if (InfoRec.TimeoutScheduled)
				{
					scheduler.Remove(InfoRec.Timeout);
					InfoRec.TimeoutScheduled = false;
				}

				LockInfo Info = InfoRec.Info;

				lock (this.lockedResources)
				{
					Info.Queue.Remove(InfoRec);

					if (!Info.Locked && Info.Queue.First is null)
						this.lockedResources.Remove(Info.Resource);
				}

				InfoRec.Callback?.Invoke(this, new ClusterResourceLockEventArgs(ResourceName,
					LockSuccessful, LockedBy, InfoRec.State));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Locks a singleton resource in the cluster.
		/// </summary>
		/// <param name="ResourceName">Name of the resource</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		public Task<ClusterResourceLockEventArgs> LockAsync(string ResourceName, int TimeoutMilliseconds)
		{
			TaskCompletionSource<ClusterResourceLockEventArgs> Result = new TaskCompletionSource<ClusterResourceLockEventArgs>();

			this.Lock(ResourceName, TimeoutMilliseconds, (sender, e) =>
			{
				Result.TrySetResult(e);
			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Releases a resource.
		/// </summary>
		/// <param name="ResourceName">Name of the resource.</param>
		/// <exception cref="ArgumentException">If the resource is not locked.</exception>
		public void Release(string ResourceName)
		{
			LockInfo Info;
			LockInfoRec InfoRec;

			lock (this.lockedResources)
			{
				if (!this.lockedResources.TryGetValue(ResourceName, out Info) || !Info.Locked)
					throw new ArgumentException("Resource not locked.", nameof(ResourceName));

				Info.Locked = false;

				if (Info.Queue.First is null)
				{
					this.lockedResources.Remove(ResourceName);
					InfoRec = null;
				}
				else
					InfoRec = Info.Queue.First.Value;
			}

			this.SendMessageAcknowledged(new Release()
			{
				Resource = ResourceName
			}, null, null);

			if (!(InfoRec is null))
			{
				scheduler.Add(DateTime.Now.AddMilliseconds(this.rnd.Next(50) + 1), (P) =>
				{
					this.Lock(Info, InfoRec);
				}, null);
			}
		}

		internal void Released(string ResourceName)
		{
			LockInfo Info;
			LockInfoRec InfoRec;

			lock (this.lockedResources)
			{
				if (!this.lockedResources.TryGetValue(ResourceName, out Info) || Info.Locked)
					return;

				if (Info.Queue.First is null)
				{
					this.lockedResources.Remove(ResourceName);
					return;
				}
				else
					InfoRec = Info.Queue.First.Value;
			}

			scheduler.Add(DateTime.Now.AddMilliseconds(this.rnd.Next(50) + 1), (P) =>
			{
				this.Lock(Info, InfoRec);
			}, null);
		}

	}
}
