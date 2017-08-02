using System;
using System.Collections.Generic;
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
		private ulong receivedPacketsWindow = 0;
		private ulong leftEdgeSeqNr = 0;
		private ulong currentSeqNr = 0;
		private ushort currentEpoch = 0;

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
		{
			this.rnd = RandomNumberGenerator.Create();

			this.communicationLayer = CommunicationLayer;
			this.communicationLayer.PacketReceived += this.DataReceived;
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

				this.RecordReceived(Rec);
			}
		}

		private void RecordReceived(DTLSPlaintext Record)
		{
			if (Record.version.major != 254 || Record.version.minor != 253)
				return; // Not DTLS 1.2

			// Anti-replay §4.1.2.6

			if (Record.epoch != this.currentEpoch)
				return;

			long Offset = (long)(Record.sequence_number - this.leftEdgeSeqNr);

			if (Offset < 0)
				return;

			if (Offset < 64 && (this.receivedPacketsWindow & (1UL << (int)Offset)) != 0)
				return;

			// MAC validation

			if (!this.ProcessRecord(Record))
				return;

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

		private bool ProcessRecord(DTLSPlaintext Record)
		{
			return false;
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
		private void SendRecord(ContentType Type, byte[] Fragment)
		{
			if (Fragment.Length == 0 || Fragment.Length > ushort.MaxValue)
				throw new ArgumentException("Fragment too large to be encoded.", "Fragment");

			ushort Epoch = this.currentEpoch;
			ulong SequenceNr = this.currentSeqNr++;
			ushort Length = (ushort)Fragment.Length;
			byte[] Record = new byte[Length + 13];

			Record[0] = (byte)Type;
			Record[1] = 254;
			Record[2] = 253;
			Record[3] = (byte)(Epoch >> 8);
			Record[4] = (byte)Epoch;

			Record[10] = (byte)SequenceNr;
			SequenceNr >>= 8;
			Record[9] = (byte)SequenceNr;
			SequenceNr >>= 8;
			Record[8] = (byte)SequenceNr;
			SequenceNr >>= 8;
			Record[7] = (byte)SequenceNr;
			SequenceNr >>= 8;
			Record[6] = (byte)SequenceNr;
			SequenceNr >>= 8;
			Record[5] = (byte)SequenceNr;

			Record[11] = (byte)(Length >> 8);
			Record[12] = (byte)Length;

			Array.Copy(Fragment, 0, Record, 13, Length);

			this.communicationLayer.SendPacket(Record);
		}

		/// <summary>
		/// Sends a handshake message.
		/// </summary>
		/// <param name="Type">Type of handshake message.</param>
		/// <param name="Payload">Payload.</param>
		private void SendHandshake(HandshakeType Type, byte[] Payload)
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

			Fragment[4] = 0;  // Message sequence number.
			Fragment[5] = 0;

			Fragment[6] = 0;    // Fragment offset.
			Fragment[7] = 0;
			Fragment[8] = 0;

			Fragment[9] = Fragment[1];  // Fragment length.
			Fragment[10] = Fragment[2];
			Fragment[11] = Fragment[3];

			Array.Copy(Payload, 0, Fragment, 12, Payload.Length);

			this.SendRecord(ContentType.handshake, Fragment);
		}

		/// <summary>
		/// HelloRequest is a simple notification that the client should begin
		/// the negotiation process anew.
		/// </summary>
		public void SendHelloRequest()
		{
			this.SendHandshake(HandshakeType.hello_request, new byte[0]);
		}

		/// <summary>
		/// Sends the Client Hello message.
		/// </summary>
		public void SendClientHello()
		{
			ICipher[] Ciphers = ciphers;
			ushort CipherLen = (ushort)(Ciphers.Length * 2);
			byte[] ClientHello = new byte[40 + CipherLen];
			byte[] Rnd = new byte[28];
			int Pos;
			ushort i16;

			lock (this.rnd)
			{
				this.rnd.GetBytes(Rnd);
			}

			ClientHello[0] = 254;	// Protocol version.
			ClientHello[1] = 253;

			this.SetUnixTime(ClientHello, 2);

			Array.Copy(Rnd, 0, ClientHello, 6, 28);

			ClientHello[34] = 0;    // Session ID length
			ClientHello[35] = 0;    // Cookie length

			ClientHello[36] = (byte)(CipherLen >> 8);
			ClientHello[37] = (byte)CipherLen;

			Pos = 38;
			foreach (ICipher Cipher in Ciphers)
			{
				i16 = Cipher.IanaCipherSuite;
				ClientHello[Pos++] = (byte)(i16 >> 8);
				ClientHello[Pos++] = (byte)i16;
			}

			ClientHello[Pos++] = 1;		// Compression method length 1
			ClientHello[Pos++] = 0;		// null compression.

			this.SendHandshake(HandshakeType.client_hello, ClientHello);
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

	}
}
