using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking;
using Waher.Things.Matter.Crypto;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// Event arguments for unsolicited messages (messages initiating new exchanges from peers).
	/// </summary>
	public class UnsolicitedMessageEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for unsolicited messages.
		/// </summary>
		/// <param name="Exchange">New exchange, created for the message.</param>
		/// <param name="Message">Message.</param>
		public UnsolicitedMessageEventArgs(Exchange Exchange, MatterMessage Message)
		{
			this.Exchange = Exchange;
			this.Message = Message;
		}

		/// <summary>
		/// New exchange, created for the message.
		/// </summary>
		public Exchange Exchange { get; }

		/// <summary>
		/// Message.
		/// </summary>
		public MatterMessage Message { get; }

		/// <summary>
		/// Set to true by the handler that processes the message.
		/// </summary>
		public bool Handled { get; set; }
	}

	/// <summary>
	/// Delegate for unsolicited message handlers.
	/// </summary>
	/// <param name="Sender">Sender.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task UnsolicitedMessageEventHandler(object Sender, UnsolicitedMessageEventArgs e);

	/// <summary>
	/// Manages sessions, exchanges and the Message Reliability Protocol (MRP) on top of a UDP transport
	/// (Matter Core Specification, chapter 4).
	/// </summary>
	public class ExchangeManager : IDisposable
	{
		/// <summary>
		/// Maximum number of transmissions of a reliable message (MRP_MAX_TRANSMISSIONS).
		/// </summary>
		public const int MaxTransmissions = 5;

		/// <summary>
		/// MRP backoff base (MRP_BACKOFF_BASE).
		/// </summary>
		public const double BackoffBase = 1.6;

		/// <summary>
		/// MRP backoff jitter (MRP_BACKOFF_JITTER).
		/// </summary>
		public const double BackoffJitter = 0.25;

		/// <summary>
		/// MRP backoff margin (MRP_BACKOFF_MARGIN).
		/// </summary>
		public const double BackoffMargin = 1.1;

		/// <summary>
		/// MRP backoff threshold (MRP_BACKOFF_THRESHOLD).
		/// </summary>
		public const int BackoffThreshold = 1;

		/// <summary>
		/// Standalone acknowledgement timeout (MRP_STANDALONE_ACK_TIMEOUT).
		/// </summary>
		public static readonly TimeSpan StandaloneAckTimeout = TimeSpan.FromMilliseconds(200);

		/// <summary>
		/// Default expected processing time of a request.
		/// </summary>
		public static readonly TimeSpan DefaultProcessingTime = TimeSpan.FromSeconds(2);

		private static readonly Random jitter = new Random();

		private readonly Dictionary<ushort, SecureSession> secureSessions = new Dictionary<ushort, SecureSession>();
		private readonly Dictionary<ulong, UnsecuredSession> unsecuredSessions = new Dictionary<ulong, UnsecuredSession>();
		private readonly Dictionary<ExchangeKey, Exchange> exchanges = new Dictionary<ExchangeKey, Exchange>();
		private readonly UdpTransport transport;
		private readonly ICommunicationLayer sniffers;
		private readonly SessionParameters ownParameters;
		private uint unsecuredCounter;
		private ushort nextExchangeId;
		private ushort nextSessionId;
		private bool disposed = false;

		/// <summary>
		/// Manages sessions, exchanges and MRP.
		/// </summary>
		/// <param name="Transport">UDP transport.</param>
		/// <param name="Sniffers">Optional communication layer for sniffers.</param>
		public ExchangeManager(UdpTransport Transport, ICommunicationLayer Sniffers)
		{
			this.transport = Transport;
			this.sniffers = Sniffers;
			this.ownParameters = SessionParameters.Own;
			this.unsecuredCounter = (MatterCrypto.RandomUInt32() & 0x0fffffff) + 1;
			this.nextExchangeId = MatterCrypto.RandomUInt16();
			this.nextSessionId = (ushort)(MatterCrypto.RandomUInt16() | 1);

			this.transport.OnDatagramReceived += this.Transport_OnDatagramReceived;
		}

		/// <summary>
		/// UDP transport.
		/// </summary>
		public UdpTransport Transport => this.transport;

		/// <summary>
		/// Session parameters of the local node.
		/// </summary>
		public SessionParameters OwnParameters => this.ownParameters;

		/// <summary>
		/// Event raised when a peer initiates a new exchange.
		/// </summary>
		public event UnsolicitedMessageEventHandler OnUnsolicitedMessage = null;

		/// <summary>
		/// Event raised when a peer closes a secure session.
		/// </summary>
		public event EventHandlerAsync<SecureSession> OnSessionClosed = null;

		#region Sessions

		internal uint NextUnsecuredCounter()
		{
			lock (this.unsecuredSessions)
			{
				return this.unsecuredCounter++;
			}
		}

		/// <summary>
		/// Creates a new unsecured session for initiating session establishment with a peer.
		/// </summary>
		/// <param name="Peer">Address of peer.</param>
		/// <returns>Unsecured session.</returns>
		public UnsecuredSession CreateUnsecuredSession(IPEndPoint Peer)
		{
			lock (this.unsecuredSessions)
			{
				ulong Id;

				do
				{
					Id = MatterCrypto.RandomUInt64();
				}
				while (Id == 0 || this.unsecuredSessions.ContainsKey(Id));

				UnsecuredSession Session = new UnsecuredSession(Peer, Id, true);
				this.unsecuredSessions[Id] = Session;
				return Session;
			}
		}

		/// <summary>
		/// Allocates a free local session ID.
		/// </summary>
		/// <returns>Session ID.</returns>
		public ushort AllocateSessionId()
		{
			lock (this.secureSessions)
			{
				while (true)
				{
					ushort Id = this.nextSessionId++;
					if (Id == 0)
						continue;

					if (!this.secureSessions.ContainsKey(Id))
					{
						this.secureSessions[Id] = null;     // Reserve
						return Id;
					}
				}
			}
		}

		/// <summary>
		/// Releases a reserved session ID that was not used.
		/// </summary>
		/// <param name="Id">Session ID.</param>
		public void ReleaseSessionId(ushort Id)
		{
			lock (this.secureSessions)
			{
				if (this.secureSessions.TryGetValue(Id, out SecureSession Session) && Session is null)
					this.secureSessions.Remove(Id);
			}
		}

		/// <summary>
		/// Registers an established secure session.
		/// </summary>
		/// <param name="Session">Session.</param>
		public void AddSecureSession(SecureSession Session)
		{
			lock (this.secureSessions)
			{
				this.secureSessions[Session.LocalSessionId] = Session;
			}
		}

		/// <summary>
		/// Removes a session.
		/// </summary>
		/// <param name="Session">Session.</param>
		public void RemoveSession(Session Session)
		{
			Session.IsClosed = true;

			if (Session is SecureSession Secure)
			{
				lock (this.secureSessions)
				{
					if (this.secureSessions.TryGetValue(Secure.LocalSessionId, out SecureSession S) && (S == Secure || S is null))
						this.secureSessions.Remove(Secure.LocalSessionId);
				}
			}
			else if (Session is UnsecuredSession Unsecured)
			{
				lock (this.unsecuredSessions)
				{
					this.unsecuredSessions.Remove(Unsecured.EphemeralNodeId);
				}
			}

			List<Exchange> ToFail = null;

			lock (this.exchanges)
			{
				foreach (KeyValuePair<ExchangeKey, Exchange> P in this.exchanges)
				{
					if (P.Key.Session == Session)
					{
						ToFail ??= new List<Exchange>();
						ToFail.Add(P.Value);
					}
				}

				if (!(ToFail is null))
				{
					foreach (Exchange Ex in ToFail)
						this.exchanges.Remove(new ExchangeKey(Ex.Session, Ex.Id, Ex.IsInitiator));
				}
			}

			if (!(ToFail is null))
			{
				foreach (Exchange Ex in ToFail)
				{
					Ex.Pending?.Cancel.Cancel();
					Ex.Fail(new MatterException("Session closed."));
				}
			}
		}

		/// <summary>
		/// Sends a CloseSession status report to the peer and removes the session.
		/// </summary>
		/// <param name="Session">Secure session.</param>
		public async Task CloseSessionAsync(SecureSession Session)
		{
			if (Session.IsClosed)
				return;

			try
			{
				Exchange Ex = this.CreateExchange(Session);
				StatusReport Report = new StatusReport(GeneralStatusCode.Success, ProtocolIds.SecureChannel, (ushort)SecureChannelStatusCode.CloseSession);
				await Ex.SendAsync(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.StatusReport, Report.Encode(), false);
				await Ex.CloseAsync();
			}
			catch (Exception)
			{
				// Ignore
			}
			finally
			{
				this.RemoveSession(Session);
			}
		}

		/// <summary>
		/// Gets the currently registered secure sessions.
		/// </summary>
		/// <returns>Sessions.</returns>
		public SecureSession[] GetSecureSessions()
		{
			List<SecureSession> Result = new List<SecureSession>();

			lock (this.secureSessions)
			{
				foreach (SecureSession S in this.secureSessions.Values)
				{
					if (!(S is null))
						Result.Add(S);
				}
			}

			return Result.ToArray();
		}

		#endregion

		#region Exchanges

		/// <summary>
		/// Creates a new exchange, as initiator.
		/// </summary>
		/// <param name="Session">Session.</param>
		/// <returns>Exchange.</returns>
		public Exchange CreateExchange(Session Session)
		{
			if (Session.IsClosed)
				throw new MatterException("Session closed.");

			lock (this.exchanges)
			{
				while (true)
				{
					ushort Id = this.nextExchangeId++;
					ExchangeKey Key = new ExchangeKey(Session, Id, true);

					if (!this.exchanges.ContainsKey(Key))
					{
						Exchange Result = new Exchange(this, Session, Id, true);
						this.exchanges[Key] = Result;
						return Result;
					}
				}
			}
		}

		internal void ExchangeClosed(Exchange Exchange)
		{
			lock (this.exchanges)
			{
				ExchangeKey Key = new ExchangeKey(Exchange.Session, Exchange.Id, Exchange.IsInitiator);

				if (this.exchanges.TryGetValue(Key, out Exchange Ex) && Ex == Exchange)
					this.exchanges.Remove(Key);
			}
		}

		/// <summary>
		/// Calculates the worst-case time for delivering a reliable message to a peer, including retransmissions.
		/// </summary>
		/// <param name="Parameters">Session parameters of the receiving party.</param>
		/// <param name="Active">If the receiving party is considered active.</param>
		/// <returns>Worst-case time.</returns>
		public static TimeSpan MaxRetransmissionTime(SessionParameters Parameters, bool Active)
		{
			double Total = 0;
			int i;

			for (i = 0; i < MaxTransmissions; i++)
			{
				if (Active && Total > Parameters.ActiveThresholdMs)
					Active = false;

				Total += GetBackoff(Parameters, Active, i, true).TotalMilliseconds;
			}

			return TimeSpan.FromMilliseconds(Total);
		}

		/// <summary>
		/// Computes the MRP backoff time after a given transmission (Matter Core Specification, 4.12.2.1).
		/// </summary>
		/// <param name="Parameters">Session parameters of the receiving party.</param>
		/// <param name="Active">If the receiving party is considered active.</param>
		/// <param name="TransmissionNumber">Transmission number (0 = initial transmission).</param>
		/// <param name="Maximum">If the maximum backoff (without randomness) is to be calculated.</param>
		/// <returns>Backoff time.</returns>
		public static TimeSpan GetBackoff(SessionParameters Parameters, bool Active, int TransmissionNumber, bool Maximum)
		{
			double Base = Active ? Parameters.ActiveIntervalMs : Parameters.IdleIntervalMs;
			double r;

			if (Maximum)
				r = 1;
			else
			{
				lock (jitter)
				{
					r = jitter.NextDouble();
				}
			}

			double Ms = Base * BackoffMargin * Math.Pow(BackoffBase, Math.Max(0, TransmissionNumber - BackoffThreshold)) *
				(1 + r * BackoffJitter);

			return TimeSpan.FromMilliseconds(Ms);
		}

		/// <summary>
		/// Gets a suitable timeout for waiting for a response from a peer.
		/// </summary>
		/// <param name="Session">Session.</param>
		/// <param name="ProcessingTime">Expected processing time of peer.</param>
		/// <returns>Timeout.</returns>
		public TimeSpan GetResponseTimeout(Session Session, TimeSpan ProcessingTime)
		{
			return MaxRetransmissionTime(Session.PeerParameters, Session.IsPeerActive) +
				MaxRetransmissionTime(this.ownParameters, true) +
				ProcessingTime + TimeSpan.FromSeconds(5);
		}

		internal async Task SendAsync(Exchange Exchange, ushort ProtocolId, byte Opcode, byte[] Payload, bool Reliable)
		{
			PendingMessage Prev = Exchange.Pending;
			if (!(Prev is null) && !Prev.Acked.Task.IsCompleted)
			{
				Task Timeout = Task.Delay(MaxRetransmissionTime(Exchange.Session.PeerParameters, false) + TimeSpan.FromSeconds(1));
				await Task.WhenAny(Prev.Acked.Task, Timeout);

				if (!Prev.Acked.Task.IsCompleted)
					throw new MatterTimeoutException("Previous message on exchange not acknowledged.");
			}

			ProtocolHeader PH = new ProtocolHeader()
			{
				Flags = (Exchange.IsInitiator ? ExchangeFlags.Initiator : ExchangeFlags.None) |
					(Reliable ? ExchangeFlags.Reliability : ExchangeFlags.None),
				Opcode = Opcode,
				ExchangeId = Exchange.Id,
				ProtocolId = ProtocolId,
				AckedMessageCounter = Exchange.TakePendingAck()
			};

			byte[] Datagram = this.Encode(Exchange.Session, PH, Payload, out uint Counter);

			if (Reliable)
			{
				PendingMessage Msg = new PendingMessage(Counter, Datagram);
				Exchange.Pending = Msg;
				this.StartRetransmissions(Exchange, Msg);
			}

			await this.Transmit(Datagram, Exchange.Session.PeerAddress, PH, Counter);
		}

		private async void StartRetransmissions(Exchange Exchange, PendingMessage Msg)
		{
			try
			{
				Session Session = Exchange.Session;
				int i;

				for (i = 0; i < MaxTransmissions; i++)
				{
					TimeSpan Backoff = GetBackoff(Session.PeerParameters, Session.IsPeerActive, i, false);

					try
					{
						await Task.Delay(Backoff, Msg.Cancel.Token);
					}
					catch (TaskCanceledException)
					{
						return;
					}

					if (Msg.Acked.Task.IsCompleted || this.disposed || Session.IsClosed)
						return;

					if (i + 1 < MaxTransmissions)
					{
						this.sniffers?.Information("Retransmitting message " + Msg.Counter.ToString() + " (" + (i + 2).ToString() + "/" +
							MaxTransmissions.ToString() + ") on " + Exchange.ToString() + ".");

						try
						{
							await this.transport.SendAsync(Msg.Datagram, Session.PeerAddress);
							this.sniffers?.TransmitBinary(true, Msg.Datagram);
						}
						catch (Exception ex)
						{
							this.sniffers?.Error(ex.Message);
						}
					}
				}

				if (!Msg.Acked.Task.IsCompleted)
				{
					MatterTimeoutException ex = new MatterTimeoutException("Message not acknowledged by peer (MRP retransmission limit reached).");
					this.sniffers?.Warning(ex.Message);
					Msg.Acked.TrySetException(ex);
					Exchange.Fail(ex);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		internal void SendStandaloneAckNoWait(Session Session, ushort ExchangeId, bool AsInitiator, uint AckedCounter)
		{
			Task _ = this.SendStandaloneAck(Session, ExchangeId, AsInitiator, AckedCounter);
		}

		internal async Task SendStandaloneAck(Session Session, ushort ExchangeId, bool AsInitiator, uint AckedCounter)
		{
			if (this.disposed || Session.IsClosed)
				return;

			try
			{
				ProtocolHeader PH = new ProtocolHeader()
				{
					Flags = AsInitiator ? ExchangeFlags.Initiator : ExchangeFlags.None,
					Opcode = (byte)SecureChannelOpcode.StandaloneAck,
					ExchangeId = ExchangeId,
					ProtocolId = ProtocolIds.SecureChannel,
					AckedMessageCounter = AckedCounter
				};

				byte[] Datagram = this.Encode(Session, PH, Array.Empty<byte>(), out uint Counter);
				await this.Transmit(Datagram, Session.PeerAddress, PH, Counter);
			}
			catch (Exception ex)
			{
				this.sniffers?.Error("Unable to send standalone acknowledgement: " + ex.Message);
			}
		}

		private async Task Transmit(byte[] Datagram, IPEndPoint Destination, ProtocolHeader PH, uint Counter)
		{
			if (!(this.sniffers is null) && this.sniffers.HasSniffers)
			{
				this.sniffers.TransmitText("TX to " + Destination.ToString() + ": " + PH.ToString() + ", Counter=" + Counter.ToString());
				this.sniffers.TransmitBinary(true, Datagram);
			}

			await this.transport.SendAsync(Datagram, Destination);
		}

		private byte[] Encode(Session Session, ProtocolHeader PH, byte[] Payload, out uint Counter)
		{
			MessageHeader Header = new MessageHeader();
			Counter = Session.NextMessageCounter(this);
			Header.MessageCounter = Counter;

			if (Session is SecureSession Secure)
			{
				Header.SessionId = Secure.PeerSessionId;
				Header.SessionType = SessionType.Unicast;

				byte[] HeaderBytes = Header.Encode();
				byte[] Plain = PH.Encode(Payload);
				byte[] Encrypted = Secure.Encrypt(HeaderBytes, Header.SecurityFlags, Counter, Plain);

				return MatterCrypto.Concat(HeaderBytes, Encrypted);
			}
			else if (Session is UnsecuredSession Unsecured)
			{
				Header.SessionId = 0;

				if (Unsecured.IsInitiator)
					Header.SourceNodeId = Unsecured.EphemeralNodeId;
				else
					Header.DestinationNodeId = Unsecured.EphemeralNodeId;

				return MatterCrypto.Concat(Header.Encode(), PH.Encode(Payload));
			}
			else
				throw new MatterException("Unsupported session type.");
		}

		#endregion

		#region Reception

		private async Task Transport_OnDatagramReceived(byte[] Data, IPEndPoint From)
		{
			if (this.disposed)
				return;

			MessageHeader Header;
			int HeaderLength;

			try
			{
				Header = MessageHeader.Decode(Data, out HeaderLength);
			}
			catch (Exception ex)
			{
				this.sniffers?.Error("Invalid message received from " + From.ToString() + ": " + ex.Message);
				return;
			}

			if (Header.SessionType != SessionType.Unicast)
			{
				this.sniffers?.Information("Group message from " + From.ToString() + " ignored.");
				return;
			}

			Session Session;
			byte[] Plain;

			if (Header.SessionId == 0)
			{
				UnsecuredSession Unsecured;

				if (!Header.DestinationNodeId.HasValue)
				{
					this.sniffers?.Information("Unsecured message without destination node ID from " + From.ToString() + " ignored.");
					return;
				}

				lock (this.unsecuredSessions)
				{
					if (!this.unsecuredSessions.TryGetValue(Header.DestinationNodeId.Value, out Unsecured))
						Unsecured = null;
				}

				if (Unsecured is null)
				{
					this.sniffers?.Information("Unsecured message for unknown session from " + From.ToString() + " ignored.");
					return;
				}

				Session = Unsecured;
				Plain = new byte[Data.Length - HeaderLength];
				Array.Copy(Data, HeaderLength, Plain, 0, Plain.Length);
			}
			else
			{
				SecureSession Secure;

				lock (this.secureSessions)
				{
					if (!this.secureSessions.TryGetValue(Header.SessionId, out Secure))
						Secure = null;
				}

				if (Secure is null)
				{
					this.sniffers?.Information("Message for unknown session " + Header.SessionId.ToString() + " from " + From.ToString() + " ignored.");
					return;
				}

				byte[] HeaderBytes = new byte[HeaderLength];
				byte[] Encrypted = new byte[Data.Length - HeaderLength];
				Array.Copy(Data, 0, HeaderBytes, 0, HeaderLength);
				Array.Copy(Data, HeaderLength, Encrypted, 0, Encrypted.Length);

				try
				{
					Plain = Secure.Decrypt(HeaderBytes, Header.SecurityFlags, Header.MessageCounter, Encrypted);
				}
				catch (Exception ex)
				{
					this.sniffers?.Error("Unable to decrypt message from " + From.ToString() + ": " + ex.Message);
					return;
				}

				Session = Secure;
			}

			ProtocolHeader PH;
			byte[] Payload;

			try
			{
				PH = ProtocolHeader.Decode(Plain, out Payload);
			}
			catch (Exception ex)
			{
				this.sniffers?.Error("Invalid protocol header from " + From.ToString() + ": " + ex.Message);
				return;
			}

			if (!(this.sniffers is null) && this.sniffers.HasSniffers)
			{
				this.sniffers.ReceiveText("RX from " + From.ToString() + ": " + PH.ToString() + ", Counter=" + Header.MessageCounter.ToString());
				this.sniffers.ReceiveBinary(true, Data);
			}

			Session.PeerActivity(From);

			if (PH.AckedMessageCounter.HasValue)
				this.ProcessAck(Session, PH);

			bool IsNew = Session.Reception.CheckAndRegister(Header.MessageCounter);

			if (!IsNew)
			{
				this.sniffers?.Information("Duplicate message " + Header.MessageCounter.ToString() + " ignored.");

				if (PH.NeedsAck)
					await this.SendStandaloneAck(Session, PH.ExchangeId, !PH.IsInitiator, Header.MessageCounter);

				return;
			}

			MatterMessage Message = new MatterMessage(Header, PH, Payload, From, Session);

			if (Message.Is(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.StandaloneAck))
				return;

			ExchangeKey Key = new ExchangeKey(Session, PH.ExchangeId, !PH.IsInitiator);
			Exchange Exchange;

			lock (this.exchanges)
			{
				if (!this.exchanges.TryGetValue(Key, out Exchange))
					Exchange = null;
			}

			if (Exchange is null)
			{
				if (PH.IsInitiator)
				{
					Exchange = new Exchange(this, Session, PH.ExchangeId, false);

					lock (this.exchanges)
					{
						this.exchanges[Key] = Exchange;
					}

					if (PH.NeedsAck)
						Exchange.SetPendingAck(Header.MessageCounter);

					Exchange.Enqueue(Message);

					Task _ = Task.Run(() => this.ProcessUnsolicited(Exchange, Message));
				}
				else
				{
					if (PH.NeedsAck)
						await this.SendStandaloneAck(Session, PH.ExchangeId, true, Header.MessageCounter);
				}

				return;
			}

			if (PH.NeedsAck)
				Exchange.SetPendingAck(Header.MessageCounter);

			Exchange.Enqueue(Message);
		}

		private void ProcessAck(Session Session, ProtocolHeader PH)
		{
			ExchangeKey Key = new ExchangeKey(Session, PH.ExchangeId, !PH.IsInitiator);
			Exchange Exchange;

			lock (this.exchanges)
			{
				if (!this.exchanges.TryGetValue(Key, out Exchange))
					Exchange = null;
			}

			PendingMessage Pending = Exchange?.Pending;

			if (!(Pending is null) && Pending.Counter == PH.AckedMessageCounter.Value)
			{
				Pending.Cancel.Cancel();
				Pending.Acked.TrySetResult(true);
			}
		}

		private async Task ProcessUnsolicited(Exchange Exchange, MatterMessage Message)
		{
			try
			{
				if (Message.IsStatusReport)
				{
					StatusReport Report = StatusReport.Decode(Message.Payload);

					if (Report.IsCloseSession && Exchange.Session is SecureSession Secure)
					{
						this.sniffers?.Information("Peer closed " + Secure.ToString() + ".");
						await Exchange.CloseAsync();
						this.RemoveSession(Secure);
						await this.OnSessionClosed.Raise(this, Secure);
						return;
					}
				}

				UnsolicitedMessageEventArgs e = new UnsolicitedMessageEventArgs(Exchange, Message);
				UnsolicitedMessageEventHandler h = this.OnUnsolicitedMessage;

				if (!(h is null))
				{
					foreach (UnsolicitedMessageEventHandler Handler in h.GetInvocationList())
					{
						try
						{
							await Handler(this, e);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}

						if (e.Handled)
							break;
					}
				}

				if (!e.Handled)
				{
					this.sniffers?.Information("Unhandled unsolicited message: " + Message.ToString());
					await Exchange.CloseAsync();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				await Exchange.CloseAsync();
			}
		}

		#endregion

		/// <inheritdoc/>
		public void Dispose()
		{
			if (this.disposed)
				return;

			this.disposed = true;
			this.transport.OnDatagramReceived -= this.Transport_OnDatagramReceived;

			List<Exchange> ToClose = new List<Exchange>();

			lock (this.exchanges)
			{
				ToClose.AddRange(this.exchanges.Values);
				this.exchanges.Clear();
			}

			foreach (Exchange Ex in ToClose)
			{
				Ex.Pending?.Cancel.Cancel();
				Ex.Fail(new ObjectDisposedException(nameof(ExchangeManager)));
			}
		}

		private readonly struct ExchangeKey : IEquatable<ExchangeKey>
		{
			public readonly Session Session;
			public readonly ushort Id;
			public readonly bool LocalIsInitiator;

			public ExchangeKey(Session Session, ushort Id, bool LocalIsInitiator)
			{
				this.Session = Session;
				this.Id = Id;
				this.LocalIsInitiator = LocalIsInitiator;
			}

			public bool Equals(ExchangeKey Other)
			{
				return this.Session == Other.Session && this.Id == Other.Id && this.LocalIsInitiator == Other.LocalIsInitiator;
			}

			public override bool Equals(object obj)
			{
				return obj is ExchangeKey Key && this.Equals(Key);
			}

			public override int GetHashCode()
			{
				return this.Session.GetHashCode() ^ (this.Id << 1) ^ (this.LocalIsInitiator ? 1 : 0);
			}
		}

		/// <summary>
		/// Gets a diagnostic string describing the manager state.
		/// </summary>
		/// <returns>String.</returns>
		public string GetDiagnostics()
		{
			StringBuilder sb = new StringBuilder();

			lock (this.secureSessions)
			{
				sb.Append("Secure sessions: ");
				sb.AppendLine(this.secureSessions.Count.ToString());
			}

			lock (this.exchanges)
			{
				sb.Append("Exchanges: ");
				sb.AppendLine(this.exchanges.Count.ToString());
			}

			return sb.ToString();
		}
	}
}
