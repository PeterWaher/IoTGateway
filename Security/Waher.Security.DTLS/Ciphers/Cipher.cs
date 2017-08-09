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
		private SHA256 hashFunction;
		private byte[] masterSecret;
		private byte[] client_write_MAC_key;
		private byte[] server_write_MAC_key;
		private byte[] client_write_key;
		private byte[] server_write_key;
		private byte[] client_write_IV;
		private byte[] server_write_IV;
		private byte[] clientRandom;
		private byte[] serverRandom;
		private ulong nonceCount = 0;
		private bool isClient = false;

		private int macKeyLength;
		private int encKeyLength;
		private int fixedIvLength;

		/// <summary>
		/// Base class for all ciphers used in (D)TLS.
		/// </summary>
		/// <param name="MacKeyLength">MAC key length.</param>
		/// <param name="EncKeyLength">Encryption key size.</param>
		/// <param name="FixedIvLength">Fixed IV length.</param>
		public Cipher(int MacKeyLength, int EncKeyLength, int FixedIvLength)
		{
			this.hashFunction = SHA256.Create();
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
		/// If the endpoint where the cipher is being used, is a client endpoint (true),
		/// or a server endpoint (false).
		/// </summary>
		public bool IsClient
		{
			get { return this.isClient; }
			set { this.isClient = value; }
		}

		/// <summary>
		/// Master secret.
		/// </summary>
		public byte[] MasterSecret
		{
			get { return this.masterSecret; }
			set
			{
				this.masterSecret = value;

				// Key calculation: RFC 5246 §6.3:

				byte[] KeyBlock = this.PRF(this.masterSecret, "key expansion",
					Concat(this.serverRandom, this.clientRandom),
					(uint)((this.macKeyLength + this.encKeyLength + this.fixedIvLength) << 1));

				int Pos = 0;

				this.client_write_MAC_key = new byte[this.macKeyLength];
				Array.Copy(KeyBlock, Pos, this.client_write_MAC_key, 0, this.macKeyLength);
				Pos += this.macKeyLength;

				this.server_write_MAC_key = new byte[this.macKeyLength];
				Array.Copy(KeyBlock, Pos, this.server_write_MAC_key, 0, this.macKeyLength);
				Pos += this.macKeyLength;

				this.client_write_key = new byte[this.encKeyLength];
				Array.Copy(KeyBlock, Pos, this.client_write_key, 0, this.encKeyLength);
				Pos += this.encKeyLength;

				this.server_write_key = new byte[this.encKeyLength];
				Array.Copy(KeyBlock, Pos, this.server_write_key, 0, this.encKeyLength);
				Pos += this.encKeyLength;

				this.client_write_IV = new byte[this.fixedIvLength];
				Array.Copy(KeyBlock, Pos, this.client_write_IV, 0, this.fixedIvLength);
				Pos += this.fixedIvLength;

				this.server_write_IV = new byte[this.fixedIvLength];
				Array.Copy(KeyBlock, Pos, this.server_write_IV, 0, this.fixedIvLength);
				Pos += this.fixedIvLength;
			}
		}

		/// <summary>
		/// Client random
		/// </summary>
		public byte[] ClientRandom
		{
			get { return this.clientRandom; }
			set { this.clientRandom = value; }
		}

		/// <summary>
		/// Server random
		/// </summary>
		public byte[] ServerRandom
		{
			get { return this.serverRandom; }
			set { this.serverRandom = value; }
		}

		/// <summary>
		/// Client write key.
		/// </summary>
		public byte[] ClientWriteKey
		{
			get { return this.client_write_key; }
		}

		/// <summary>
		/// Server write key.
		/// </summary>
		public byte[] ServerWriteKey
		{
			get { return this.server_write_key; }
		}

		/// <summary>
		/// Client write IV.
		/// </summary>
		public byte[] ClientWriteIV
		{
			get { return this.client_write_IV; }
		}

		/// <summary>
		/// Server write IV.
		/// </summary>
		public byte[] ServerWriteIV
		{
			get { return this.server_write_IV; }
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			if (this.hashFunction != null)
			{
				this.hashFunction.Dispose();
				this.hashFunction = null;
			}
		}

		/// <summary>
		/// If the cipher can be used by the endpoint.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <returns>If the cipher can be used.</returns>
		public abstract bool CanBeUsed(DtlsEndpoint Endpoint);

		/// <summary>
		/// Sends the Client Key Exchange message.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		public abstract void SendClientKeyExchange(DtlsEndpoint Endpoint);

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
		/// <param name="Client">If the client acts as a client (true), or a server (false).</param>
		/// <param name="Handshake">Entire handshake communication.</param>
		public virtual void SendFinished(DtlsEndpoint Endpoint, bool Client, byte[] Handshake)
		{
			byte[] Hash = this.hashFunction.ComputeHash(Handshake);
			string Label = Client ? "client finished" : "server finished";
			byte[] VerifyData = this.PRF(this.masterSecret, Label, Hash, 12);

			Endpoint.SendHandshake(HandshakeType.finished, VerifyData, false);
		}

		/// <summary>
		/// Gets a new Nonce value.
		/// </summary>
		/// <param name="ClientSender">If the client is the sender (true), or the server (false).</param>
		/// <returns>New nonce value.</returns>
		protected byte[] GetNonce(bool ClientSender)
		{
			// Nonce calculation defind in RFC 5288, §3.

			byte[] Nonce = new byte[12];

			if (ClientSender)
				Array.Copy(this.client_write_IV, 0, Nonce, 0, this.fixedIvLength);
			else
				Array.Copy(this.server_write_IV, 0, Nonce, 0, this.fixedIvLength);

			ulong c = this.nonceCount++;
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
		public abstract void ServerKeyExchange(byte[] Data, ref int Offset);

		/// <summary>
		/// Encrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <returns>Encrypted data.</returns>
		public abstract byte[] Encrypt(byte[] Data, byte[] Header);

		/// <summary>
		/// Decrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <returns>Decrypted data, or null if authentication failed.</returns>
		public abstract byte[] Decrypt(byte[] Data, byte[] Header);
	}
}
