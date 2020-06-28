using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;
using Waher.Security;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// DTLS endpoint class. Manages a client or server DTLS endpoint connection, as defined
	/// in RFC 6347: https://tools.ietf.org/html/rfc6347.
	/// </summary>
	public class DtlsEndpoint : Sniffable, IDisposable
	{
		private static ICipher[] ciphers = null;
		private static Dictionary<ushort, ICipher> ciphersPerCode = null;

		private Cache<object, EndpointState> states;
		private Scheduler timeouts;
		private readonly DtlsMode mode;
		private RandomNumberGenerator rnd;
		private ICommunicationLayer communicationLayer;
		private readonly IUserSource users;
		private readonly string requiredPrivilege;
		private double probabilityPacketLoss = 0;

		static DtlsEndpoint()
		{
			InitCiphers();
			Types.OnInvalidated += (sender, e) => InitCiphers();
		}

		private static void InitCiphers()
		{
			List<ICipher> Ciphers = new List<ICipher>();
			Dictionary<ushort, ICipher> PerCode = new Dictionary<ushort, ICipher>();
			TypeInfo TI;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICipher)))
			{
				TI = T.GetTypeInfo();
				if (TI.IsAbstract || TI.IsGenericTypeDefinition)
					continue;

				try
				{
					ICipher Cipher = (ICipher)Activator.CreateInstance(T);
					Ciphers.Add(Cipher);
					PerCode[Cipher.IanaCipherSuite] = Cipher;
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			Ciphers.Sort((x, y) => y.Priority - x.Priority);
			ciphers = Ciphers.ToArray();
			ciphersPerCode = PerCode;
		}

		/// <summary>
		/// DTLS endpoint class. Manages a client or server DTLS endpoint connection, as defined
		/// in RFC 6347: https://tools.ietf.org/html/rfc6347.
		/// </summary>
		/// <param name="Mode">DTLS Mode of operation.</param>
		/// <param name="CommunicationLayer">Communication layer.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public DtlsEndpoint(DtlsMode Mode, ICommunicationLayer CommunicationLayer, params ISniffer[] Sniffers)
			: this(Mode, CommunicationLayer, null, null, Sniffers)
		{
		}

		/// <summary>
		/// DTLS endpoint class. Manages a client or server DTLS endpoint connection, as defined
		/// in RFC 6347: https://tools.ietf.org/html/rfc6347.
		/// </summary>
		/// <param name="Mode">DTLS Mode of operation.</param>
		/// <param name="CommunicationLayer">Communication layer.</param>
		/// <param name="Users">User data source, if pre-shared keys should be allowed by a DTLS server endpoint.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public DtlsEndpoint(DtlsMode Mode, ICommunicationLayer CommunicationLayer, IUserSource Users,
			params ISniffer[] Sniffers)
			: this(Mode, CommunicationLayer, Users, null, Sniffers)
		{
		}

		/// <summary>
		/// DTLS endpoint class. Manages a client or server DTLS endpoint connection, as defined
		/// in RFC 6347: https://tools.ietf.org/html/rfc6347.
		/// </summary>
		/// <param name="Mode">DTLS Mode of operation.</param>
		/// <param name="CommunicationLayer">Communication layer.</param>
		/// <param name="Users">User data source, if pre-shared keys should be allowed by a DTLS server endpoint.</param>
		/// <param name="RequiredPrivilege">Required privilege, for the user to be acceptable
		/// in PSK handshakes.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public DtlsEndpoint(DtlsMode Mode, ICommunicationLayer CommunicationLayer, IUserSource Users,
			string RequiredPrivilege, params ISniffer[] Sniffers)
		: base(Sniffers)
		{
			this.mode = Mode;
			this.users = Users;
			this.requiredPrivilege = RequiredPrivilege;
			this.rnd = RandomNumberGenerator.Create();
			this.timeouts = new Scheduler();
			this.states = new Cache<object, EndpointState>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromHours(1));

			this.states.Removed += States_Removed;

			this.communicationLayer = CommunicationLayer;
			this.communicationLayer.PacketReceived += this.DataReceived;
		}

		private void States_Removed(object Sender, CacheItemEventArgs<object, EndpointState> e)
		{
			if (e.Value.state == DtlsState.SessionEstablished ||
				e.Value.state == DtlsState.Handshake)
			{
				e.Value.State = DtlsState.Closed;
				this.SendAlert(AlertLevel.fatal, AlertDescription.close_notify, e.Value);
			}

			e.Value.Dispose();
		}

		/// <summary>
		/// User data source, if pre-shared keys should be allowed by a DTLS server endpoint.
		/// </summary>
		public IUserSource Users
		{
			get { return this.users; }
		}

		/// <summary>
		/// Required privilege, for the user to be acceptable in PSK handshakes.
		/// </summary>
		public string RequiredPrivilege
		{
			get { return this.requiredPrivilege; }
		}

		/// <summary>
		/// Probability of packet loss. Is by default 0.
		/// Can be used to simulate lossy network.
		/// </summary>
		public double ProbabilityPacketLoss
		{
			get { return probabilityPacketLoss; }
			set
			{
				if (value < 0 || value > 1)
				{
					throw new ArgumentOutOfRangeException("Valid probabilities lie between 0 and 1.",
						nameof(ProbabilityPacketLoss));
				}

				this.probabilityPacketLoss = value;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.timeouts != null)
			{
				this.timeouts.Dispose();
				this.timeouts = null;
			}

			if (this.states != null)
			{
				this.states.Clear();
				this.states.Dispose();
				this.states = null;
			}

			if (this.communicationLayer != null)
			{
				this.communicationLayer.PacketReceived -= this.DataReceived;
				this.communicationLayer = null;
			}

			if (this.rnd != null)
			{
				this.rnd.Dispose();
				this.rnd = null;
			}
		}

		private double NextDouble()
		{
			byte[] A = new byte[4];

			lock (this.rnd)
			{
				this.rnd.GetBytes(A);
			}

			double d = BitConverter.ToUInt32(A, 0);
			d /= uint.MaxValue;

			return d;
		}

		private bool PacketLost()
		{
			return (this.NextDouble() <= this.probabilityPacketLoss);
		}

		private void DataReceived(byte[] Data, object RemoteEndpoint)
		{
			int Pos = 0;
			int Len = Data.Length;
			int Start;

			if (this.probabilityPacketLoss > 0 && this.PacketLost())
			{
				if (this.HasSniffers)
					this.Warning(DateTime.Now.ToString("T") + " Received packet lost.");

				return;
			}

			this.ReceiveBinary(Data);

			EndpointState State = this.GetState(RemoteEndpoint, false);
			bool First = true;

			while (Pos + 13 <= Len)
			{
				Start = Pos;

				DTLSPlaintext Rec = new DTLSPlaintext()
				{
					type = (ContentType)Data[Pos],
					version = new ProtocolVersion()
					{
						major = Data[Pos + 1],
						minor = Data[Pos + 2]
					},
					epoch = GetUInt16(Data, Pos + 3),
					sequence_number = GetUInt48(Data, Pos + 5),
					length = GetUInt16(Data, Pos + 11),
					fragment = null,
					datagram = Data,
					recordOffset = Pos
				};

				Pos += 13;
				if (Pos + Rec.length > Len)
					break;

				Rec.fragment = new byte[Rec.length];
				Array.Copy(Data, Pos, Rec.fragment, 0, Rec.length);
				Pos += Rec.length;

				if (!this.RecordReceived(Rec, Data, Start, State, First))
					break;

				First = false;
			}
		}

		private bool RecordReceived(DTLSPlaintext Record, byte[] RecordData, int Start,
			EndpointState State, bool StartOfFlight)
		{
			if (Record.version.major != 254)
			{
				this.Error(DateTime.Now.ToString("T") + " Packet dropped. Protocol version not recognized.");
				return false; // Not DTLS 1.x
			}

			// Anti-replay §4.1.2.6

			bool ValidEpoch = Record.epoch == State.currentEpoch;

			if (State.acceptRollbackPrevEpoch)
			{
				if (Record.epoch == State.currentEpoch - 1)
				{
					if (Record.type != ContentType.change_cipher_spec)
					{
						State.currentEpoch--;
						State.currentCipher = State.previousCipher;
						State.leftEdgeSeqNr = State.previousLeftEdgeSeqNr;
						State.receivedPacketsWindow = State.previousReceivedPacketsWindow;
						State.currentSeqNr = State.previousSeqNr;
						State.next_receive_seq = State.flightRxSeq = State.previousFlightRxSeq;
						State.message_seq = State.flightTxSeq = State.previousFlightTxSeq;
					}

					ValidEpoch = true;
				}

				State.acceptRollbackPrevEpoch = false;
			}

			if (!ValidEpoch)
			{
				this.Error(DateTime.Now.ToString("T") + " Packet dropped. Unexpected epoch (" +
					Record.epoch + ", expected " + State.currentEpoch.ToString() + ").");
				return false;
			}

			long Offset = (long)(Record.sequence_number - State.leftEdgeSeqNr);

			if (Offset < 0)
			{
				this.Error(DateTime.Now.ToString("T") + " Packet dropped. Old sequence number.");
				return false;
			}

			if (Offset < 64 && (State.receivedPacketsWindow & (1UL << (int)Offset)) != 0)
			{
				this.Error(DateTime.Now.ToString("T") + " Packet dropped. Sequence number already processed.");
				return false;
			}

			if (Record.epoch > 0 && State.currentCipher != null)
			{
				try
				{
					Record.fragment = State.currentCipher.Decrypt(Record.fragment, RecordData, Start, State);
				}
				catch (Exception ex)
				{
					this.Exception(ex);
					Record.fragment = null;
				}

				if (Record.fragment is null)
				{
					this.Error(DateTime.Now.ToString("T") + " Packet dropped. Decryption failed.");
					State.acceptRollbackPrevEpoch = true;
					return false;
				}

				Record.length = (ushort)Record.fragment.Length;

				if (this.HasSniffers)
				{
					byte[] NewRecord = new byte[13 + Record.length];
					Array.Copy(Record.datagram, Record.recordOffset, NewRecord, 0, 13);
					Array.Copy(Record.fragment, 0, NewRecord, 13, Record.length);

					Record.datagram = NewRecord;
					Record.recordOffset = 0;
				}
			}

			// TODO: Queue future sequence numbers, is handshake. These must be processed in order.

			if (!this.ProcessRecord(Record, State, StartOfFlight))
				return false;

			// Update receive window

			if (Offset >= 64)
			{
				ulong Diff = (ulong)(Offset - 63);

				if (Diff >= 64)
					State.receivedPacketsWindow = 0;
				else
					State.receivedPacketsWindow >>= (int)Diff;

				State.leftEdgeSeqNr += Diff;
				Offset -= (long)Diff;
			}

			State.receivedPacketsWindow |= 1UL << (int)Offset;

			return true;
		}

		private string TlsVersion(byte[] Data, int Offset)
		{
			StringBuilder Version = new StringBuilder();

			byte Major = Data[Offset++];
			byte Minor = Data[Offset++];

			if (Major >= 128)
			{
				Version.Append('D');

				Major ^= 255;
				Minor ^= 255;
			}

			Version.Append("TLS ");
			Version.Append(Major.ToString());
			Version.Append('.');
			Version.Append(Minor.ToString());

			return Version.ToString();
		}

		private void SniffMsg(byte[] Data, int Offset, bool Rx)
		{
			StringBuilder Msg = new StringBuilder();

			Msg.Append(DateTime.Now.ToString("T"));

			if (Rx)
				Msg.Append(" RX: ");
			else
				Msg.Append(" TX: ");

			ContentType ContentType = (ContentType)Data[Offset];
			string Ver = this.TlsVersion(Data, Offset + 1);
			ushort Epoch = GetUInt16(Data, Offset + 3);
			ulong SeqNr = GetUInt48(Data, Offset + 5);
			uint? MsgNr = null;
			int? Len = null;
			int? FOffset = null;
			int? FLen = null;

			Msg.Append(ContentType.ToString());
			Msg.Append(", ");

			switch (ContentType)
			{
				case ContentType.handshake:
					HandshakeType HandshakeType = (HandshakeType)Data[Offset + 13];
					Len = GetUInt24(Data, Offset + 14);
					MsgNr = GetUInt16(Data, Offset + 17);
					FOffset = GetUInt24(Data, Offset + 19);
					FLen = GetUInt24(Data, Offset + 22);

					Msg.Append(HandshakeType.ToString());
					break;

				case ContentType.alert:
					AlertLevel AlertLevel = (AlertLevel)Data[Offset + 13];
					AlertDescription AlertDescription = (AlertDescription)Data[Offset + 14];

					Msg.Append(AlertLevel.ToString());
					Msg.Append(", ");
					Msg.Append(AlertDescription.ToString());
					break;
			}

			Msg.Append(" (");
			Msg.Append(Ver);
			Msg.Append(", Epoch: ");
			Msg.Append(Epoch.ToString());
			Msg.Append(", seq: ");
			Msg.Append(SeqNr.ToString());

			if (MsgNr.HasValue)
			{
				Msg.Append(", msg: ");
				Msg.Append(MsgNr.Value.ToString());
				Msg.Append(", len: ");
				Msg.Append(Len.Value.ToString());
				Msg.Append(", foffs: ");
				Msg.Append(FOffset.Value.ToString());
				Msg.Append(", flen: ");
				Msg.Append(FLen.Value.ToString());
			}

			Msg.Append(')');

			this.Information(Msg.ToString());
		}

		private bool ProcessRecord(DTLSPlaintext Record, EndpointState State, bool StartOfFlight)
		{
			try
			{
				if (this.HasSniffers)
					this.SniffMsg(Record.datagram, Record.recordOffset, true);

				switch (Record.type)
				{
					case ContentType.handshake:
						HandshakeType HandshakeType = (HandshakeType)Record.fragment[0];

						int PayloadLen = Record.fragment[1];
						PayloadLen <<= 8;
						PayloadLen |= Record.fragment[2];
						PayloadLen <<= 8;
						PayloadLen |= Record.fragment[3];

						int MessageSeqNr = Record.fragment[4];
						MessageSeqNr <<= 8;
						MessageSeqNr |= Record.fragment[5];

						if (MessageSeqNr != State.next_receive_seq)
						{
							if (MessageSeqNr != 0)
							{
								this.Error(DateTime.Now.ToString("T") +
									" Packet dropped. Expected message number " +
									State.next_receive_seq.ToString() + ", but was " +
									MessageSeqNr.ToString() + ".");

								return false;  // Not the expected handshake sequence number.
							}
							else if (HandshakeType == HandshakeType.client_hello ||
							   HandshakeType == HandshakeType.hello_request)
							{
								State.message_seq = 0;
								State.next_receive_seq = 0;
							}
							else
							{
								this.Error(DateTime.Now.ToString("T") +
									" Packet dropped. Expected message number " +
									State.next_receive_seq.ToString() + ", but was " +
									MessageSeqNr.ToString() + ".");

								return false;  // Not the expected handshake sequence number.
							}
						}

						if (StartOfFlight)
						{
							lock (State.lastFlight)
							{
								State.flightNr++;
								State.lastFlight.Clear();
								State.timeoutSeconds = 1;
								State.flightTxSeq = State.message_seq;
								State.flightRxSeq = State.next_receive_seq;
							}
						}

						State.next_receive_seq++;

						int FragmentOffset = GetUInt24(Record.fragment, 6);
						int FragmentLength = GetUInt24(Record.fragment, 9);

						if (FragmentOffset > 0 || FragmentLength != PayloadLen)
						{
							this.Error(DateTime.Now.ToString("T") +
								" Packet dropped. Fragmented messages not supported.");

							return false;   // TODO: Reassembly of fragmented messages.
						}

						int Pos = 12;

						this.AddHandshakeMessageToHash(HandshakeType, Record.fragment, 0,
							Record.fragment.Length, State, false);

						switch (HandshakeType)
						{
							case HandshakeType.hello_verify_request:
								if (this.mode == DtlsMode.Server)
									break;

								if (Record.fragment[Pos++] != 254)  // Major version.
									this.HandshakeFailure(State, "DTLS version mismatch.", AlertDescription.protocol_version);
								else
								{
									Pos++;  // Minor version.

									int Len = Record.fragment[Pos++];
									State.cookie = new byte[Len + 1];
									Array.Copy(Record.fragment, Pos - 1, State.cookie, 0, Len + 1);
									Pos += Len;

									this.SendClientHello(State);
								}
								break;

							case HandshakeType.server_hello:
								if (this.mode == DtlsMode.Server)
									break;

								if (Record.fragment[Pos++] != 254 || Record.fragment[Pos++] != 253)  // Protocol version.
									this.HandshakeFailure(State, "DTLS version mismatch.", AlertDescription.protocol_version);
								else
								{
									State.serverRandom = new byte[32];
									Array.Copy(Record.fragment, Pos, State.serverRandom, 0, 32);
									Pos += 32;

									byte[] PrevSessionId = State.sessionId;

									int Len = Record.fragment[Pos++];
									State.sessionId = new byte[Len];
									Array.Copy(Record.fragment, Pos, State.sessionId, 0, Len);
									Pos += Len;

									ushort CipherSuite = Record.fragment[Pos++];
									CipherSuite <<= 8;
									CipherSuite |= Record.fragment[Pos++];

									byte CompressionMethod = Record.fragment[Pos++];

									// TODO: Compression methods.
									// TODO: Extensions

									State.pendingCipher = null;

									if (!ciphersPerCode.TryGetValue(CipherSuite, out State.pendingCipher))
										State.pendingCipher = null;

									if (State.pendingCipher is null || CompressionMethod != 0)
									{
										this.HandshakeFailure(State, "Cipher and compression mode agreement not reached.",
											AlertDescription.handshake_failure);
									}
								}
								break;

							case HandshakeType.server_hello_done:
								if (this.mode == DtlsMode.Server)
									break;

								State.pendingCipher.SendClientKeyExchange(this, State);
								break;

							case HandshakeType.server_key_exchange:
								if (this.mode == DtlsMode.Server)
									break;

								if (State.pendingCipher != null)
									State.pendingCipher.ServerKeyExchange(Record.fragment, ref Pos, State);
								break;

							case HandshakeType.client_hello:
								if (this.mode == DtlsMode.Client)
									break;

								if (Record.fragment[Pos++] != 254)  // Major version.
									break;

								Pos++;  // Minor version.

								byte[] ClientRandom = new byte[32];
								Array.Copy(Record.fragment, Pos, ClientRandom, 0, 32);
								Pos += 32;

								byte SessionIdLen = Record.fragment[Pos++];
								byte[] SessionId = new byte[SessionIdLen];
								Array.Copy(Record.fragment, Pos, SessionId, 0, SessionIdLen);
								Pos += SessionIdLen;

								byte CookieLen = Record.fragment[Pos++];
								byte[] Cookie = new byte[CookieLen];
								Array.Copy(Record.fragment, Pos, Cookie, 0, CookieLen);
								Pos += CookieLen;

								int CipherPos = Pos;
								ushort NrCiphers = Record.fragment[Pos++];
								NrCiphers <<= 1;
								NrCiphers |= Record.fragment[Pos++];

								if ((NrCiphers & 1) != 0)
									break;

								NrCiphers >>= 1;

								int i;
								ushort CipherCode = 0;
								ICipher Cipher = null;

								for (i = 0; i < NrCiphers; i++)
								{
									CipherCode = Record.fragment[Pos++];
									CipherCode <<= 8;
									CipherCode |= Record.fragment[Pos++];

									if (ciphersPerCode.TryGetValue(CipherCode, out Cipher))
									{
										Pos += (NrCiphers - i - 1) << 1;
										break;
									}
								}

								int CompressionPos = Pos;
								byte NrCompressionMethods = Record.fragment[Pos++];
								bool NullCompression = false;

								for (i = 0; i < NrCompressionMethods; i++)
								{
									if (Record.fragment[Pos++] == 0)
									{
										NullCompression = true;
										Pos += (NrCompressionMethods - i - 1);
										break;
									}
								}

								if (Cipher is null || !NullCompression)
								{
									this.SendAlert(AlertLevel.warning, AlertDescription.handshake_failure, State);
									break;
								}

								// TODO: Extensions.
								// TODO: Session resumption (RFC 6347, § 4.2.4, Figure 2).

								using (IncrementalHash CookieHash = IncrementalHash.CreateHMAC(
									HashAlgorithmName.SHA256, State.cookieRandom))
								{
									CookieHash.AppendData(Encoding.UTF8.GetBytes(State.remoteEndpoint.ToString()));
									CookieHash.AppendData(Record.fragment, 0, 2);   // Version.
									CookieHash.AppendData(ClientRandom);
									CookieHash.AppendData(Record.fragment, CipherPos, 2 + (NrCiphers << 1));
									CookieHash.AppendData(Record.fragment, CompressionPos, 1 + NrCompressionMethods);

									byte[] Cookie2 = CookieHash.GetHashAndReset();
									byte CookieLen2 = (byte)Cookie2.Length;

									if (CookieLen == 0 || !AreEqual(Cookie, Cookie2))
									{
										byte[] HelloVerifyRequest = new byte[3 + CookieLen2];
										HelloVerifyRequest[0] = 254;
										HelloVerifyRequest[1] = 253;
										HelloVerifyRequest[2] = (byte)CookieLen2;

										Array.Copy(Cookie2, 0, HelloVerifyRequest, 3, CookieLen2);

										this.SendHandshake(HandshakeType.hello_verify_request,
											HelloVerifyRequest, false, true, State);

										break;
									}
								}

								SessionIdLen = 32;
								SessionId = new byte[32];

								State.serverRandom = new byte[32];

								lock (this.rnd)
								{
									this.rnd.GetBytes(SessionId);
									this.rnd.GetBytes(State.serverRandom);
								}

								try
								{
									this.OnIncomingHandshakeStarted?.Invoke(this, 
										new RemoteEndpointEventArgs(State.remoteEndpoint));
								}
								catch (Exception ex)
								{
									Log.Critical(ex);
								}

								this.SetUnixTime(State.serverRandom, 0);

								byte[] ServerHello = new byte[70];
								State.sessionId = SessionId;
								State.clientRandom = ClientRandom;

								ServerHello[0] = 254;
								ServerHello[1] = 253;
								Array.Copy(State.serverRandom, 0, ServerHello, 2, 32);
								ServerHello[34] = 32;
								Array.Copy(SessionId, 0, ServerHello, 35, 32);
								ServerHello[67] = (byte)(CipherCode >> 8);
								ServerHello[68] = (byte)CipherCode;
								ServerHello[69] = 0;

								State.pendingCipher = Cipher;
								State.clientFinished = false;
								State.serverFinished = false;

								this.SendHandshake(HandshakeType.server_hello, ServerHello,
									true, true, State);
								Cipher.SendServerKeyExchange(this, State);
								break;

							case HandshakeType.client_key_exchange:
								if (this.mode == DtlsMode.Client)
									break;

								if (State.pendingCipher != null)
									State.pendingCipher.ClientKeyExchange(Record.fragment, ref Pos, State);
								break;

							case HandshakeType.finished:
								if (State.currentCipher is null)
									break;

								byte[] VerifyData = new byte[12];
								Array.Copy(Record.fragment, Pos, VerifyData, 0, 12);
								Pos += 12;

								if (!State.currentCipher.VerifyFinished(VerifyData, State))
								{
									this.HandshakeFailure(State, "Verify data not valid.", AlertDescription.decrypt_error);
									break;
								}

								if (State.isClient)
									State.serverFinished = true;
								else
									State.clientFinished = true;

								if (State.clientFinished && State.serverFinished)
									this.HandshakeSuccess(State);
								else
								{
									ulong Temp = State.currentSeqNr;
									try
									{
										State.currentSeqNr = State.previousSeqNr;
										State.currentEpoch--;

										this.SendRecord(ContentType.change_cipher_spec, 
											new byte[] { 1 }, true, false, State);
									}
									finally
									{
										State.currentSeqNr = Temp;
										State.currentEpoch++;
									}

									State.currentCipher.SendFinished(this, State, false);
								}
								break;

							case HandshakeType.hello_request:
								if (this.mode == DtlsMode.Server)
									break;

								if (State.state == DtlsState.SessionEstablished)
								{
									State.State = DtlsState.Handshake;
									this.StartHandshake(State.remoteEndpoint);
								}
								break;

							case HandshakeType.certificate:             // TODO
							case HandshakeType.certificate_request:     // TODO
							case HandshakeType.certificate_verify:      // TODO
								break;
						}
						break;

					case ContentType.change_cipher_spec:

						// Make sure the hash of the other side is calculated before 
						// the finished message is received.

						if (StartOfFlight)
						{
							lock (State.lastFlight)
							{
								State.flightNr++;
								State.lastFlight.Clear();
								State.timeoutSeconds = 1;
								State.flightTxSeq = State.message_seq;
								State.flightRxSeq = State.next_receive_seq;
							}
						}

						if (State.isClient)
							State.CalcServerHandshakeHash();
						else
							State.CalcClientHandshakeHash();

						this.ChangeCipherSpec(State, State.isClient);
						break;

					case ContentType.alert:
						if (Record.fragment.Length >= 2)
						{
							AlertLevel Level = (AlertLevel)Record.fragment[0];
							AlertDescription Description = (AlertDescription)Record.fragment[1];

							if (Description == AlertDescription.close_notify)
							{
								if (this.HasSniffers)
									this.Information(DateTime.Now.ToString("T") + " Session closed.");

								if (State.state == DtlsState.Handshake ||
									State.state == DtlsState.SessionEstablished)
								{
									this.SendAlert(Level, Description, State);  // Send close notification back.
								}

								State.State = DtlsState.Closed;
								this.states.Remove(State.remoteEndpoint);
							}
							else if (Description == AlertDescription.handshake_failure)
							{
								this.HandshakeFailure(State, "Handshake failed.", Description);
							}
							else if (Level == AlertLevel.fatal)
							{
								if (State.state == DtlsState.Handshake)
									this.HandshakeFailure(State, "Fatal error.", Description);
								else
									this.SessionFailure(State, "Fatal error.", Description);
							}
							else if (this.HasSniffers)
							{
								this.Warning(DateTime.Now.ToString("T") + " Non-fatal alert received: " +
									Description.ToString());
							}
						}
						break;

					case ContentType.application_data:
						if (State.State != DtlsState.SessionEstablished)
							break;

						try
						{
							this.OnApplicationDataReceived?.Invoke(this,
								new ApplicationDataEventArgs(State.remoteEndpoint, Record.fragment));
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
						break;

					default:
						break;
				}

				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				this.HandshakeFailure(State, "Unexpected error: " + ex.Message, AlertDescription.internal_error);
				return false;
			}
		}

		/// <summary>
		/// Event raised when application data has been received.
		/// </summary>
		public event ApplicationDataEventHandler OnApplicationDataReceived = null;

		internal void SendAlert(AlertLevel Level, AlertDescription Description, EndpointState State)
		{
			this.SendRecord(ContentType.alert, new byte[] { (byte)Level, (byte)Description },
				false, false, State);
		}

		internal static bool AreEqual(byte[] A1, byte[] A2)
		{
			if ((A1 is null) ^ (A2 is null))
				return false;

			if (A1 is null)
				return true;

			int i, c = A1.Length;
			if (c != A2.Length)
				return false;

			for (i = 0; i < c; i++)
			{
				if (A1[i] != A2[i])
					return false;
			}

			return true;
		}

		internal void HandshakeSuccess(EndpointState State)
		{
			State.State = DtlsState.SessionEstablished;

			try
			{
				this.OnHandshakeSuccessful?.Invoke(this, 
					new RemoteEndpointEventArgs(State.remoteEndpoint));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when handshake has been successful.
		/// </summary>
		public event RemoteEndpointEventHandler OnHandshakeSuccessful = null;

		/// <summary>
		/// Event raised when an incoming handshake has begun.
		/// </summary>
		public event RemoteEndpointEventHandler OnIncomingHandshakeStarted = null;

		private void HandshakeFailure(EndpointState State, string Reason, AlertDescription Descripton)
		{
			State.State = DtlsState.Failed;

			try
			{
				this.OnHandshakeFailed?.Invoke(this, new FailureEventArgs(State.remoteEndpoint,
					Reason, Descripton));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			if (this.states != null)
				this.states.Remove(State.remoteEndpoint);
		}

		/// <summary>
		/// Event raised when handshake fails.
		/// </summary>
		public event FailureEventHandler OnHandshakeFailed = null;

		private void SessionFailure(EndpointState State, string Reason, AlertDescription Descripton)
		{
			State.State = DtlsState.Failed;

			try
			{
				this.OnSessionFailed?.Invoke(this, new FailureEventArgs(State.remoteEndpoint,
					Reason, Descripton));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			if (this.states != null)
				this.states.Remove(State.remoteEndpoint);
		}

		/// <summary>
		/// Event raised when session fails.
		/// </summary>
		public event FailureEventHandler OnSessionFailed = null;

		/// <summary>
		/// Event raised, when DTLS state is changed.
		/// </summary>
		public event StateChangedEventHandler OnStateChanged = null;

		internal void StateChanged(object RemoteEndpoint, DtlsState State)
		{
			try
			{
				this.OnStateChanged?.Invoke(this, new StateChangedEventArgs(RemoteEndpoint, State));
			}
			catch (Exception ex)
			{
				Log.Critical(ex, RemoteEndpoint.ToString());
			}
		}

		private static ushort GetUInt16(byte[] Data, int Pos)
		{
			ushort Result = Data[Pos++];
			Result <<= 8;
			Result |= Data[Pos];

			return Result;
		}

		private static int GetUInt24(byte[] Data, int Pos)
		{
			int Result = Data[Pos++];
			Result <<= 8;
			Result |= Data[Pos++];
			Result <<= 8;
			Result |= Data[Pos];

			return Result;
		}

		private static ulong GetUInt48(byte[] Data, int Pos)
		{
			ulong Result = 0;
			int i;

			for (i = 0; i < 6; i++)
			{
				Result <<= 8;
				Result |= Data[Pos++];
			}

			return Result;
		}

		/// <summary>
		/// Sends a DTLS-encoded record.
		/// </summary>
		/// <param name="Type">Record type.</param>
		/// <param name="Fragment">Fragment.</param>
		/// <param name="More">If more records is to be included in the same datagram.</param>
		/// <param name="Resendable">If flight is resendable.</param>
		/// <param name="State">Endpoint state.</param>
		internal void SendRecord(ContentType Type, byte[] Fragment, bool More, bool Resendable,
			EndpointState State)
		{
			// §3, RFC 6655, defines seq_num: In DTLS, the 64-bit seq_num is the 16-bit epoch concatenated with the 48 - bit seq_num.

			ResendableRecord Rec;

			if (Resendable)
			{
				lock (State.lastFlight)
				{
					Rec = new ResendableRecord()
					{
						Type = Type,
						Fragment = Fragment,
						More = More
					};

					State.lastFlight.AddLast(Rec);
				}
			}
			else
				Rec = null;

			ushort Epoch = State.currentEpoch;
			ulong SequenceNr = State.currentSeqNr;

			State.currentSeqNr = (State.currentSeqNr + 1) & 0xffffffffffff;

			ulong SeqNum = Epoch;
			SeqNum <<= 48;
			SeqNum |= SequenceNr;

			ushort Length = (ushort)Fragment.Length;
			byte[] Header = new byte[13];
			byte[] Payload;
			int i;

			Header[0] = (byte)Type;
			Header[1] = 254;
			Header[2] = 253;

			for (i = 7; i >= 0; i--)
			{
				Header[3 + i] = (byte)SeqNum;
				SeqNum >>= 8;
			}

			Header[11] = (byte)(Length >> 8);
			Header[12] = (byte)Length;

			if (State.currentEpoch > 0 && State.currentCipher != null)
			{
				Payload = State.currentCipher.Encrypt(Fragment, Header, 0, State);
				Length = (ushort)Payload.Length;

				Header[11] = (byte)(Length >> 8);
				Header[12] = (byte)Length;
			}
			else
				Payload = Fragment;

			if (Payload.Length == 0 || Payload.Length > ushort.MaxValue)
				throw new ArgumentException("Fragment too large to be encoded.", nameof(Fragment));

			byte[] Record = new byte[Length + 13];

			Array.Copy(Header, 0, Record, 0, 13);
			Array.Copy(Payload, 0, Record, 13, Length);

			if (this.HasSniffers)
			{
				byte[] Readable = new byte[13 + Fragment.Length];

				Array.Copy(Record, 0, Readable, 0, 13);
				Array.Copy(Fragment, 0, Readable, 13, Fragment.Length);

				this.SniffMsg(Readable, 0, false);
			}

			if (More && State.buffer is null)
				State.buffer = new MemoryStream();

			if (State.buffer != null)
				State.buffer.Write(Record, 0, Record.Length);

			if (!More)
			{
				if (State.buffer != null)
				{
					Record = State.buffer.ToArray();
					State.buffer = null;
				}

				this.TransmitBinary(Record);

				if (this.probabilityPacketLoss == 0 || !this.PacketLost())
					this.communicationLayer.SendPacket(Record, State.remoteEndpoint);
				else if (this.HasSniffers)
					this.Warning(DateTime.Now.ToString("T") + " Transmitted packet lost.");

				if (Rec != null)
				{
					this.timeouts.Add(DateTime.Now.AddSeconds(State.timeoutSeconds + this.NextDouble()),
						this.CheckResend, new object[] { State, State.flightNr });
				}
			}
		}

		private void CheckResend(object P)
		{
			object[] A = (object[])P;
			EndpointState State = (EndpointState)A[0];
			long FlightNr = (long)A[1];
			LinkedList<ResendableRecord> Resend;

			if (FlightNr != State.flightNr)
				return;
			else if (State.timeoutSeconds >= 8)
			{
				if (this.HasSniffers)
					this.Error(DateTime.Now.ToString("T") + " Timeout. No response.");

				this.HandshakeFailure(State, "Timeout. No response.", AlertDescription.handshake_failure);
				return;
			}

			lock (State.lastFlight)
			{
				Resend = new LinkedList<ResendableRecord>();

				foreach (ResendableRecord Rec in State.lastFlight)
					Resend.AddLast(Rec);

				State.lastFlight.Clear();
			}

			State.timeoutSeconds <<= 1;

			if (State.acceptRollbackPrevEpoch)
			{
				State.currentEpoch--;
				State.currentCipher = State.previousCipher;
				State.leftEdgeSeqNr = State.previousLeftEdgeSeqNr;
				State.receivedPacketsWindow = State.previousReceivedPacketsWindow;
				State.currentSeqNr = State.previousSeqNr;
				State.next_receive_seq = State.flightRxSeq;
				State.message_seq = State.flightTxSeq;
				State.acceptRollbackPrevEpoch = false;
			}

			if (this.HasSniffers)
				this.Warning(DateTime.Now.ToString("T") + " Resending last flight.");

			foreach (ResendableRecord Rec in Resend)
			{
				this.SendRecord(Rec.Type, Rec.Fragment, Rec.More, true, State);

				if (Rec.Type == ContentType.change_cipher_spec)
					this.ChangeCipherSpec(State, State.isClient);
			}
		}

		/// <summary>
		/// Sends a handshake message.
		/// </summary>
		/// <param name="Type">Type of handshake message.</param>
		/// <param name="Payload">Payload.</param>
		/// <param name="More">If more records is to be included in the same datagram.</param>
		/// <param name="Resendable">If flight of records is resendable.</param>
		/// <param name="State">Endpoint state.</param>
		internal void SendHandshake(HandshakeType Type, byte[] Payload, bool More, bool Resendable, EndpointState State)
		{
			// TODO: Fragmentation of handshake message.

			byte[] Fragment = new byte[12 + Payload.Length];
			int Len = Payload.Length;

			if (Len > 0xffffff)
				throw new ArgumentException("Payload too big.", nameof(Payload));

			Fragment[0] = (byte)Type;

			Fragment[3] = (byte)Len;
			Len >>= 8;
			Fragment[2] = (byte)Len;
			Len >>= 8;
			Fragment[1] = (byte)Len;

			ushort MessageSeq = State.message_seq;

			Fragment[4] = (byte)(MessageSeq >> 8);
			Fragment[5] = (byte)MessageSeq;

			Fragment[6] = 0;    // Fragment offset.
			Fragment[7] = 0;
			Fragment[8] = 0;

			Fragment[9] = Fragment[1];  // Fragment length.
			Fragment[10] = Fragment[2];
			Fragment[11] = Fragment[3];

			Array.Copy(Payload, 0, Fragment, 12, Payload.Length);

			this.AddHandshakeMessageToHash(Type, Fragment, 0, Fragment.Length, State, true);

			this.SendRecord(ContentType.handshake, Fragment, More, Resendable, State);
			State.message_seq++;
		}

		/// <summary>
		/// HelloRequest is a simple notification that the client should begin
		/// the negotiation process anew.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <exception cref="DtlsException">If DTLS endpoint in client mode.</exception>
		public void SendHelloRequest(object RemoteEndpoint)
		{
			if (this.mode == DtlsMode.Client)
				throw new DtlsException("DTLS endpoints in client mode cannot request a party to start handshaking.");

			this.SendHandshake(HandshakeType.hello_request, new byte[0], false, true,
				this.GetState(RemoteEndpoint, false));
		}

		/// <summary>
		/// Starts connecting to the remote endpoint.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public void StartHandshake(object RemoteEndpoint)
		{
			this.StartHandshake(RemoteEndpoint, null);
		}

		/// <summary>
		/// Starts connecting to the remote endpoint.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Credentials">Credentials.</param>
		public void StartHandshake(object RemoteEndpoint, IDtlsCredentials Credentials)
		{
			if (this.mode == DtlsMode.Server)
				throw new DtlsException("DTLS server endpoints cannot start handshakes.");

			EndpointState State = this.GetState(RemoteEndpoint, true);
			State.credentials = Credentials;

			this.SendClientHello(State);
		}

		private EndpointState GetState(object RemoteEndpoint, bool IsClient)
		{
			if (!this.states.TryGetValue(RemoteEndpoint, out EndpointState Result))
			{
				Result = new EndpointState(this, RemoteEndpoint)
				{
					isClient = IsClient
				};

				lock (this.rnd)
				{
					if (IsClient)
						this.rnd.GetBytes(Result.clientRandom);
					else
						this.rnd.GetBytes(Result.serverRandom);

					this.rnd.GetBytes(Result.cookieRandom);
				}

				this.states.Add(RemoteEndpoint, Result);
			}

			return Result;
		}

		/// <summary>
		/// Sends the Client Hello message.
		/// </summary>
		/// <param name="State">Endpoint state.</param>
		private void SendClientHello(EndpointState State)
		{
			LinkedList<ICipher> Ciphers = new LinkedList<ICipher>();
			ushort CipherLen = 0;

			foreach (ICipher Cipher in ciphers)
			{
				if (Cipher.CanBeUsed(State))
				{
					Ciphers.AddLast(Cipher);
					CipherLen += 2;
				}
			}

			byte[] ClientHello = new byte[39 + State.sessionId.Length + State.cookie.Length + CipherLen];
			int Pos;
			ushort i16;

			ClientHello[0] = 254;   // Protocol version.
			ClientHello[1] = 253;

			this.SetUnixTime(State.clientRandom, 0);
			Array.Copy(State.clientRandom, 0, ClientHello, 2, 32);

			Pos = 34;

			ClientHello[Pos++] = (byte)State.sessionId.Length;
			Array.Copy(State.sessionId, 0, ClientHello, Pos, State.sessionId.Length);
			Pos += State.sessionId.Length;

			Array.Copy(State.cookie, 0, ClientHello, Pos, State.cookie.Length);
			Pos += State.cookie.Length;

			ClientHello[Pos++] = (byte)(CipherLen >> 8);
			ClientHello[Pos++] = (byte)CipherLen;

			foreach (ICipher Cipher in Ciphers)
			{
				i16 = Cipher.IanaCipherSuite;
				ClientHello[Pos++] = (byte)(i16 >> 8);
				ClientHello[Pos++] = (byte)i16;
			}

			ClientHello[Pos++] = 1;     // Compression method length 1
			ClientHello[Pos++] = 0;     // null compression.

			State.clientFinished = false;
			State.serverFinished = false;

			this.SendHandshake(HandshakeType.client_hello, ClientHello, false, true, State);
		}

		private void SetUnixTime(byte[] Rec, int Pos)
		{
			int UnixTime = (int)((DateTime.UtcNow - UnixEpoch).TotalSeconds + 0.5);

			Rec[Pos + 3] = (byte)UnixTime;
			UnixTime >>= 8;
			Rec[Pos + 2] = (byte)UnixTime;
			UnixTime >>= 8;
			Rec[Pos + 1] = (byte)UnixTime;
			UnixTime >>= 8;
			Rec[Pos] = (byte)UnixTime;
		}

		/// <summary>
		/// Unix epoch
		/// </summary>
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Adds a handshake message to the hash computation.
		/// </summary>
		/// <param name="Type">Handshake type.</param>
		/// <param name="Msg">Binary Message</param>
		/// <param name="Offset">Start of handshake section.</param>
		/// <param name="Count">Number of bytes.</param>
		/// <param name="State">Endpoint state.</param>
		/// <param name="Sending">If handshake message was sent (true), or received (false).</param>
		private void AddHandshakeMessageToHash(HandshakeType Type, byte[] Msg, int Offset, int Count,
			EndpointState State, bool Sending)
		{
			byte[] HashMsg;

			if (Offset == 0 && Count == Msg.Length)
				HashMsg = Msg;
			else
			{
				HashMsg = new byte[Count];
				Array.Copy(Msg, Offset, HashMsg, 0, Count);
			}

			switch (Type)
			{
				case HandshakeType.client_hello:
					State.handshake_client_hello = HashMsg;
					State.handshake_server_hello = null;
					State.handshake_server_certificate = null;
					State.handshake_server_key_exchange = null;
					State.handshake_certificate_request = null;
					State.handshake_server_hello_done = null;
					State.handshake_client_certificate = null;
					State.handshake_client_key_exchange = null;
					State.handshake_certificate_verify = null;
					State.handshake_client_finished = null;
					break;

				case HandshakeType.server_hello:
					State.handshake_server_hello = HashMsg;
					break;

				case HandshakeType.certificate:
					if (Sending)
					{
						if (State.isClient)
							State.handshake_client_certificate = HashMsg;
						else
							State.handshake_server_certificate = HashMsg;
					}
					else
					{
						if (State.isClient)
							State.handshake_server_certificate = HashMsg;
						else
							State.handshake_client_certificate = HashMsg;
					}
					break;

				case HandshakeType.server_key_exchange:
					State.handshake_server_key_exchange = HashMsg;
					break;

				case HandshakeType.certificate_request:
					State.handshake_certificate_request = HashMsg;
					break;

				case HandshakeType.server_hello_done:
					State.handshake_server_hello_done = HashMsg;
					break;

				case HandshakeType.client_key_exchange:
					State.handshake_client_key_exchange = HashMsg;
					break;

				case HandshakeType.certificate_verify:
					State.handshake_certificate_verify = HashMsg;
					break;

				case HandshakeType.finished:
					if (Sending)
					{
						if (State.isClient)
							State.handshake_client_finished = HashMsg;
					}
					else
					{
						if (!State.isClient)
							State.handshake_client_finished = HashMsg;
					}
					break;
			}
		}

		/// <summary>
		/// Chooses the pending cipher as the current cipher, and increases the epoch by one.
		/// </summary>
		/// <param name="State">Endpoint state.</param>
		/// <param name="IsClient">If the endpoint is a client.</param>
		internal void ChangeCipherSpec(EndpointState State, bool IsClient)
		{
			State.previousCipher = State.currentCipher;
			State.previousLeftEdgeSeqNr = State.leftEdgeSeqNr;
			State.previousReceivedPacketsWindow = State.receivedPacketsWindow;
			State.previousSeqNr = State.currentSeqNr;
			State.previousFlightRxSeq = State.flightRxSeq;
			State.previousFlightTxSeq = State.flightTxSeq;

			State.currentEpoch++;
			State.currentSeqNr = 0;
			State.leftEdgeSeqNr = 0;
			State.receivedPacketsWindow = 0;

			State.currentCipher = State.pendingCipher;
			State.isClient = IsClient;
			State.acceptRollbackPrevEpoch = true;
		}

		/// <summary>
		/// Sends application data to a remote endpoint.
		/// </summary>
		/// <param name="ApplicationData">Application data to send.</param>
		/// <param name="RemoteEndpoint">Remote endpoint to send the data to.</param>
		/// <exception cref="DtlsException">Thrown, if there's no session established to the 
		/// remote endpoint.</exception>
		public void SendApplicationData(byte[] ApplicationData, object RemoteEndpoint)
		{
			if (!this.states.TryGetValue(RemoteEndpoint, out EndpointState State) ||
				State.state != DtlsState.SessionEstablished)
			{
				throw new DtlsException("No DTLS session established with " +
					  RemoteEndpoint.ToString());
			}

			this.SendRecord(ContentType.application_data, ApplicationData, false, false, State);
		}

		/// <summary>
		/// Closes a session to a remote endpoint.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public void CloseSession(object RemoteEndpoint)
		{
			this.states.Remove(RemoteEndpoint);
		}

	}
}
