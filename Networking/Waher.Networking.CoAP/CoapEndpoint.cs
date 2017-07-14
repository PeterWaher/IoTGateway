using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Runtime.Timing;
using Waher.Runtime.Inventory;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP client. CoAP is defined in RFC7252:
	/// https://tools.ietf.org/html/rfc7252
	/// </summary>
	public class CoapEndpoint : Sniffable, IDisposable
	{
		/// <summary>
		/// Default CoAP port = 5683
		/// </summary>
		public const int DefaultCoapPort = 5683;

		/// <summary>
		/// DEfault CoAP over DTLS port = 5684
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

		private LinkedList<Message> outputQueue = new LinkedList<Message>();
		private Dictionary<ushort, Message> outgoingMessages = new Dictionary<ushort, Message>();
		private Dictionary<ulong, Message> activeTokens = new Dictionary<ulong, Message>();
		private Dictionary<string, CoapResource> resources = new Dictionary<string, CoapResource>(StringComparer.CurrentCultureIgnoreCase);
		private Random gen = new Random();
		private Scheduler scheduler;
		private LinkedList<Tuple<UdpClient, IPEndPoint, bool>> coapOutgoing = new LinkedList<Tuple<UdpClient, IPEndPoint, bool>>();
		private LinkedList<UdpClient> coapIncoming = new LinkedList<UdpClient>();
		private Cache<string, ResponseCacheRec> blockedResponses = new Cache<string, ResponseCacheRec>(int.MaxValue, TimeSpan.MaxValue, new TimeSpan(0, 1, 0));
		private ushort msgId = 0;
		private uint tokenMsb = 0;
		private bool isWriting = false;
		private bool disposed = false;

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
				if (T.GetTypeInfo().IsAbstract)
					continue;

				try
				{
					Option = (CoapOption)Activator.CreateInstance(T);
					if (Options.ContainsKey(Option.OptionNumber))
						throw new Exception("Option number " + Option.OptionNumber + " already defined.");

					Options[Option.OptionNumber] = Option;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			optionTypes = Options;

			Dictionary<int, ICoapContentFormat> ByCode = new Dictionary<int, ICoapContentFormat>();
			Dictionary<string, ICoapContentFormat> ByType = new Dictionary<string, ICoapContentFormat>();
			ICoapContentFormat ContentFormat;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICoapContentFormat)))
			{
				if (T.GetTypeInfo().IsAbstract)
					continue;

				try
				{
					ContentFormat = (ICoapContentFormat)Activator.CreateInstance(T);
					if (ByCode.ContainsKey(ContentFormat.ContentFormat))
						throw new Exception("Content format number " + ContentFormat.ContentFormat + " already defined.");

					ByCode[ContentFormat.ContentFormat] = ContentFormat;
					ByType[ContentFormat.ContentType] = ContentFormat;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
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
			: this(DefaultCoapPort, false, false)
		{
		}

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		/// <param name="Port">Port number to listen for incoming traffic.</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public CoapEndpoint(int Port, params ISniffer[] Sniffers)
			: this(Port, false, false, Sniffers)
		{
		}

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		/// <param name="Port">Port number to listen for incoming traffic.</param>
		/// <param name="LoopbackTransmission">If transmission on the loopback interface should be permitted.</param>
		/// <param name="LoopbackReception">If reception on the loopback interface should be permitted.</param>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public CoapEndpoint(int Port, bool LoopbackTransmission, bool LoopbackReception, params ISniffer[] Sniffers)
		: base(Sniffers)
		{
			NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
			UdpClient Outgoing;
			UdpClient Incoming;

			foreach (NetworkInterface Interface in Interfaces)
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
					AddressFamily AddressFamily = UnicastAddress.Address.AddressFamily;
					bool IsLoopback = Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback;

					if (AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4)
						MulticastAddress = IPAddress.Parse("224.0.1.187");
					else if (AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6)
						MulticastAddress = IPAddress.Parse("[FF02::FD]");
					else
						continue;

					if (!IsLoopback || LoopbackTransmission)
					{
						try
						{
							Outgoing = new UdpClient(AddressFamily)
							{
								MulticastLoopback = false
							};

							if (AddressFamily == AddressFamily.InterNetwork)
							{
								Outgoing.DontFragment = true;
								Outgoing.MulticastLoopback = false;
							}
						}
						catch (Exception)
						{
							continue;
						}

						Outgoing.EnableBroadcast = true;
						Outgoing.MulticastLoopback = false;
						Outgoing.Ttl = 30;
						Outgoing.Client.Bind(new IPEndPoint(UnicastAddress.Address, 0));
						Outgoing.JoinMulticastGroup(MulticastAddress);

						IPEndPoint EP = new IPEndPoint(MulticastAddress, Port);
						this.coapOutgoing.AddLast(new Tuple<UdpClient, IPEndPoint, bool>(Outgoing, EP, IsLoopback));

						this.BeginReceive(Outgoing);
					}

					if (!IsLoopback || LoopbackReception)
					{
						try
						{
							Incoming = new UdpClient(AddressFamily)
							{
								ExclusiveAddressUse = false
							};

							Incoming.Client.Bind(new IPEndPoint(UnicastAddress.Address, Port));
							this.BeginReceive(Incoming);

							this.coapIncoming.AddLast(Incoming);
						}
						catch (Exception)
						{
							Incoming = null;
						}

						try
						{
							Incoming = new UdpClient(Port, AddressFamily)
							{
								MulticastLoopback = false
							};

							Incoming.JoinMulticastGroup(MulticastAddress);
							this.BeginReceive(Incoming);

							this.coapIncoming.AddLast(Incoming);
						}
						catch (Exception)
						{
							Incoming = null;
						}
					}
				}
			}

			this.scheduler = new Scheduler();

			this.Register(new CoRE.CoreResource(this));
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;

			if (this.scheduler != null)
			{
				this.scheduler.Dispose();
				this.scheduler = null;
			}

			foreach (Tuple<UdpClient, IPEndPoint, bool> P in this.coapOutgoing)
			{
				try
				{
					P.Item1.Dispose();
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			this.coapOutgoing.Clear();

			foreach (UdpClient Client in this.coapIncoming)
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

		private async void BeginReceive(UdpClient Client)
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await Client.ReceiveAsync();
					if (this.disposed)
						return;

					this.ReceiveBinary(Data.Buffer);

					try
					{
						this.Decode(Client, Data.Buffer, Data.RemoteEndPoint);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
			}
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			Init();
		}

		private void Decode(UdpClient Client, byte[] Packet, IPEndPoint From)
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
							this.Error(ex.Message);

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
						if (IncomingMessage.LocationPath == null)
							IncomingMessage.LocationPath = "/" + ((CoapOptionLocationPath)Option).Value;
						else
							IncomingMessage.LocationPath = IncomingMessage.Path + "/" + ((CoapOptionLocationPath)Option).Value;
						break;

					case 11:
						if (IncomingMessage.Path == null)
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
						if (IncomingMessage.UriQuery == null)
							IncomingMessage.UriQuery = new Dictionary<string, string>();

						CoapOptionKeyValue Query = (CoapOptionKeyValue)Option;

						IncomingMessage.UriQuery[Query.Key] = Query.KeyValue;
						break;

					case 17:
						IncomingMessage.Accept = ((CoapOptionAccept)Option).Value;
						break;

					case 20:
						if (IncomingMessage.LocationQuery == null)
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

				if (OutgoingMessage != null)
				{
					if (OutgoingMessage.token == Token)
					{
						if (Code == CoapCode.Continue && IncomingMessage.Block1 != null)
						{
							if (IncomingMessage.Block1.Size < OutgoingMessage.blockSize)
							{
								OutgoingMessage.blockNr *= (OutgoingMessage.blockSize / IncomingMessage.Block1.Size);
								OutgoingMessage.blockSize = IncomingMessage.Block1.Size;
							}

							OutgoingMessage.blockNr++;
							Pos = OutgoingMessage.blockNr * OutgoingMessage.blockSize;

							if (Pos >= OutgoingMessage.payload.Length)
								this.Fail(Client, OutgoingMessage.callback, OutgoingMessage.state);
							else
							{
								this.Transmit(Client, OutgoingMessage.destination, null, OutgoingMessage.messageType, OutgoingMessage.messageCode,
									OutgoingMessage.token, true, OutgoingMessage.payload, OutgoingMessage.blockNr, OutgoingMessage.blockSize,
									OutgoingMessage.callback, OutgoingMessage.state, OutgoingMessage.payloadResponseStream, OutgoingMessage.options);
							}
						}
						else
						{
							if (IncomingMessage.Block2 != null)
							{
								OutgoingMessage.options = Remove(OutgoingMessage.options, 27);     // Remove Block1 option, if available.

								IncomingMessage.Payload = OutgoingMessage.BlockReceived(Client, IncomingMessage);
								if (IncomingMessage.Payload == null)
									OutgoingMessage = null;
							}

							if (OutgoingMessage != null)
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

								if (OutgoingMessage2 != null)
								{
									IncomingMessage.BaseUri = new Uri(OutgoingMessage.GetUri());
									OutgoingMessage.ResponseReceived(Client, IncomingMessage);
								}
								else
									this.Fail(Client, OutgoingMessage.callback, OutgoingMessage.state);
							}
						}
					}
					else if (Code != CoapCode.EmptyMessage || Type != CoapMessageType.ACK)
						this.Fail(Client, OutgoingMessage.callback, OutgoingMessage.state);
				}
			}
			else
			{
				lock (this.activeTokens)
				{
					if (!this.activeTokens.TryGetValue(Token, out OutgoingMessage))
						OutgoingMessage = null;
				}

				if (OutgoingMessage != null)
				{
					OutgoingMessage.responseReceived = true;

					if (IncomingMessage.Type == CoapMessageType.CON)
					{
						this.Transmit(Client, From, IncomingMessage.MessageId, CoapMessageType.ACK, CoapCode.EmptyMessage, Token,
							false, null, 0, 64, null, null, null);
					}

					if (IncomingMessage.Block2 != null)
					{
						IncomingMessage.Payload = OutgoingMessage.BlockReceived(Client, IncomingMessage);
						if (IncomingMessage.Payload != null)
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

							OutgoingMessage.ResponseReceived(Client, IncomingMessage);
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

						OutgoingMessage.ResponseReceived(Client, IncomingMessage);
					}
				}
				else
				{
					// TODO: Blocked messages (Block1)
					// TODO: RST to observation notifications that have been unregistered.

					CoapResource Resource = null;
					string Path = IncomingMessage.Path ?? "/";
					string SubPath = string.Empty;
					int i;

					lock (this.resources)
					{
						while (!string.IsNullOrEmpty(Path))
						{
							if (this.resources.TryGetValue(Path, out Resource))
							{
								if (Resource.HandlesSubPaths || string.IsNullOrEmpty(SubPath))
									break;
								else
									Resource = null;
							}

							i = Path.LastIndexOf('/');
							if (i < 0)
								break;

							if (string.IsNullOrEmpty(SubPath))
								SubPath = Path.Substring(i + 1);
							else
								SubPath = Path.Substring(i + 1) + "/" + SubPath;

							Path = Path.Substring(0, i);
						}
					}

					if (Resource != null)
					{
						CoapResponse Response = new CoapResponse(Client, this, IncomingMessage.From, IncomingMessage);
						string Key = From.Address.ToString() + " " + Token.ToString();

						if (this.blockedResponses.TryGetValue(Key, out ResponseCacheRec CachedResponse))
						{
							int Block2Nr = (int)IncomingMessage.Block2?.Number;

							Response.Respond(CoapCode.Content, CachedResponse.Payload, Block2Nr,
								CachedResponse.BlockSize, CachedResponse.Options);
						}
						else
						{
							try
							{
								switch (IncomingMessage.Code)
								{
									case CoapCode.GET:
										if (Resource is ICoapGetMethod GetMethod && GetMethod.AllowsGET)
											GetMethod.GET(IncomingMessage, Response);
										else if (Type == CoapMessageType.CON)
											Response.RST(CoapCode.MethodNotAllowed);
										break;

									case CoapCode.POST:
										if (Resource is ICoapPostMethod PostMethod && PostMethod.AllowsPOST)
											PostMethod.POST(IncomingMessage, Response);
										else if (Type == CoapMessageType.CON)
											Response.RST(CoapCode.MethodNotAllowed);
										break;

									case CoapCode.PUT:
										if (Resource is ICoapPutMethod PutMethod && PutMethod.AllowsPUT)
											PutMethod.PUT(IncomingMessage, Response);
										else if (Type == CoapMessageType.CON)
											Response.RST(CoapCode.MethodNotAllowed);
										break;

									case CoapCode.DELETE:
										if (Resource is ICoapDeleteMethod DeleteMethod && DeleteMethod.AllowsDELETE)
											DeleteMethod.DELETE(IncomingMessage, Response);
										else if (Type == CoapMessageType.CON)
											Response.RST(CoapCode.MethodNotAllowed);
										break;

									case CoapCode.FETCH:
										if (Resource is ICoapFetchMethod FetchMethod && FetchMethod.AllowsFETCH)
											FetchMethod.FETCH(IncomingMessage, Response);
										else if (Type == CoapMessageType.CON)
											Response.RST(CoapCode.MethodNotAllowed);
										break;

									case CoapCode.PATCH:
										if (Resource is ICoapPatchMethod PatchMethod && PatchMethod.AllowsPATCH)
											PatchMethod.PATCH(IncomingMessage, Response);
										else if (Type == CoapMessageType.CON)
											Response.RST(CoapCode.MethodNotAllowed);
										break;

									case CoapCode.iPATCH:
										if (Resource is ICoapIPatchMethod IPatchMethod && IPatchMethod.AllowsiPATCH)
											IPatchMethod.iPATCH(IncomingMessage, Response);
										else if (Type == CoapMessageType.CON)
											Response.RST(CoapCode.MethodNotAllowed);
										break;

									default:
										if (Type == CoapMessageType.CON)
											Response.RST(CoapCode.MethodNotAllowed);
										break;
								}
							}
							catch (CoapException ex)
							{
								if (Type == CoapMessageType.CON && !Response.Responded)
									Response.RST(ex.ErrorCode);
							}
							catch (Exception ex)
							{
								Log.Critical(ex);

								if (Type == CoapMessageType.CON && !Response.Responded)
									Response.RST(CoapCode.InternalServerError);
							}

							if (Type == CoapMessageType.CON && !Response.Responded)
								Response.ACK();
						}
					}
					else
					{
						CoapMessageEventHandler h = this.OnIncomingRequest;

						if (h != null)
						{
							CoapMessageEventArgs e = new CoapMessageEventArgs(Client, this, IncomingMessage);
							try
							{
								h(this, e);
							}
							catch (CoapException ex)
							{
								if (Type == CoapMessageType.CON && !e.Responded)
									e.RST(ex.ErrorCode);
							}
							catch (Exception ex)
							{
								Log.Critical(ex);

								if (Type == CoapMessageType.CON && !e.Responded)
									e.RST(CoapCode.InternalServerError);
							}

							if (!e.Responded && Type == CoapMessageType.CON)
								e.ACK();
						}
						else if (Type == CoapMessageType.CON)
							this.Transmit(Client, From, MessageId, CoapMessageType.RST, CoapCode.EmptyMessage, Token, false, null, 0, 64, null, null, null);
					}
				}
			}
		}

		/// <summary>
		/// Registers a CoAP resource on the endpoint.
		/// </summary>
		/// <param name="Resource">Resource to register.</param>
		public void Register(CoapResource Resource)
		{
			lock (this.resources)
			{
				if (!this.resources.ContainsKey(Resource.Path))
					this.resources[Resource.Path] = Resource;
				else
					throw new Exception("Path already registered.");
			}
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		public void Register(string Path, CoapMethodHandler GET)
		{
			this.Register(Path, GET, false, false, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		public void Register(string Path, CoapMethodHandler GET, bool HandlesSubPaths)
		{
			this.Register(Path, GET, HandlesSubPaths, false, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		public void Register(string Path, CoapMethodHandler GET, bool HandlesSubPaths,
			bool Observable)
		{
			this.Register(Path, GET, HandlesSubPaths, Observable, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		public void Register(string Path, CoapMethodHandler GET, bool HandlesSubPaths,
			bool Observable, string Title)
		{
			this.Register(Path, GET, HandlesSubPaths, Observable, Title, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		public void Register(string Path, CoapMethodHandler GET, bool HandlesSubPaths,
			bool Observable, string Title, string[] ResourceTypes)
		{
			this.Register(Path, GET, HandlesSubPaths, Observable, Title, ResourceTypes, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		public void Register(string Path, CoapMethodHandler GET, bool HandlesSubPaths,
			bool Observable, string Title, string[] ResourceTypes, string[] InterfaceDescriptions)
		{
			this.Register(Path, GET, HandlesSubPaths, Observable, Title, ResourceTypes,
				InterfaceDescriptions, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		public void Register(string Path, CoapMethodHandler GET, bool HandlesSubPaths,
			bool Observable, string Title, string[] ResourceTypes, string[] InterfaceDescriptions,
			int[] ContentFormats)
		{
			this.Register(Path, GET, HandlesSubPaths, Observable, Title, ResourceTypes,
				InterfaceDescriptions, ContentFormats, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <param name="MaximumSizeEstimate">Optional maximum size estimate of resource. Can be null.</param>
		public void Register(string Path, CoapMethodHandler GET, bool HandlesSubPaths,
			bool Observable, string Title, string[] ResourceTypes, string[] InterfaceDescriptions,
			int[] ContentFormats, int? MaximumSizeEstimate)
		{
			this.Register(new CoapGetDelegateResource(Path, GET, HandlesSubPaths, Observable,
				Title, ResourceTypes, InterfaceDescriptions, ContentFormats, MaximumSizeEstimate));
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		public void Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST)
		{
			this.Register(Path, GET, POST, false, false, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		public void Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			bool HandlesSubPaths)
		{
			this.Register(Path, GET, POST, HandlesSubPaths, false, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		public void Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			bool HandlesSubPaths, bool Observable)
		{
			this.Register(Path, GET, POST, HandlesSubPaths, Observable, null, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		public void Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			bool HandlesSubPaths, bool Observable, string Title)
		{
			this.Register(Path, GET, POST, HandlesSubPaths, Observable, Title, null, null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		public void Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			bool HandlesSubPaths, bool Observable, string Title, string[] ResourceTypes)
		{
			this.Register(Path, GET, POST, HandlesSubPaths, Observable, Title, ResourceTypes,
				null, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		public void Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			bool HandlesSubPaths, bool Observable, string Title, string[] ResourceTypes,
			string[] InterfaceDescriptions)
		{
			this.Register(Path, GET, POST, HandlesSubPaths, Observable, Title, ResourceTypes,
				InterfaceDescriptions, null, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		public void Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			bool HandlesSubPaths, bool Observable, string Title, string[] ResourceTypes,
			string[] InterfaceDescriptions, int[] ContentFormats)
		{
			this.Register(Path, GET, POST, HandlesSubPaths, Observable, Title, ResourceTypes, 
				InterfaceDescriptions, ContentFormats, null);
		}

		/// <summary>
		/// Registers a CoAP resource serving the GET method on the endpoint.
		/// </summary>
		/// <param name="Path">Path to resource.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">POST method handler.</param>
		/// <param name="HandlesSubPaths">If the resource supports sub-paths.</param>
		/// <param name="Observable">If the resource is observable.</param>
		/// <param name="Title">Optional CoRE title. Can be null.</param>
		/// <param name="ResourceTypes">Optional set of CoRE resource types. Can be null or empty.</param>
		/// <param name="InterfaceDescriptions">Optional set of CoRE interface descriptions. Can be null or empty.</param>
		/// <param name="ContentFormats">Optional set of content format representations supported by the resource. Can be null or empty.</param>
		/// <param name="MaximumSizeEstimate">Optional maximum size estimate of resource. Can be null.</param>
		public void Register(string Path, CoapMethodHandler GET, CoapMethodHandler POST,
			bool HandlesSubPaths, bool Observable, string Title, string[] ResourceTypes,
			string[] InterfaceDescriptions, int[] ContentFormats, int? MaximumSizeEstimate)
		{
			this.Register(new CoapGetPostDelegateResource(Path, GET, POST, HandlesSubPaths,
				Observable, Title, ResourceTypes, InterfaceDescriptions, ContentFormats, MaximumSizeEstimate));
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
			if (Resource == null)
				return false;

			lock (this.resources)
			{
				if (this.resources.TryGetValue(Resource.Path, out CoapResource Resource2) && Resource2 == Resource)
				{
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
		public event CoapMessageEventHandler OnIncomingRequest = null;

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

			if (Options != null && Options.Length > 0)
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
					Length = Value == null ? 0 : Value.Length;

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

					if (Value != null)
						ms.Write(Value, 0, Value.Length);
				}
			}

			if (Payload != null && Payload.Length > 0)
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
			if (Options == null)
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
					if (Result == null)
					{
						Result = new List<CoapOption>();
						for (j = 0; j < i; j++)
							Result.Add(Options[j]);
					}
				}
				else if (Result != null)
					Result.Add(Option);
			}

			if (Result == null)
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
			if (Options1.Length == 0)
				return Options2;
			else if (Options2.Length == 0)
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

		internal Task Transmit(UdpClient Client, IPEndPoint Destination, ushort? MessageID, CoapMessageType MessageType, CoapCode Code, ulong? Token,
			bool UpdateTokenTable, byte[] Payload, int BlockNr, int BlockSize, CoapResponseEventHandler Callback, object State,
			MemoryStream PayloadResponseStream, params CoapOption[] Options)
		{
			Message Message;
			byte[] OrgPayload = Payload;

			if (!IsBlockSizeValid(BlockSize))
				throw new ArgumentException("Invalid block size.", "BlockSize");

			if (BlockNr * BlockSize > (Payload == null ? 0 : Payload.Length))
				throw new ArgumentException("Invalid block number.", "BlockNr");

			if (Payload != null && Payload.Length > BlockSize)
			{
				int Pos = BlockNr * BlockSize;
				int NrLeft = Payload.Length - Pos;

				if ((int)Code >= 64 || Code == CoapCode.EmptyMessage)	// Response
				{
					string Key = Destination.Address.ToString() + " " + Token.ToString();

					if (!this.blockedResponses.ContainsKey(Key))
					{
						this.blockedResponses.Add(Key, new ResponseCacheRec()
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
				else	// Request
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
				blockSize = BlockSize
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
				lock (outgoingMessages)
				{
					do
					{
						MessageID = this.msgId++;
					}
					while (this.outgoingMessages.ContainsKey(MessageID.Value));

					if (Message.acknowledged || Message.callback != null)
						this.outgoingMessages[MessageID.Value] = Message;
				}
			}

			Message.messageID = MessageID.Value;
			Message.token = Token.Value;

			if (Message.acknowledged)
			{
				lock (this.gen)
				{
					Message.timeoutMilliseconds = (int)Math.Round(1000 * (ACK_TIMEOUT + (ACK_RANDOM_FACTOR - 1) * gen.NextDouble()));
				}
			}
			else if (Message.callback != null)
			{
				Message.timeoutMilliseconds = 1000 * ACK_TIMEOUT;
				Message.retryCount = MAX_RETRANSMIT;
			}

			Message.encoded = this.Encode(Message.messageType, Code, Message.token, MessageID.Value, Payload, Options);

			return this.SendMessage(Client, Message);
		}

		private async Task SendMessage(UdpClient Client, Message Message)
		{
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

			this.TransmitBinary(Message.encoded);

			bool Sent = false;

			if (Client != null)
			{
				await this.BeginTransmit(Client, Message);

				if (Message.acknowledged || Message.callback != null)
					this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds), this.CheckRetry, new object[] { Client, Message });

				Sent = true;
			}
			else
			{
				foreach (Tuple<UdpClient, IPEndPoint, bool> P in this.coapOutgoing)
				{
					if (P.Item1.Client.AddressFamily != Message.destination.AddressFamily)
						continue;

					if (IPAddress.IsLoopback(Message.destination.Address) ^ P.Item3)
						continue;

					await this.BeginTransmit(P.Item1, Message);

					if (Message.acknowledged || Message.callback != null)
						this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds), this.CheckRetry, new object[] { P.Item1, Message });

					Sent = true;
				}
			}

			if (!Sent)
			{
				lock (this.outgoingMessages)
				{
					this.outgoingMessages.Remove(Message.messageID);
				}

				lock (this.activeTokens)
				{
					this.activeTokens.Remove(Message.token);
				}

				this.Fail(Client, Message.callback, Message.state);
			}
		}

		private async Task BeginTransmit(UdpClient Client, Message Message)
		{
			if (this.disposed)
				return;

			try
			{
				while (Message != null)
				{
					await Client.SendAsync(Message.encoded, Message.encoded.Length, Message.destination);
					if (this.disposed)
						return;

					if (Message.acknowledged || Message.callback != null)
						this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds), this.CheckRetry, new object[] { Client, Message });

					lock (this.outputQueue)
					{
						if (this.outputQueue.First == null)
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
				this.Error(ex.Message);
			}
		}

		private class Message
		{
			public CoapResponseEventHandler callback;
			public CoapEndpoint endpoint;
			public object state;
			public IPEndPoint destination;
			public CoapMessageType messageType;
			public CoapCode messageCode;
			public CoapOption[] options;
			public byte[] payload;
			public int blockSize;
			public int blockNr;
			public MemoryStream payloadResponseStream = null;
			public ushort messageID;
			public ulong token;
			public byte[] encoded;
			public int timeoutMilliseconds;
			public int retryCount = 0;
			public bool acknowledged;
			public bool responseReceived = false;

			internal void ResponseReceived(UdpClient Client, CoapMessage Response)
			{
				this.responseReceived = true;

				if (this.callback != null)
				{
					try
					{
						this.callback(this.endpoint, new CoapResponseEventArgs(Client, this.endpoint,
							Response.Type != CoapMessageType.RST && (int)Response.Code >= 0x40 && (int)Response.Code <= 0x5f,
							this.state, Response));
					}
					catch (Exception ex)
					{
						this.endpoint.Error(ex.Message);
						Log.Critical(ex);
					}
				}
			}

			internal byte[] BlockReceived(UdpClient Client, CoapMessage IncomingMessage)
			{
				if (this.payloadResponseStream == null)
					this.payloadResponseStream = new MemoryStream();

				if (IncomingMessage.Payload != null)
					this.payloadResponseStream.Write(IncomingMessage.Payload, 0, IncomingMessage.Payload.Length);

				if (IncomingMessage.Block2.More)
				{
					List<CoapOption> Options = new List<CoapOption>();

					if (this.options != null)
					{
						foreach (CoapOption Option in this.options)
						{
							if (!(Option is CoapOptionBlock2))
								Options.Add(Option);
						}
					}

					Options.Add(new CoapOptionBlock2(IncomingMessage.Block2.Number + 1, false, IncomingMessage.Block2.Size));

					this.endpoint.Transmit(Client, this.destination, null,
						this.messageType == CoapMessageType.ACK ? CoapMessageType.CON : this.messageType,
						this.messageCode, this.token, true, null, 0, this.blockSize, this.callback, this.state,
						this.payloadResponseStream, Options.ToArray());

					return null;
				}
				else
				{
					byte[] Result = this.payloadResponseStream.ToArray();
					this.payloadResponseStream.Dispose();
					this.payloadResponseStream = null;

					return Result;
				}
			}

			internal string GetUri()
			{
				string Host = null;
				int? Port = null;
				string Path = null;

				if (this.options != null)
				{
					foreach (CoapOption Option in this.options)
					{
						switch (Option.OptionNumber)
						{
							case 3:
								Host = ((CoapOptionUriHost)Option).Value;
								break;

							case 7:
								Port = (int)((CoapOptionUriPort)Option).Value;
								break;

							case 11:
								if (Path == null)
									Path = "/";
								else
									Path += "/";

								Path += ((CoapOptionUriPath)Option).Value;
								break;
						}
					}
				}

				return CoapEndpoint.GetUri(Host, Port, Path, null);
			}
		}

		private void CheckRetry(object State)
		{
			object[] P = (object[])State;
			UdpClient Client = (UdpClient)P[0];
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
				lock (this.activeTokens)
				{
					this.activeTokens.Remove(Message.token);
				}

				this.Fail(Client, Message.callback, Message.state);
				return;
			}

			Message.timeoutMilliseconds *= 2;
			Task T = this.SendMessage(Client, Message);
		}

		private void Request(IPEndPoint Destination, bool Acknowledged, ulong? Token, CoapCode Code, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			this.Transmit(null, Destination, null, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Token, true,
				Payload, 0, BlockSize, Callback, State, null, Options);
		}

		private void Request(IPEndPoint Destination, bool Acknowledged, ulong? Token, CoapCode Code, byte[] Payload, int BlockSize,
			params CoapOption[] Options)
		{
			this.Transmit(null, Destination, null, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Token, true,
				Payload, 0, BlockSize, null, null, null, Options);
		}

		private void Respond(UdpClient Client, IPEndPoint Destination, bool Acknowledged, CoapCode Code, ulong Token, byte[] Payload, int BlockSize,
			params CoapOption[] Options)
		{
			this.Transmit(Client, Destination, null, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Token, false,
				Payload, 0, BlockSize, null, null, null, Options);
		}

		private void ACK(UdpClient Client, IPEndPoint Destination, ushort MessageId)
		{
			this.Transmit(Client, Destination, MessageId, CoapMessageType.ACK, CoapCode.EmptyMessage, 0, false, null, 0, 64, null, null, null);
		}

		private void Reset(UdpClient Client, IPEndPoint Destination, ushort MessageId)
		{
			this.Transmit(Client, Destination, MessageId, CoapMessageType.RST, CoapCode.EmptyMessage, 0, false, null, 0, 64, null, null, null);
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
		public static object Decode(int ContentFormat, byte[] Payload, Uri BaseUri)
		{
			if (contentFormatsByCode.TryGetValue(ContentFormat, out ICoapContentFormat Format))
				return InternetContent.Decode(Format.ContentType, Payload, BaseUri);
			else
				return Payload;
		}

		/// <summary>
		/// Tries to encode CoAP content.
		/// </summary>
		/// <param name="Payload">Payload.</param>
		/// <param name="ContentFormat">Content format of encoded content.</param>
		/// <returns>Decoded object.</returns>
		public static byte[] Encode(object Payload, out int ContentFormat)
		{
			byte[] Data = InternetContent.Encode(Payload, Encoding.UTF8, out string ContentType);
			if (contentFormatsByContentType.TryGetValue(ContentType, out ICoapContentFormat Format))
			{
				ContentFormat = Format.ContentFormat;
				return Data;
			}
			else
				throw new Exception("Unable to encode content of type " + ContentType);
		}

		private async Task<IPEndPoint> GetIPEndPoint(string Destination, int Port, CoapResponseEventHandler Callback, object State)
		{
			IPAddress[] Addresses = await Dns.GetHostAddressesAsync(Destination);
			int c = Addresses.Length;

			if (c == 0)
			{
				this.Fail(null, Callback, State);
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

		private void Fail(UdpClient Client, CoapResponseEventHandler Callback, object State)
		{
			if (Callback != null)
			{
				try
				{
					Callback(this, new CoapResponseEventArgs(Client, this, false, State, null));
				}
				catch (Exception ex)
				{
					this.Error(ex.Message);
					Log.Critical(ex);
				}
			}
		}

		private static CoapOption[] GetQueryOptions(Uri Uri, out int Port, params CoapOption[] Options)
		{
			if (Uri.Scheme != "coap")
				throw new ArgumentException("Invalid URI scheme.", "Uri");

			List<CoapOption> Options2 = new List<CoapOption>();
			int i;

			foreach (CoapOption Option in Options)
			{
				i = Option.OptionNumber;
				if (i == 15 || i == 7 || i == 11 || i == 3)
					throw new ArgumentException("Conflicting CoAP options.", "Options");

				Options2.Add(Option);
			}

			Port = Uri.Port;
			if (Port < 0)
				Port = DefaultCoapPort;

			Options2.Add(new CoapOptionUriHost(Uri.Authority));
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

			if (UriQuery != null)
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
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void GET(IPEndPoint Destination, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, null, CoapCode.GET, null, 64, Callback, State, Options);
		}

		/// <summary>
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task GET(string Destination, int Port, bool Acknowledged, CoapResponseEventHandler Callback, object State,
			params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (EndPoint != null)
				this.GET(EndPoint, Acknowledged, Callback, State, Options);
		}

		/// <summary>
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task GET(string Uri, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			return this.GET(new Uri(Uri), Acknowledged, Callback, State, Options);
		}

		/// <summary>
		/// Performs a GET operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task GET(Uri Uri, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, Options);

			if (IPAddress.TryParse(Uri.Authority, out IPAddress Addr))
				this.GET(new IPEndPoint(Addr, Port), Acknowledged, Callback, State, Options2);
			else
				await this.GET(Uri.Authority, Port, Acknowledged, Callback, State, Options2);
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="CoapEndpoint.UnregisterObservation(IPEndPoint, bool, ulong, CoapResponseEventHandler, object, CoapOption[])"/> 
		/// to cancel an active observation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void Observe(IPEndPoint Destination, bool Acknowledged, CoapResponseEventHandler Callback, object State,
			params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, null, CoapCode.GET, null, 64, Callback, State, Merge(Options, new CoapOptionObserve(0)));
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="CoapEndpoint.UnregisterObservation(string, int, bool, ulong, CoapResponseEventHandler, object, CoapOption[])"/> 
		/// to cancel an active observation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task Observe(string Destination, int Port, bool Acknowledged, CoapResponseEventHandler Callback, object State,
			params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (EndPoint != null)
				this.Observe(EndPoint, Acknowledged, Callback, State, Options);
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="CoapEndpoint.UnregisterObservation(string, bool, ulong, CoapResponseEventHandler, object, CoapOption[])"/> 
		/// to cancel an active observation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task Observe(string Uri, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			return this.Observe(new Uri(Uri), Acknowledged, Callback, State, Options);
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="CoapEndpoint.UnregisterObservation(Uri, bool, ulong, CoapResponseEventHandler, object, CoapOption[])"/> 
		/// to cancel an active observation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task Observe(Uri Uri, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, Options);

			if (IPAddress.TryParse(Uri.Authority, out IPAddress Addr))
				this.Observe(new IPEndPoint(Addr, Port), Acknowledged, Callback, State, Options2);
			else
				await this.Observe(Uri.Authority, Port, Acknowledged, Callback, State, Options2);
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(IPEndPoint, bool, CoapResponseEventHandler, object, CoapOption[])"/>.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Token">Registration token.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void UnregisterObservation(IPEndPoint Destination, bool Acknowledged, ulong Token, CoapResponseEventHandler Callback,
			object State, params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, Token, CoapCode.GET, null, 64, Callback, State, Merge(Options, new CoapOptionObserve(1)));
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(string, bool, CoapResponseEventHandler, object, CoapOption[])"/>.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Token">Registration token.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task UnregisterObservation(string Destination, int Port, bool Acknowledged, ulong Token, CoapResponseEventHandler Callback, object State,
			params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (EndPoint != null)
				this.UnregisterObservation(EndPoint, Acknowledged, Token, Callback, State, Options);
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(string, int, bool, CoapResponseEventHandler, object, CoapOption[])"/>
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Token">Registration token.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task UnregisterObservation(string Uri, bool Acknowledged, ulong Token, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			return this.UnregisterObservation(new Uri(Uri), Acknowledged, Token, Callback, State, Options);
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(Uri, bool, CoapResponseEventHandler, object, CoapOption[])"/>
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Token">Registration token.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task UnregisterObservation(Uri Uri, bool Acknowledged, ulong Token, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, Options);

			if (IPAddress.TryParse(Uri.Authority, out IPAddress Addr))
				this.UnregisterObservation(new IPEndPoint(Addr, Port), Acknowledged, Token, Callback, State, Options2);
			else
				await this.UnregisterObservation(Uri.Authority, Port, Acknowledged, Token, Callback, State, Options2);
		}

		/// <summary>
		/// Performs a POST operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void POST(IPEndPoint Destination, bool Acknowledged, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, null, CoapCode.POST, Payload, BlockSize, Callback, State, Options);
		}

		/// <summary>
		/// Performs a POST operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task POST(string Destination, int Port, bool Acknowledged, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (EndPoint != null)
				this.POST(EndPoint, Acknowledged, Payload, BlockSize, Callback, State, Options);
		}

		/// <summary>
		/// Performs a POST operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task POST(string Uri, bool Acknowledged, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			return this.POST(new Uri(Uri), Acknowledged, Payload, BlockSize, Callback, State, Options);
		}

		/// <summary>
		/// Performs a POST operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task POST(Uri Uri, bool Acknowledged, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, Options);

			if (IPAddress.TryParse(Uri.Authority, out IPAddress Addr))
				this.POST(new IPEndPoint(Addr, Port), Acknowledged, Payload, BlockSize, Callback, State, Options2);
			else
				await this.POST(Uri.Authority, Port, Acknowledged, Payload, BlockSize, Callback, State, Options2);
		}

		/// <summary>
		/// Performs a PUT operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void PUT(IPEndPoint Destination, bool Acknowledged, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, null, CoapCode.PUT, Payload, BlockSize, Callback, State, Options);
		}

		/// <summary>
		/// Performs a PUT operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task PUT(string Destination, int Port, bool Acknowledged, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (EndPoint != null)
				this.PUT(EndPoint, Acknowledged, Payload, BlockSize, Callback, State, Options);
		}

		/// <summary>
		/// Performs a PUT operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task PUT(string Uri, bool Acknowledged, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			return this.PUT(new Uri(Uri), Acknowledged, Payload, BlockSize, Callback, State, Options);
		}

		/// <summary>
		/// Performs a PUT operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Payload">Payload to post.</param>
		/// <param name="BlockSize">Block size, in case payload needs to be divided into blocks.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task PUT(Uri Uri, bool Acknowledged, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, Options);

			if (IPAddress.TryParse(Uri.Authority, out IPAddress Addr))
				this.PUT(new IPEndPoint(Addr, Port), Acknowledged, Payload, BlockSize, Callback, State, Options2);
			else
				await this.PUT(Uri.Authority, Port, Acknowledged, Payload, BlockSize, Callback, State, Options2);
		}

		/// <summary>
		/// Performs a DELETE operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void DELETE(IPEndPoint Destination, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, null, CoapCode.DELETE, null, 64, Callback, State, Options);
		}

		/// <summary>
		/// Performs a DELETE operation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task DELETE(string Destination, int Port, bool Acknowledged, CoapResponseEventHandler Callback, object State,
			params CoapOption[] Options)
		{
			IPEndPoint EndPoint = await this.GetIPEndPoint(Destination, Port, Callback, State);
			if (EndPoint != null)
				this.DELETE(EndPoint, Acknowledged, Callback, State, Options);
		}

		/// <summary>
		/// Performs a DELETE operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public Task DELETE(string Uri, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			return this.DELETE(new Uri(Uri), Acknowledged, Callback, State, Options);
		}

		/// <summary>
		/// Performs a DELETE operation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task DELETE(Uri Uri, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			CoapOption[] Options2 = GetQueryOptions(Uri, out int Port, Options);

			if (IPAddress.TryParse(Uri.Authority, out IPAddress Addr))
				this.DELETE(new IPEndPoint(Addr, Port), Acknowledged, Callback, State, Options2);
			else
				await this.DELETE(Uri.Authority, Port, Acknowledged, Callback, State, Options2);
		}

	}
}
