using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Abstract base class for AES-256 based End-to-End encryption schemes.
	/// </summary>
	public abstract class Aes256 : E2eEndpoint
	{
		private readonly static RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		
		/// <summary>
		/// AES encryption object
		/// </summary>
		protected Aes aes;

		/// <summary>
		/// Abstract base class for AES-256 based End-to-End encryption schemes.
		/// </summary>
		public Aes256()
		{
			this.aes = Aes.Create();
			this.aes.BlockSize = 128;
			this.aes.KeySize = 256;
			this.aes.Mode = CipherMode.CBC;
			this.aes.Padding = PaddingMode.None;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			this.aes?.Dispose();
			this.aes = null;
		}

		/// <summary>
		/// Encrypts binary data
		/// </summary>
		/// <param name="Data">Binary Data</param>
		/// <param name="Key">Encryption Key</param>
		/// <param name="IV">Initiation Vector</param>
		/// <returns>Encrypted Data</returns>
		public byte[] Encrypt(byte[] Data, byte[] Key, byte[] IV)
		{
			int c = Data.Length;
			int d = 0;
			int i = c;

			do
			{
				i >>= 7;
				d++;
			}
			while (i != 0);

			int ContentLen = c + d;
			int BlockLen = (ContentLen + 15) & ~0xf;
			byte[] Encrypted = new byte[BlockLen];
			int j = 0;

			i = c;

			do
			{
				Encrypted[j] = (byte)(i & 127);
				i >>= 7;
				if (i != 0)
					Encrypted[j] |= 0x80;

				j++;
			}
			while (i != 0);

			Array.Copy(Data, 0, Encrypted, j, c);

			if (ContentLen < BlockLen)
			{
				c = BlockLen - ContentLen;
				byte[] Bin = new byte[c];

				lock (rnd)
				{
					rnd.GetBytes(Bin);
				}

				Array.Copy(Bin, 0, Encrypted, ContentLen, c);
			}

			lock (this.aes)
			{
				using (ICryptoTransform Aes = this.aes.CreateEncryptor(Key, IV))
				{
					Encrypted = Aes.TransformFinalBlock(Encrypted, 0, BlockLen);
				}
			}

			return Encrypted;
		}

		/// <summary>
		/// Decrypts binary data
		/// </summary>
		/// <param name="Data">Binary Data</param>
		/// <param name="Key">Encryption Key</param>
		/// <param name="IV">Initiation Vector</param>
		/// <returns>Decrypted Data</returns>
		public byte[] Decrypt(byte[] Data, byte[] Key, byte[] IV)
		{
			lock (this.aes)
			{
				using (ICryptoTransform Aes = this.aes.CreateDecryptor(Key, IV))
				{
					Data = Aes.TransformFinalBlock(Data, 0, Data.Length);
				}
			}

			int c = 0;
			int i = 0;
			int Offset = 0;
			byte b;

			do
			{
				b = Data[i++];

				c |= (b & 127) << Offset;
				Offset += 7;
			}
			while (b >= 0x80);

			if (c + i > Data.Length)
				return null;

			byte[] Decrypted = new byte[c];

			Array.Copy(Data, i, Decrypted, 0, c);

			return Decrypted;
		}

	}
}
