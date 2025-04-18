﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.Transport;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Runtime.Timing;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.DTLS;
using Waher.Content.Binary;
#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Connectivity;
#endif

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP client. CoAP is defined in RFC7252:
	/// https://tools.ietf.org/html/rfc7252
	/// </summary>
	public class CoapEndpoint : CommunicationLayer, IDisposableAsync
	{
		/// <summary>
		/// Default CoAP port = 5683
		/// </summary>
		public const int DefaultCoapPort = 5683;

		/// <summary>
		/// Default CoAP over DTLS port = 5684
		/// </summary>
		public const int DefaultCoapsPort = 5684;

		internal const int ACK_TIMEOUT = 2;   // seconds
		internal const double ACK_RANDOM_FACTOR = 1.5;
		internal const int MAX_RETRANSMIT = 4;
		internal const int NSTART = 1;
		internal const int DEFAULT_LEISURE = 5;   // seconds
		internal const int PROBING_RATE = 1;  // byte/second

		private static readonly CoapOptionComparer optionComparer = new CoapOptionComparer();
		private static Dictionary<int, CoapOption> optionTypes = null;
		private static Dictionary<int, ICoapContentFormat> contentFormatsByCode = null;
		private static Dictionary<string, ICoapContentFormat> contentFormatsByContentType = null;

		private readonly Dictionary<ushort, Message> outgoingMessages = new Dictionary<ushort, Message>();
		private readonly Dictionary<ulong, Message> activeTokens = new Dictionary<ulong, Message>();
		private readonly Dictionary<string, CoapResource> resources = new Dictionary<string, CoapResource>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Random gen = new Random();
		private Scheduler scheduler;
		private readonly LinkedList<ClientBase> coapOutgoing = new LinkedList<ClientBase>();
		private readonly LinkedList<ClientBase> coapIncoming = new LinkedList<ClientBase>();
		private readonly Cache<string, ResponseCacheRec> blockedResponses = new Cache<string, ResponseCacheRec>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromMinutes(1), true);
		private readonly IUserSource users;
		private readonly string requiredPrivilege;
		private ushort msgId = 0;
		private uint tokenMsb = 0;
		private ushort lastNonMsgId = 0;
		private Message lastNonMsg = null;

		static CoapEndpoint()
		{
			Types.OnInvalidated += Types_OnInvalidated;
			Init();
		}

		private class ResponseCacheRec
		{
			public byte[] Payload;
			public int BlockSize;
			public CoapOption[] Options;
		}

		private static void Init()
		{
			Dictionary<int, CoapOption> Options = new Dictionary<int, CoapOption>();
			CoapOption Option;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICoapOption)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					Option = (CoapOption)CI.Invoke(Types.NoParameters);
					if (Options.ContainsKey(Option.OptionNumber))
						throw new Exception("Option number " + Option.OptionNumber + " already defined.");

					Options[Option.OptionNumber] = Option;
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			optionTypes = Options;

			Dictionary<int, ICoapContentFormat> ByCode = new Dictionary<int, ICoapContentFormat>();
			Dictionary<string, ICoapContentFormat> ByType = new Dictionary<string, ICoapContentFormat>();
			ICoapContentFormat ContentFormat;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICoapContentFormat)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					ContentFormat = (ICoapContentFormat)CI.Invoke(Types.NoParameters);
					if (ByCode.ContainsKey(ContentFormat.ContentFormat))
						throw new Exception("Content format number " + ContentFormat.ContentFormat + " already defined.");

					ByCode[ContentFormat.ContentFormat] = ContentFormat;
					ByType[ContentFormat.ContentType] = ContentFormat;
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			contentFormatsByCode = ByCode;
			contentFormatsByContentType = ByType;
		}

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public CoapEndpoint(params ISniffer[] Sniffers)
			: this(new int[] { DefaultCoapPort }, null, null, null, false, false, Sniffers)
		{
		}

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		/// <param name="CoapPort">CoAP port number to listen for incoming unencrypted traffic.</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public CoapEndpoint(int CoapPort, params ISniffer[] Sniffers)
			: this(new int[] { CoapPort }, null, null, null, false, false, Sniffers)
		{
		}

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		/// <param name="CoapsPort">CoAPs port number to listen for incoming encrypted traffic.</param>
		/// <param name="Users">User data source, if pre-shared keys should be allowed by a DTLS server endpoint.</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public CoapEndpoint(int CoapsPort, IUserSource Users, params ISniffer[] Sniffers)
			: this(null, new int[] { CoapsPort }, Users, null, false, false, Sniffers)
		{
		}

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		/// <param name="CoapsPort">CoAPs port number to listen for incoming encrypted traffic.</param>
		/// <param name="Users">User data source, if pre-shared keys should be allowed by a DTLS server endpoint.</param>
		/// <param name="RequiredPrivilege">Required privilege, for the user to be acceptable
		/// in PSK handshakes.</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public CoapEndpoint(int CoapsPort, IUserSource Users, string RequiredPrivilege, params ISniffer[] Sniffers)
			: this(null, new int[] { CoapsPort }, Users, RequiredPrivilege, false, false, Sniffers)
		{
		}

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		/// <param name="CoapPorts">CoAP port numbers, to listen for incoming unencrypted traffic.</param>
		/// <param name="CoapsPorts">CoAPs port numbers, to listen for incoming encrypted traffic.</param>
		/// <param name="Users">User data source, if pre-shared keys should be allowed by a DTLS server endpoint.</param>
		/// <param name="RequiredPrivilege">Required privilege, for the user to be acceptable
		/// in PSK handshakes.</param>
		/// <param name="LoopbackTransmission">If transmission on the loopback interface should be permitted.</param>
		/// <param name="LoopbackReception">If reception on the loopback interface should be permitted.</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public CoapEndpoint(int[] CoapPorts, int[] CoapsPorts, IUserSource Users, string RequiredPrivilege,
			bool LoopbackTransmission, bool LoopbackReception, params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			LinkedList<KeyValuePair<int, bool>> Ports = new LinkedList<KeyValuePair<int, bool>>();
			UdpClient Outgoing;
			UdpClient Incoming;

			this.users = Users;
			this.requiredPrivilege = RequiredPrivilege;

			if (!(CoapPorts is null))
			{
				foreach (int Port in CoapPorts)
					Ports.AddLast(new KeyValuePair<int, bool>(Port, false));
			}

			if (!(CoapsPorts is null))
			{
				foreach (int Port in CoapsPorts)
					Ports.AddLast(new KeyValuePair<int, bool>(Port, true));
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
					IPAddress MulticastAddress;
#else
			foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				switch (Interface.NetworkInterfaceType)
				{
					case NetworkInterfaceType.Loopback:
						if (!LoopbackReception && !LoopbackTransmission)
							continue;
						break;
				}

				IPInterfaceProperties Properties = Interface.GetIPProperties();
				IPAddress MulticastAddress;

				foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
				{
					IPAddress Address = UnicastAddress.Address;
					AddressFamily AddressFamily = Address.AddressFamily;
					bool IsLoopback = Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback;
#endif
					if (AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4)
						MulticastAddress = IPAddress.Parse("224.0.1.187");
					else if (AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6)
						MulticastAddress = IPAddress.Parse("[FF02::FD]");
					else
						continue;

					foreach (KeyValuePair<int, bool> PortRec in Ports)
					{
						int Port = PortRec.Key;
						bool Encrypted = PortRec.Value;

						if (!IsLoopback || LoopbackTransmission)
						{
							try
							{
								Outgoing = new UdpClient(AddressFamily)
								{
									//DontFragment = true,
									MulticastLoopback = false
								};
							}
							catch (NotSupportedException)
							{
								continue;
							}
							catch (Exception ex)
							{
								Log.Exception(ex);
								continue;
							}

							Outgoing.EnableBroadcast = !Encrypted;
							Outgoing.Ttl = 30;
							Outgoing.Client.Bind(new IPEndPoint(Address, 0));

							if (!Encrypted)
							{
								if (IsMulticastAddress(MulticastAddress))
									Outgoing.JoinMulticastGroup(MulticastAddress);
								else
									this.Warning("Address provided is not a multi-cast address.");
							}

							IPEndPoint EP = new IPEndPoint(MulticastAddress, Port);
							ClientBase OutgoingClient;

							if (Encrypted)
							{
								DtlsOverUdp Dtls = new DtlsOverUdp(Outgoing, DtlsMode.Both,
									this.users, this.requiredPrivilege, Sniffers);

								OutgoingClient = new OutgoingDtlsClient()
								{
									Endpoint = this,
									Dtls = Dtls,
									IsLoopback = IsLoopback
								};

								Dtls.Tag = OutgoingClient;
							}
							else
							{
								OutgoingClient = new OutgoingUdpClient()
								{
									Endpoint = this,
									Client = Outgoing,
									MulticastAddress = EP,
									IsLoopback = IsLoopback
								};
							}

							OutgoingClient.BeginReceive();
							this.coapOutgoing.AddLast(OutgoingClient);
						}

						if (!IsLoopback || LoopbackReception)
						{
							ClientBase IncomingClient;

							try
							{
								Incoming = new UdpClient(AddressFamily)
								{
									//DontFragment = true,
									ExclusiveAddressUse = false
								};

								Incoming.Client.Bind(new IPEndPoint(Address, Port));

								if (Encrypted)
								{
									DtlsOverUdp Dtls = new DtlsOverUdp(Incoming, DtlsMode.Both,
										this.users, this.requiredPrivilege, Sniffers);

									IncomingClient = new IncomingDtlsClient()
									{
										Endpoint = this,
										Dtls = Dtls,
										IsLoopback = IsLoopback
									};

									Dtls.Tag = IncomingClient;
								}
								else
								{
									IncomingClient = new IncomingUdpClient()
									{
										Endpoint = this,
										Client = Incoming,
										IsLoopback = IsLoopback
									};
								}

								IncomingClient.BeginReceive();
								this.coapIncoming.AddLast(IncomingClient);
							}
							catch (Exception)
							{
								Incoming = null;
							}

							if (!Encrypted)
							{
								try
								{
									Incoming = new UdpClient(Port, AddressFamily)
									{
										//DontFragment = true,
										MulticastLoopback = false
									};

									if (IsMulticastAddress(MulticastAddress))
										Incoming.JoinMulticastGroup(MulticastAddress);
									else
										this.Warning("Address provided is not a multi-cast address.");

									IncomingClient = new IncomingUdpClient()
									{
										Endpoint = this,
										Client = Incoming,
										IsLoopback = false
									};

									IncomingClient.BeginReceive();
									this.coapIncoming.AddLast(IncomingClient);
								}
								catch (Exception)
								{
									Incoming = null;
								}
							}
						}
					}
				}
			}

			this.scheduler = new Scheduler();

			this.Register(new CoreResource(this));
		}

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
			this.scheduler?.Dispose();
			this.scheduler = null;

			foreach (ClientBase Client in this.coapOutgoing)
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

			this.coapOutgoing.Clear();

			foreach (ClientBase Client in this.coapIncoming)
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

			this.coapIncoming.Clear();

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

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			Init();
		}

		internal async Task Decode(ClientBase Client, byte[] Packet, IPEndPoint From)
		{
			if (Packet.Length < 4)
			{
				this.Error("Datagram too short.");
				return;
			}

			byte b = Packet[0];
			int TokenLength = b & 15;
			if (TokenLength > 8)
			{
				this.Error("Invalid token length.");
				return;
			}

			b >>= 4;
			CoapMessageType Type = (CoapMessageType)(b & 3);
			b >>= 2;
			if (b != 1)
			{
				this.Error("Unrecognized version.");
				return;
			}

			CoapCode Code = (CoapCode)Packet[1];
			ushort MessageId = Packet[2];
			MessageId <<= 8;
			MessageId |= Packet[3];

			ulong Token = 0;
			int Offset = 0;
			int Pos = 4;
			int Len = Packet.Length;

			while (TokenLength > 0)
			{
				if (Pos >= Len)
				{
					this.Error("Unexpected end of packet.");
					return;
				}

				Token |= (ulong)Packet[Pos++] << Offset;
				Offset += 8;
				TokenLength--;
			}

			List<CoapOption> Options = new List<CoapOption>();
			byte[] Payload;

			if (Pos < Len)
			{
				int OptionNumber = 0;
				int Delta;
				int Length;

				while (Pos < Len)
				{
					b = Packet[Pos++];
					if (b == 0xff)
						break;

					Length = b & 15;
					Delta = b >> 4;

					if (Delta == 13)
					{
						if (Pos >= Len)
						{
							this.Error("Unexpected end of packet.");
							return;
						}

						Delta += Packet[Pos++];
					}
					else if (Delta == 14)
					{
						if (Pos + 1 >= Len)
						{
							this.Error("Unexpected end of packet.");
							return;
						}

						Delta = Packet[Pos++];
						Delta <<= 8;
						Delta |= Packet[Pos++];
						Delta += 269;
					}
					else if (Delta == 15)
					{
						this.Error("Invalid delta-value.");
						return;
					}

					if (Length == 13)
					{
						if (Pos >= Len)
						{
							this.Error("Unexpected end of packet.");
							return;
						}

						Length += Packet[Pos++];
					}
					else if (Length == 14)
					{
						if (Pos + 1 >= Len)
						{
							this.Error("Unexpected end of packet.");
							return;
						}

						Length = Packet[Pos++];
						Length <<= 8;
						Length |= Packet[Pos++];
						Length += 269;
					}
					else if (Length == 15)
					{
						this.Error("Invalid length-value.");
						return;
					}

					if (Length == 0)
						Payload = null;
					else
					{
						if (Pos + Length > Len)
						{
							this.Error("Unexpected end of packet.");
							return;
						}

						Payload = new byte[Length];
						Array.Copy(Packet, Pos, Payload, 0, Length);
						Pos += Length;
					}

					OptionNumber += Delta;

					if (optionTypes.TryGetValue(OptionNumber, out CoapOption Option))
					{
						try
						{
							Option = Option.Create(Payload);
						}
						catch (Exception ex)
						{
							this.Exception(ex);

							if (Option.Critical)
								return;
							else
								Option = new CoapOptionUnknown(OptionNumber, Payload, false);
						}
					}
					else
						Option = new CoapOptionUnknown(OptionNumber, Payload, (OptionNumber & 1) != 0);

					Options.Add(Option);
				}

				if (Pos >= Len)
					Payload = null;
				else
				{
					Payload = new byte[Len - Pos];
					Array.Copy(Packet, Pos, Payload, 0, Len - Pos);
				}
			}
			else
				Payload = null;

			CoapMessage IncomingMessage = new CoapMessage(Type, Code, MessageId, Token, Options.ToArray(), Payload, From);

			foreach (CoapOption Option in Options)
			{
				switch (Option.OptionNumber)
				{
					case 3:
						IncomingMessage.Host = ((CoapOptionUriHost)Option).Value;
						break;

					case 6:
						ulong l = ((CoapOptionObserve)Option).Value;
						if (l < 0 || l > 0xffffff)
						{
							this.Error("Invalid observe value.");
							return;
						}

						IncomingMessage.Observe = (uint)l;
						break;

					case 7:
						l = ((CoapOptionUriPort)Option).Value;
						if (l < 0 || l > ushort.MaxValue)
						{
							this.Error("Invalid port number.");
							return;
						}

						IncomingMessage.Port = (ushort)l;
						break;

					case 8:
						if (IncomingMessage.LocationPath is null)
							IncomingMessage.LocationPath = "/" + ((CoapOptionLocationPath)Option).Value;
						else
							IncomingMessage.LocationPath = IncomingMessage.LocationPath + "/" + ((CoapOptionLocationPath)Option).Value;
						break;

					case 11:
						if (IncomingMessage.Path is null)
							IncomingMessage.Path = "/" + ((CoapOptionUriPath)Option).Value;
						else
							IncomingMessage.Path = IncomingMessage.Path + "/" + ((CoapOptionUriPath)Option).Value;
						break;

					case 12:
						l = ((CoapOptionContentFormat)Option).Value;
						if (l < 0 || l > ushort.MaxValue)
						{
							this.Error("Invalid content format.");
							return;
						}

						IncomingMessage.ContentFormat = (ushort)((CoapOptionContentFormat)Option).Value;
						break;

					case 14:
						l = ((CoapOptionMaxAge)Option).Value;
						if (l < 0 || l > uint.MaxValue)
						{
							this.Error("Invalid max age.");
							return;
						}

						IncomingMessage.MaxAge = (uint)l;
						break;

					case 15:
						if (IncomingMessage.UriQuery is null)
							IncomingMessage.UriQuery = new Dictionary<string, string>();

						CoapOptionKeyValue Query = (CoapOptionKeyValue)Option;

						IncomingMessage.UriQuery[Query.Key] = Query.KeyValue;
						break;

					case 17:
						IncomingMessage.Accept = ((CoapOptionAccept)Option).Value;
						break;

					case 20:
						if (IncomingMessage.LocationQuery is null)
							IncomingMessage.LocationQuery = new Dictionary<string, string>();

						Query = (CoapOptionLocationQuery)Option;

						IncomingMessage.LocationQuery[Query.Key] = Query.KeyValue;
						break;

					case 23:
						IncomingMessage.Block2 = (CoapOptionBlock2)Option;
						break;

					case 27:
						IncomingMessage.Block1 = (CoapOptionBlock1)Option;
						break;

					case 28:
						l = ((CoapOptionSize2)Option).Value;
						if (l < 0 || l > uint.MaxValue)
						{
							this.Error("Invalid size2.");
							return;
						}

						IncomingMessage.Size2 = (uint)l;
						break;

					case 60:
						l = ((CoapOptionSize1)Option).Value;
						if (l < 0 || l > uint.MaxValue)
						{
							this.Error("Invalid size1.");
							return;
						}

						IncomingMessage.Size1 = (uint)l;
						break;
				}
			}

			// TODO: Return error responses if invalid options are found.

			Message OutgoingMessage;
			Message OutgoingMessage2;

			if (Type == CoapMessageType.ACK || Type == CoapMessageType.RST)
			{
				lock (this.outgoingMessages)
				{
					if (this.outgoingMessages.TryGetValue(MessageId, out OutgoingMessage))
					{
						if (OutgoingMessage.destination.Equals(From))
							this.outgoingMessages.Remove(MessageId);
						else
							OutgoingMessage = null;
					}
					else
						OutgoingMessage = null;
				}

				if (OutgoingMessage is null && Type == CoapMessageType.RST && MessageId == this.lastNonMsgId)
				{
					OutgoingMessage = this.lastNonMsg;
					this.lastNonMsg = null;
					this.lastNonMsgId = 0;
				}

				if (!(OutgoingMessage is null))
				{
					if (OutgoingMessage.token == Token)
					{
						if (Code == CoapCode.Continue && !(IncomingMessage.Block1 is null))
						{
							if (IncomingMessage.Block1.Size < OutgoingMessage.blockSize)
							{
								OutgoingMessage.blockNr *= (OutgoingMessage.blockSize / IncomingMessage.Block1.Size);
								OutgoingMessage.blockSize = IncomingMessage.Block1.Size;
							}

							OutgoingMessage.blockNr++;
							Pos = OutgoingMessage.blockNr * OutgoingMessage.blockSize;

							if (Pos >= OutgoingMessage.payload.Length)
								await this.Fail(Client, OutgoingMessage);
							else
							{
								await this.Transmit(Client, OutgoingMessage.destination, Client.IsEncrypted,
									null, OutgoingMessage.messageType, OutgoingMessage.messageCode,
									OutgoingMessage.token, true, OutgoingMessage.payload,
									OutgoingMessage.blockNr, OutgoingMessage.blockSize,
									OutgoingMessage.resource, OutgoingMessage.callback, OutgoingMessage.state,
									OutgoingMessage.payloadResponseStream, OutgoingMessage.credentials,
									OutgoingMessage.options);
							}
						}
						else
						{
							if (!(IncomingMessage.Block2 is null))
							{
								OutgoingMessage.options = Remove(OutgoingMessage.options, 27);     // Remove Block1 option, if available.

								IncomingMessage.Payload = await OutgoingMessage.BlockReceived(Client, IncomingMessage);
								if (IncomingMessage.Payload is null)
									OutgoingMessage = null;
							}

							if (!(OutgoingMessage is null))
							{
								lock (this.activeTokens)
								{
									if (this.activeTokens.TryGetValue(Token, out OutgoingMessage2) && OutgoingMessage2 == OutgoingMessage)
									{
										if (!IncomingMessage.Observe.HasValue)
											this.activeTokens.Remove(Token);
									}
									else
										OutgoingMessage2 = null;
								}

								if (!(OutgoingMessage2 is null))
								{
									IncomingMessage.BaseUri = new Uri(OutgoingMessage.GetUri());
									await OutgoingMessage.ResponseReceived(Client, IncomingMessage);
								}
								else
									await this.Fail(Client, OutgoingMessage);
							}
						}
					}
					else if (Type == CoapMessageType.RST)
					{
						lock (this.activeTokens)
						{
							if (this.activeTokens.TryGetValue(Token, out Message Msg) && Msg == OutgoingMessage)
								this.activeTokens.Remove(Token);
						}

						OutgoingMessage.resource?.UnregisterSubscription(IncomingMessage.From, Token);

						await this.Fail(Client, OutgoingMessage);
					}
				}
			}
			else
			{
				lock (this.activeTokens)
				{
					if (!this.activeTokens.TryGetValue(Token, out OutgoingMessage))
						OutgoingMessage = null;
				}

				if (!(OutgoingMessage is null))
				{
					OutgoingMessage.responseReceived = true;

					if (IncomingMessage.Type == CoapMessageType.CON)
					{
						await this.Transmit(Client, From, Client.IsEncrypted, IncomingMessage.MessageId,
							CoapMessageType.ACK, CoapCode.EmptyMessage, Token, false, null, 0, 64,
							null, null, null, null, null, null);
					}

					if (!(IncomingMessage.Block2 is null))
					{
						IncomingMessage.Payload = await OutgoingMessage.BlockReceived(Client, IncomingMessage);
						if (!(IncomingMessage.Payload is null))
						{
							if (!IncomingMessage.Observe.HasValue)
							{
								lock (this.activeTokens)
								{
									this.activeTokens.Remove(Token);
								}

								lock (this.outgoingMessages)
								{
									if (this.outgoingMessages.TryGetValue(OutgoingMessage.messageID, out OutgoingMessage2) &&
										OutgoingMessage2 == OutgoingMessage)
									{
										this.outgoingMessages.Remove(OutgoingMessage.messageID);
									}
								}
							}

							await OutgoingMessage.ResponseReceived(Client, IncomingMessage);
						}
					}
					else
					{
						if (!IncomingMessage.Observe.HasValue)
						{
							lock (this.activeTokens)
							{
								this.activeTokens.Remove(Token);
							}
						}

						await OutgoingMessage.ResponseReceived(Client, IncomingMessage);
					}
				}
				else
				{
					// TODO: Blocked messages (Block1)

					CoapResource Resource = null;
					string Path = IncomingMessage.Path ?? "/";
					string SubPath = string.Empty;
					int i;

					lock (this.resources)
					{
						while (true)
						{
							if (this.resources.TryGetValue(Path, out Resource))
							{
								if (string.IsNullOrEmpty(SubPath) || Resource.HandlesSubPaths)
								{
									IncomingMessage.SubPath = SubPath;
									break;
								}
								else
									Resource = null;
							}

							i = Path.LastIndexOf('/');
							if (i < 0)
								break;

							SubPath = Path.Substring(i) + SubPath;
							Path = Path.Substring(0, i);
						}
					}

					if (!(Resource is null))
					{
						CoapOptionObserve ObserveResponse = null;

						if (IncomingMessage.Observe.HasValue && Resource.Observable)
						{
							switch (IncomingMessage.Observe.Value)
							{
								case 0:
									ObservationRegistration Registration =
										Resource.RegisterSubscription(Client, this, IncomingMessage);

									ObserveResponse = new CoapOptionObserve(Registration.SequenceNumber);
									Registration.IncSeqNr();
									break;

								case 1:
									Resource.UnregisterSubscription(IncomingMessage.From,
										IncomingMessage.Token);
									break;
							}
						}

						if (ObserveResponse is null)
							await this.ProcessRequest(Resource, Client, IncomingMessage, false);
						else
							await this.ProcessRequest(Resource, Client, IncomingMessage, false, ObserveResponse);
					}
					else
					{
						EventHandlerAsync<CoapMessageEventArgs> h = this.OnIncomingRequest;

						if (!(h is null))
						{
							CoapMessageEventArgs e = new CoapMessageEventArgs(Client, this, IncomingMessage, null);
							try
							{
								await h(this, e);
							}
							catch (CoapException ex)
							{
								if (Type == CoapMessageType.CON && !e.Responded)
									await e.RST(ex.ErrorCode);
							}
							catch (Exception ex)
							{
								Log.Exception(ex);

								if (Type == CoapMessageType.CON && !e.Responded)
									await e.RST(CoapCode.InternalServerError);
							}

							if (!e.Responded && Type == CoapMessageType.CON)
								await e.ACK();
						}
						else
						{
							await this.Transmit(Client, From, Client.IsEncrypted, MessageId,
								CoapMessageType.RST, CoapCode.NotFound, Token, false,
								null, 0, 64, null, null, null, null, null);
						}
					}
				}
			}
		}

		internal void RemoveBlockedResponse(string Key)
		{
			this.blockedResponses.Remove(Key);
		}

		internal async Task ProcessRequest(CoapResource Resource, ClientBase Client,
			CoapMessage IncomingMessage, bool AlreadyResponded, params CoapOption[] AdditionalResponseOptions)
		{
			CoapResponse Response = new CoapResponse(Client, this, IncomingMessage.From,
				IncomingMessage, Resource.Notifications, Resource, AdditionalResponseOptions);
			string Key = IncomingMessage.From.Address.ToString() + " " + IncomingMessage.Token.ToString();
			int Block2Nr = !(IncomingMessage.Block2 is null) ? IncomingMessage.Block2.Number : 0;

			Response.Responded = AlreadyResponded;

			if (Block2Nr > 0 && this.blockedResponses.TryGetValue(Key, out ResponseCacheRec CachedResponse))
			{
				await Response.Respond(CoapCode.Content, CachedResponse.Payload, Block2Nr,
					CachedResponse.BlockSize, CachedResponse.Options);
			}
			else
			{
				try
				{
					switch (IncomingMessage.Code)
					{
						case CoapCode.GET:
							if (Resource.GetMethod?.AllowsGET ?? false)
								await Resource.GetMethod.GET(IncomingMessage, Response);
							else if (IncomingMessage.Type == CoapMessageType.CON)
								await Response.RST(CoapCode.MethodNotAllowed);
							break;

						case CoapCode.POST:
							if (Resource.PostMethod?.AllowsPOST ?? false)
								await Resource.PostMethod.POST(IncomingMessage, Response);
							else if (IncomingMessage.Type == CoapMessageType.CON)
								await Response.RST(CoapCode.MethodNotAllowed);
							break;

						case CoapCode.PUT:
							if (Resource.PutMethod?.AllowsPUT ?? false)
								await Resource.PutMethod.PUT(IncomingMessage, Response);
							else if (IncomingMessage.Type == CoapMessageType.CON)
								await Response.RST(CoapCode.MethodNotAllowed);
							break;

						case CoapCode.DELETE:
							if (Resource.DeleteMethod?.AllowsDELETE ?? false)
								await Resource.DeleteMethod.DELETE(IncomingMessage, Response);
							else if (IncomingMessage.Type == CoapMessageType.CON)
								await Response.RST(CoapCode.MethodNotAllowed);
							break;

						case CoapCode.FETCH:
							if (Resource.FetchMethod?.AllowsFETCH ?? false)
								await Resource.FetchMethod.FETCH(IncomingMessage, Response);
							else if (IncomingMessage.Type == CoapMessageType.CON)
								await Response.RST(CoapCode.MethodNotAllowed);
							break;

						case CoapCode.PATCH:
							if (Resource.PatchMethod?.AllowsPATCH ?? false)
								await Resource.PatchMethod.PATCH(IncomingMessage, Response);
							else if (IncomingMessage.Type == CoapMessageType.CON)
								await Response.RST(CoapCode.MethodNotAllowed);
							break;

						case CoapCode.iPATCH:
							if (Resource.IPatchMethod?.AllowsIPATCH ?? false)
								await Resource.IPatchMethod.IPATCH(IncomingMessage, Response);
							else if (IncomingMessage.Type == CoapMessageType.CON)
								await Response.RST(CoapCode.MethodNotAllowed);
							break;

						default:
							if (IncomingMessage.Type == CoapMessageType.CON)
								await Response.RST(CoapCode.MethodNotAllowed);
							break;
					}
				}
				catch (CoapException ex)
				{
					if (IncomingMessage.Type == CoapMessageType.CON && !Response.Responded)
						await Response.RST(ex.ErrorCode);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (IncomingMessage.Type == CoapMessageType.CON && !Response.Responded)
						await Response.RST(CoapCode.InternalServerError);
				}

				if (IncomingMessage.Type == CoapMessageType.CON && !Response.Responded)
					await Response.ACK();
			}
		}

		/// <summary>
		/// Registers a CoAP resource on the endpoint.
		/// </summary>
		/// <param name="Resource">Resource to register.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(CoapResource Resource)
		{
			lock (this.resources)
			{
				if (!this.resources.ContainsKey(Resource.Path))
				{
					Resource.Endpoint = this;
					this.resources[Resource.Path] = Resource;
				}
				else
					throw new Exception("Path already registered.");
			}

			return Resource;
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET)
		{
			return this.Register(Path, GET, Notifications.None, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, Notifications Notifications)
		{
			return this.Register(Path, GET, Notifications, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, Notifications Notifications, string Title)
		{
			return this.Register(Path, GET, Notifications, Title, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, Notifications Notifications,
			string Title, string[] ResourceTypes)
		{
			return this.Register(Path, GET, Notifications, Title, ResourceTypes, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, Notifications Notifications, string Title,
			string[] ResourceTypes, string[] InterfaceDescriptions)
		{
			return this.Register(Path, GET, Notifications, Title, ResourceTypes, InterfaceDescriptions,
				null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, Notifications Notifications, string Title,
			string[] ResourceTypes, string[] InterfaceDescriptions, int[] ContentFormats)
		{
			return this.Register(Path, GET, Notifications, Title, ResourceTypes, InterfaceDescriptions,
				ContentFormats, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <param name="MaximumSizeEstimate">Optional maximum size estimate of resource. Can be null.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, Notifications Notifications,
			string Title, string[] ResourceTypes, string[] InterfaceDescriptions,
			int[] ContentFormats, int? MaximumSizeEstimate)
		{
			return this.Register(new CoapGetDelegateResource(Path, GET, Notifications, Title, ResourceTypes,
				InterfaceDescriptions, ContentFormats, MaximumSizeEstimate));
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST)
		{
			return this.Register(Path, GET, POST, Notifications.None, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			Notifications Notifications)
		{
			return this.Register(Path, GET, POST, Notifications, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			Notifications Notifications, string Title)
		{
			return this.Register(Path, GET, POST, Notifications, Title, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			Notifications Notifications, string Title, string[] ResourceTypes)
		{
			return this.Register(Path, GET, POST, Notifications, Title, ResourceTypes, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			Notifications Notifications, string Title, string[] ResourceTypes, string[] InterfaceDescriptions)
		{
			return this.Register(Path, GET, POST, Notifications, Title, ResourceTypes,
				InterfaceDescriptions, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			Notifications Notifications, string Title, string[] ResourceTypes, string[] InterfaceDescriptions,
			int[] ContentFormats)
		{
			return this.Register(Path, GET, POST, Notifications, Title, ResourceTypes, InterfaceDescriptions,
				ContentFormats, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="Notifications">If the resource is observable, and how notifications are to be sent.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <param name="MaximumSizeEstimate">Optional maximum size estimate of resource. Can be null.</param>
		/// <returns>Returns the registered resource.</returns>
		/// <exception cref="Exception">If a resource with the same path has already been registered.</exception>
		public CoapResource Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			Notifications Notifications, string Title, string[] ResourceTypes, string[] InterfaceDescriptions,
			int[] ContentFormats, int? MaximumSizeEstimate)
		{
			return this.Register(new CoapGetPostDelegateResource(Path, GET, POST, Notifications, Title,
				ResourceTypes, InterfaceDescriptions, ContentFormats, MaximumSizeEstimate));
		}

		/// <summary>
		/// Gets an array of registered resources.
		/// </summary>
		/// <returns>Resources.</returns>
		internal CoapResource[] GetResources()
		{
			CoapResource[] Result;

			lock (this.resources)
			{
				Result = new CoapResource[this.resources.Count];
				this.resources.Values.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Unregisters a CoAP resource.
		/// </summary>
		/// <param name="Resource">Resource to unregister.</param>
		/// <returns>If the resource was found and removed.</returns>
		public bool Unregister(CoapResource Resource)
		{
			if (Resource is null)
				return false;

			lock (this.resources)
			{
				if (this.resources.TryGetValue(Resource.Path, out CoapResource Resource2) && Resource2 == Resource)
				{
					Resource.Endpoint = null;
					this.resources.Remove(Resource.Path);
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Event raised when an incoming requestm that does not correspond to a registered resource has been received.
		/// </summary>
		public event EventHandlerAsync<CoapMessageEventArgs> OnIncomingRequest = null;

		private byte[] Encode(CoapMessageType Type, CoapCode Code, ulong Token, ushort MessageID, byte[] Payload, params CoapOption[] Options)
		{
			MemoryStream ms = new MemoryStream(128);
			ulong Temp;
			byte b, b2;

			b = 1 << 6; // Version
			b |= (byte)((byte)Type << 4);

			Temp = Token;
			b2 = 0;

			while (Temp > 0)
			{
				Temp >>= 8;
				b2++;
			}

			b |= b2;
			ms.WriteByte(b);
			ms.WriteByte((byte)Code);
			ms.WriteByte((byte)(MessageID >> 8));
			ms.WriteByte((byte)MessageID);

			while (b2 > 0)
			{
				ms.WriteByte((byte)Token);
				Token >>= 8;
				b2--;
			}

			if (!(Options is null) && Options.Length > 0)
			{
				byte[] Value;
				int LastNumber = 0;
				int Delta;
				int Length;

				Delta = 0;
				foreach (CoapOption Option in Options)
					Option.originalOrder = Delta++;

				Array.Sort(Options, optionComparer);

				foreach (CoapOption Option in Options)
				{
					Delta = Option.OptionNumber - LastNumber;
					LastNumber += Delta;

					Value = Option.GetValue();
					Length = Value is null ? 0 : Value.Length;

					if (Delta < 13)
						b = (byte)(Delta << 4);
					else if (Delta < 269)
						b = 13 << 4;
					else
						b = 14 << 4;

					if (Length < 13)
						b |= (byte)Length;
					else if (Length < 269)
						b |= 13;
					else
						b |= 14;

					ms.WriteByte(b);

					if (Delta >= 13)
					{
						if (Delta < 269)
							ms.WriteByte((byte)(Delta - 13));
						else
						{
							Delta -= 269;
							ms.WriteByte((byte)(Delta >> 8));
							ms.WriteByte((byte)Delta);
						}
					}

					if (Length >= 13)
					{
						if (Length < 269)
							ms.WriteByte((byte)(Length - 13));
						else
						{
							Length -= 269;
							ms.WriteByte((byte)(Length >> 8));
							ms.WriteByte((byte)Length);
						}
					}

					if (!(Value is null))
						ms.Write(Value, 0, Value.Length);
				}
			}

			if (!(Payload is null) && Payload.Length > 0)
			{
				ms.WriteByte(0xff);
				ms.Write(Payload, 0, Payload.Length);
			}

			byte[] Result = ms.ToArray();

			ms.Dispose();

			return Result;
		}

		/// <summary>
		/// Checks if an array of options contains a given option number.
		/// </summary>
		/// <param name="Options">Array of options. Can be null or empty.</param>
		/// <param name="OptionNumber">Option number to check for.</param>
		/// <returns>If an option with the given option number was found.</returns>
		public static bool HasOption(CoapOption[] Options, int OptionNumber)
		{
			if (Options is null)
				return false;

			foreach (CoapOption Option in Options)
			{
				if (Option.OptionNumber == OptionNumber)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Removes options of a given type from a set of options.
		/// </summary>
		/// <param name="Options">Original set of options.</param>
		/// <param name="OptionNumber">Option number to remove.</param>
		/// <returns>Potentially new set of options.</returns>
		public static CoapOption[] Remove(CoapOption[] Options, int OptionNumber)
		{
			List<CoapOption> Result = null;
			CoapOption Option;
			int i, j, c = Options.Length;

			for (i = 0; i < c; i++)
			{
				Option = Options[i];

				if (Option.OptionNumber == OptionNumber)
				{
					if (Result is null)
					{
						Result = new List<CoapOption>();
						for (j = 0; j < i; j++)
							Result.Add(Options[j]);
					}
				}
				else
					Result?.Add(Option);
			}

			if (Result is null)
				return Options;
			else
				return Result.ToArray();
		}

		/// <summary>
		/// Merges two sets of options.
		/// </summary>
		/// <param name="Options1">First set of options.</param>
		/// <param name="Options2">Second set of options.</param>
		/// <returns>Merged set of options.</returns>
		public static CoapOption[] Merge(CoapOption[] Options1, params CoapOption[] Options2)
		{
			if (Options1 is null || Options1.Length == 0)
				return Options2;
			else if (Options2 is null || Options2.Length == 0)
				return Options1;
			else
			{
				List<CoapOption> Merged = new List<CoapOption>();

				Merged.AddRange(Options1);
				Merged.AddRange(Options2);

				return Merged.ToArray();
			}
		}

		internal static bool IsBlockSizeValid(int Size)
		{
			int Value = 0;
			int NrBits = 0;

			while (Size != 0)
			{
				if ((Size & 1) != 0)
					NrBits++;

				Size >>= 1;
				Value++;
			}

			Value -= 5;
			return (Value >= 0 && Value <= 7 && NrBits == 1);
		}

		internal Task Transmit(ClientBase Client, IPEndPoint Destination, bool Encrypted, ushort? MessageID,
			CoapMessageType MessageType, CoapCode Code, ulong? Token, bool UpdateTokenTable,
			byte[] Payload, int BlockNr, int BlockSize, CoapResource Resource, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			MemoryStream PayloadResponseStream, IDtlsCredentials Credentials, params CoapOption[] Options)
		{
			Message Message;
			byte[] OrgPayload = Payload;

			if (!IsBlockSizeValid(BlockSize))
				throw new ArgumentException("Invalid block size.", nameof(BlockSize));

			if (BlockNr * BlockSize > (Payload is null ? 0 : Payload.Length))
				throw new ArgumentException("Invalid block number.", nameof(BlockNr));

			if (!(Payload is null) && Payload.Length > BlockSize)
			{
				int Pos = BlockNr * BlockSize;
				int NrLeft = Payload.Length - Pos;

				if ((int)Code >= 64 || Code == CoapCode.EmptyMessage)   // Response
				{
					string Prefix = Destination.Address.ToString() + " ";
					string Key = Prefix + Token.ToString();

					if (BlockNr == 0 || !this.blockedResponses.ContainsKey(Key))
					{
						this.blockedResponses.Add(Key, new ResponseCacheRec()
						{
							Payload = Payload,
							BlockSize = BlockSize,
							Options = Options
						});

						this.blockedResponses.Add(Prefix + "0", new ResponseCacheRec()
						{
							Payload = Payload,
							BlockSize = BlockSize,
							Options = Options
						});
					}

					if (NrLeft <= BlockSize)
						Options = Merge(Remove(Options, 23), new CoapOptionBlock2(BlockNr, false, BlockSize));
					else
					{
						Options = Merge(Remove(Options, 23), new CoapOptionBlock2(BlockNr, true, BlockSize));
						NrLeft = BlockSize;
					}
				}
				else    // Request
				{
					if (NrLeft <= BlockSize)
						Options = Merge(Remove(Options, 27), new CoapOptionBlock1(BlockNr, false, BlockSize));
					else
					{
						Options = Merge(Remove(Options, 27), new CoapOptionBlock1(BlockNr, true, BlockSize));
						NrLeft = BlockSize;
					}
				}

				Payload = new byte[NrLeft];
				Array.Copy(OrgPayload, Pos, Payload, 0, NrLeft);
			}

			Message = new Message()
			{
				endpoint = this,
				messageType = MessageType,
				messageCode = Code,
				payloadResponseStream = PayloadResponseStream,
				options = Options,
				acknowledged = MessageType == CoapMessageType.CON,
				destination = Destination,
				callback = Callback,
				state = State,
				payload = OrgPayload,
				blockNr = BlockNr,
				blockSize = BlockSize,
				credentials = Credentials,
				resource = Resource
			};

			if (!Token.HasValue)
			{
				ulong l;

				lock (this.activeTokens)
				{
					do
					{
						l = ++this.tokenMsb;
						if (l == 0)
							l = ++this.tokenMsb;

						l <<= 16;
						l |= (uint)this.gen.Next(0x10000);
						l <<= 16;
						l |= (uint)this.gen.Next(0x10000);
					}
					while (this.activeTokens.ContainsKey(l));

					Token = l;

					if (UpdateTokenTable)
						this.activeTokens[l] = Message;
				}
			}
			else if (UpdateTokenTable)
			{
				lock (this.activeTokens)
				{
					this.activeTokens[Token.Value] = Message;
				}
			}

			if (!MessageID.HasValue)
			{
				lock (this.outgoingMessages)
				{
					do
					{
						MessageID = this.msgId++;
					}
					while (this.outgoingMessages.ContainsKey(MessageID.Value));

					if (Message.acknowledged || !(Message.callback is null))
						this.outgoingMessages[MessageID.Value] = Message;
				}
			}

			Message.messageID = MessageID.Value;
			Message.token = Token.Value;

			if (Message.acknowledged)
			{
				lock (this.gen)
				{
					Message.timeoutMilliseconds = (int)Math.Round(1000 * (ACK_TIMEOUT + (ACK_RANDOM_FACTOR - 1) * this.gen.NextDouble()));
				}
			}
			else
			{
				this.lastNonMsg = Message;
				this.lastNonMsgId = Message.messageID;

				if (!(Message.callback is null))
				{
					Message.timeoutMilliseconds = 1000 * ACK_TIMEOUT;
					Message.retryCount = MAX_RETRANSMIT;
				}
			}

			Message.encoded = this.Encode(Message.messageType, Code, Message.token, MessageID.Value, Payload, Options);

			return this.SendMessage(Client, Encrypted, Message);
		}

		private async Task SendMessage(ClientBase Client, bool Encrypted, Message Message)
		{
			if (!(Client is null))
			{
				Client.BeginTransmit(Message);

				if (Message.acknowledged || !(Message.callback is null))
				{
					this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds),
						this.CheckRetry, new object[] { Client, Message });
				}
			}
			else
			{
				bool Sent = false;

				foreach (ClientBase P in this.coapOutgoing)
				{
					if (P.IsEncrypted ^ Encrypted)
						continue;

					if (!P.CanSend(Message))
						continue;

					P.BeginTransmit(Message);

					if (Message.acknowledged || !(Message.callback is null))
					{
						this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds),
							this.CheckRetry, new object[] { P, Message });
					}

					Sent = true;
				}

				if (!Sent)
					await this.Fail(Client, Message);
			}
		}

		private async Task CheckRetry(object State)
		{
			object[] P = (object[])State;
			ClientBase Client = (ClientBase)P[0];
			Message Message = (Message)P[1];
			bool Fail = false;

			if (Message.responseReceived)
				return;

			lock (this.outgoingMessages)
			{
				if (!this.outgoingMessages.TryGetValue(Message.messageID, out Message Message2) || Message != Message2)
					return;

				Message.retryCount++;
				if (Message.retryCount >= MAX_RETRANSMIT)
				{
					this.outgoingMessages.Remove(Message.messageID);
					Fail = true;
				}
			}

			if (Fail)
			{
				await this.Fail(Client, Message);
				return;
			}

			Message.timeoutMilliseconds *= 2;
			await this.SendMessage(Client, Client.IsEncrypted, Message);
		}

		private Task Request(IPEndPoint Destination, bool Encrypted, bool Acknowledged, ulong? Token,
			CoapCode Code, byte[] Payload, int BlockSize, IDtlsCredentials Credentials,
			EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			return this.Transmit(null, Destination, Encrypted, null,
				Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Token, true,
				Payload, 0, BlockSize, null, Callback, State, null, Credentials, Options);
		}

		/// <summary>
		/// Tries to get a CoAP Content Format object, that can be used to decode content.
		/// </summary>
		/// <param name="ContentFormat">Content format number.</param>
		/// <param name="Format">Content format object.</param>
		/// <returns>If a content format was found.</returns>
		public static bool TryGetContentFormat(int ContentFormat, out ICoapContentFormat Format)
		{
			return contentFormatsByCode.TryGetValue(ContentFormat, out Format);
		}

		/// <summary>
		/// Tries to get a CoAP Content Format object, that can be used to encode content.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Format">Content format object.</param>
		/// <returns>If a content format was found.</returns>
		public static bool TryGetContentFormat(string ContentType, out ICoapContentFormat Format)
		{
			return contentFormatsByContentType.TryGetValue(ContentType, out Format);
		}

		/// <summary>
		/// Tries to decode CoAP content.
		/// </summary>
		/// <param name="ContentFormat">Content format.</param>
		/// <param name="Payload">Payload.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		[Obsolete("Use DecodeAsync for better asynchronous performance.")]
		public static ContentResponse Decode(int ContentFormat, byte[] Payload, Uri BaseUri)
		{
			return DecodeAsync(ContentFormat, Payload, BaseUri).Result;
		}

		/// <summary>
		/// Tries to decode CoAP content.
		/// </summary>
		/// <param name="ContentFormat">Content format.</param>
		/// <param name="Payload">Payload.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		public static async Task<ContentResponse> DecodeAsync(int ContentFormat, byte[] Payload, Uri BaseUri)
		{
			if (contentFormatsByCode.TryGetValue(ContentFormat, out ICoapContentFormat Format))
				return await InternetContent.DecodeAsync(Format.ContentType, Payload, BaseUri);
			else
				return new ContentResponse(BinaryCodec.DefaultContentType, Payload, Payload);
		}

		/// <summary>
		/// Tries to encode CoAP content.
		/// </summary>
		/// <param name="Payload">Payload.</param>
		/// <param name="ContentFormat">Content format of encoded content.</param>
		/// <returns>Decoded object.</returns>
		[Obsolete("Use EncodeAsync for better asynchronous performance.")]
		public static byte[] Encode(object Payload, out int ContentFormat)
		{
			KeyValuePair<byte[], int> P = EncodeAsync(Payload).Result;
			ContentFormat = P.Value;
			return P.Key;
		}

		/// <summary>
		/// Tries to encode CoAP content.
		/// </summary>
		/// <param name="Payload">Payload.</param>
		/// <returns>Decoded object and Content format of encoded content..</returns>
		public static async Task<KeyValuePair<byte[], int>> EncodeAsync(object Payload)
		{
			ContentResponse P = await InternetContent.EncodeAsync(Payload, Encoding.UTF8);

			if (contentFormatsByContentType.TryGetValue(P.ContentType, out ICoapContentFormat Format))
				return new KeyValuePair<byte[], int>(P.Encoded, Format.ContentFormat);
			else
				throw new Exception("Unable to encode content of type " + P.ContentType);
		}

		private async Task<IPEndPoint> GetIPEndPoint(string Destination, int Port, EventHandlerAsync<CoapResponseEventArgs> Callback, object State)
		{
			IPAddress[] Addresses = await Dns.GetHostAddressesAsync(Destination);
			int c = Addresses.Length;

			if (c == 0)
			{
				await this.Fail(null, Callback, State, null);
				return null;
			}

			IPAddress Addr;

			if (c == 1)
				Addr = Addresses[0];
			else
			{
				lock (this.gen)
				{
					Addr = Addresses[this.gen.Next(c)];
				}
			}

			return new IPEndPoint(Addr, Port);
		}

		internal Task Fail(ClientBase Client, Message Message)
		{
			lock (this.outgoingMessages)
			{
				if (this.outgoingMessages.TryGetValue(Message.messageID, out Message Msg) && Msg == Message)
					this.outgoingMessages.Remove(Message.messageID);
			}

			lock (this.activeTokens)
			{
				if (this.activeTokens.TryGetValue(Message.token, out Message Msg) && Msg == Message)
					this.activeTokens.Remove(Message.token);
			}

			return this.Fail(Client, Message.callback, Message.state, Message.resource);
		}

		internal async Task Fail(ClientBase Client, EventHandlerAsync<CoapResponseEventArgs> Callback, object State, CoapResource Resource)
		{
			await Callback.Raise(this, new CoapResponseEventArgs(Client, this, false, State, null, Resource));
		}

		private static CoapOption[] GetQueryOptions(Uri Uri, out int Port, out bool Encrypted, params CoapOption[] Options)
		{
			switch (Uri.Scheme.ToLower())
			{
				case "coap":
					Encrypted = false;
					break;

				case "coaps":
					Encrypted = true;
					break;

				default:
					throw new ArgumentException("Invalid URI scheme.", nameof(Uri));
			}

			List<CoapOption> Options2 = new List<CoapOption>();
			int i;

			foreach (CoapOption Option in Options)
			{
				i = Option.OptionNumber;
				if (i == 15 || i == 7 || i == 11 || i == 3)
					throw new ArgumentException("Conflicting CoAP options.", nameof(Options));

				Options2.Add(Option);
			}

			Port = Uri.Port;
			if (Port < 0)
			{
				if (Encrypted)
					Port = DefaultCoapsPort;
				else
					Port = DefaultCoapPort;
			}

			Options2.Add(new CoapOptionUriHost(Uri.Host));
			Options2.Add(new CoapOptionUriPort((ulong)Port));

			string s = Uri.AbsolutePath;
			if (s.StartsWith("/"))
				s = s.Substring(1);

			if (s.EndsWith("/"))
				s = s.Substring(0, s.Length - 1);

			if (!string.IsNullOrEmpty(s))
			{
				foreach (string Segment in s.Split('/'))
					Options2.Add(new CoapOptionUriPath(Segment));
			}

			s = Uri.Query;

			if (s.StartsWith("?"))
				s = s.Substring(1);

			if (!string.IsNullOrEmpty(s))
			{
				foreach (string Pair in s.Split('&'))
					Options2.Add(new CoapOptionUriQuery(Pair));
			}

			return Options2.ToArray();
		}

		internal static string GetUri(string Host, int? Port, string Path, Dictionary<string, string> UriQuery)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("coap://");

			if (!string.IsNullOrEmpty(Host))
				sb.Append(Host);

			if (Port.HasValue)
			{
				sb.Append(':');
				sb.Append(Port.Value);
			}

			if (!string.IsNullOrEmpty(Path))
				sb.Append(Path);
			else
				sb.Append("/");

			if (!(UriQuery is null))
			{
				bool First = true;

				foreach (KeyValuePair<string, string> P in UriQuery)
				{
					if (First)
					{
						sb.Append('?');
						First = false;
					}
					else
						sb.Append('&');

					sb.Append(P.Key);
					sb.Append('=');
					sb.Append(P.Value);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Gets an array of active tokens.
		/// </summary>
		/// <returns>Array of active tokens.</returns>
		public ulong[] GetActiveTokens()
		{
			ulong[] Result;

			lock (this.activeTokens)
			{
				Result = new ulong[this.activeTokens.Count];
				this.activeTokens.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Gets an array of active message IDs.
		/// </summary>
		/// <returns>Array of active message IDs.</returns>
		public ushort[] GetActiveMessageIDs()
		{
			ushort[] Result;

			lock (this.outgoingMessages)
			{
				Result = new ushort[this.outgoingMessages.Count];
				this.outgoingMessages.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Gets an array of registered resources.
		/// </summary>
		/// <returns>Registered resources.</returns>
		public CoapResource[] GetRegisteredResources()
		{
			CoapResource[] Result;

			lock (this.resources)
			{
				Result = new CoapResource[this.resources.Count];
				this.resources.Values.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void GET(IPEndPoint Destination, bool Encrypted, bool Acknowledged,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			this.Request(Destination, Encrypted, Acknowledged, null, CoapCode.GET, null, 64,
				Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task GET(string Destination, int Port, bool Encrypted, bool Acknowledged,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (!(EndPoint is null))
				this.GET(EndPoint, Encrypted, Acknowledged, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task GET(string Uri, bool Acknowledged, IDtlsCredentials Credentials,
			EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			return this.GET(new Uri(Uri), Acknowledged, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task GET(Uri Uri, bool Acknowledged, IDtlsCredentials Credentials,
			EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, out bool Encrypted, Options);

			if (IPAddress.TryParse(Uri.Host, out IPAddress Addr))
			{
				this.GET(new IPEndPoint(Addr, Port), Encrypted, Acknowledged, Credentials,
					  Callback, State, Options2);
			}
			else
			{
				await this.GET(Uri.Host, Port, Encrypted, Acknowledged, Credentials,
					  Callback, State, Options2);
			}
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="CoapEndpoint.UnregisterObservation(IPEndPoint, bool, bool, ulong, IDtlsCredentials, EventHandlerAsync{CoapResponseEventArgs}, object, CoapOption[])"/> 
		/// to cancel an active observation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void Observe(IPEndPoint Destination, bool Encrypted, bool Acknowledged,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			this.Request(Destination, Encrypted, Acknowledged, null, CoapCode.GET, null, 64,
				Credentials, Callback, State, Merge(Options, new CoapOptionObserve(0)));
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="CoapEndpoint.UnregisterObservation(string, int, bool, bool, ulong, IDtlsCredentials, EventHandlerAsync{CoapResponseEventArgs}, object, CoapOption[])"/> 
		/// to cancel an active observation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task Observe(string Destination, int Port, bool Encrypted, bool Acknowledged,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (!(EndPoint is null))
				this.Observe(EndPoint, Encrypted, Acknowledged, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="CoapEndpoint.UnregisterObservation(string, bool, ulong, IDtlsCredentials, EventHandlerAsync{CoapResponseEventArgs}, object, CoapOption[])"/> 
		/// to cancel an active observation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task Observe(string Uri, bool Acknowledged, IDtlsCredentials Credentials,
			EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			return this.Observe(new Uri(Uri), Acknowledged, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="CoapEndpoint.UnregisterObservation(Uri, bool, ulong, IDtlsCredentials, EventHandlerAsync{CoapResponseEventArgs}, object, CoapOption[])"/> 
		/// to cancel an active observation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task Observe(Uri Uri, bool Acknowledged, IDtlsCredentials Credentials,
			EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, out bool Encrypted, Options);

			if (IPAddress.TryParse(Uri.Host, out IPAddress Addr))
				this.Observe(new IPEndPoint(Addr, Port), Encrypted, Acknowledged, Credentials, Callback, State, Options2);
			else
				await this.Observe(Uri.Host, Port, Encrypted, Acknowledged, Credentials, Callback, State, Options2);
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(IPEndPoint, bool, bool, IDtlsCredentials, EventHandlerAsync{CoapResponseEventArgs}, object, CoapOption[])"/>.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Token">Registration token.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void UnregisterObservation(IPEndPoint Destination, bool Encrypted, bool Acknowledged, ulong Token,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback,
			object State, params CoapOption[] Options)
		{
			this.Request(Destination, Encrypted, Acknowledged, Token, CoapCode.GET, null, 64,
				Credentials, Callback, State, Merge(Options, new CoapOptionObserve(1)));
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(string, bool, IDtlsCredentials, EventHandlerAsync{CoapResponseEventArgs}, object, CoapOption[])"/>.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Token">Registration token.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task UnregisterObservation(string Destination, int Port, bool Encrypted,
			bool Acknowledged, ulong Token, IDtlsCredentials Credentials,
			EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (!(EndPoint is null))
			{
				this.UnregisterObservation(EndPoint, Encrypted, Acknowledged, Token,
					  Credentials, Callback, State, Options);
			}
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(string, int, bool, bool, IDtlsCredentials, EventHandlerAsync{CoapResponseEventArgs}, object, CoapOption[])"/>
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Token">Registration token.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task UnregisterObservation(string Uri, bool Acknowledged, ulong Token,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			return this.UnregisterObservation(new Uri(Uri), Acknowledged, Token, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(Uri, bool, IDtlsCredentials, EventHandlerAsync{CoapResponseEventArgs}, object, CoapOption[])"/>
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Token">Registration token.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task UnregisterObservation(Uri Uri, bool Acknowledged, ulong Token,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, out bool Encrypted, Options);

			if (IPAddress.TryParse(Uri.Host, out IPAddress Addr))
			{
				this.UnregisterObservation(new IPEndPoint(Addr, Port), Encrypted, Acknowledged, Token,
					  Credentials, Callback, State, Options2);
			}
			else
			{
				await this.UnregisterObservation(Uri.Host, Port, Encrypted, Acknowledged, Token,
					  Credentials, Callback, State, Options2);
			}
		}

		/// <summary>
		/// Performs a POST operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void POST(IPEndPoint Destination, bool Encrypted, bool Acknowledged, byte[] Payload, int BlockSize,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			this.Request(Destination, Encrypted, Acknowledged, null, CoapCode.POST, Payload,
				BlockSize, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a POST operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task POST(string Destination, int Port, bool Encrypted, bool Acknowledged, byte[] Payload, int BlockSize,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (!(EndPoint is null))
			{
				this.POST(EndPoint, Encrypted, Acknowledged, Payload, BlockSize, Credentials,
					  Callback, State, Options);
			}
		}

		/// <summary>
		/// Performs a POST operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task POST(string Uri, bool Acknowledged, byte[] Payload, int BlockSize,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			return this.POST(new Uri(Uri), Acknowledged, Payload, BlockSize,
				Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a POST operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task POST(Uri Uri, bool Acknowledged, byte[] Payload, int BlockSize,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, out bool Encrypted, Options);

			if (IPAddress.TryParse(Uri.Host, out IPAddress Addr))
				this.POST(new IPEndPoint(Addr, Port), Encrypted, Acknowledged, Payload, BlockSize, Credentials, Callback, State, Options2);
			else
				await this.POST(Uri.Host, Port, Encrypted, Acknowledged, Payload, BlockSize, Credentials, Callback, State, Options2);
		}

		/// <summary>
		/// Performs a PUT operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void PUT(IPEndPoint Destination, bool Encrypted, bool Acknowledged, byte[] Payload, int BlockSize,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			this.Request(Destination, Encrypted, Acknowledged, null, CoapCode.PUT, Payload,
				BlockSize, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a PUT operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task PUT(string Destination, bool Encrypted, int Port, bool Acknowledged, byte[] Payload, int BlockSize,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (!(EndPoint is null))
			{
				this.PUT(EndPoint, Encrypted, Acknowledged, Payload, BlockSize, Credentials,
					  Callback, State, Options);
			}
		}

		/// <summary>
		/// Performs a PUT operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task PUT(string Uri, bool Acknowledged, byte[] Payload, int BlockSize,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			return this.PUT(new Uri(Uri), Acknowledged, Payload, BlockSize, Credentials,
				Callback, State, Options);
		}

		/// <summary>
		/// Performs a PUT operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task PUT(Uri Uri, bool Acknowledged, byte[] Payload, int BlockSize,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, out bool Encrypted, Options);

			if (IPAddress.TryParse(Uri.Host, out IPAddress Addr))
			{
				this.PUT(new IPEndPoint(Addr, Port), Encrypted, Acknowledged, Payload, BlockSize,
					  Credentials, Callback, State, Options2);
			}
			else
			{
				await this.PUT(Uri.Host, Encrypted, Port, Acknowledged, Payload, BlockSize,
					  Credentials, Callback, State, Options2);
			}
		}

		/// <summary>
		/// Performs a DELETE operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void DELETE(IPEndPoint Destination, bool Encrypted, bool Acknowledged,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			this.Request(Destination, Encrypted, Acknowledged, null, CoapCode.DELETE, null, 64,
				Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a DELETE operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Encrypted">If encrypted message service (DTLS) is to be used.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task DELETE(string Destination, int Port, bool Encrypted, bool Acknowledged,
			IDtlsCredentials Credentials, EventHandlerAsync<CoapResponseEventArgs> Callback, object State,
			params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (!(EndPoint is null))
				this.DELETE(EndPoint, Encrypted, Acknowledged, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a DELETE operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task DELETE(string Uri, bool Acknowledged, IDtlsCredentials Credentials,
			EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			return this.DELETE(new Uri(Uri), Acknowledged, Credentials, Callback, State, Options);
		}

		/// <summary>
		/// Performs a DELETE operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task DELETE(Uri Uri, bool Acknowledged, IDtlsCredentials Credentials,
			EventHandlerAsync<CoapResponseEventArgs> Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, out bool Encrypted, Options);

			if (IPAddress.TryParse(Uri.Host, out IPAddress Addr))
				this.DELETE(new IPEndPoint(Addr, Port), Encrypted, Acknowledged, Credentials, Callback, State, Options2);
			else
				await this.DELETE(Uri.Host, Port, Encrypted, Acknowledged, Credentials, Callback, State, Options2);
		}

		/// <summary>
		/// Schedules a one-time event.
		/// </summary>
		/// <param name="Callback">Method to call when event is due.</param>
		/// <param name="When">When the event is to be executed.</param>
		/// <param name="State">State object</param>
		/// <returns>Timepoint of when event was scheduled.</returns>
		public DateTime ScheduleEvent(Func<object, Task> Callback, DateTime When, object State)
		{
			if (!(this.scheduler is null))
				return this.scheduler.Add(When, Callback, State);
			else
				return DateTime.MinValue;
		}

		/// <summary>
		/// Cancels a scheduled event.
		/// </summary>
		/// <param name="When">When event is scheduled</param>
		/// <returns>If event was found and removed.</returns>
		public bool CancelScheduledEvent(DateTime When)
		{
			return this.scheduler?.Remove(When) ?? false;
		}

	}
}
