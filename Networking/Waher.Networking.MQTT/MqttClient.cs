using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Security.Cryptography.Certificates;
#else
using System.Security.Cryptography.X509Certificates;
#endif

namespace Waher.Networking.MQTT
{
	/// <summary>
	/// Connection error event handler.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Exception">Information about error received.</param>
	public delegate Task MqttExceptionEventHandler(object Sender, Exception Exception);

	/// <summary>
	/// Event handler for state change events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewState">New state reported.</param>
	public delegate Task StateChangedEventHandler(object Sender, MqttState NewState);

	/// <summary>
	/// Event handler used for events raised when data has been successfully acknowledged.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="PacketIdentifier">Packet identifier of data successfully published.</param>
	//#pragma warning disable
	public delegate Task PacketAcknowledgedEventHandler(object Sender, ushort PacketIdentifier);
	//#pragma warning restore

	/// <summary>
	/// Event handler for events raised when content has been received.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Content">Content received.</param>
	public delegate Task ContentReceivedEventHandler(object Sender, MqttContent Content);

	/// <summary>
	/// Manages an MQTT connection. Implements MQTT v3.1.1, as defined in
	/// http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
	/// </summary>
	public class MqttClient : Sniffable, IDisposable
	{
		private const int KeepAliveTimeSeconds = 30;

#if !WINDOWS_UWP
		private readonly X509Certificate clientCertificate = null;
#endif
		private readonly SortedDictionary<DateTime, OutputRecord> packetByTimeout = new SortedDictionary<DateTime, OutputRecord>();
		private readonly SortedDictionary<int, DateTime> timeoutByPacketIdentifier = new SortedDictionary<int, DateTime>();
		private readonly Random rnd = new Random();
		private readonly object synchObj = new object();
		private readonly Dictionary<ushort, MqttContent> contentCache = new Dictionary<ushort, MqttContent>();
		private readonly string clientId = Guid.NewGuid().ToString().Substring(0, 23);
		private BinaryTcpClient client = null;
		private readonly byte[] willData = null;
		private Timer secondTimer = null;
		private DateTime nextPing = DateTime.MinValue;
		private readonly MqttQualityOfService willQoS = MqttQualityOfService.AtMostOnce;
		private readonly string host;
		private readonly string userName;
		private readonly string password;
		private readonly string willTopic = null;
		private readonly int port;
		private int keepAliveSeconds;
		private MqttState state;
		private readonly bool tls;
		private bool trustServer = false;
		private readonly bool will = false;
		private readonly bool willRetain = false;

#if !WINDOWS_UWP
		/// <summary>
		/// Manages an MQTT connection. Implements MQTT v3.1.1, as defined in
		/// http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
		/// </summary>
		/// <param name="Host">Host name or IP address of MQTT server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="ClientCertificate">Client certificate.</param>
		/// <param name="Sniffers">Sniffers to use.</param>
		public MqttClient(string Host, int Port, X509Certificate ClientCertificate, params ISniffer[] Sniffers)
			: this(Host, Port, ClientCertificate, null, MqttQualityOfService.AtMostOnce, false, null, Sniffers)
		{
		}
#endif

		/// <summary>
		/// Manages an MQTT connection. Implements MQTT v3.1.1, as defined in
		/// http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
		/// </summary>
		/// <param name="Host">Host name or IP address of MQTT server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="Tls">If TLS is used to encrypt communication.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Sniffers">Sniffers to use.</param>
		public MqttClient(string Host, int Port, bool Tls, string UserName, string Password, params ISniffer[] Sniffers)
			: this(Host, Port, Tls, UserName, Password, null, MqttQualityOfService.AtMostOnce, false, null, Sniffers)
		{
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Manages an MQTT connection. Implements MQTT v3.1.1, as defined in
		/// http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
		/// </summary>
		/// <param name="Host">Host name or IP address of MQTT server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="ClientCertificate">Client certificate.</param>
		/// <param name="WillTopic">Topic to publish the last will and testament, in case the connection drops unexpectedly.</param>
		/// <param name="WillQoS">Quality of Service of last will and testament, in case the connection drops unexpectedly.</param>
		/// <param name="WillRetain">If last will and testament should be retained, in case the connection drops unexpectedly.</param>
		/// <param name="WillData">Data of last will and testament, in case the connection drops unexpectedly.</param>
		/// <param name="Sniffers">Sniffers to use.</param>
		public MqttClient(string Host, int Port, X509Certificate ClientCertificate, string WillTopic,
			MqttQualityOfService WillQoS, bool WillRetain, byte[] WillData, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.host = Host;
			this.port = Port;
			this.tls = true;
			this.userName = string.Empty;
			this.password = string.Empty;
			this.clientCertificate = ClientCertificate;
			this.state = MqttState.Offline;
			this.will = !string.IsNullOrEmpty(WillTopic) && WillData != null;
			this.willTopic = WillTopic;
			this.willQoS = WillQoS;
			this.willRetain = WillRetain;
			this.willData = WillData;

			if (this.will && this.willData.Length > 65535)
				throw new ArgumentException("Will data too large.", nameof(WillData));

			Task.Run(() => this.BeginConnect());
		}
#endif

		/// <summary>
		/// Manages an MQTT connection. Implements MQTT v3.1.1, as defined in
		/// http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
		/// </summary>
		/// <param name="Host">Host name or IP address of MQTT server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="Tls">If TLS is used to encrypt communication.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="WillTopic">Topic to publish the last will and testament, in case the connection drops unexpectedly.</param>
		/// <param name="WillQoS">Quality of Service of last will and testament, in case the connection drops unexpectedly.</param>
		/// <param name="WillRetain">If last will and testament should be retained, in case the connection drops unexpectedly.</param>
		/// <param name="WillData">Data of last will and testament, in case the connection drops unexpectedly.</param>
		/// <param name="Sniffers">Sniffers to use.</param>
		public MqttClient(string Host, int Port, bool Tls, string UserName, string Password, string WillTopic,
			MqttQualityOfService WillQoS, bool WillRetain, byte[] WillData, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.userName = UserName;
			this.password = Password;
#if !WINDOWS_UWP
			this.clientCertificate = null;
#endif
			this.state = MqttState.Offline;
			this.will = !string.IsNullOrEmpty(WillTopic) && WillData != null;
			this.willTopic = WillTopic;
			this.willQoS = WillQoS;
			this.willRetain = WillRetain;
			this.willData = WillData;

			if (this.will && this.willData.Length > 65535)
				throw new ArgumentException("Will data too large.", nameof(WillData));

			Task.Run(() => this.BeginConnect());
		}

		private class OutputRecord
		{
			public byte[] Packet;
			public int PacketIdentifier;
			public EventHandler Callback;
		}

		private async Task BeginConnect()
		{
			try
			{
				this.Information("Connecting to " + Host + ":" + Port.ToString());

				this.DisposeClient();

				this.State = MqttState.Connecting;

				this.client = new BinaryTcpClient();
				await this.client.ConnectAsync(Host, Port, this.tls);

				if (this.tls)
				{
					this.State = MqttState.StartingEncryption;
#if WINDOWS_UWP
					await this.client.UpgradeToTlsAsClient(SocketProtectionLevel.Tls12, this.trustServer);
#else
					await this.client.UpgradeToTlsAsClient(this.clientCertificate, SslProtocols.Tls12, this.trustServer);
#endif
					this.client.Continue();
				}

				this.client.OnSent += this.Client_OnSent;
				this.client.OnReceived += this.Client_OnReceived;
				this.client.OnDisconnected += this.Client_OnDisconnected;
				this.client.OnError += this.ConnectionError;

				await this.CONNECT(KeepAliveTimeSeconds);
			}
			catch (Exception ex)
			{
				await this.ConnectionError(this, ex);
			}
		}

		/// <summary>
		/// Reconnects a client after an error or if it's offline. Reconnecting, instead of creating a completely new connection,
		/// saves time.
		/// </summary>
		public async Task Reconnect()
		{
			try
			{
				this.secondTimer?.Dispose();
				this.secondTimer = null;

				if (this.state == MqttState.Connected)
					await this.DISCONNECT();

				this.DisposeClient();
			}
			catch (Exception)
			{
				// Ignore
			}
			finally
			{
				this.secondTimer = null;
				this.client = null;
			}

			this.state = MqttState.Offline;

			await this.BeginConnect();
		}

		private async Task ConnectionError(object Sender, Exception ex)
		{
			MqttExceptionEventHandler h = this.OnConnectionError;
			if (h != null)
			{
				try
				{
					await h(this, ex);
				}
				catch (Exception ex2)
				{
					this.Exception(ex2);
					Log.Critical(ex2);
				}
			}

			await this.Error(ex);

			this.State = MqttState.Error;
		}

		private async Task CONNECT(int KeepAliveSeconds)
		{
			this.State = MqttState.Authenticating;
			this.keepAliveSeconds = KeepAliveSeconds;
			this.nextPing = DateTime.Now.AddMilliseconds(KeepAliveSeconds * 500);
			this.secondTimer = new Timer(this.SecondTimer_Elapsed, null, 1000, 1000);

			BinaryOutput Payload = new BinaryOutput();
			Payload.WriteString("MQTT");
			Payload.WriteByte(4);   // v3.1.1

			byte b = 2;     // Clean session.

			if (this.will)
			{
				b |= 4;
				b |= (byte)(((int)this.willQoS) << 3);

				if (this.willRetain)
					b |= 32;
			}

			if (!string.IsNullOrEmpty(this.userName))
			{
				b |= 128;

				if (!string.IsNullOrEmpty(this.password))
					b |= 64;
			}

			Payload.WriteByte(b);

			Payload.WriteByte((byte)(KeepAliveSeconds >> 8));
			Payload.WriteByte((byte)KeepAliveSeconds);

			Payload.WriteString(this.clientId);

			if (this.will)
			{
				Payload.WriteString(this.willTopic);

				int l = this.willData.Length;

				Payload.WriteByte((byte)(l >> 8));
				Payload.WriteByte((byte)l);
				Payload.WriteBytes(this.willData);
			}

			if (!string.IsNullOrEmpty(this.userName))
			{
				Payload.WriteString(this.userName);

				if (!string.IsNullOrEmpty(this.password))
					Payload.WriteString(this.password);
			}

			byte[] PayloadData = Payload.GetPacket();

			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.CONNECT << 4);
			Packet.WriteUInt((uint)PayloadData.Length);
			Packet.WriteBytes(PayloadData);

			byte[] PacketData = Packet.GetPacket();

			await this.Write(PacketData, 0, null);
			this.inputState = 0;
		}

		private async Task Write(byte[] Packet, int PacketIdentifier, EventHandler Callback)
		{
			if (PacketIdentifier != 0 && ((Packet[0] >> 1) & 3) > 0)
			{
				DateTime Timeout = DateTime.Now.AddSeconds(2);
				lock (this.synchObj)
				{
					while (this.packetByTimeout.ContainsKey(Timeout))
						Timeout = Timeout.AddTicks(rnd.Next(1, 10));

					this.packetByTimeout[Timeout] = new OutputRecord()
					{
						Packet = Packet,
						PacketIdentifier = PacketIdentifier,
						Callback = Callback
					};

					this.timeoutByPacketIdentifier[PacketIdentifier] = Timeout;
				}
			}

			await this.client.SendAsync(Packet, Callback);
			this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);
		}

		private void Client_OnDisconnected(object sender, EventArgs e)
		{
			if (this.state != MqttState.Error)
				this.State = MqttState.Offline;
		}

		private Task<bool> Client_OnSent(object Sender, byte[] Buffer, int Offset, int Count)
		{
			if (this.HasSniffers)
				this.TransmitBinary(BinaryTcpClient.ToArray(Buffer, Offset, Count));

			return Task.FromResult<bool>(true);
		}

		private async Task<bool> Client_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			if (this.HasSniffers)
				this.ReceiveBinary(BinaryTcpClient.ToArray(Buffer, Offset, Count));

			byte b;
			bool Result = true;

			while (Count-- > 0)
			{
				b = Buffer[Offset++];

				switch (this.inputState)
				{
					case 0:
						this.inputPacket = new MemoryStream();
						this.inputPacket.WriteByte(b);
						this.inputPacketType = (MqttControlPacketType)(b >> 4);
						this.inputRemainingLength = 0;
						this.inputOffset = 0;
						this.inputState++;
						break;

					case 1:
						this.inputRemainingLength |= ((b & 127) << this.inputOffset);
						this.inputOffset += 7;

						this.inputPacket.WriteByte(b);
						if ((b & 128) == 0)
						{
							switch (this.inputPacketType)
							{
								case MqttControlPacketType.CONNECT:
								case MqttControlPacketType.CONNACK:
								case MqttControlPacketType.PINGREQ:
								case MqttControlPacketType.PINGRESP:
								case MqttControlPacketType.DISCONNECT:
								case MqttControlPacketType.PUBLISH:
								default:
									if (this.inputRemainingLength == 0)
									{
										this.inputState = 0;
										if (!await this.ProcessInputPacket())
											Result = false;
									}
									else
										this.inputState += 3;
									break;

								case MqttControlPacketType.PUBACK:
								case MqttControlPacketType.PUBREC:
								case MqttControlPacketType.PUBREL:
								case MqttControlPacketType.PUBCOMP:
								case MqttControlPacketType.SUBSCRIBE:
								case MqttControlPacketType.SUBACK:
								case MqttControlPacketType.UNSUBSCRIBE:
								case MqttControlPacketType.UNSUBACK:
									this.inputState++;
									break;
							}

						}
						break;

					case 2:
						this.inputPacket.WriteByte(b);
						this.inputState++;
						this.inputRemainingLength--;
						break;

					case 3:
						this.inputPacket.WriteByte(b);
						this.inputRemainingLength--;

						if (this.inputRemainingLength == 0)
						{
							this.inputState = 0;
							if (!await this.ProcessInputPacket())
								Result = false;
						}
						else
							this.inputState++;

						break;

					case 4:
						this.inputPacket.WriteByte(b);
						this.inputRemainingLength--;

						if (this.inputRemainingLength == 0)
						{
							this.inputState = 0;
							if (!await this.ProcessInputPacket())
								Result = false;
						}
						break;
				}
			}

			return Result;
		}

		private void PacketDelivered(int PacketIdentifier)
		{
			lock (this.synchObj)
			{
				if (this.timeoutByPacketIdentifier.TryGetValue(PacketIdentifier, out DateTime TP))
				{
					this.timeoutByPacketIdentifier.Remove(PacketIdentifier);
					this.packetByTimeout.Remove(TP);
				}
			}
		}

		private async void SecondTimer_Elapsed(object State)
		{
			DateTime Now = DateTime.Now;

			try
			{
				if (Now >= this.nextPing)
				{
					await this.PING();
					this.nextPing = Now.AddMilliseconds(this.keepAliveSeconds * 500);
				}

				LinkedList<KeyValuePair<DateTime, OutputRecord>> Resend = null;

				lock (this.synchObj)
				{
					foreach (KeyValuePair<DateTime, OutputRecord> P in this.packetByTimeout)
					{
						if (Now < P.Key)
							break;

						if (Resend is null)
							Resend = new LinkedList<KeyValuePair<DateTime, OutputRecord>>();

						Resend.AddLast(P);
					}

					if (Resend != null)
					{
						foreach (KeyValuePair<DateTime, OutputRecord> P in Resend)
						{
							this.packetByTimeout.Remove(P.Key);
							this.timeoutByPacketIdentifier.Remove(P.Value.PacketIdentifier);
						}
					}
				}

				if (Resend != null)
				{
					foreach (KeyValuePair<DateTime, OutputRecord> P in Resend)
						await this.Write(P.Value.Packet, P.Value.PacketIdentifier, P.Value.Callback);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task<bool> ProcessInputPacket()
		{
			try
			{
				BinaryInput Packet = new BinaryInput(this.inputPacket);
				MqttHeader Header = MqttHeader.Parse(Packet);

				switch (Header.ControlPacketType)
				{
					case MqttControlPacketType.CONNECT:
					default:
						throw new Exception("Received command from server that is not handled: " + Header.ControlPacketType.ToString());

					case MqttControlPacketType.CONNACK:
						bool SessionPresent = (Packet.ReadByte() & 1) != 0;
						byte ReturnCode = Packet.ReadByte();

						try
						{
							switch (ReturnCode)
							{
								case 0:
									this.State = MqttState.Connected;
									this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);
									break;

								case 1:
									throw new IOException("Connection Refused, unacceptable protocol version.");

								case 2:
									throw new IOException("Connection Refused, identifier rejected.");

								case 3:
									throw new IOException("Connection Refused, Server unavailable.");

								case 4:
									throw new IOException("Connection Refused, bad user name or password.");

								case 5:
									throw new IOException("Connection Refused, not authorized.");

								default:
									throw new IOException("Unrecognized error code returned: " + ReturnCode.ToString());
							}
						}
						catch (Exception ex)
						{
							await this.ConnectionError(this, ex);
							this.DisposeClient();
							return false;
						}
						break;

					case MqttControlPacketType.PINGREQ:
						await this.PINGRESP();
						break;

					case MqttControlPacketType.PINGRESP:
						EventHandlerAsync h = this.OnPingResponse;
						if (h != null)
						{
							try
							{
								await h(this, new EventArgs());
							}
							catch (Exception ex)
							{
								this.Exception(ex);
								Log.Critical(ex);
							}
						}
						break;

					case MqttControlPacketType.PUBLISH:
						string Topic = Packet.ReadString();

						if (Header.QualityOfService > MqttQualityOfService.AtMostOnce)
							Header.PacketIdentifier = Packet.ReadUInt16();
						else
							Header.PacketIdentifier = 0;

						int c = Packet.BytesLeft;
						byte[] Data = Packet.ReadBytes(c);
						MqttContent Content = new MqttContent(Header, Topic, Data);

						switch (Header.QualityOfService)
						{
							case MqttQualityOfService.AtMostOnce:
								await this.ContentReceived(Content);
								break;

							case MqttQualityOfService.AtLeastOnce:
								await this.PUBACK(Header.PacketIdentifier);
								await this.ContentReceived(Content);
								break;

							case MqttQualityOfService.ExactlyOnce:
								lock (this.synchObj)
								{
									this.contentCache[Header.PacketIdentifier] = Content;
								}
								await this.PUBREC(Header.PacketIdentifier);
								break;
						}
						break;

					case MqttControlPacketType.PUBACK:
						this.PacketDelivered(Header.PacketIdentifier);
						PacketAcknowledgedEventHandler h2 = this.OnPublished;
						if (h2 != null)
						{
							try
							{
								await h2(this, Header.PacketIdentifier);
							}
							catch (Exception ex)
							{
								this.Exception(ex);
							}
						}
						break;

					case MqttControlPacketType.PUBREC:
						this.PacketDelivered(Header.PacketIdentifier);
						await this.PUBREL(Header.PacketIdentifier);
						break;

					case MqttControlPacketType.PUBREL:
						lock (this.synchObj)
						{
							if (this.contentCache.TryGetValue(Header.PacketIdentifier, out Content))
								this.contentCache.Remove(Header.PacketIdentifier);
							else
								Content = null;
						}
						await this.PUBCOMP(Header.PacketIdentifier);

						if (Content != null)
							await this.ContentReceived(Content);
						break;

					case MqttControlPacketType.PUBCOMP:
						this.PacketDelivered(Header.PacketIdentifier);
						h2 = this.OnPublished;
						if (h2 != null)
						{
							try
							{
								await h2(this, Header.PacketIdentifier);
							}
							catch (Exception ex)
							{
								this.Exception(ex);
							}
						}
						break;

					case MqttControlPacketType.SUBACK:
						this.PacketDelivered(Header.PacketIdentifier);
						h2 = this.OnSubscribed;
						if (h2 != null)
						{
							try
							{
								await h2(this, Header.PacketIdentifier);
							}
							catch (Exception ex)
							{
								this.Exception(ex);
							}
						}
						break;

					case MqttControlPacketType.UNSUBACK:
						this.PacketDelivered(Header.PacketIdentifier);
						h2 = this.OnUnsubscribed;
						if (h2 != null)
						{
							try
							{
								await h2(this, Header.PacketIdentifier);
							}
							catch (Exception ex)
							{
								this.Exception(ex);
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				await this.Error(ex);
			}

			return true;
		}

		private async Task Error(Exception ex)
		{
			MqttExceptionEventHandler h = this.OnError;
			if (h != null)
			{
				try
				{
					await h(this, ex);
				}
				catch (Exception ex2)
				{
					this.Exception(ex2);
					Log.Critical(ex2);
				}
			}
		}

		private int inputState = 0;
		private MemoryStream inputPacket = null;
		private MqttControlPacketType inputPacketType;
		private int inputRemainingLength;
		private int inputOffset;

		/// <summary>
		/// Sends a PING message to the server. This is automatically done to keep the connection alive. Only call this method
		/// if you want to send additional PING messages, apart from the ones sent to keep the connection alive.
		/// </summary>
		public async Task PING()
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PINGREQ << 4);
			Packet.WriteUInt(0);

			byte[] PacketData = Packet.GetPacket();

			await this.Write(PacketData, 0, null);

			EventHandlerAsync h = this.OnPing;
			if (h != null)
			{
				try
				{
					await h(this, new EventArgs());
				}
				catch (Exception ex)
				{
					this.Exception(ex);
					Log.Critical(ex);
				}
			}
		}

		private Task PINGRESP()
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PINGRESP << 4);
			Packet.WriteUInt(0);

			byte[] PacketData = Packet.GetPacket();

			return this.Write(PacketData, 0, null);
		}

		/// <summary>
		/// Event raised when a connection to a broker could not be made.
		/// </summary>
		public event MqttExceptionEventHandler OnConnectionError = null;

		/// <summary>
		/// Event raised when an error was encountered.
		/// </summary>
		public event MqttExceptionEventHandler OnError = null;

		/// <summary>
		/// Event raised whenever a ping message is sent, i.e. when connection is idle.
		/// </summary>
		public event EventHandlerAsync OnPing = null;

		/// <summary>
		/// Event raised whenever a ping response is received, i.e. when connection is kept active, while idle.
		/// </summary>
		public event EventHandlerAsync OnPingResponse = null;

		/// <summary>
		/// Host or IP address of MQTT server.
		/// </summary>
		public string Host
		{
			get { return this.host; }
		}

		/// <summary>
		/// Port number to connect to.
		/// </summary>
		public int Port
		{
			get { return this.port; }
		}

		/// <summary>
		/// If encryption is to be used or not.
		/// </summary>
		public bool Tls
		{
			get { return this.tls; }
		}

		/// <summary>
		/// If server should be trusted, regardless if the operating system could validate its certificate or not.
		/// </summary>
		public bool TrustServer
		{
			get { return this.trustServer; }
			set { this.trustServer = value; }
		}

		/// <summary>
		/// Client ID
		/// </summary>
		public string ClientId
		{
			get { return this.clientId; }
		}

		/// <summary>
		/// Certificate used by the server.
		/// </summary>
#if WINDOWS_UWP
		public Certificate ServerCertificate
#else
		public X509Certificate ServerCertificate
#endif
		{
			get { return this.client?.RemoteCertificate; }
		}

		/// <summary>
		/// If the server certificate is valid.
		/// </summary>
		public bool ServerCertificateValid => this.client?.RemoteCertificateValid ?? false;

		/// <summary>
		/// Current state of connection.
		/// </summary>
		public MqttState State
		{
			get { return this.state; }
			internal set
			{
				if (this.state != value)
				{
					this.state = value;

					this.Information("Switching state to " + value.ToString());

					StateChangedEventHandler h = this.OnStateChanged;
					if (h != null)
					{
						try
						{
							h(this, value);
						}
						catch (Exception ex)
						{
							this.Exception(ex);
							Log.Critical(ex);
						}
					}
				}
			}
		}

		/// <summary>
		/// Event raised whenever the internal state of the connection changes.
		/// </summary>
		public event StateChangedEventHandler OnStateChanged = null;

		private ushort packetIdentifier = 0;

		/// <summary>
		/// Publishes information on a topic.
		/// </summary>
		/// <param name="Topic">Topic name</param>
		/// <param name="QoS">Quality of service</param>
		/// <param name="Retain">If topic should retain information.</param>
		/// <param name="Data">Binary data to send.</param>
		/// <returns>Packet identifier assigned to data.</returns>
		public Task<ushort> PUBLISH(string Topic, MqttQualityOfService QoS, bool Retain, byte[] Data)
		{
			return this.PUBLISH(Topic, QoS, Retain, false, Data);
		}

		/// <summary>
		/// Publishes information on a topic.
		/// </summary>
		/// <param name="Topic">Topic name</param>
		/// <param name="QoS">Quality of service</param>
		/// <param name="Retain">If topic should retain information.</param>
		/// <param name="Data">Binary data to send.</param>
		/// <returns>Packet identifier assigned to data.</returns>
		public Task<ushort> PUBLISH(string Topic, MqttQualityOfService QoS, bool Retain, BinaryOutput Data)
		{
			return this.PUBLISH(Topic, QoS, Retain, false, Data.GetPacket());
		}

		private async Task<ushort> PUBLISH(string Topic, MqttQualityOfService QoS, bool Retain, bool Duplicate, byte[] Data)
		{
			BinaryOutput Payload = new BinaryOutput();
			ushort PacketIdentifier;

			Payload.WriteString(Topic);

			if (QoS > MqttQualityOfService.AtMostOnce)
			{
				PacketIdentifier = this.packetIdentifier++;
				if (PacketIdentifier == 0)
					PacketIdentifier = this.packetIdentifier++;

				Payload.WriteUInt16(PacketIdentifier);
			}
			else
				PacketIdentifier = 0;

			Payload.WriteBytes(Data);

			byte[] PayloadData = Payload.GetPacket();

			BinaryOutput Packet = new BinaryOutput();
			byte b = (byte)((int)MqttControlPacketType.PUBLISH << 4);
			if (Duplicate)
				b |= 8;

			b |= (byte)((int)QoS << 1);

			if (Retain)
				b |= 1;

			Packet.WriteByte(b);
			Packet.WriteUInt((uint)PayloadData.Length);
			Packet.WriteBytes(PayloadData);

			byte[] PacketData = Packet.GetPacket();

			await this.Write(PacketData, PacketIdentifier, null);

			return PacketIdentifier;
		}

		/// <summary>
		/// Event raised when data has been successfully published.
		/// </summary>
		public event PacketAcknowledgedEventHandler OnPublished = null;

		/// <summary>
		/// Event raised when subscription has been successfully received.
		/// </summary>
		public event PacketAcknowledgedEventHandler OnSubscribed = null;

		private Task PUBACK(ushort PacketIdentifier)
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PUBACK << 4);
			Packet.WriteUInt(2);
			Packet.WriteUInt16(PacketIdentifier);

			byte[] PacketData = Packet.GetPacket();

			return this.Write(PacketData, 0, null);
		}

		private Task PUBREC(ushort PacketIdentifier)
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PUBREC << 4);
			Packet.WriteUInt(2);
			Packet.WriteUInt16(PacketIdentifier);

			byte[] PacketData = Packet.GetPacket();

			return this.Write(PacketData, 0, null);
		}

		private Task PUBREL(ushort PacketIdentifier)
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)(((int)MqttControlPacketType.PUBREL << 4) | 2));
			Packet.WriteUInt(2);
			Packet.WriteUInt16(PacketIdentifier);

			byte[] PacketData = Packet.GetPacket();

			return this.Write(PacketData, PacketIdentifier, null);
		}

		private Task PUBCOMP(ushort PacketIdentifier)
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PUBCOMP << 4);
			Packet.WriteUInt(2);
			Packet.WriteUInt16(PacketIdentifier);

			byte[] PacketData = Packet.GetPacket();

			return this.Write(PacketData, 0, null);
		}

		/// <summary>
		/// Subscribes to information from a topic. Topics can include wildcards.
		/// </summary>
		/// <param name="Topic">Topic string.</param>
		/// <param name="QoS">Quality of Service.</param>
		/// <returns>Packet identifier assigned to subscription.</returns>
		public Task<ushort> SUBSCRIBE(string Topic, MqttQualityOfService QoS)
		{
			return this.SUBSCRIBE(new KeyValuePair<string, MqttQualityOfService>(Topic, QoS));
		}

		/// <summary>
		/// Subscribes to information from a set of topics. Topics can include wildcards.
		/// </summary>
		/// <param name="Topics">Topics</param>
		/// <returns>Packet identifier assigned to subscription.</returns>
		public Task<ushort> SUBSCRIBE(params string[] Topics)
		{
			int i, c = Topics.Length;
			KeyValuePair<string, MqttQualityOfService>[] Topics2 = new KeyValuePair<string, MqttQualityOfService>[c];

			for (i = 0; i < c; i++)
				Topics2[i] = new KeyValuePair<string, MqttQualityOfService>(Topics[i], MqttQualityOfService.ExactlyOnce);

			return SUBSCRIBE(Topics2);
		}

		/// <summary>
		/// Subscribes to information from a set of topics. Topics can include wildcards.
		/// </summary>
		/// <param name="Topics">Topics together with Quality of Service levels for each topic.</param>
		/// <returns>Packet identifier assigned to subscription.</returns>
		public async Task<ushort> SUBSCRIBE(params KeyValuePair<string, MqttQualityOfService>[] Topics)
		{
			BinaryOutput Payload = new BinaryOutput();
			ushort PacketIdentifier;

			PacketIdentifier = this.packetIdentifier++;
			if (PacketIdentifier == 0)
				PacketIdentifier = this.packetIdentifier++;

			Payload.WriteUInt16(PacketIdentifier);

			foreach (KeyValuePair<string, MqttQualityOfService> Pair in Topics)
			{
				Payload.WriteString(Pair.Key);
				Payload.WriteByte((byte)Pair.Value);
			}

			byte[] PayloadData = Payload.GetPacket();

			BinaryOutput Packet = new BinaryOutput();
			byte b = (byte)((int)MqttControlPacketType.SUBSCRIBE << 4);
			b |= 2;

			Packet.WriteByte(b);
			Packet.WriteUInt((uint)PayloadData.Length);
			Packet.WriteBytes(PayloadData);

			byte[] PacketData = Packet.GetPacket();

			await this.Write(PacketData, PacketIdentifier, null);

			return PacketIdentifier;
		}

		private async Task ContentReceived(MqttContent Content)
		{
			ContentReceivedEventHandler h = this.OnContentReceived;
			if (h != null)
			{
				try
				{
					await h(this, Content);
				}
				catch (Exception ex)
				{
					this.Exception(ex);
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when new content has been received.
		/// </summary>
		public event ContentReceivedEventHandler OnContentReceived = null;

		/// <summary>
		/// Unsubscribes from information earlier subscribed to. Topics can include wildcards.
		/// </summary>
		/// <param name="Topics">Topics</param>
		/// <returns>Packet identifier assigned to unsubscription.</returns>
		public async Task<ushort> UNSUBSCRIBE(params string[] Topics)
		{
			BinaryOutput Payload = new BinaryOutput();
			ushort PacketIdentifier;

			PacketIdentifier = this.packetIdentifier++;
			if (PacketIdentifier == 0)
				PacketIdentifier = this.packetIdentifier++;

			Payload.WriteUInt16(PacketIdentifier);

			foreach (string Topic in Topics)
				Payload.WriteString(Topic);

			byte[] PayloadData = Payload.GetPacket();

			BinaryOutput Packet = new BinaryOutput();
			byte b = (byte)((int)MqttControlPacketType.UNSUBSCRIBE << 4);
			b |= 2;

			Packet.WriteByte(b);
			Packet.WriteUInt((uint)PayloadData.Length);
			Packet.WriteBytes(PayloadData);

			byte[] PacketData = Packet.GetPacket();

			await this.Write(PacketData, PacketIdentifier, null);

			return PacketIdentifier;
		}

		/// <summary>
		/// Event raised when unsubscription has been successfully received.
		/// </summary>
		public event PacketAcknowledgedEventHandler OnUnsubscribed = null;

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		public void Dispose()
		{
			Task _ = this.DisposeAsync();
		}

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		public async Task DisposeAsync()
		{ 
			if (this.state == MqttState.Connected)
				await this.DISCONNECT();

			lock (this.synchObj)
			{
				this.packetByTimeout?.Clear();
				this.timeoutByPacketIdentifier?.Clear();
				this.contentCache?.Clear();
			}

			this.secondTimer?.Dispose();
			this.secondTimer = null;

			this.DisposeClient();
		}

		private void DisposeClient()
		{
			this.client?.Dispose();
			this.client = null;
		}

		private async Task DISCONNECT()
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.DISCONNECT << 4);
			Packet.WriteUInt(2);

			byte[] PacketData = Packet.GetPacket();

			TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();

			await this.Write(PacketData, 0, (sender, e) =>
			{
				this.State = MqttState.Offline;
				Done.TrySetResult(true);
			});

			await Task.WhenAny(Done.Task, Task.Delay(1000));
		}

	}
}
