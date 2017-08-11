using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Waher.Security.DTLS.Ciphers
{
	/// <summary>
	/// Base class for all ciphers used in (D)TLS.
	/// </summary>
	public abstract class Cipher : ICipher
	{
		/// <summary>
		/// Size of MAC keys, in bytes.
		/// </summary>
		protected int macKeyLength;

		/// <summary>
		/// Size of encryption keys, in bytes.
		/// </summary>
		protected int encKeyLength;

		/// <summary>
		/// Sized of fixed IV part of nonces, in bytes.
		/// </summary>
		protected int fixedIvLength;

		/// <summary>
		/// Base class for all ciphers used in (D)TLS.
		/// </summary>
		/// <param name="MacKeyLength">MAC key length.</param>
		/// <param name="EncKeyLength">Encryption key size.</param>
		/// <param name="FixedIvLength">Fixed IV length.</param>
		public Cipher(int MacKeyLength, int EncKeyLength, int FixedIvLength)
		{
			this.macKeyLength = MacKeyLength;
			this.encKeyLength = EncKeyLength;
			this.fixedIvLength = FixedIvLength;
		}

		/// <summary>
		/// Cipher name.
		/// </summary>
		public abstract string Name
		{
			get;
		}

		/// <summary>
		/// IANA cipher suite code:
		/// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-4
		/// </summary>
		public abstract ushort IanaCipherSuite
		{
			get;
		}

		/// <summary>
		/// Priority. The higher the number, the higher priority.
		/// </summary>
		public abstract int Priority
		{
			get;
		}

		/// <summary>
		/// Sets the master secret for the session.
		/// </summary>
		/// <param name="Value">Master secret.</param>
		/// <param name="State">Endpoint state.</param>
		public virtual void SetMasterSecret(byte[] Value, EndpointState State)
		{
			State.masterSecret = Value;

			// Key calculation: RFC 5246 §6.3:

			byte[] KeyBlock = this.PRF(State.masterSecret, "key expansion",
				Concat(State.serverRandom, State.clientRandom),
				(uint)((this.macKeyLength + this.encKeyLength + this.fixedIvLength) << 1));

			int Pos = 0;

			State.client_write_MAC_key = new byte[this.macKeyLength];
			Array.Copy(KeyBlock, Pos, State.client_write_MAC_key, 0, this.macKeyLength);
			Pos += this.macKeyLength;

			State.server_write_MAC_key = new byte[this.macKeyLength];
			Array.Copy(KeyBlock, Pos, State.server_write_MAC_key, 0, this.macKeyLength);
			Pos += this.macKeyLength;

			State.client_write_key = new byte[this.encKeyLength];
			Array.Copy(KeyBlock, Pos, State.client_write_key, 0, this.encKeyLength);
			Pos += this.encKeyLength;

			State.server_write_key = new byte[this.encKeyLength];
			Array.Copy(KeyBlock, Pos, State.server_write_key, 0, this.encKeyLength);
			Pos += this.encKeyLength;

			State.client_write_IV = new byte[this.fixedIvLength];
			Array.Copy(KeyBlock, Pos, State.client_write_IV, 0, this.fixedIvLength);
			Pos += this.fixedIvLength;

			State.server_write_IV = new byte[this.fixedIvLength];
			Array.Copy(KeyBlock, Pos, State.server_write_IV, 0, this.fixedIvLength);
			Pos += this.fixedIvLength;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// If the cipher can be used by the endpoint.
		/// </summary>
		/// <param name="State">Endpoint state.</param>
		/// <returns>If the cipher can be used.</returns>
		public abstract bool CanBeUsed(EndpointState State);

		/// <summary>
		/// Sends the Client Key Exchange message flight.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		public abstract void SendClientKeyExchange(DtlsEndpoint Endpoint, EndpointState State);

		/// <summary>
		/// Sends the Server Key Exchange message flight.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		public abstract void SendServerKeyExchange(DtlsEndpoint Endpoint, EndpointState State);

		/// <summary>
		/// Pseudo-random function for the cipher, as defined in §5 of RFC 5246:
		/// https://tools.ietf.org/html/rfc5246#section-5
		/// </summary>
		/// <param name="Secret">Secret</param>
		/// <param name="Label">Label</param>
		/// <param name="Seed">Seed</param>
		/// <param name="NrBytes">Number of bytes to generate.</param>
		public virtual byte[] PRF(byte[] Secret, string Label, byte[] Seed, uint NrBytes)
		{
			// RFC 5246, §5, HMAC and the Pseudorandom Function:

			byte[] Result = new byte[NrBytes];
			byte[] A, P;
			int Pos = 0;
			uint BytesLeft = NrBytes;

			Seed = Concat(Encoding.UTF8.GetBytes(Label), Seed);
			A = Seed;

			using (HMACSHA256 Hmac = new HMACSHA256(Secret))
			{
				while (BytesLeft > 0)
				{
					A = Hmac.ComputeHash(A);
					P = Hmac.ComputeHash(Concat(A, Seed));

					if (BytesLeft < P.Length)
					{
						Array.Copy(P, 0, Result, Pos, (int)BytesLeft);
						Pos += (int)BytesLeft;
						BytesLeft = 0;
					}
					else
					{
						Array.Copy(P, 0, Result, Pos, P.Length);
						Pos += P.Length;
						BytesLeft -= (uint)P.Length;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Concatenates two binary arrays.
		/// </summary>
		/// <param name="A1">Array 1</param>
		/// <param name="A2">Array 2</param>
		/// <returns>A1+A2</returns>
		public static byte[] Concat(byte[] A1, byte[] A2)
		{
			int c1, c2;

			if (A1 == null || (c1 = A1.Length) == 0)
				return A2;
			else if (A2 == null || (c2 = A2.Length) == 0)
				return A1;

			byte[] Result = new byte[c1 + c2];

			Array.Copy(A1, 0, Result, 0, c1);
			Array.Copy(A2, 0, Result, c1, c2);

			return Result;
		}

		/// <summary>
		/// Finishes the handshake.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		public virtual void SendFinished(DtlsEndpoint Endpoint, EndpointState State)
		{
			if (State.masterSecret == null)
				Endpoint.SendAlert(AlertLevel.fatal, AlertDescription.handshake_failure, State);
			else
			{
				string Label;
				byte[] HandshakeHash;
				byte[] VerifyData;

				if (State.isClient)
				{
					State.clientFinished = true;
					Label = "client finished";
					State.CalcClientHandshakeHash();
					HandshakeHash = State.clientHandshakeHash;
				}
				else
				{
					State.serverFinished = true;
					Label = "server finished";
					State.CalcServerHandshakeHash();
					HandshakeHash = State.serverHandshakeHash;
				}

				VerifyData = this.PRF(State.masterSecret, Label, HandshakeHash, 12);

				Endpoint.SendHandshake(HandshakeType.finished, VerifyData, false, State);

				if (State.clientFinished && State.serverFinished)
					Endpoint.HandshakeSuccess(State);
			}
		}

		/// <summary>
		/// Verifies the claims in a finished message.
		/// </summary>
		/// <param name="VerifyData">Verify data in finished message.</param>
		/// <param name="State">Endpoint state.</param>
		/// <returns>If the <paramref name="VerifyData"/> is valid or not.</returns>
		public virtual bool VerifyFinished(byte[] VerifyData, EndpointState State)
		{
			if (State.masterSecret == null)
				return false;

			string Label;
			byte[] HandshakeHash;
			byte[] VerifyData0;

			if (State.isClient)
			{
				Label = "server finished";
				State.CalcServerHandshakeHash();
				HandshakeHash = State.serverHandshakeHash;
			}
			else
			{
				Label = "client finished";
				State.CalcClientHandshakeHash();
				HandshakeHash = State.clientHandshakeHash;
			}

			VerifyData0 = this.PRF(State.masterSecret, Label, HandshakeHash, 12);

			return DtlsEndpoint.AreEqual(VerifyData0, VerifyData);
		}

		/// <summary>
		/// Creates a new Nonce value.
		/// </summary>
		/// <param name="ClientSender">If the client is the sender (true), or the server (false).</param>
		/// <param name="State">Endpoint state.</param>
		/// <returns>New nonce value.</returns>
		protected byte[] CreateNonce(bool ClientSender, EndpointState State)
		{
			// Nonce calculation defind in RFC 5288, §3.

			byte[] Nonce = new byte[12];

			if (ClientSender)
				Array.Copy(State.client_write_IV, 0, Nonce, 0, this.fixedIvLength);
			else
				Array.Copy(State.server_write_IV, 0, Nonce, 0, this.fixedIvLength);

			ulong c = State.nonceCount++;
			int Pos = 12;
			int i;

			for (i = 0; i < 8; i++)
			{
				Nonce[--Pos] = (byte)c;
				c >>= 8;
			}

			return Nonce;
		}

		/// <summary>
		/// Allows the cipher to process any server key information sent by the DTLS server.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Offset">Offset where data begins.</param>
		/// <param name="State">Endpoint state.</param>
		public abstract void ServerKeyExchange(byte[] Data, ref int Offset, EndpointState State);

		/// <summary>
		/// Allows the cipher to process any client key information sent by the DTLS client.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Offset">Offset where data begins.</param>
		/// <param name="State">Endpoint state.</param>
		public abstract void ClientKeyExchange(byte[] Data, ref int Offset, EndpointState State);

		/// <summary>
		/// Encrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <param name="Start">Start offset of header.</param>
		/// <param name="State">Endpoint state.</param>
		/// <returns>Encrypted data.</returns>
		public abstract byte[] Encrypt(byte[] Data, byte[] Header, int Start, EndpointState State);

		/// <summary>
		/// Decrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <param name="Start">Start offset of header.</param>
		/// <param name="State">Endpoint state.</param>
		/// <returns>Decrypted data, or null if authentication failed.</returns>
		public abstract byte[] Decrypt(byte[] Data, byte[] Header, int Start, EndpointState State);
	}
}
