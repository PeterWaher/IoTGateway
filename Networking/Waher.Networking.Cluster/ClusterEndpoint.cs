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

namespace Waher.Networking.Cluster
{
	/// <summary>
	/// Represents one endpoint (or participant) in the network cluster.
	/// </summary>
	public class ClusterEndpoint : CommunicationLayer, IDisposableAsync
	{
		private static readonly Dictionary<Type, ObjectInfo> objectInfo = new Dictionary<Type, ObjectInfo>();
		private static Dictionary<Type, IProperty> propertyTypes = null;
		private static ObjectInfo rootObject;

		private readonly LinkedList<ClusterUdpClient> outgoing = new LinkedList<ClusterUdpClient>();
		private readonly LinkedList<ClusterUdpClient> incoming = new LinkedList<ClusterUdpClient>();
		private readonly IPEndPoint destination;
		internal readonly byte[] key;
		internal Aes aes;
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
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public ClusterEndpoint(IPAddress MulticastAddress, int Port, byte[] SharedSecret, params ISniffer[] Sniffers)
			: base(false, Sniffers)
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
			this.currentStatus = new Cache<string, object>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromSeconds(30), true);
			this.currentStatus.Removed += this.CurrentStatus_Removed;

			if (Types.TryGetModuleParameter("Scheduler", out Scheduler Scheduler))
			{
				this.scheduler = Scheduler;
				this.internalScheduler = false;
			}
			else
			{
				this.scheduler = new Scheduler();
				this.internalScheduler = true;
			}

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

					if (Address.AddressFamily != MulticastAddress.AddressFamily)
						continue;

					if (!IsLoopback)
					{
						Client = null;

						try
						{
							Client = new UdpClient(AddressFamily)
							{
								//DontFragment = true,
								ExclusiveAddressUse = true,
								MulticastLoopback = true,
								EnableBroadcast = true,
								Ttl = 30
							};

							this.Information("Binding to " + Address);

							Client.Client.Bind(new IPEndPoint(Address, 0));

							if (IsMulticastAddress(MulticastAddress) && Interface.SupportsMulticast)
							{
								this.Information("Joining Multicast address " + MulticastAddress);

								try
								{
									Client.JoinMulticastGroup(MulticastAddress);
								}
								catch (Exception ex)
								{
									Log.Exception(ex);
								}
							}
							else
								this.Warning("Address provided is not a multi-cast address.");
						}
						catch (NotSupportedException)
						{
							Client?.Dispose();
							continue;
						}
						catch (Exception ex)
						{
							Client?.Dispose();
							Log.Exception(ex);
							continue;
						}

						ClusterUdpClient = new ClusterUdpClient(this, Client, Address);
						ClusterUdpClient.BeginReceive();

						this.outgoing.AddLast(ClusterUdpClient);

						Client = null;

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
							Client?.Dispose();
							continue;
						}
						catch (Exception ex)
						{
							Client?.Dispose();
							Log.Exception(ex);
							continue;
						}

						ClusterUdpClient = new ClusterUdpClient(this, Client, Address);
						ClusterUdpClient.BeginReceive();

						this.incoming.AddLast(ClusterUdpClient);
						this.endpoints = null;
					}
				}
			}

			try
			{
				Client = new UdpClient(Port, MulticastAddress.AddressFamily)
				{
					MulticastLoopback = true
				};

				if (IsMulticastAddress(MulticastAddress))
				{
					this.Information("Joining Multicast address " + MulticastAddress);

					try
					{
						Client.JoinMulticastGroup(MulticastAddress);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
				else
					this.Warning("Address provided is not a multi-cast address.");

				ClusterUdpClient = new ClusterUdpClient(this, Client, null);
				ClusterUdpClient.BeginReceive();

				this.incoming.AddLast(ClusterUdpClient);
				this.endpoints = null;
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
		public IPEndPoint[] Endpoints
		{
			get
			{
				if (this.endpoints is null)
				{
					List<IPEndPoint> Result = new List<IPEndPoint>();
					LinkedListNode<ClusterUdpClient> Loop = this.incoming.First;

					while (!(Loop is null))
					{
						Result.Add(Loop.Value.EndPoint);
						Loop = Loop.Next;
					}

					this.endpoints = Result.ToArray();
				}

				return this.endpoints;
			}
		}

		private IPEndPoint[] endpoints = null;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use DisposeAsync")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public async Task DisposeAsync()
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
							ToRelease ??= new LinkedList<string>();
							ToRelease.AddLast(Info.Resource);
						}
					}

					this.lockedResources.Clear();
				}

				if (!(ToRelease is null))
				{
					foreach (string Resource in ToRelease)
					{
						await this.SendMessageAcknowledged(new Release()
						{
							Resource = Resource
						}, null, null);
					}
				}

				this.shuttingDown = true;
				await this.SendMessageUnacknowledged(new ShuttingDown());

				this.shutDown?.WaitOne(2000);
				this.shutDown?.Dispose();
				this.shutDown = null;

				this.aes?.Dispose();
				this.aes = null;
			}
		}

		internal async Task Dispose2()
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

			this.shutDown?.Set();
		}

		private void Clear(LinkedList<ClusterUdpClient> Clients)
		{
			LinkedListNode<ClusterUdpClient> Loop = Clients.First;

			while (!(Loop is null))
			{
				try
				{
					Loop.Value.Dispose();
				}
				catch (Exception)
				{
					// Ignore
				}

				Loop = Loop.Next;
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
				ConstructorInfo DefaultConstructor = Types.GetDefaultConstructor(T);
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
				Types.OnInvalidated += (Sender, e) => Init();

			propertyTypes = PropertyTypes;
		}

		/// <summary>
		/// Serializes an object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Binary representation</returns>
		public byte[] Serialize(object Object)
		{
			using Serializer Output = new Serializer();
			rootObject.Serialize(Output, Object);
			return Output.ToArray();
		}

		/// <summary>
		/// Deserializes an object.
		/// </summary>
		/// <param name="Data">Binary representation of object.</param>
		/// <returns>Deserialized object</returns>
		/// <exception cref="KeyNotFoundException">If the corresponding type, or any of the embedded properties, could not be found.</exception>
		public object Deserialize(byte[] Data)
		{
			using Deserializer Input = new Deserializer(Data);
			return rootObject.Deserialize(Input, typeof(object));
		}

		internal async void DataReceived(bool ConstantBuffer, byte[] Data, IPEndPoint From)
		{
			try
			{
				if (Data.Length == 0)
					return;

				this.ReceiveBinary(ConstantBuffer, Data);

				using Deserializer Input = new Deserializer(Data);
				byte Command = Input.ReadByte();

				switch (Command)
				{
					case 0: // Unacknowledged message
						object Object = rootObject.Deserialize(Input, typeof(object));

						if (this.HasSniffers)
							this.Information("Unacknowledged message received: " + Object.GetType().FullName);

						if (Object is IClusterMessage Message)
						{
							await Message.MessageReceived(this, From);

							await this.OnMessageReceived.Raise(this, new ClusterMessageEventArgs(Message));
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

							if (this.HasSniffers)
								this.Information("Acknowledged message received: " + Object.GetType().FullName);

							if (!((Message = Object as IClusterMessage) is null))
							{
								try
								{
									Ack = await Message.MessageReceived(this, From);

									await this.OnMessageReceived.Raise(this, new ClusterMessageEventArgs(Message));
								}
								catch (Exception ex)
								{
									this.Exception(ex);
									Log.Exception(ex);
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

							if (this.HasSniffers)
								this.Information("Sending " + (Ack ? "ACK" : "NACK") + " to " + From.ToString());

							this.Transmit(true, Output.ToArray(), From);
						}
						break;

					case 2: // ACK
					case 3: // NACK
						Id = Input.ReadGuid();
						s = Id.ToString();

						if (this.HasSniffers)
							this.Information(Command == 2 ? "ACK received" : "NACK received");

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

								await MessageStatus.Callback.Raise(this, new ClusterMessageAckEventArgs(
									MessageStatus.Message, MessageStatus.GetResponses(CurrentStatus),
									MessageStatus.State));
							}
						}
						else if (this.HasSniffers)
							this.Warning("Response discarded. No status found for endpoing " + s);
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

							if (this.HasSniffers)
								this.Information("Command received: " + Object.GetType().FullName);

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

							this.Transmit(true, Output.ToArray(), From);
						}
						break;

					case 5: // Command Response
						Id = Input.ReadGuid();
						s = Id.ToString();

						if (this.currentStatus.TryGetValue(s, out Obj) &&
							Obj is CommandStatusBase CommandStatus)
						{
							Object = rootObject.Deserialize(Input, typeof(object));

							if (this.HasSniffers)
								this.Information("Command Response received: " + Object.GetType().FullName);

							CommandStatus.AddResponse(From, Object);

							EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

							if (CommandStatus.IsComplete(CurrentStatus))
							{
								this.currentStatus.Remove(s);
								this.scheduler.Remove(CommandStatus.Timeout);

								await CommandStatus.RaiseResponseEvent(CurrentStatus);
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

							if (this.HasSniffers)
								this.Error(ex.Message);

							CommandStatus2.AddError(From, ex);

							EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

							if (CommandStatus2.IsComplete(CurrentStatus))
							{
								this.currentStatus.Remove(s);
								this.scheduler.Remove(CommandStatus2.Timeout);

								await CommandStatus2.RaiseResponseEvent(CurrentStatus);
							}
						}
						break;

					default:
						this.Error("Invalid command received.");
						break;
				}
			}
			catch (Exception ex)
			{
				this.Exception(ex);
				Log.Exception(ex);
			}
		}

		internal bool IsEcho(IPEndPoint Endpoint)
		{
			LinkedListNode<ClusterUdpClient> Loop = this.outgoing.First;

			while (Loop != null)
			{
				if (Endpoint.Equals(Loop.Value.EndPoint))
					return true;

				Loop = Loop.Next;
			}

			return false;
		}

		private bool IsEndpoint(LinkedList<ClusterUdpClient> Clients, string Address, int Port)
		{
			LinkedListNode<ClusterUdpClient> Loop = Clients.First;

			while (!(Loop is null))
			{
				if (Loop.Value.IsEndpoint(Address, Port))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Event raised when a cluster message has been received.
		/// </summary>
		public event EventHandlerAsync<ClusterMessageEventArgs> OnMessageReceived = null;

		private void Transmit(bool ConstantBuffer, byte[] Message, IPEndPoint Destination)
		{
			this.TransmitBinary(ConstantBuffer, Message);

			LinkedListNode<ClusterUdpClient> Loop = this.outgoing.First;

			while (!(Loop is null))
			{
				Loop.Value.BeginTransmit(ConstantBuffer, Message, Destination);
				Loop = Loop.Next;
			}
		}

		/// <summary>
		/// Sends an unacknowledged message ("at most once")
		/// </summary>
		/// <param name="Message">Message object</param>
		public Task SendMessageUnacknowledged(IClusterMessage Message)
		{
			return this.SendMessageUnacknowledged(Message, this.destination);
		}

		/// <summary>
		/// Sends an unacknowledged message ("at most once")
		/// </summary>
		/// <param name="Message">Message object</param>
		/// <param name="Destination">Send the message to this endpoint.</param>
		public async Task SendMessageUnacknowledged(IClusterMessage Message, IPEndPoint Destination)
		{
			if (this.HasSniffers)
				this.Information("Sending unacknowledged message: " + Message.GetType().FullName);

			Serializer Output = new Serializer();

			try
			{
				Output.WriteByte(0);    // Unacknowledged message.
				rootObject.Serialize(Output, Message);
				this.Transmit(true, Output.ToArray(), Destination);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				if (this.shuttingDown)
					await this.Dispose2();
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
		public Task SendMessageAcknowledged(IClusterMessage Message,
			EventHandlerAsync<ClusterMessageAckEventArgs> Callback, object State)
		{
			return this.SendMessageAcknowledged(Message, this.destination, Callback, State);
		}

		/// <summary>
		/// Sends a message using acknowledged service. ("at least once")
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		/// <param name="Destination">Send the message to this endpoint.</param>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task SendMessageAcknowledged(IClusterMessage Message, IPEndPoint Destination,
			EventHandlerAsync<ClusterMessageAckEventArgs> Callback, object State)
		{
			if (this.HasSniffers)
				this.Information("Sending acknowledged message: " + Message.GetType().FullName);

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
					Destination = Destination,
					Callback = Callback,
					State = State,
					TimeLimit = Now.AddSeconds(30)
				};

				this.currentStatus[s] = Rec;

				Rec.Timeout = this.scheduler.Add(Now.AddSeconds(2), this.ResendAcknowledgedMessage, Rec);

				this.Transmit(true, Bin, Destination);

				EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

				if (Rec.IsComplete(CurrentStatus))
				{
					this.currentStatus.Remove(s);
					this.scheduler.Remove(Rec.Timeout);

					await Rec.Callback.Raise(this, new ClusterMessageAckEventArgs(
						Rec.Message, Rec.GetResponses(CurrentStatus), Rec.State));
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				if (this.shuttingDown)
					await this.Dispose2();
			}
			finally
			{
				Output.Dispose();
			}
		}

		private async Task ResendAcknowledgedMessage(object P)
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

					await Rec.Callback.Raise(this, new ClusterMessageAckEventArgs(
						Rec.Message, Rec.GetResponses(CurrentStatus), Rec.State));
				}
				else
				{
					using Serializer Output = new Serializer();
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
					this.Transmit(true, Output.ToArray(), Rec.Destination);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Sends a message using acknowledged service.
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		public Task<EndpointAcknowledgement[]> SendMessageAcknowledgedAsync(IClusterMessage Message)
		{
			return this.SendMessageAcknowledgedAsync(Message, this.destination);
		}

		/// <summary>
		/// Sends a message using acknowledged service.
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		/// <param name="Destination">Send the message to this endpoint.</param>
		public async Task<EndpointAcknowledgement[]> SendMessageAcknowledgedAsync(IClusterMessage Message, IPEndPoint Destination)
		{
			TaskCompletionSource<EndpointAcknowledgement[]> Result = new TaskCompletionSource<EndpointAcknowledgement[]>();

			await this.SendMessageAcknowledged(Message, Destination, (Sender, e) =>
			{
				Result.TrySetResult(e.Responses);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Sends a message using assured service. ("exactly once")
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendMessageAssured(IClusterMessage Message,
			EventHandlerAsync<ClusterMessageAckEventArgs> Callback, object State)
		{
			return this.SendMessageAssured(Message, this.destination, Callback, State);
		}

		/// <summary>
		/// Sends a message using assured service. ("exactly once")
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		/// <param name="Destination">Send the message to this endpoint.</param>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task SendMessageAssured(IClusterMessage Message, IPEndPoint Destination,
			EventHandlerAsync<ClusterMessageAckEventArgs> Callback, object State)
		{
			Guid MessageId = Guid.NewGuid();

			await this.SendMessageAcknowledged(new Transport()
			{
				MessageID = MessageId,
				Message = Message
			}, Destination, async (Sender, e) =>
			{
				await this.SendMessageAcknowledged(new Deliver()
				{
					MessageID = MessageId
				}, Destination, async (sender2, e2) =>
				{
					await Callback.Raise(this, new ClusterMessageAckEventArgs(Message, e2.Responses, State));
				}, null);
			}, null);
		}

		/// <summary>
		/// Sends a message using assured service. ("exactly once")
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		public Task<EndpointAcknowledgement[]> SendMessageAssuredAsync(IClusterMessage Message)
		{
			return this.SendMessageAssuredAsync(Message, this.destination);
		}

		/// <summary>
		/// Sends a message using assured service. ("exactly once")
		/// </summary>
		/// <param name="Message">Message to send using acknowledged service.</param>
		/// <param name="Destination">Send the message to this endpoint.</param>
		public async Task<EndpointAcknowledgement[]> SendMessageAssuredAsync(IClusterMessage Message, IPEndPoint Destination)
		{
			TaskCompletionSource<EndpointAcknowledgement[]> Result = new TaskCompletionSource<EndpointAcknowledgement[]>();

			await this.SendMessageAssured(Message, Destination, (Sender, e) =>
			{
				Result.TrySetResult(e.Responses);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		private void SendAlive(object _)
		{
			Task.Run(() => this.SendAlive(this.destination));
		}

		private async Task SendAlive(IPEndPoint Destination)
		{
			try
			{
				ClusterGetStatusEventArgs e = new ClusterGetStatusEventArgs();

				await this.GetStatus.Raise(this, e);

				this.localStatus = e.Status;

				await this.SendMessageUnacknowledged(new Alive()
				{
					Status = this.localStatus
				}, Destination);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Event raised when current status is needed.
		/// </summary>
		public event EventHandlerAsync<ClusterGetStatusEventArgs> GetStatus = null;

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
		public async void AddRemoteStatus(IPEndPoint Endpoint, object Status)
		{
			try
			{
				bool New;

				lock (this.remoteStatus)
				{
					New = !this.remoteStatus.ContainsKey(Endpoint);
					this.remoteStatus[Endpoint] = Status;
				}

				if (New)
				{
					await this.SendAlive(Endpoint);

					if (this.HasSniffers)
						this.Information("New remote endpoint: " + Endpoint.ToString());

					await this.EndpointOnline.Raise(this, new ClusterEndpointEventArgs(Endpoint));
				}
				else
					this.Information("Remote endpoint status updated: " + Endpoint.ToString());

				await this.EndpointStatus.Raise(this, new ClusterEndpointStatusEventArgs(Endpoint, Status));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				this.Exception(ex);
			}
		}

		/// <summary>
		/// Explicitly removes a remote cluster endpoint status object.
		/// </summary>
		/// <param name="Endpoint">Cluster endpoint.</param>
		/// <returns>If the corresponding endpoint status object was found and removed.</returns>
		public async Task<bool> RemoveRemoteStatus(IPEndPoint Endpoint)
		{
			bool Removed;

			lock (this.remoteStatus)
			{
				Removed = this.remoteStatus.Remove(Endpoint);
			}

			if (Removed)
			{
				if (this.HasSniffers)
					this.Warning("Remote endpoint removed: " + Endpoint.ToString());

				await this.EndpointOffline.Raise(this, new ClusterEndpointEventArgs(Endpoint));
			}

			return Removed;
		}

		/// <summary>
		/// Event raised when a new endpoint is available in the cluster.
		/// </summary>
		public event EventHandlerAsync<ClusterEndpointEventArgs> EndpointOnline = null;

		/// <summary>
		/// Event raised when an endpoint goes offline.
		/// </summary>
		public event EventHandlerAsync<ClusterEndpointEventArgs> EndpointOffline = null;

		/// <summary>
		/// Event raised when status has been reported by an endpoint.
		/// </summary>
		public event EventHandlerAsync<ClusterEndpointStatusEventArgs> EndpointStatus = null;

		internal void StatusReported(object Status, IPEndPoint RemoteEndpoint)
		{
			this.AddRemoteStatus(RemoteEndpoint, Status);

			string s = RemoteEndpoint.ToString();

			if (!this.currentStatus.ContainsKey(s))
				this.currentStatus[s] = RemoteEndpoint;
		}

		internal Task EndpointShutDown(IPEndPoint RemoteEndpoint)
		{
			return this.RemoveRemoteStatus(RemoteEndpoint);
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

					await this.OnMessageReceived.Raise(this, new ClusterMessageEventArgs(Message));

					return b;
				}
				else
					return false;
			}
			else
				return false;
		}

		private async Task CurrentStatus_Removed(object Sender, CacheItemEventArgs<string, object> e)
		{
			if (e.Value is IPEndPoint RemoteEndpoint)
			{
				if (this.HasSniffers)
					this.Warning("Remote endpoint dropped: " + RemoteEndpoint.ToString());

				await this.RemoveRemoteStatus(RemoteEndpoint);
			}
			else if (e.Value is MessageStatus MessageStatus)
			{
				if (e.Reason != RemovedReason.Manual)
				{
					this.scheduler.Remove(MessageStatus.Timeout);

					if (this.HasSniffers)
						this.Warning("Timeout: " + MessageStatus.Message.GetType().FullName);

					await MessageStatus.Callback.Raise(this, new ClusterMessageAckEventArgs(
						MessageStatus.Message, MessageStatus.GetResponses(this.GetRemoteStatuses()),
						MessageStatus.State));
				}
			}
		}

		/// <summary>
		/// Sends an acknowledged ping message to the other servers in the cluster.
		/// </summary>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Ping(EventHandlerAsync<ClusterMessageAckEventArgs> Callback, object State)
		{
			return this.SendMessageAcknowledged(new Messages.Ping(), Callback, State);
		}

		/// <summary>
		/// Sends an acknowledged ping message to the other servers in the cluster.
		/// </summary>
		public async Task<EndpointAcknowledgement[]> PingAsync()
		{
			TaskCompletionSource<EndpointAcknowledgement[]> Result = new TaskCompletionSource<EndpointAcknowledgement[]>();

			await this.Ping((Sender, e) =>
			{
				Result.TrySetResult(e.Responses);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
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
		public async Task ExecuteCommand<ResponseType>(IClusterCommand Command,
			EventHandlerAsync<ClusterResponseEventArgs<ResponseType>> Callback, object State)
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
					IdStr = s,
					Command = Command,
					CommandBinary = Bin,
					TimeLimit = Now.AddSeconds(30),
					Callback = Callback,
					State = State
				};

				this.currentStatus[s] = Rec;

				Rec.Timeout = this.scheduler.Add(Now.AddSeconds(2), this.ResendCommand<ResponseType>, Rec);

				this.Transmit(true, Bin, this.destination);

				EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

				if (Rec.IsComplete(CurrentStatus))
				{
					this.currentStatus.Remove(Rec.IdStr);
					this.scheduler.Remove(Rec.Timeout);

					await Rec.Callback.Raise(this, new ClusterResponseEventArgs<ResponseType>(
						Rec.Command, Rec.GetResponses(CurrentStatus), Rec.State));
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				if (this.shuttingDown)
					await this.Dispose2();
			}
			finally
			{
				Output.Dispose();
			}
		}

		private async Task ResendCommand<ResponseType>(object P)
		{
			CommandStatus<ResponseType> Rec = (CommandStatus<ResponseType>)P;
			DateTime Now = DateTime.Now;

			try
			{
				EndpointStatus[] CurrentStatus = this.GetRemoteStatuses();

				if (Rec.IsComplete(CurrentStatus) || Now >= Rec.TimeLimit)
				{
					this.currentStatus.Remove(Rec.IdStr);
					this.scheduler.Remove(Rec.Timeout);

					await Rec.Callback.Raise(this, new ClusterResponseEventArgs<ResponseType>(
						Rec.Command, Rec.GetResponses(CurrentStatus), Rec.State));
				}
				else
				{
					using Serializer Output = new Serializer();

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
					this.Transmit(true, Output.ToArray(), this.destination);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
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
		public async Task<EndpointResponse<ResponseType>[]> ExecuteCommandAsync<ResponseType>(IClusterCommand Command)
		{
			TaskCompletionSource<EndpointResponse<ResponseType>[]> Result =
				new TaskCompletionSource<EndpointResponse<ResponseType>[]>();

			await this.ExecuteCommand<ResponseType>(Command, (Sender, e) =>
			{
				Result.TrySetResult(e.Responses);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Asks endpoints in the cluster to echo a text string back to the sender.
		/// </summary>
		/// <param name="Text">Text to echo.</param>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Echo(string Text, EventHandlerAsync<ClusterResponseEventArgs<string>> Callback, object State)
		{
			return this.ExecuteCommand(new Echo()
			{
				Text = Text
			}, Callback, State);
		}

		/// <summary>
		/// Asks endpoints in the cluster to echo a text string back to the sender.
		/// </summary>
		/// <param name="Text">Text to echo.</param>
		public async Task<EndpointResponse<string>[]> EchoAsync(string Text)
		{
			TaskCompletionSource<EndpointResponse<string>[]> Result = new TaskCompletionSource<EndpointResponse<string>[]>();

			await this.Echo(Text, (Sender, e) =>
			{
				Result.TrySetResult(e.Responses);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Asks endpoints in the cluster to return assemblies available in their runtime environment.
		/// </summary>
		/// <param name="Callback">Method to call when responses have been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetAssemblies(EventHandlerAsync<ClusterResponseEventArgs<string[]>> Callback, object State)
		{
			return this.ExecuteCommand(new Assemblies(), Callback, State);
		}

		/// <summary>
		/// Asks endpoints in the cluster to return assemblies available in their runtime environment.
		/// </summary>
		public async Task<EndpointResponse<string[]>[]> GetAssembliesAsync()
		{
			TaskCompletionSource<EndpointResponse<string[]>[]> Result = new TaskCompletionSource<EndpointResponse<string[]>[]>();

			await this.GetAssemblies((Sender, e) =>
			{
				Result.TrySetResult(e.Responses);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Locks a singleton resource in the cluster.
		/// </summary>
		/// <param name="ResourceName">Name of the resource</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <param name="Callback">Method to call when operation completes or fails.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task Lock(string ResourceName, int TimeoutMilliseconds,
			EventHandlerAsync<ClusterResourceLockEventArgs> Callback, object State)
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
				return this.LockResult(false, null, InfoRec);
			else
				return this.Lock(Info, InfoRec);
		}

		private Task Lock(LockInfo Info, LockInfoRec InfoRec)
		{
			return this.SendMessageAcknowledged(new Lock()
			{
				Resource = Info.Resource
			}, (Sender, e) =>
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

				return this.LockResult(Ok, LockedBy, InfoRec);

			}, null);
		}

		private async Task LockResult(bool Ok, IPEndPoint LockedBy, LockInfoRec InfoRec)
		{
			if (Ok)
				await this.Raise(InfoRec.Info.Resource, true, null, InfoRec);
			else if (DateTime.Now >= InfoRec.Timeout)
				await this.Raise(InfoRec.Info.Resource, false, LockedBy, InfoRec);
			else if (!InfoRec.TimeoutScheduled)
			{
				InfoRec.LockedBy = LockedBy;
				InfoRec.Timeout = this.scheduler.Add(InfoRec.Timeout, this.LockTimeout, InfoRec);
				InfoRec.TimeoutScheduled = true;
			}
		}

		private Task LockTimeout(object P)
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

			return this.Raise(Info.Resource, false, InfoRec.LockedBy, InfoRec);
		}

		private async Task Raise(string ResourceName, bool LockSuccessful, IPEndPoint LockedBy,
			LockInfoRec InfoRec)
		{
			try
			{
				if (InfoRec.TimeoutScheduled)
				{
					this.scheduler.Remove(InfoRec.Timeout);
					InfoRec.TimeoutScheduled = false;
				}

				LockInfo Info = InfoRec.Info;

				lock (this.lockedResources)
				{
					Info.Queue.Remove(InfoRec);

					if (!Info.Locked && Info.Queue.First is null)
						this.lockedResources.Remove(Info.Resource);
				}

				await InfoRec.Callback.Raise(this, new ClusterResourceLockEventArgs(ResourceName,
					LockSuccessful, LockedBy, InfoRec.State));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Locks a singleton resource in the cluster.
		/// </summary>
		/// <param name="ResourceName">Name of the resource</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		public async Task<ClusterResourceLockEventArgs> LockAsync(string ResourceName, int TimeoutMilliseconds)
		{
			TaskCompletionSource<ClusterResourceLockEventArgs> Result = new TaskCompletionSource<ClusterResourceLockEventArgs>();

			await this.Lock(ResourceName, TimeoutMilliseconds, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Releases a resource.
		/// </summary>
		/// <param name="ResourceName">Name of the resource.</param>
		/// <exception cref="ArgumentException">If the resource is not locked.</exception>
		public async Task Release(string ResourceName)
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

			await this.SendMessageAcknowledged(new Release()
			{
				Resource = ResourceName
			}, null, null);

			if (!(InfoRec is null))
			{
				this.scheduler.Add(DateTime.Now.AddMilliseconds(this.rnd.Next(50) + 1), (P) =>
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

			this.scheduler.Add(DateTime.Now.AddMilliseconds(this.rnd.Next(50) + 1), (P) =>
			{
				this.Lock(Info, InfoRec);
			}, null);
		}

	}
}
