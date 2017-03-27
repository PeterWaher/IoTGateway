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
using Waher.Networking.CoAP.Options;
using Waher.Networking.Sniffers;
using Waher.Runtime.Timing;
using Waher.Script;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP client. CoAP is defined in RFC7252:
	/// https://tools.ietf.org/html/rfc7252
	/// </summary>
	public class CoapClient : Sniffable, IDisposable
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
		private static Dictionary<int, CoapOption> optionTypes = GetOptions(true);
		private static Dictionary<int, ICoapContentFormat> contentFormats = GetContentFormats();

		private LinkedList<Message> outputQueue = new LinkedList<Message>();
		private Dictionary<ushort, Message> outgoingMessages = new Dictionary<ushort, Message>();
		private Dictionary<ulong, Message> activeTokens = new Dictionary<ulong, Message>();
		private Random gen = new Random();
		private Scheduler scheduler;
		private LinkedList<KeyValuePair<UdpClient, IPEndPoint>> coapOutgoing = new LinkedList<KeyValuePair<UdpClient, IPEndPoint>>();
		private LinkedList<UdpClient> coapIncoming = new LinkedList<UdpClient>();
		private ushort msgId = 0;
		private uint tokenMsb = 0;
		private bool isWriting = false;
		private bool disposed = false;

		/// <summary>
		/// CoAP client. CoAP is defined in RFC7252:
		/// https://tools.ietf.org/html/rfc7252
		/// </summary>
		/// <param name="Sniffers">Optional set of sniffers to use.</param>
		public CoapClient(params ISniffer[] Sniffers)
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
						continue;
				}

				IPInterfaceProperties Properties = Interface.GetIPProperties();
				IPAddress MulticastAddress;

				foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
				{
					if (UnicastAddress.Address.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4)
					{
						try
						{
							Outgoing = new UdpClient(AddressFamily.InterNetwork);
							MulticastAddress = IPAddress.Parse("224.0.1.187");
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
							MulticastAddress = IPAddress.Parse("[FF02::FD]");
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

					IPEndPoint EP = new IPEndPoint(MulticastAddress, DefaultCoapPort);
					this.coapOutgoing.AddLast(new KeyValuePair<UdpClient, IPEndPoint>(Outgoing, EP));

					Outgoing.BeginReceive(this.EndReceive, Outgoing);

					try
					{
						Incoming = new UdpClient(Outgoing.Client.AddressFamily);
						Incoming.ExclusiveAddressUse = false;
						Incoming.Client.Bind(new IPEndPoint(UnicastAddress.Address, DefaultCoapPort));

						Incoming.BeginReceive(this.EndReceive, Incoming);

						this.coapIncoming.AddLast(Incoming);
					}
					catch (Exception)
					{
						Incoming = null;
					}

					try
					{
						Incoming = new UdpClient(DefaultCoapPort, Outgoing.Client.AddressFamily);
						Incoming.MulticastLoopback = false;
						Incoming.JoinMulticastGroup(MulticastAddress);

						Incoming.BeginReceive(this.EndReceive, Incoming);

						this.coapIncoming.AddLast(Incoming);
					}
					catch (Exception)
					{
						Incoming = null;
					}
				}
			}

			this.scheduler = new Scheduler(ThreadPriority.BelowNormal, "CoAP tasks");
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

			foreach (KeyValuePair<UdpClient, IPEndPoint> P in this.coapOutgoing)
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

			this.coapOutgoing.Clear();

			foreach (UdpClient Client in this.coapIncoming)
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

			this.coapIncoming.Clear();

			foreach (ISniffer Sniffer in this.Sniffers)
			{
				IDisposable Disposable = Sniffer as IDisposable;
				if (Disposable != null)
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

		private void EndReceive(IAsyncResult ar)
		{
			if (this.disposed)
				return;

			try
			{
				UdpClient UdpClient = (UdpClient)ar.AsyncState;
				IPEndPoint RemoteIP = null;
				byte[] Packet = UdpClient.EndReceive(ar, ref RemoteIP);

				this.ReceiveBinary(Packet);

				try
				{
					this.Decode(Packet, RemoteIP);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				UdpClient.BeginReceive(this.EndReceive, UdpClient);
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
			}
		}

		private static Dictionary<int, CoapOption> GetOptions(bool First)
		{
			Dictionary<int, CoapOption> Result = new Dictionary<int, CoapOption>();
			ConstructorInfo CI;
			CoapOption Option;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICoapOption)))
			{
				if (T.IsAbstract)
					continue;

				CI = T.GetConstructor(Types.NoTypes);
				if (CI == null)
					continue;

				try
				{
					Option = (CoapOption)CI.Invoke(Types.NoParameters);
					if (Result.ContainsKey(Option.OptionNumber))
						throw new Exception("Option number " + Option.OptionNumber + " already defined.");

					Result[Option.OptionNumber] = Option;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			if (First)
				Types.OnInvalidated += Types_OnInvalidated;

			return Result;
		}

		private static Dictionary<int, ICoapContentFormat> GetContentFormats()
		{
			Dictionary<int, ICoapContentFormat> Result = new Dictionary<int, ICoapContentFormat>();
			ConstructorInfo CI;
			ICoapContentFormat ContentFormat;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICoapContentFormat)))
			{
				if (T.IsAbstract)
					continue;

				CI = T.GetConstructor(Types.NoTypes);
				if (CI == null)
					continue;

				try
				{
					ContentFormat = (ICoapContentFormat)CI.Invoke(Types.NoParameters);
					if (Result.ContainsKey(ContentFormat.ContentFormat))
						throw new Exception("Content format number " + ContentFormat.ContentFormat + " already defined.");

					Result[ContentFormat.ContentFormat] = ContentFormat;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			return Result;
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			optionTypes = GetOptions(false);
			contentFormats = GetContentFormats();
		}

		private void Decode(byte[] Packet, IPEndPoint From)
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
				CoapOption Option;
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

					if (optionTypes.TryGetValue(OptionNumber, out Option))
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
								this.Fail(OutgoingMessage.callback, OutgoingMessage.state);
							else
							{
								this.Transmit(OutgoingMessage.destination, null, OutgoingMessage.messageType, OutgoingMessage.messageCode,
									OutgoingMessage.token, true, OutgoingMessage.payload, OutgoingMessage.blockNr, OutgoingMessage.blockSize,
									OutgoingMessage.callback, OutgoingMessage.state, OutgoingMessage.payloadResponseStream, OutgoingMessage.options);
							}
						}
						else
						{
							if (IncomingMessage.Block2 != null)
							{
								OutgoingMessage.options = Exclude(OutgoingMessage.options, 27);		// Remove Block1 option, if available.

								IncomingMessage.Payload = OutgoingMessage.BlockReceived(IncomingMessage);
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
									OutgoingMessage.ResponseReceived(IncomingMessage);
								}
								else
									this.Fail(OutgoingMessage.callback, OutgoingMessage.state);
							}
						}
					}
					else
						this.Fail(OutgoingMessage.callback, OutgoingMessage.state);
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
						this.Transmit(From, IncomingMessage.MessageId, CoapMessageType.ACK, CoapCode.EmptyMessage, Token,
							false, null, 0, 64, null, null, null);
					}

					if (IncomingMessage.Block2 != null)
					{
						IncomingMessage.Payload = OutgoingMessage.BlockReceived(IncomingMessage);
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

							OutgoingMessage.ResponseReceived(IncomingMessage);
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

						OutgoingMessage.ResponseReceived(IncomingMessage);
					}
				}
				else
				{
					// TODO: Blocked messages (Block1)
					// TODO: RST to observation notifications that have been unregistered.

					CoapMessageEventHandler h = this.OnIncomingRequest;

					if (h != null)
					{
						CoapMessageEventArgs e = new CoapMessageEventArgs(this, IncomingMessage);
						try
						{
							h(this, e);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}

						if (!e.Responded)
						{
							if (Type == CoapMessageType.CON)
								this.Transmit(From, MessageId, CoapMessageType.ACK, CoapCode.EmptyMessage, Token, false, null, 0, 64, null, null, null);
						}
					}
					else if (Type == CoapMessageType.CON)
						this.Transmit(From, MessageId, CoapMessageType.RST, CoapCode.EmptyMessage, Token, false, null, 0, 64, null, null, null);
				}
			}
		}

		/// <summary>
		/// Event raised when an incoming request has been received.
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

		internal void Transmit(IPEndPoint Destination, ushort? MessageID, CoapMessageType MessageType, CoapCode Code, ulong? Token,
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
				if (NrLeft <= BlockSize)
					Options = Join(Exclude(Options, 27), new CoapOptionBlock1(BlockNr, false, BlockSize));
				else
				{
					Options = Join(Exclude(Options, 27), new CoapOptionBlock1(BlockNr, true, BlockSize));
					NrLeft = BlockSize;
				}

				Payload = new byte[NrLeft];
				Array.Copy(OrgPayload, Pos, Payload, 0, NrLeft);
			}

			Message = new Message()
			{
				client = this,
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

			this.SendMessage(Message);
		}

		private void SendMessage(Message Message)
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

			foreach (KeyValuePair<UdpClient, IPEndPoint> P in this.coapOutgoing)
			{
				if (P.Key.Client.AddressFamily != Message.destination.AddressFamily)
					continue;

				P.Key.BeginSend(Message.encoded, Message.encoded.Length, Message.destination, this.MessageSent,
					new KeyValuePair<Message, UdpClient>(Message, P.Key));

				Sent = true;
			}

			if (Sent)
			{
				if (Message.acknowledged || Message.callback != null)
					this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds), this.CheckRetry, Message);
			}
			else
			{
				lock (this.outgoingMessages)
				{
					this.outgoingMessages.Remove(Message.messageID);
				}

				lock (this.activeTokens)
				{
					this.activeTokens.Remove(Message.token);
				}

				this.Fail(Message.callback, Message.state);
			}
		}

		private void MessageSent(IAsyncResult ar)
		{
			if (this.disposed)
				return;

			try
			{
				KeyValuePair<Message, UdpClient> P = (KeyValuePair<Message, UdpClient>)ar.AsyncState;
				Message Message;

				P.Value.EndSend(ar);

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

				if (Message != null)
				{
					P.Value.BeginSend(Message.encoded, Message.encoded.Length, Message.destination, this.MessageSent, P);

					if (Message.acknowledged || Message.callback != null)
						this.scheduler.Add(DateTime.Now.AddMilliseconds(Message.timeoutMilliseconds), this.CheckRetry, Message);
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
			public CoapClient client;
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

			internal void ResponseReceived(CoapMessage Response)
			{
				this.responseReceived = true;

				if (this.callback != null)
				{
					try
					{
						this.callback(this.client, new CoapResponseEventArgs(this.client,
							Response.Type != CoapMessageType.RST && (int)Response.Code >= 0x40 && (int)Response.Code <= 0x5f,
							this.state, Response));
					}
					catch (Exception ex)
					{
						this.client.Error(ex.Message);
						Log.Critical(ex);
					}
				}
			}

			internal byte[] BlockReceived(CoapMessage IncomingMessage)
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

					this.client.Transmit(this.destination, null,
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

				return CoapClient.GetUri(Host, Port, Path, null);
			}
		}

		private void CheckRetry(object State)
		{
			Message Message = (Message)State;
			Message Message2;
			bool Fail = false;

			if (Message.responseReceived)
				return;

			lock (this.outgoingMessages)
			{
				if (!this.outgoingMessages.TryGetValue(Message.messageID, out Message2) || Message != Message2)
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

				this.Fail(Message.callback, Message.state);
				return;
			}

			Message.timeoutMilliseconds *= 2;
			this.SendMessage(Message);
		}

		private void Request(IPEndPoint Destination, bool Acknowledged, ulong? Token, CoapCode Code, byte[] Payload, int BlockSize,
			CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			this.Transmit(Destination, null, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Token, true,
				Payload, 0, BlockSize, Callback, State, null, Options);
		}

		private void Request(IPEndPoint Destination, bool Acknowledged, ulong? Token, CoapCode Code, byte[] Payload, int BlockSize,
			params CoapOption[] Options)
		{
			this.Transmit(Destination, null, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Token, true,
				Payload, 0, BlockSize, null, null, null, Options);
		}

		private void Respond(IPEndPoint Destination, bool Acknowledged, CoapCode Code, ulong Token, byte[] Payload, int BlockSize,
			params CoapOption[] Options)
		{
			this.Transmit(Destination, null, Acknowledged ? CoapMessageType.CON : CoapMessageType.NON, Code, Token, false,
				Payload, 0, BlockSize, null, null, null, Options);
		}

		private void ACK(IPEndPoint Destination, ushort MessageId)
		{
			this.Transmit(Destination, MessageId, CoapMessageType.ACK, CoapCode.EmptyMessage, 0, false, null, 0, 64, null, null, null);
		}

		private void Reset(IPEndPoint Destination, ushort MessageId)
		{
			this.Transmit(Destination, MessageId, CoapMessageType.RST, CoapCode.EmptyMessage, 0, false, null, 0, 64, null, null, null);
		}

		/// <summary>
		/// Tries to get a CoAP Content Format object, that can be used to decode content.
		/// </summary>
		/// <param name="ContentFormat">Content format number.</param>
		/// <param name="Format">Content format object.</param>
		/// <returns>If a content format was found.</returns>
		public static bool TryGetContentFormat(int ContentFormat, out ICoapContentFormat Format)
		{
			return contentFormats.TryGetValue(ContentFormat, out Format);
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
			ICoapContentFormat Format;

			if (contentFormats.TryGetValue(ContentFormat, out Format))
				return InternetContent.Decode(Format.ContentType, Payload, BaseUri);
			else
				return Payload;
		}

		private async Task<IPEndPoint> GetIPEndPoint(string Destination, int Port, CoapResponseEventHandler Callback, object State)
		{
			IPAddress[] Addresses = await Dns.GetHostAddressesAsync(Destination);
			int c = Addresses.Length;

			if (c == 0)
			{
				this.Fail(Callback, State);
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

		private void Fail(CoapResponseEventHandler Callback, object State)
		{
			if (Callback != null)
			{
				try
				{
					Callback(this, new CoapResponseEventArgs(this, false, State, null));
				}
				catch (Exception ex)
				{
					this.Error(ex.Message);
					Log.Critical(ex);
				}
			}
		}

		private static CoapOption[] Exclude(CoapOption[] Options, int OptionNrToExclude)
		{
			if (Options == null)
				return null;

			int i, c = Options.Length;

			for (i = 0; i < c; i++)
			{
				if (Options[i].OptionNumber == OptionNrToExclude)
				{
					CoapOption[] Result = new CoapOption[c - 1];

					if (i > 0)
						Array.Copy(Options, 0, Result, 0, i);

					if (i < c - 1)
						Array.Copy(Options, i + 1, Result, i, c - i - 1);

					return Result;
				}
			}

			return Options;
		}

		private static CoapOption[] Join(CoapOption[] Options, params CoapOption[] Options2)
		{
			if (Options.Length == 0)
				return Options2;
			else if (Options2.Length == 0)
				return Options;
			else
			{
				List<CoapOption> Result = new List<CoAP.CoapOption>();

				Result.AddRange(Options);
				Result.AddRange(Options2);

				return Result.ToArray();
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
			int Port;
			CoapOption[] Options2 = GetQueryOptions(Uri, out Port, Options);
			IPAddress Addr;

			if (IPAddress.TryParse(Uri.Authority, out Addr))
				this.GET(new IPEndPoint(Addr, Port), Acknowledged, Callback, State, Options2);
			else
				await this.GET(Uri.Authority, Port, Acknowledged, Callback, State, Options2);
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="UnregisterObservation"/> to cancel an active observation.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void Observe(IPEndPoint Destination, bool Acknowledged, CoapResponseEventHandler Callback, object State,
			params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, null, CoapCode.GET, null, 64, Callback, State, Join(Options, new CoapOptionObserve(0)));
		}

		/// <summary>
		/// Performs an Observe operation.
		/// 
		/// Call <see cref="UnregisterObservation"/> to cancel an active observation.
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
		/// Call <see cref="UnregisterObservation"/> to cancel an active observation.
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
		/// Call <see cref="UnregisterObservation"/> to cancel an active observation.
		/// </summary>
		/// <param name="Uri">URI pointing out resource.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task Observe(Uri Uri, bool Acknowledged, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			int Port;
			CoapOption[] Options2 = GetQueryOptions(Uri, out Port, Options);
			IPAddress Addr;

			if (IPAddress.TryParse(Uri.Authority, out Addr))
				this.Observe(new IPEndPoint(Addr, Port), Acknowledged, Callback, State, Options2);
			else
				await this.Observe(Uri.Authority, Port, Acknowledged, Callback, State, Options2);
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(IPEndPoint, bool, CoapResponseEventHandler, object, CoapOption[])"/>.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public void UnregisterObservation(IPEndPoint Destination, bool Acknowledged, ulong Token, CoapResponseEventHandler Callback,
			object State, params CoapOption[] Options)
		{
			this.Request(Destination, Acknowledged, Token, CoapCode.GET, null, 64, Callback, State, Join(Options, new CoapOptionObserve(1)));
		}

		/// <summary>
		/// Unregisters an active observation registration made by <see cref="Observe(string, bool, CoapResponseEventHandler, object, CoapOption[])"/>.
		/// </summary>
		/// <param name="Destination">Request resource from this locaton.</param>
		/// <param name="Port">Port number of destination.</param>
		/// <param name="Acknowledged">If acknowledged message service is to be used.</param>
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
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Options">CoAP options to include in the request.</param>
		public async Task UnregisterObservation(Uri Uri, bool Acknowledged, ulong Token, CoapResponseEventHandler Callback, object State, params CoapOption[] Options)
		{
			int Port;
			CoapOption[] Options2 = GetQueryOptions(Uri, out Port, Options);
			IPAddress Addr;

			if (IPAddress.TryParse(Uri.Authority, out Addr))
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
			int Port;
			CoapOption[] Options2 = GetQueryOptions(Uri, out Port, Options);
			IPAddress Addr;

			if (IPAddress.TryParse(Uri.Authority, out Addr))
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
			int Port;
			CoapOption[] Options2 = GetQueryOptions(Uri, out Port, Options);
			IPAddress Addr;

			if (IPAddress.TryParse(Uri.Authority, out Addr))
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
			int Port;
			CoapOption[] Options2 = GetQueryOptions(Uri, out Port, Options);
			IPAddress Addr;

			if (IPAddress.TryParse(Uri.Authority, out Addr))
				this.DELETE(new IPEndPoint(Addr, Port), Acknowledged, Callback, State, Options2);
			else
				await this.DELETE(Uri.Authority, Port, Acknowledged, Callback, State, Options2);
		}

	}
}
