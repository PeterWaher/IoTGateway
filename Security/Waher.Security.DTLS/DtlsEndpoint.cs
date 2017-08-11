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
	/// DTLS endpoint class.
	/// </summary>
	public class DtlsEndpoint : Sniffable, IDisposable
	{
		private static ICipher[] ciphers = null;
		private static Dictionary<ushort, ICipher> ciphersPerCode = null;

		private Cache<object, EndpointState> states;
		private RandomNumberGenerator rnd;
		private ICommunicationLayer communicationLayer;
		private IUserSource users;

		static DtlsEndpoint()
		{
			InitCiphers();
			Types.OnInvalidated += (sender, e) => InitCiphers();
		}

		private static void InitCiphers()
		{
			List<ICipher> Ciphers = new List<ICipher>();
			Dictionary<ushort, ICipher> PerCode = new Dictionary<ushort, ICipher>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICipher)))
			{
				if (T.GetTypeInfo().IsAbstract)
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
		/// DTLS endpoint class.
		/// </summary>
		/// <param name="CommunicationLayer">Communication layer.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public DtlsEndpoint(ICommunicationLayer CommunicationLayer, params ISniffer[] Sniffers)
			: this(CommunicationLayer, null, Sniffers)
		{
		}

		/// <summary>
		/// DTLS endpoint class.
		/// </summary>
		/// <param name="CommunicationLayer">Communication layer.</param>
		/// <param name="Users">User data source, if pre-shared keys should be allowed by a DTLS server endpoint.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public DtlsEndpoint(ICommunicationLayer CommunicationLayer, IUserSource Users,
			params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.users = Users;
			this.rnd = RandomNumberGenerator.Create();
			this.states = new Cache<object, EndpointState>(int.MaxValue,
				TimeSpan.MaxValue, new TimeSpan(1, 0, 0));

			this.states.Removed += States_Removed;

			this.communicationLayer = CommunicationLayer;
			this.communicationLayer.PacketReceived += this.DataReceived;
		}

		private void States_Removed(object Sender, CacheItemEventArgs<object, EndpointState> e)
		{
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
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
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

			if (this.states != null)
			{
				this.states.Clear();
				this.states.Dispose();
				this.states = null;
			}
		}

		private void DataReceived(byte[] Data, object RemoteEndpoint)
		{
			int Pos = 0;
			int Len = Data.Length;
			int Start;

			this.ReceiveBinary(Data);

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
					fragment = null
				};

				Pos += 13;
				if (Pos + Rec.length > Len)
					break;

				Rec.fragment = new byte[Rec.length];
				Array.Copy(Data, Pos, Rec.fragment, 0, Rec.length);
				Pos += Rec.length;

				this.RecordReceived(Rec, Data, Start, RemoteEndpoint);
			}
		}

		private void RecordReceived(DTLSPlaintext Record, byte[] RecordData, int Start,
			object RemoteEndpoint)
		{
			if (Record.version.major != 254)
				return; // Not DTLS 1.x

			EndpointState State = this.GetState(RemoteEndpoint, false);

			// Anti-replay §4.1.2.6

			if (State.acceptRollbackPrevEpoch)
			{
				if (Record.epoch == State.currentEpoch - 1)
				{
					State.currentEpoch--;
					State.currentCipher = State.previousCipher;
					State.leftEdgeSeqNr = State.previousLeftEdgeSeqNr;
					State.receivedPacketsWindow = State.previousReceivedPacketsWindow;
					State.currentSeqNr = State.previousSeqNr;
				}

				State.acceptRollbackPrevEpoch = false;
			}

			if (Record.epoch != State.currentEpoch)
				return;

			long Offset = (long)(Record.sequence_number - State.leftEdgeSeqNr);

			if (Offset < 0)
				return;

			if (Offset < 64 && (State.receivedPacketsWindow & (1UL << (int)Offset)) != 0)
				return;

			if (State.currentEpoch > 0 && State.currentCipher != null)
			{
				Record.fragment = State.currentCipher.Decrypt(Record.fragment, RecordData, Start, State);
				if (Record.fragment == null)
					return;

				Record.length = (ushort)Record.fragment.Length;
			}

			// TODO: Queue future sequence numbers, is handshake. These must be processed in order.

			this.ProcessRecord(Record, State);

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
		}

		private void ProcessRecord(DTLSPlaintext Record, EndpointState State)
		{
			try
			{
				if (this.HasSniffers)
				{
					switch (Record.type)
					{
						case ContentType.handshake:
							this.Information("RX: " + Record.type.ToString() + ", " +
								((HandshakeType)Record.fragment[0]).ToString());
							break;

						case ContentType.alert:
							this.Information("RX: " + Record.type.ToString() + ", " +
								((AlertLevel)Record.fragment[0]).ToString() +
								((AlertDescription)Record.fragment[1]).ToString());
							break;

						default:
							this.Information("RX: " + Record.type.ToString());
							break;
					}
				}

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
								break;  // Not the expected handshake sequence number.
							else
								State.next_receive_seq = 0;
						}

						State.next_receive_seq++;

						int FragmentOffset = Record.fragment[6];
						FragmentOffset <<= 8;
						FragmentOffset |= Record.fragment[7];
						FragmentOffset <<= 8;
						FragmentOffset |= Record.fragment[8];

						int FragmentLength = Record.fragment[9];
						FragmentLength <<= 8;
						FragmentLength |= Record.fragment[10];
						FragmentLength <<= 8;
						FragmentLength |= Record.fragment[11];

						if (FragmentOffset > 0 || FragmentLength != PayloadLen)
							break;   // TODO: Reassembly of fragmented messages.

						int Pos = 12;

						this.AddHandshakeMessageToHash("<-" + HandshakeType.ToString(),
							Record.fragment, 0, Record.fragment.Length,
							HandshakeType == HandshakeType.client_hello, State);

						switch (HandshakeType)
						{
							case HandshakeType.hello_verify_request:
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

									if (State.pendingCipher == null || CompressionMethod != 0)
									{
										this.HandshakeFailure(State, "Cipher and compression mode agreement not reached.",
											AlertDescription.handshake_failure);
									}
									else if (AreEqual(PrevSessionId, State.sessionId))
									{
										this.HandshakeSuccess(State);   // TODO: ChangeCipherSpec and Finished before Success.
									}
								}
								break;

							case HandshakeType.server_hello_done:
								State.pendingCipher.SendClientKeyExchange(this, State);
								break;

							case HandshakeType.server_key_exchange:
								if (State.pendingCipher != null)
									State.pendingCipher.ServerKeyExchange(Record.fragment, ref Pos, State);
								break;

							case HandshakeType.client_hello:
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

								if (Cipher == null || !NullCompression)
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
											HelloVerifyRequest, false, State);

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

								this.SendHandshake(HandshakeType.server_hello, ServerHello, true, State);
								Cipher.SendServerKeyExchange(this, State);
								break;

							case HandshakeType.client_key_exchange:
								if (State.pendingCipher != null)
									State.pendingCipher.ClientKeyExchange(Record.fragment, ref Pos, State);
								break;

							case HandshakeType.hello_request:           // TODO
							case HandshakeType.certificate:             // TODO
							case HandshakeType.certificate_request:     // TODO
							case HandshakeType.certificate_verify:      // TODO
							case HandshakeType.finished:                // TODO
								break;
						}
						break;

					case ContentType.change_cipher_spec:
						this.ChangeCipherSpec(State, State.isClient);
						break;

					case ContentType.alert:
						if (Record.fragment.Length >= 2)
						{
							AlertLevel Level = (AlertLevel)Record.fragment[0];
							AlertDescription Description = (AlertDescription)Record.fragment[1];

							if (Level == AlertLevel.fatal)
								this.HandshakeFailure(State, "Fatal error.", Description);
							else
							{
								this.Warning("Non-fatal alert received: " + Description.ToString());

								switch (Description)
								{
									case AlertDescription.close_notify:
										State.State = DtlsState.Closed;
										break;

									case AlertDescription.handshake_failure:
										this.HandshakeFailure(State, "Handshake failed.", Description);
										break;
								}
							}
						}
						break;

					case ContentType.application_data:
					// TODO
					default:
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				this.HandshakeFailure(State, "Unexpected error: " + ex.Message, AlertDescription.internal_error);
			}
		}

		internal void SendAlert(AlertLevel Level, AlertDescription Description, EndpointState State)
		{
			this.SendRecord(ContentType.alert, new byte[] { (byte)Level, (byte)Description }, false, State);
		}

		private static bool AreEqual(byte[] A1, byte[] A2)
		{
			if ((A1 == null) ^ (A2 == null))
				return false;

			if (A1 == null)
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

		private void HandshakeSuccess(EndpointState State)
		{
			State.State = DtlsState.SessionEstablished;

			try
			{
				this.OnHandshakeSuccessful?.Invoke(this, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when handshake has been successful.
		/// </summary>
		public event EventHandler OnHandshakeSuccessful = null;

		private void HandshakeFailure(EndpointState State, string Reason, AlertDescription Descripton)
		{
			State.State = DtlsState.Failed;

			try
			{
				this.OnHandshakeFailed?.Invoke(this, new HandshakeFailureEventArgs(Reason, Descripton));
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
		public event HandshakeFailureEventHandler OnHandshakeFailed = null;

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
		/// <param name="State">Endpoint state.</param>
		internal void SendRecord(ContentType Type, byte[] Fragment, bool More, EndpointState State)
		{
			// §3, RFC 6655, defines seq_num: In DTLS, the 64-bit seq_num is the 16-bit epoch concatenated with the 48 - bit seq_num.

			if (this.HasSniffers)
			{
				switch (Type)
				{
					case ContentType.handshake:
						this.Information("TX: " + Type.ToString() + ", " +
							((HandshakeType)Fragment[0]).ToString());
						break;

					case ContentType.alert:
						this.Information("TX: " + Type.ToString() + ", " +
							((AlertLevel)Fragment[0]).ToString() +
							((AlertDescription)Fragment[1]).ToString());
						break;

					default:
						this.Information("TX: " + Type.ToString());
						break;
				}
			}

			ushort Epoch = State.currentEpoch;
			ulong SequenceNr = State.currentSeqNr;

			State.currentSeqNr = (State.currentSeqNr + 1) & 0xffffffffffff;

			ulong SeqNum = Epoch;
			SeqNum <<= 48;
			SeqNum |= SequenceNr;

			ushort Length = (ushort)Fragment.Length;
			byte[] Header = new byte[13];
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
				Fragment = State.currentCipher.Encrypt(Fragment, Header, 0, State);
				Length = (ushort)Fragment.Length;

				Header[11] = (byte)(Length >> 8);
				Header[12] = (byte)Length;
			}

			if (Fragment.Length == 0 || Fragment.Length > ushort.MaxValue)
				throw new ArgumentException("Fragment too large to be encoded.", "Fragment");

			byte[] Record = new byte[Length + 13];

			Array.Copy(Header, 0, Record, 0, 13);
			Array.Copy(Fragment, 0, Record, 13, Length);

			if (State.buffer != null)
			{
				State.buffer.Write(Record, 0, Record.Length);

				if (!More)
				{
					byte[] Data = State.buffer.ToArray();

					this.TransmitBinary(Data);
					this.communicationLayer.SendPacket(Data, State.remoteEndpoint);
					State.buffer = null;
				}
			}
			else if (More)
			{
				State.buffer = new MemoryStream();
				State.buffer.Write(Record, 0, Record.Length);
			}
			else
			{
				this.TransmitBinary(Record);
				this.communicationLayer.SendPacket(Record, State.remoteEndpoint);
			}
		}

		/// <summary>
		/// Sends a handshake message.
		/// </summary>
		/// <param name="Type">Type of handshake message.</param>
		/// <param name="Payload">Payload.</param>
		/// <param name="More">If more records is to be included in the same datagram.</param>
		/// <param name="State">Endpoint state.</param>
		internal void SendHandshake(HandshakeType Type, byte[] Payload, bool More, EndpointState State)
		{
			// TODO: Fragmentation of handshake message.

			byte[] Fragment = new byte[12 + Payload.Length];
			int Len = Payload.Length;

			if (Len > 0xffffff)
				throw new ArgumentException("Payload too big.");

			Fragment[0] = (byte)Type;

			Fragment[3] = (byte)Len;
			Len >>= 8;
			Fragment[2] = (byte)Len;
			Len >>= 8;
			Fragment[1] = (byte)Len;

			ushort MessageSeq = State.message_seq++;

			Fragment[4] = (byte)(MessageSeq >> 8);
			Fragment[5] = (byte)MessageSeq;

			Fragment[6] = 0;    // Fragment offset.
			Fragment[7] = 0;
			Fragment[8] = 0;

			Fragment[9] = Fragment[1];  // Fragment length.
			Fragment[10] = Fragment[2];
			Fragment[11] = Fragment[3];

			Array.Copy(Payload, 0, Fragment, 12, Payload.Length);

			this.AddHandshakeMessageToHash(Type.ToString() + "->", Fragment, 0, Fragment.Length,
				Type == HandshakeType.client_hello, State);    // TODO: Handle retransmissions.

			this.SendRecord(ContentType.handshake, Fragment, More, State);
		}

		/// <summary>
		/// HelloRequest is a simple notification that the client should begin
		/// the negotiation process anew.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public void SendHelloRequest(object RemoteEndpoint)
		{
			this.SendHandshake(HandshakeType.hello_request, new byte[0], false,
				this.GetState(RemoteEndpoint, false));
		}

		/// <summary>
		/// Starts connecting to the remote endpoint.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public void StartHandshake(object RemoteEndpoint)
		{
			this.StartHandshake(RemoteEndpoint, (byte[])null, (byte[])null);
		}

		/// <summary>
		/// Starts connecting to the remote endpoint.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="PskIdentity">Identity of Pre-shared Key (PSK).</param>
		/// <param name="PskKey">Pre-shared Key (PSK).</param>
		public void StartHandshake(object RemoteEndpoint, string PskIdentity, string PskKey)
		{
			this.StartHandshake(RemoteEndpoint, Encoding.UTF8.GetBytes(PskIdentity),
				Encoding.UTF8.GetBytes(PskKey));
		}

		/// <summary>
		/// Starts connecting to the remote endpoint.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="PskIdentity">Identity of Pre-shared Key (PSK).</param>
		/// <param name="PskKey">Pre-shared Key (PSK).</param>
		public void StartHandshake(object RemoteEndpoint, string PskIdentity, byte[] PskKey)
		{
			this.StartHandshake(RemoteEndpoint, Encoding.UTF8.GetBytes(PskIdentity), PskKey);
		}

		/// <summary>
		/// Starts connecting to the remote endpoint.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="PskIdentity">Identity of Pre-shared Key (PSK).</param>
		/// <param name="PskKey">Pre-shared Key (PSK).</param>
		public void StartHandshake(object RemoteEndpoint, byte[] PskIdentity, byte[] PskKey)
		{
			EndpointState State = this.GetState(RemoteEndpoint, true);

			State.pskIdentity = PskIdentity;
			State.pskKey = PskKey;

			this.SendClientHello(this.GetState(RemoteEndpoint, true));
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

			this.SendHandshake(HandshakeType.client_hello, ClientHello, false, State);

			// TODO: Retries.
		}

		private void SetUnixTime(byte[] Rec, int Pos)
		{
			int UnixTime = (int)((DateTime.UtcNow - unixEpoch).TotalSeconds + 0.5);

			Rec[Pos + 3] = (byte)UnixTime;
			UnixTime >>= 8;
			Rec[Pos + 2] = (byte)UnixTime;
			UnixTime >>= 8;
			Rec[Pos + 1] = (byte)UnixTime;
			UnixTime >>= 8;
			Rec[Pos] = (byte)UnixTime;
		}

		private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Adds a handshake message to the hash computation.
		/// </summary>
		/// <param name="Label">Label for message.</param>
		/// <param name="Msg">Binary Message</param>
		/// <param name="Offset">Start of handshake section.</param>
		/// <param name="Count">Number of bytes.</param>
		/// <param name="First">If the message is the first in the sequence.</param>
		/// <param name="State">Endpoint state.</param>
		private void AddHandshakeMessageToHash(string Label, byte[] Msg, int Offset, int Count,
			bool First, EndpointState State)
		{
			if (First)
			{
				if (State.handshakeHashCalculator == null)
				{
					State.handshakeHashCalculator = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
					State.handshakeHashCalculator2 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
				}
				else
				{
					State.handshakeHashCalculator.GetHashAndReset();
					State.handshakeHashCalculator2.GetHashAndReset();
				}

				//this.Information("Handshake restarted for " + State.remoteEndpoint.ToString());
			}

			if (State.handshakeHashCalculator != null)
			{
				State.handshakeHashCalculator.AppendData(Msg, Offset, Count);
				State.handshakeHashCalculator2.AppendData(Msg, Offset, Count);
				State.handshakeHash = null;
				State.handshakeHash2 = null;

				//this.Information(ToString(Label, Msg, Offset, Count));
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

			State.currentEpoch++;
			State.currentSeqNr = 0;
			State.leftEdgeSeqNr = 0;
			State.receivedPacketsWindow = 0;

			State.currentCipher = State.pendingCipher;
			State.isClient = IsClient;

			if (IsClient)
				State.acceptRollbackPrevEpoch = true;
		}

		internal static string ToString(string Label, byte[] A)
		{
			return ToString(Label, A, 0, A?.Length ?? 0);
		}

		internal static string ToString(string Label, byte[] A, int Offset, int Count)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(Label);
			sb.AppendLine(":");

			int i, j;

			for (i = 0; i < Count; i++)
			{
				j = i & 15;

				if (j == 0 && i > 0)
					sb.AppendLine();
				else if (i > 0)
					sb.Append(" ");

				sb.Append(A[i + Offset].ToString("x2"));
			}

			sb.AppendLine();

			return sb.ToString();
		}

	}
}
