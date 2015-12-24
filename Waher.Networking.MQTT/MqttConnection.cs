//#define LineListener

using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Waher.Events;

namespace Waher.Networking.MQTT
{
	/// <summary>
	/// Connection error event handler.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Exception">Information about error received.</param>
	public delegate void MqttExceptionEventHandler(MqttConnection Sender, Exception Exception);

	/// <summary>
	/// Event handler for state change events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewState">New state reported.</param>
	public delegate void StateChangedEventHandler(MqttConnection Sender, MqttState NewState);

	/// <summary>
	/// Event handler used for events raised when data has been successfully acknowledged.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="PacketIdentifier">Packet identifier of data successfully published.</param>
#pragma warning disable
	public delegate void PacketAcknowledgedEventHandler(MqttConnection Sender, ushort PacketIdentifier);
#pragma warning enable

	/// <summary>
	/// Event handler for events raised when content has been received.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Content">Content received.</param>
	public delegate void ContentReceivedEventHandler(MqttConnection Sender, MqttContent Content);

	/// <summary>
	/// Manages an MQTT connection. Implements MQTT v3.1.1, as defined in
	/// http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
	/// </summary>
	public class MqttConnection : IDisposable
	{
		private const int BufferSize = 16384;
		private const int KeepAliveTimeSeconds = 30;

		private Dictionary<ushort, MqttContent> contentCache = new Dictionary<ushort, MqttContent>();
		private byte[] buffer = new byte[BufferSize];
		private string clientId = Guid.NewGuid().ToString().Substring(0, 23);
		private TcpClient client = null;
		private Stream stream = null;
		private Timer secondTimer = null;
		private DateTime nextPing = DateTime.MinValue;
		private string host;
		private string userName;
		private string password;
		private int port;
		private int keepAliveSeconds;
		private MqttState state;
		private bool tls;
		private bool trustServer = false;

		/// <summary>
		/// Manages an MQTT connection. Implements MQTT v3.1.1, as defined in
		/// http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
		/// </summary>
		/// <param name="Host">Host name or IP address of MQTT server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="Tls">If TLS is used to encrypt communication.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		public MqttConnection(string Host, int Port, bool Tls, string UserName, string Password)
		{
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.userName = UserName;
			this.password = Password;
			this.state = MqttState.Connecting;
			this.client = new TcpClient();
			this.client.BeginConnect(Host, Port, this.ConnectCallback, null);
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				this.client.EndConnect(ar);
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
				return;
			}

			if (this.tls)
			{
				this.State = MqttState.StartingEncryption;

				SslStream SslStream = new SslStream(this.client.GetStream(), false, this.RemoteCertificateValidationCallback);
				this.stream = SslStream;

				SslStream.BeginAuthenticateAsClient(this.host, null, SslProtocols.Tls, true, this.AuthenticateAsClientCallback, null);
			}
			else
			{
				this.stream = this.client.GetStream();
				this.CONNECT(KeepAliveTimeSeconds);
			}
		}

		private void ConnectionError(Exception ex)
		{
			MqttExceptionEventHandler h = this.OnConnectionError;
			if (h != null)
			{
				try
				{
					h(this, ex);
				}
				catch (Exception ex2)
				{
					Log.Critical(ex2);
				}
			}

			this.Error(ex);

			this.State = MqttState.Error;
		}

		private void AuthenticateAsClientCallback(IAsyncResult ar)
		{
			try
			{
				((SslStream)this.stream).EndAuthenticateAsClient(ar);
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
				return;
			}

			this.CONNECT(KeepAliveTimeSeconds);
		}

		private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors != SslPolicyErrors.None)
				return this.trustServer;

			return true;
		}

		private void CONNECT(int KeepAliveSeconds)
		{
			this.State = MqttState.Authenticating;
			this.keepAliveSeconds = KeepAliveSeconds;
			this.nextPing = DateTime.Now.AddMilliseconds(KeepAliveSeconds * 500);
			this.secondTimer = new Timer(this.secondTimer_Elapsed, null, 1000, 1000);

			BinaryOutput Payload = new BinaryOutput();
			Payload.WriteString("MQTT");
			Payload.WriteByte(4);	// v3.1.1

			byte b = 2;		// Clean session.

			Payload.WriteByte(b);

			Payload.WriteByte((byte)(KeepAliveSeconds >> 8));
			Payload.WriteByte((byte)KeepAliveSeconds);

			Payload.WriteString(this.clientId);
			if (!string.IsNullOrEmpty(this.userName))
			{
				b |= 128;
				Payload.WriteString(this.userName);

				if (!string.IsNullOrEmpty(this.password))
				{
					b |= 64;
					Payload.WriteString(this.password);
				}
			}

			byte[] PayloadData = Payload.GetPacket();

			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.CONNECT << 4);
			Packet.WriteUInt((uint)PayloadData.Length);
			Packet.WriteBytes(PayloadData);

			byte[] PacketData = Packet.GetPacket();

			this.BeginWrite(PacketData, 0);
			this.inputState = 0;
			this.BeginRead();
		}

		private void BeginWrite(byte[] Packet, int PacketIdentifier)
		{
			lock (this.outputQueue)
			{
				if (this.isWriting)
					this.outputQueue.AddLast(new KeyValuePair<byte[], int>(Packet, PacketIdentifier));
				else
					this.DoBeginWriteLocked(Packet, PacketIdentifier);
			}
		}

		private void DoBeginWriteLocked(byte[] Packet, int PacketIdentifier)
		{
#if LineListener
			StringBuilder sb = null;

			foreach (byte b in Packet)
			{
				if (sb == null)
					sb = new StringBuilder();
				else
					sb.Append(' ');

				sb.Append(b.ToString("X2"));
			}

			LLOut(sb.ToString(), Color.Black, Color.White);
#endif
			this.stream.BeginWrite(Packet, 0, Packet.Length, this.EndWrite, null);
			this.isWriting = true;

			if (PacketIdentifier != 0 && ((Packet[0] >> 1) & 3) > 0)
			{
				DateTime Timeout = DateTime.Now.AddSeconds(2);
				while (this.packetByTimeout.ContainsKey(Timeout))
					Timeout = Timeout.AddTicks(rnd.Next(1, 10));

				this.packetByTimeout[Timeout] = new KeyValuePair<byte[], int>(Packet, PacketIdentifier);
				this.timeoutByPacketIdentifier[PacketIdentifier] = Timeout;
			}
		}

		private void PacketDelivered(int PacketIdentifier)
		{
			lock (this.outputQueue)
			{
				DateTime TP;

				if (this.timeoutByPacketIdentifier.TryGetValue(PacketIdentifier, out TP))
				{
					this.timeoutByPacketIdentifier.Remove(PacketIdentifier);
					this.packetByTimeout.Remove(TP);
				}
			}
		}

		private void secondTimer_Elapsed(object State)
		{
			DateTime Now = DateTime.Now;

			if (Now >= this.nextPing)
			{
				this.PING();
				this.nextPing = Now.AddMilliseconds(this.keepAliveSeconds * 500);
			}

			LinkedList<KeyValuePair<DateTime, KeyValuePair<byte[], int>>> Resend = null;

			lock (this.outputQueue)
			{
				foreach (KeyValuePair<DateTime, KeyValuePair<byte[], int>> P in this.packetByTimeout)
				{
					if (Now < P.Key)
						break;

					if (Resend == null)
						Resend = new LinkedList<KeyValuePair<DateTime, KeyValuePair<byte[], int>>>();

					Resend.AddLast(P);
				}

				if (Resend != null)
				{
					foreach (KeyValuePair<DateTime, KeyValuePair<byte[], int>> P in Resend)
					{
						this.packetByTimeout.Remove(P.Key);
						this.timeoutByPacketIdentifier.Remove(P.Value.Value);
					}
				}
			}

			if (Resend != null)
			{
				foreach (KeyValuePair<DateTime, KeyValuePair<byte[], int>> P in Resend)
					this.BeginWrite(P.Value.Key, P.Value.Value);
			}
		}

		private LinkedList<KeyValuePair<byte[], int>> outputQueue = new LinkedList<KeyValuePair<byte[], int>>();
		private SortedDictionary<DateTime, KeyValuePair<byte[], int>> packetByTimeout = new SortedDictionary<DateTime, KeyValuePair<byte[], int>>();
		private SortedDictionary<int, DateTime> timeoutByPacketIdentifier = new SortedDictionary<int, DateTime>();
		private Random rnd = new Random();
		private bool isWriting = false;

#if LineListener
		private void LLOut(string s, Color Fg, Color Bg)
		{
			Color FgBak = RetroApplication.ForegroundColor;
			Color BgBak = RetroApplication.BackgroundColor;

			RetroApplication.ForegroundColor = Fg;
			RetroApplication.BackgroundColor = Bg;

			Console.Out.WriteLine(s);

			RetroApplication.ForegroundColor = FgBak;
			RetroApplication.BackgroundColor = BgBak;
		}
#endif

		private void EndWrite(IAsyncResult ar)
		{
			if (this.stream == null)
				return;

			try
			{
				this.stream.EndWrite(ar);

				this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);

				lock (this.outputQueue)
				{
					LinkedListNode<KeyValuePair<byte[], int>> Next = this.outputQueue.First;

					if (Next == null)
						this.isWriting = false;
					else
					{
						this.outputQueue.RemoveFirst();
						this.DoBeginWriteLocked(Next.Value.Key, Next.Value.Value);
					}
				}
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);

				lock (this.outputQueue)
				{
					this.outputQueue.Clear();
					this.isWriting = false;
				}
			}
		}

		private void BeginRead()
		{
			this.stream.BeginRead(this.buffer, 0, BufferSize, this.EndRead, null);
		}

		private void EndRead(IAsyncResult ar)
		{
			int NrRead;

			if (this.stream == null)
				return;

			try
			{
				NrRead = this.stream.EndRead(ar);
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
				return;
			}

			bool ContinueReading = true;

			try
			{
				if (NrRead > 0)
				{
					int i;
					byte b;

#if LineListener
					StringBuilder sb = null;

					for (i = 0; i < NrRead; i++)
					{
						b = this.buffer[i];

						if (sb == null)
							sb = new StringBuilder();
						else
							sb.Append(' ');

						sb.Append(b.ToString("X2"));
					}

					LLOut(sb.ToString(), Color.White, Color.Navy);
#endif

					for (i = 0; i < NrRead; i++)
					{
						b = this.buffer[i];

						switch (this.inputState)
						{
							case 0:
								this.inputPacket = new MemoryStream();
								this.inputPacket.WriteByte(b);
								this.inputPacketType = (MqttControlPacketType)(b >> 4);
								this.inputPacketQoS = (MqttQualityOfService)((b >> 1) & 3);
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
												if (!this.ProcessInputPacket())
													ContinueReading = false;
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
									if (!this.ProcessInputPacket())
										ContinueReading = false;
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
									if (!this.ProcessInputPacket())
										ContinueReading = false;
								}
								break;
						}
					}
				}
			}
			finally
			{
				if (ContinueReading && this.stream != null)
					this.BeginRead();
			}
		}

		private bool ProcessInputPacket()
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
							this.ConnectionError(ex);
							this.stream.Close();
							this.client.Close();
							return false;
						}
						break;

					case MqttControlPacketType.PINGREQ:
						this.PINGRESP();
						break;

					case MqttControlPacketType.PINGRESP:
						EventHandler h = this.OnPingResponse;
						if (h != null)
						{
							try
							{
								h(this, new EventArgs());
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}
						break;

					case MqttControlPacketType.PUBLISH:
						string Topic = Packet.ReadString();
						
						if (Header.QualityOfService > MqttQualityOfService.AtMostOne)
							Header.PacketIdentifier = Packet.ReadUInt16();
						else
							Header.PacketIdentifier = 0;

						int c = Packet.BytesLeft;
						byte[] Data = Packet.ReadBytes(c);
						MqttContent Content = new MqttContent(Header, Topic, Data);

						switch (Header.QualityOfService)
						{
							case MqttQualityOfService.AtMostOne:
								this.ContentReceived(Content);
								break;

							case MqttQualityOfService.AtLeastOne:
								this.PUBACK(Header.PacketIdentifier);
								this.ContentReceived(Content);
								break;

							case MqttQualityOfService.ExactlyOne:
								lock (this.contentCache)
								{
									this.contentCache[Header.PacketIdentifier] = Content;
								}
								this.PUBREC(Header.PacketIdentifier);
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
								h2(this, Header.PacketIdentifier);
							}
							catch (Exception ex)
							{
#if LineListener
								LLOut(ex.Message, Color.Yellow, Color.DarkRed);
#endif
							}
						}
						break;

					case MqttControlPacketType.PUBREC:
						this.PacketDelivered(Header.PacketIdentifier);
						this.PUBREL(Header.PacketIdentifier);
						break;

					case MqttControlPacketType.PUBREL:
						lock (this.contentCache)
						{
							if (this.contentCache.TryGetValue(Header.PacketIdentifier, out Content))
								this.contentCache.Remove(Header.PacketIdentifier);
							else
								Content = null;
						}
						this.PUBCOMP(Header.PacketIdentifier);

						if (Content != null)
							this.ContentReceived(Content);
						break;

					case MqttControlPacketType.PUBCOMP:
						this.PacketDelivered(Header.PacketIdentifier);
						h2 = this.OnPublished;
						if (h2 != null)
						{
							try
							{
								h2(this, Header.PacketIdentifier);
							}
							catch (Exception ex)
							{
#if LineListener
								LLOut(ex.Message, Color.Yellow, Color.DarkRed);
#endif
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
								h2(this, Header.PacketIdentifier);
							}
							catch (Exception ex)
							{
#if LineListener
								LLOut(ex.Message, Color.Yellow, Color.DarkRed);
#endif
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
								h2(this, Header.PacketIdentifier);
							}
							catch (Exception ex)
							{
#if LineListener
								LLOut(ex.Message, Color.Yellow, Color.DarkRed);
#endif
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				this.Error(ex);
			}

			return true;
		}

		private void Error(Exception ex)
		{
			MqttExceptionEventHandler h = this.OnError;
			if (h != null)
			{
				try
				{
					h(this, ex);
				}
				catch (Exception ex2)
				{
					Log.Critical(ex2);
				}
			}
		}

		private int inputState = 0;
		private MemoryStream inputPacket = null;
		private MqttControlPacketType inputPacketType;
		private MqttQualityOfService inputPacketQoS;
		private int inputRemainingLength;
		private int inputOffset;

		/// <summary>
		/// Sends a PING message to the server. This is automatically done to keep the connection alive. Only call this method
		/// if you want to send additional PING messages, apart from the ones sent to keep the connection alive.
		/// </summary>
		public void PING()
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PINGREQ << 4);
			Packet.WriteUInt(0);

			byte[] PacketData = Packet.GetPacket();

			this.BeginWrite(PacketData, 0);

			EventHandler h = this.OnPing;
			if (h != null)
			{
				try
				{
					h(this, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private void PINGRESP()
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PINGRESP << 4);
			Packet.WriteUInt(0);

			byte[] PacketData = Packet.GetPacket();

			this.BeginWrite(PacketData, 0);
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
		public event EventHandler OnPing = null;

		/// <summary>
		/// Event raised whenever a ping response is received, i.e. when connection is kept active, while idle.
		/// </summary>
		public event EventHandler OnPingResponse = null;

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

					StateChangedEventHandler h = this.OnStateChanged;
					if (h != null)
					{
						try
						{
							h(this, value);
						}
						catch (Exception ex)
						{
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
		/// <param name="Retain">If topic shoudl retain information.</param>
		/// <param name="Data">Binary data to send.</param>
		/// <returns>Packet identifier assigned to data.</returns>
		public int PUBLISH(string Topic, MqttQualityOfService QoS, bool Retain, byte[] Data)
		{
			return this.PUBLISH(Topic, QoS, Retain, false, Data);
		}

		/// <summary>
		/// Publishes information on a topic.
		/// </summary>
		/// <param name="Topic">Topic name</param>
		/// <param name="QoS">Quality of service</param>
		/// <param name="Retain">If topic shoudl retain information.</param>
		/// <param name="Data">Binary data to send.</param>
		/// <returns>Packet identifier assigned to data.</returns>
		public int PUBLISH(string Topic, MqttQualityOfService QoS, bool Retain, BinaryOutput Data)
		{
			return this.PUBLISH(Topic, QoS, Retain, false, Data.GetPacket());
		}

		private ushort PUBLISH(string Topic, MqttQualityOfService QoS, bool Retain, bool Duplicate, byte[] Data)
		{
			BinaryOutput Payload = new BinaryOutput();
			ushort PacketIdentifier;

			Payload.WriteString(Topic);

			if (QoS > MqttQualityOfService.AtMostOne)
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

			this.BeginWrite(PacketData, PacketIdentifier);

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

		private void PUBACK(ushort PacketIdentifier)
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PUBACK << 4);
			Packet.WriteUInt(2);
			Packet.WriteUInt16(PacketIdentifier);

			byte[] PacketData = Packet.GetPacket();

			this.BeginWrite(PacketData, 0);
		}

		private void PUBREC(ushort PacketIdentifier)
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PUBREC << 4);
			Packet.WriteUInt(2);
			Packet.WriteUInt16(PacketIdentifier);

			byte[] PacketData = Packet.GetPacket();

			this.BeginWrite(PacketData, 0);
		}

		private void PUBREL(ushort PacketIdentifier)
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)(((int)MqttControlPacketType.PUBREL << 4) | 2));
			Packet.WriteUInt(2);
			Packet.WriteUInt16(PacketIdentifier);

			byte[] PacketData = Packet.GetPacket();

			this.BeginWrite(PacketData, PacketIdentifier);
		}

		private void PUBCOMP(ushort PacketIdentifier)
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.PUBCOMP << 4);
			Packet.WriteUInt(2);
			Packet.WriteUInt16(PacketIdentifier);

			byte[] PacketData = Packet.GetPacket();

			this.BeginWrite(PacketData, 0);
		}

		/// <summary>
		/// Subscribes to information from a set of topics. Topics can include wildcards.
		/// </summary>
		/// <param name="Topics">Topics</param>
		/// <returns>Packet identifier assigned to subscription.</returns>
		public ushort SUBSCRIBE(params string[] Topics)
		{
			int i, c = Topics.Length;
			KeyValuePair<string, MqttQualityOfService>[] Topics2 = new KeyValuePair<string, MqttQualityOfService>[c];

			for (i = 0; i < c; i++)
				Topics2[i] = new KeyValuePair<string, MqttQualityOfService>(Topics[i], MqttQualityOfService.ExactlyOne);

			return SUBSCRIBE(Topics2);
		}

		/// <summary>
		/// Subscribes to information from a set of topics. Topics can include wildcards.
		/// </summary>
		/// <param name="Topics">Topics together with Quality of Service levels for each topic.</param>
		/// <returns>Packet identifier assigned to subscription.</returns>
		public ushort SUBSCRIBE(params KeyValuePair<string, MqttQualityOfService>[] Topics)
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

			this.BeginWrite(PacketData, PacketIdentifier);

			return PacketIdentifier;
		}

		private void ContentReceived(MqttContent Content)
		{
			ContentReceivedEventHandler h = this.OnContentReceived;
			if (h != null)
			{
				try
				{
					h(this, Content);
				}
				catch (Exception ex)
				{
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
		public ushort UNSUBSCRIBE(params string[] Topics)
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

			this.BeginWrite(PacketData, PacketIdentifier);

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
			if (this.state == MqttState.Connected)
				this.DISCONNECT();

			if (this.outputQueue != null)
			{
				lock (this.outputQueue)
				{
					this.outputQueue.Clear();
					this.packetByTimeout.Clear();
					this.timeoutByPacketIdentifier.Clear();
				}
			}

			if (this.contentCache != null)
			{
				lock (this.contentCache)
				{
					this.contentCache.Clear();
				}
			}

			if (this.secondTimer != null)
			{
				this.secondTimer.Dispose();
				this.secondTimer = null;
			}

			if (this.stream != null)
			{
				this.stream.Dispose();
				this.stream = null;
			}

			if (this.client != null)
			{
				this.client.Close();
				this.client = null;
			}
		}

		private void DISCONNECT()
		{
			BinaryOutput Packet = new BinaryOutput();
			Packet.WriteByte((byte)MqttControlPacketType.DISCONNECT << 4);
			Packet.WriteUInt(2);

			byte[] PacketData = Packet.GetPacket();

			ManualResetEvent Done = new ManualResetEvent(false);

			this.stream.BeginWrite(PacketData, 0, PacketData.Length, this.EndDisconnect, Done);

			Done.WaitOne(1000);
		}

		private void EndDisconnect(IAsyncResult ar)
		{
			ManualResetEvent Done = (ManualResetEvent)ar.AsyncState;
			try
			{
				if (this.stream != null)
					this.stream.EndWrite(ar);

				this.State = MqttState.Offline;
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
			}
			finally
			{
				Done.Set();
			}
		}

	}
}
