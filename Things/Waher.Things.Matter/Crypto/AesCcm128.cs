using System;
using System.Security.Cryptography;

namespace Waher.Things.Matter.Crypto
{
	/// <summary>
	/// AES-CCM authenticated encryption with 128-bit keys (RFC 3610 / NIST SP 800-38C), as used by
	/// Matter (Crypto_AEAD_GenerateEncrypt / Crypto_AEAD_DecryptVerify). Implemented on top of AES-ECB
	/// to be independent of platform AES-CCM support.
	/// </summary>
	public static class AesCcm128
	{
		/// <summary>
		/// Encrypts and authenticates a message.
		/// </summary>
		/// <param name="Key">16-byte key.</param>
		/// <param name="Nonce">Nonce (7-13 bytes; Matter uses 13 bytes).</param>
		/// <param name="Plaintext">Plaintext.</param>
		/// <param name="AdditionalData">Additional authenticated data (may be null).</param>
		/// <param name="TagLength">Length of authentication tag (default 16).</param>
		/// <returns>Ciphertext followed by the authentication tag.</returns>
		public static byte[] Encrypt(byte[] Key, byte[] Nonce, byte[] Plaintext, byte[] AdditionalData, int TagLength = 16)
		{
			CheckParameters(Key, Nonce, TagLength);

			Plaintext = Plaintext ?? Array.Empty<byte>();
			AdditionalData = AdditionalData ?? Array.Empty<byte>();

			using (Aes Aes = CreateAes(Key))
			using (ICryptoTransform Enc = Aes.CreateEncryptor())
			{
				byte[] Tag = ComputeMac(Enc, Nonce, Plaintext, AdditionalData, TagLength);
				byte[] Result = new byte[Plaintext.Length + TagLength];

				Ctr(Enc, Nonce, Plaintext, Result, 1);

				byte[] S0 = CounterBlock(Enc, Nonce, 0);
				int i;

				for (i = 0; i < TagLength; i++)
					Result[Plaintext.Length + i] = (byte)(Tag[i] ^ S0[i]);

				return Result;
			}
		}

		/// <summary>
		/// Decrypts and verifies a message.
		/// </summary>
		/// <param name="Key">16-byte key.</param>
		/// <param name="Nonce">Nonce (7-13 bytes; Matter uses 13 bytes).</param>
		/// <param name="CiphertextAndTag">Ciphertext followed by authentication tag.</param>
		/// <param name="AdditionalData">Additional authenticated data (may be null).</param>
		/// <param name="TagLength">Length of authentication tag (default 16).</param>
		/// <returns>Plaintext.</returns>
		/// <exception cref="MatterSecurityException">If authentication fails.</exception>
		public static byte[] Decrypt(byte[] Key, byte[] Nonce, byte[] CiphertextAndTag, byte[] AdditionalData, int TagLength = 16)
		{
			CheckParameters(Key, Nonce, TagLength);

			if (CiphertextAndTag is null || CiphertextAndTag.Length < TagLength)
				throw new MatterSecurityException("Encrypted message too short.");

			AdditionalData = AdditionalData ?? Array.Empty<byte>();

			int c = CiphertextAndTag.Length - TagLength;

			using (Aes Aes = CreateAes(Key))
			using (ICryptoTransform Enc = Aes.CreateEncryptor())
			{
				byte[] Plaintext = new byte[c];
				byte[] Ciphertext = new byte[c];
				Array.Copy(CiphertextAndTag, 0, Ciphertext, 0, c);

				Ctr(Enc, Nonce, Ciphertext, Plaintext, 1);

				byte[] Tag = ComputeMac(Enc, Nonce, Plaintext, AdditionalData, TagLength);
				byte[] S0 = CounterBlock(Enc, Nonce, 0);
				int Diff = 0;
				int i;

				for (i = 0; i < TagLength; i++)
					Diff |= (byte)(Tag[i] ^ S0[i]) ^ CiphertextAndTag[c + i];

				if (Diff != 0)
					throw new MatterSecurityException("Message authentication failed.");

				return Plaintext;
			}
		}

		private static void CheckParameters(byte[] Key, byte[] Nonce, int TagLength)
		{
			if (Key is null || Key.Length != 16)
				throw new ArgumentException("Key must be 16 bytes.", nameof(Key));

			if (Nonce is null || Nonce.Length < 7 || Nonce.Length > 13)
				throw new ArgumentException("Nonce must be 7-13 bytes.", nameof(Nonce));

			if (TagLength < 4 || TagLength > 16 || (TagLength & 1) != 0)
				throw new ArgumentException("Invalid tag length.", nameof(TagLength));
		}

		private static Aes CreateAes(byte[] Key)
		{
			Aes Aes = Aes.Create();
			Aes.Mode = CipherMode.ECB;
			Aes.Padding = PaddingMode.None;
			Aes.Key = Key;
			return Aes;
		}

		private static byte[] ComputeMac(ICryptoTransform Enc, byte[] Nonce, byte[] Plaintext, byte[] AdditionalData, int TagLength)
		{
			int L = 15 - Nonce.Length;
			byte[] X = new byte[16];
			byte[] Block = new byte[16];
			int i, j, c;

			// B0

			Block[0] = (byte)((AdditionalData.Length > 0 ? 0x40 : 0) | (((TagLength - 2) / 2) << 3) | (L - 1));
			Array.Copy(Nonce, 0, Block, 1, Nonce.Length);

			long Len = Plaintext.Length;
			for (i = 15; i > Nonce.Length; i--)
			{
				Block[i] = (byte)Len;
				Len >>= 8;
			}

			Enc.TransformBlock(Block, 0, 16, X, 0);

			// Additional data

			if (AdditionalData.Length > 0)
			{
				byte[] Encoded;
				int Pos;
				c = AdditionalData.Length;

				if (c < 0xff00)
				{
					Encoded = new byte[2 + c];
					Encoded[0] = (byte)(c >> 8);
					Encoded[1] = (byte)c;
					Pos = 2;
				}
				else
				{
					Encoded = new byte[6 + c];
					Encoded[0] = 0xff;
					Encoded[1] = 0xfe;
					Encoded[2] = (byte)(c >> 24);
					Encoded[3] = (byte)(c >> 16);
					Encoded[4] = (byte)(c >> 8);
					Encoded[5] = (byte)c;
					Pos = 6;
				}

				Array.Copy(AdditionalData, 0, Encoded, Pos, c);
				MacBlocks(Enc, X, Encoded);
			}

			// Message

			if (Plaintext.Length > 0)
				MacBlocks(Enc, X, Plaintext);

			byte[] Tag = new byte[TagLength];
			Array.Copy(X, 0, Tag, 0, TagLength);

			for (j = 0; j < 16; j++)
				Block[j] = 0;

			return Tag;
		}

		private static void MacBlocks(ICryptoTransform Enc, byte[] X, byte[] Data)
		{
			byte[] Block = new byte[16];
			int Pos = 0;
			int c = Data.Length;
			int i;

			while (Pos < c)
			{
				for (i = 0; i < 16; i++)
				{
					byte b = Pos + i < c ? Data[Pos + i] : (byte)0;
					Block[i] = (byte)(X[i] ^ b);
				}

				Enc.TransformBlock(Block, 0, 16, X, 0);
				Pos += 16;
			}
		}

		private static byte[] CounterBlock(ICryptoTransform Enc, byte[] Nonce, long Counter)
		{
			int L = 15 - Nonce.Length;
			byte[] A = new byte[16];
			byte[] S = new byte[16];
			int i;

			A[0] = (byte)(L - 1);
			Array.Copy(Nonce, 0, A, 1, Nonce.Length);

			for (i = 15; i > Nonce.Length; i--)
			{
				A[i] = (byte)Counter;
				Counter >>= 8;
			}

			Enc.TransformBlock(A, 0, 16, S, 0);

			return S;
		}

		private static void Ctr(ICryptoTransform Enc, byte[] Nonce, byte[] Input, byte[] Output, long FirstCounter)
		{
			int Pos = 0;
			int c = Input.Length;
			long Counter = FirstCounter;
			int i;

			while (Pos < c)
			{
				byte[] S = CounterBlock(Enc, Nonce, Counter++);

				for (i = 0; i < 16 && Pos + i < c; i++)
					Output[Pos + i] = (byte)(Input[Pos + i] ^ S[i]);

				Pos += 16;
			}
		}
	}
}
