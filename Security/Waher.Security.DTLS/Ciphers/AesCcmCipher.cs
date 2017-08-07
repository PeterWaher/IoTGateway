using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Waher.Security.DTLS.Ciphers
{
	/// <summary>
	/// AES CCM cipher:
	/// http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38c.pdf
	/// </summary>
	public abstract class AesCcmCipher : PskCipher
	{
		private Aes aes;
		private int t;

		/// <summary>
		/// AES CCM cipher:
		/// http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38c.pdf
		/// </summary>
		/// <param name="MacKeyLength">MAC key length.</param>
		/// <param name="EncKeyLength">Encryption key size.</param>
		/// <param name="FixedIvLength">Fixed IV length.</param>
		/// <param name="KeySize">AES key size.</param>
		/// <param name="TagLength">Tag length.</param>
		public AesCcmCipher(int MacKeyLength, int EncKeyLength, int FixedIvLength, int KeySize, int TagLength)
			: base(MacKeyLength, EncKeyLength, FixedIvLength)
		{
			this.aes = Aes.Create();
			this.t = TagLength;

			aes.BlockSize = 128;
			aes.KeySize = KeySize;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.None;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (this.aes != null)
			{
				this.aes.Dispose();
				this.aes = null;
			}
		}

		/// <summary>
		/// Encrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <returns>Encrypted data.</returns>
		public override byte[] Encrypt(byte[] Data, byte[] Header)
		{
			// CCM authenticated encryption algoritm, described in: 
			// http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38c.pdf

			bool IsClient = this.IsClient;
			byte[] Nonce = this.GetNonce(IsClient);
			byte[] Key;
			byte[] IV;
			int i;

			// §6.2.3.3, RFC 5246:
			// Key: client_write_key or server_write_key

			if (IsClient)
			{
				Key = this.ClientWriteKey;
				IV = this.ClientWriteIV;
			}
			else
			{
				Key = this.ServerWriteKey;
				IV = this.ServerWriteIV;
			}

			// §A.2.1. Formatting of the Control Information and the Nonce:

			byte[] B0 = new byte[16];
			B0[0] = (byte)(0b01000010 + (((this.t - 2) / 2) << 3)); // Adata=1, q-1=2 <=> q=3, n=12.
			Array.Copy(Nonce, 0, B0, 1, 12);

			int Q = Data.Length;
			for (i = 2; i >= 0; i--)
			{
				B0[13 + i] = (byte)Q;
				Q >>= 8;
			}

			// §A.2.2. Formatting of the Associated Data:
			// A = Additional data: seq_num + TLSCompressed.type + TLSCompressed.version + TLSCompressed.length;
			// where "+" denotes concatenation.

			byte[] B1 = new byte[16];   // B1 = a + A

			B1[0] = 0;
			B1[1] = 13;
			Array.Copy(Header, 3, B1, 2, 8);    // seq_num
			Array.Copy(Header, 0, B1, 10, 3);   // Type, version
			Array.Copy(Header, 11, B1, 13, 2);  // length
			B1[15] = 0;

			using (ICryptoTransform Aes = this.aes.CreateEncryptor(Key, IV))
			{
				// §6.1. Generation-Encryption Process:

				byte[] Y0 = Aes.TransformFinalBlock(B0, 0, 16);             // IV?
				byte[] Y1 = Aes.TransformFinalBlock(XOR(B1, Y0), 0, 16);    // IV?

				byte[] Tag = new byte[t];
				Array.Copy(Y1, 0, Tag, 0, this.t);

				// §A.3. Formatting of the Counter Blocks:

				byte[] Ctr = new byte[16];

				Ctr[0] = (byte)(B0[0] & 7);
				Array.Copy(Nonce, 0, Ctr, 1, 12);
				Ctr[13] = 0;
				Ctr[14] = 0;
				Ctr[15] = 0;

				Q = Data.Length;
				int m = (Q + 127) >> 7;

				byte[] S = Aes.TransformFinalBlock(Ctr, 0, 16);   // IV?
				byte[] Result = new byte[Q + this.t];
				int j;

				Array.Copy(Data, 0, Result, 0, Q);
				Array.Copy(XOR(Tag, S), 0, Result, Q, t);

				for (i = 1; i <= m; i++)
				{
					j = i;
					Ctr[15] = (byte)j;
					j >>= 8;
					Ctr[14] = (byte)j;
					j >>= 8;
					Ctr[13] = (byte)j;

					S = Aes.TransformFinalBlock(Ctr, 0, 16);   // IV?

					j = i << 4;
					XOR(Result, j, Math.Min(16, Q - j), S);
				}

				return Result;
			}
		}

		/// <summary>
		/// Decrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <returns>Decrypted data, or null if authentication failed.</returns>
		public override byte[] Decrypt(byte[] Data, byte[] Header)
		{
			throw new NotImplementedException();
		}
	}
}
