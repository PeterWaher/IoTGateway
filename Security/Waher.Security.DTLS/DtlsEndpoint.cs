#define DEBUG_OUTPUT
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// DTLS endpoint class.
	/// </summary>
	public class DtlsEndpoint : IDisposable
	{
		private static ICipher[] ciphers = null;

		private RandomNumberGenerator rnd;
		private ICommunicationLayer communicationLayer;
		private ICipher pendingCipher = null;
		private ICipher currentCipher = null;
		private MemoryStream buffer = null;
		private MemoryStream handshake = null;
		private byte[] cookie = new byte[1] { 0 };
		private byte[] sessionId = new byte[1] { 0 };
		private byte[] clientRandom = new byte[32];
		private byte[] serverRandom = new byte[0];
		private byte[] pskIdentity;
		private byte[] pskKey;
		private ulong receivedPacketsWindow = 0;
		private ulong leftEdgeSeqNr = 0;
		private ulong currentSeqNr = 0;
		private ushort currentEpoch = 0;
		private ushort message_seq = 0;
		private ushort? next_receive_seq = null;

		static DtlsEndpoint()
		{
			InitCiphers();
			Types.OnInvalidated += (sender, e) => InitCiphers();
		}

		private static void InitCiphers()
		{
			List<ICipher> Ciphers = new List<ICipher>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICipher)))
			{
				if (T.GetTypeInfo().IsAbstract)
					continue;

				try
				{
					ICipher Cipher = (ICipher)Activator.CreateInstance(T);
					Ciphers.Add(Cipher);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			Ciphers.Sort((x, y) => y.Priority - x.Priority);
			ciphers = Ciphers.ToArray();
		}

		/// <summary>
		/// DTLS endpoint class.
		/// </summary>
		/// <param name="CommunicationLayer">Communication layer.</param>
		public DtlsEndpoint(ICommunicationLayer CommunicationLayer)
			: this(CommunicationLayer, (byte[])null, (byte[])null)
		{
		}

		/// <summary>
		/// DTLS endpoint class.
		/// </summary>
		/// <param name="CommunicationLayer">Communication layer.</param>
		/// <param name="PskIdentity">Identity of Pre-shared Key (PSK).</param>
		/// <param name="PskKey">Pre-shared Key (PSK).</param>
		public DtlsEndpoint(ICommunicationLayer CommunicationLayer, string PskIdentity, string PskKey)
			: this(CommunicationLayer, Encoding.UTF8.GetBytes(PskIdentity), Encoding.UTF8.GetBytes(PskKey))
		{
		}

		/// <summary>
		/// DTLS endpoint class.
		/// </summary>
		/// <param name="CommunicationLayer">Communication layer.</param>
		/// <param name="PskIdentity">Identity of Pre-shared Key (PSK).</param>
		/// <param name="PskKey">Pre-shared Key (PSK).</param>
		public DtlsEndpoint(ICommunicationLayer CommunicationLayer, string PskIdentity, byte[] PskKey)
				: this(CommunicationLayer, Encoding.UTF8.GetBytes(PskIdentity), PskKey)
		{
		}

		/// <summary>
		/// DTLS endpoint class.
		/// </summary>
		/// <param name="CommunicationLayer">Communication layer.</param>
		/// <param name="PskIdentity">Identity of Pre-shared Key (PSK).</param>
		/// <param name="PskKey">Pre-shared Key (PSK).</param>
		public DtlsEndpoint(ICommunicationLayer CommunicationLayer, byte[] PskIdentity, byte[] PskKey)
		{
			this.rnd = RandomNumberGenerator.Create();
			this.rnd.GetBytes(this.clientRandom);

			this.pskIdentity = PskIdentity;
			this.pskKey = PskKey;

			this.communicationLayer = CommunicationLayer;
			this.communicationLayer.PacketReceived += this.DataReceived;
		}

		/// <summary>
		/// If pre-shared keys (PSK) are used.
		/// </summary>
		internal bool UsesPsk
		{
			get
			{
				return this.pskIdentity != null && this.pskKey != null;
			}
		}

		internal byte[] PskKey
		{
			get { return this.pskKey; }
		}

		internal byte[] PskIdentity
		{
			get { return this.pskIdentity; }
		}

		internal byte[] TotalHasdshake
		{
			get
			{
				if (this.handshake == null)
					return new byte[0];
				else
					return this.handshake.ToArray();
			}
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

			if (this.buffer != null)
			{
				this.buffer.Dispose();
				this.buffer = null;
			}

			if (this.handshake != null)
			{
				this.handshake.Dispose();
				this.handshake = null;
			}
		}

		private void DataReceived(byte[] Data)
		{
			int Pos = 0;
			int Len = Data.Length;

			while (Pos + 13 <= Len)
			{
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

				this.RecordReceived(Rec, Data);
			}
		}

		private void RecordReceived(DTLSPlaintext Record, byte[] RecordData)
		{
			if (Record.version.major != 254)
				return; // Not DTLS 1.x

			// Anti-replay §4.1.2.6

			if (Record.epoch != this.currentEpoch)
				return;

			long Offset = (long)(Record.sequence_number - this.leftEdgeSeqNr);

			if (Offset < 0)
				return;

			if (Offset < 64 && (this.receivedPacketsWindow & (1UL << (int)Offset)) != 0)
				return;

			if (this.currentEpoch > 0 && this.currentCipher != null)
			{
				Record.fragment = this.currentCipher.Decrypt(Record.fragment, RecordData);
				if (Record.fragment == null)
					return;

				Record.length = (ushort)Record.fragment.Length;
			}

			// TODO: Queue future sequence numbers, is handshake. These must be processed in order.

			this.ProcessRecord(Record);

			// Update receive window

			if (Offset >= 64)
			{
				ulong Diff = (ulong)(Offset - 63);

				if (Diff >= 64)
					this.receivedPacketsWindow = 0;
				else
					this.receivedPacketsWindow >>= (int)Diff;

				this.leftEdgeSeqNr += Diff;
				Offset -= (long)Diff;
			}

			this.receivedPacketsWindow |= 1UL << (int)Offset;
		}

		private void ProcessRecord(DTLSPlaintext Record)
		{
			switch (Record.type)
			{
				case ContentType.handshake:
					try
					{
						HandshakeType HandshakeType = (HandshakeType)Record.fragment[0];

						int PayloadLen = Record.fragment[1];
						PayloadLen <<= 8;
						PayloadLen |= Record.fragment[2];
						PayloadLen <<= 8;
						PayloadLen |= Record.fragment[3];

						int MessageSeqNr = Record.fragment[4];
						MessageSeqNr <<= 8;
						MessageSeqNr |= Record.fragment[5];

						if (MessageSeqNr != this.next_receive_seq)
						{
							if (MessageSeqNr != 0)
								break;  // Not the expected handshake sequence number.
							else
								this.next_receive_seq = 0;
						}

						this.next_receive_seq++;

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
							HandshakeType == HandshakeType.client_hello);

						switch (HandshakeType)
						{
							case HandshakeType.hello_verify_request:
								if (Record.fragment[Pos++] != 254)  // Major version.
									this.HandshakeFailure("DTLS version mismatch.");
								else
								{
									Pos++;  // Minor version.

									int Len = Record.fragment[Pos++];
									this.cookie = new byte[Len + 1];
									Array.Copy(Record.fragment, Pos - 1, this.cookie, 0, Len + 1);
									Pos += Len;

									this.SendClientHello();
								}
								break;

							case HandshakeType.server_hello:
								if (Record.fragment[Pos++] != 254 || Record.fragment[Pos++] != 253)  // Protocol version.
									this.HandshakeFailure("DTLS version mismatch.");
								else
								{
									this.serverRandom = new byte[32];
									Array.Copy(Record.fragment, Pos, this.serverRandom, 0, 32);
									Pos += 32;

									byte[] PrevSessionId = this.sessionId;

									int Len = Record.fragment[Pos++];
									this.sessionId = new byte[Len + 1];
									Array.Copy(Record.fragment, Pos - 1, this.sessionId, 0, Len + 1);
									Pos += Len;

									ushort CipherSuite = Record.fragment[Pos++];
									CipherSuite <<= 8;
									CipherSuite |= Record.fragment[Pos++];

									byte CompressionMethod = Record.fragment[Pos++];

									// TODO: Compression methods.
									// TODO: Extensions

									this.pendingCipher = null;

									foreach (ICipher C in ciphers)
									{
										if (C.IanaCipherSuite == CipherSuite)
										{
											this.pendingCipher = (ICipher)Activator.CreateInstance(C.GetType());
											break;
										}
									}

									if (this.pendingCipher == null || CompressionMethod != 0)
										this.HandshakeFailure("Cipher and compression mode agreement not reached.");
									else
									{
										this.pendingCipher.ClientRandom = this.clientRandom;
										this.pendingCipher.ServerRandom = this.serverRandom;

										if (AreEqual(PrevSessionId, this.sessionId))
											this.HandshakeSuccess();
									}
								}
								break;

							case HandshakeType.server_hello_done:
								this.pendingCipher.SendClientKeyExchange(this);
								break;

							case HandshakeType.server_key_exchange:
								if (this.pendingCipher != null)
									this.pendingCipher.ServerKeyExchange(Record.fragment, ref Pos);
								break;

							case HandshakeType.client_hello:            // TODO
							case HandshakeType.hello_request:           // TODO
							case HandshakeType.certificate:             // TODO
							case HandshakeType.certificate_request:     // TODO
							case HandshakeType.certificate_verify:      // TODO
							case HandshakeType.client_key_exchange:     // TODO
							case HandshakeType.finished:                // TODO
								break;
						}
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						this.HandshakeFailure("Unexpected error: " + ex.Message);
					}
					break;

				case ContentType.change_cipher_spec:
					this.ChangeCipherSpec(false);
					break;

				case ContentType.application_data:
				case ContentType.alert:
				default:
					break;
			}
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

		private void HandshakeSuccess()
		{
			// TODO
		}

		private void HandshakeFailure(string Reason)
		{
			// TODO
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
		internal void SendRecord(ContentType Type, byte[] Fragment, bool More)
		{
			// §3, RFC 6655, defines seq_num: In DTLS, the 64-bit seq_num is the 16-bit epoch concatenated with the 48 - bit seq_num.

			ushort Epoch = this.currentEpoch;
			ulong SequenceNr = this.currentSeqNr;

			this.currentSeqNr = (this.currentSeqNr + 1) & 0xffffffffffff;

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

			if (this.currentEpoch > 0 && this.currentCipher != null)
			{
				Fragment = this.currentCipher.Encrypt(Fragment, Header);
				Length = (ushort)Fragment.Length;

				Header[11] = (byte)(Length >> 8);
				Header[12] = (byte)Length;
			}

			if (Fragment.Length == 0 || Fragment.Length > ushort.MaxValue)
				throw new ArgumentException("Fragment too large to be encoded.", "Fragment");

			byte[] Record = new byte[Length + 13];

			Array.Copy(Header, 0, Record, 0, 13);
			Array.Copy(Fragment, 0, Record, 13, Length);

			if (this.buffer != null)
			{
				this.buffer.Write(Record, 0, Record.Length);

				if (!More)
				{
					this.communicationLayer.SendPacket(this.buffer.ToArray());
					this.buffer = null;
				}
			}
			else if (More)
			{
				this.buffer = new MemoryStream();
				this.buffer.Write(Record, 0, Record.Length);
			}
			else
				this.communicationLayer.SendPacket(Record);
		}

		/// <summary>
		/// Sends a handshake message.
		/// </summary>
		/// <param name="Type">Type of handshake message.</param>
		/// <param name="Payload">Payload.</param>
		/// <param name="More">If more records is to be included in the same datagram.</param>
		internal void SendHandshake(HandshakeType Type, byte[] Payload, bool More)
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

			ushort MessageSeq = this.message_seq++;

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
				Type == HandshakeType.client_hello);    // TODO: Handle retransmissions.

			this.SendRecord(ContentType.handshake, Fragment, More);
		}

		/// <summary>
		/// HelloRequest is a simple notification that the client should begin
		/// the negotiation process anew.
		/// </summary>
		public void SendHelloRequest()
		{
			this.SendHandshake(HandshakeType.hello_request, new byte[0], false);
		}

		/// <summary>
		/// Starts connecting to the remote endpoint.
		/// </summary>
		public void StartHandshake()
		{
			this.SendClientHello();
		}

		/// <summary>
		/// Sends the Client Hello message.
		/// </summary>
		private void SendClientHello()
		{
			LinkedList<ICipher> Ciphers = new LinkedList<ICipher>();
			ushort CipherLen = 0;

			foreach (ICipher Cipher in ciphers)
			{
				if (Cipher.CanBeUsed(this))
				{
					Ciphers.AddLast(Cipher);
					CipherLen += 2;
				}
			}

			byte[] ClientHello = new byte[38 + this.sessionId.Length + this.cookie.Length + CipherLen];
			int Pos;
			ushort i16;

			ClientHello[0] = 254;   // Protocol version.
			ClientHello[1] = 253;

			this.SetUnixTime(this.clientRandom, 0);
			Array.Copy(this.clientRandom, 0, ClientHello, 2, 32);

			Pos = 34;

			Array.Copy(this.sessionId, 0, ClientHello, Pos, this.sessionId.Length);
			Pos += this.sessionId.Length;

			Array.Copy(this.cookie, 0, ClientHello, Pos, this.cookie.Length);
			Pos += this.cookie.Length;

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

			this.SendHandshake(HandshakeType.client_hello, ClientHello, false);

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
		private void AddHandshakeMessageToHash(string Label, byte[] Msg, int Offset, int Count, bool First)
		{
			if (First)
			{
				this.handshake = new MemoryStream();

#if DEBUG_OUTPUT
				Console.Out.WriteLine("Clear");
#endif
			}

			if (this.handshake != null)
			{
				this.handshake.Write(Msg, Offset, Count);

#if DEBUG_OUTPUT
				Print(Label, Msg, Offset, Count);
#endif
			}
		}

		/// <summary>
		/// Chooses the pending cipher as the current cipher, and increases the epoch by one.
		/// </summary>
		/// <param name="IsClient">If the endpoint is a client.</param>
		internal void ChangeCipherSpec(bool IsClient)
		{
			this.currentEpoch++;
			this.currentSeqNr = 0;
			this.leftEdgeSeqNr = 0;
			this.receivedPacketsWindow = 0;
			this.currentCipher = this.pendingCipher;
			this.currentCipher.IsClient = IsClient;
		}

#if DEBUG_OUTPUT
		private static void Print(string Label, byte[] A, int Offset, int Count)
		{
			Console.Out.Write(Label);
			Console.Out.WriteLine(":");

			int i, j;

			for (i = 0; i < Count; i++)
			{
				j = i & 15;

				if (j == 0 && i > 0)
					Console.Out.WriteLine();
				else if (i > 0)
					Console.Out.Write(" ");

				Console.Out.Write(A[i + Offset].ToString("x2"));
			}

			Console.Out.WriteLine();
		}
#endif

	}
}
