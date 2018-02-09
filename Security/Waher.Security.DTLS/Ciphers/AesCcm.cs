using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Waher.Security.DTLS.Ciphers
{
	/// <summary>
	/// AES CCM encryptor/decryptor:
	/// http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38c.pdf
	/// </summary>
	public class AesCcm : IDisposable
	{
		private byte[] iv = new byte[16];   // zeroes.
		private Aes aes;
		private int t;
		private int n;
		private int q;

		/// <summary>
		/// AES CCM encryptor/decryptor:
		/// http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38c.pdf
		/// </summary>
		/// <param name="Klen">AES key length, in bits.</param>
		/// <param name="Tlen">Tag length, in bytes.</param>
		/// <param name="Nlen">Nonce length, in bytes.</param>
		public AesCcm(int Klen, int Tlen, int Nlen)
		{
			if (Tlen < 4 || Tlen > 16 || (Tlen & 1) == 1)
				throw new ArgumentException("Valid Tlen values are 4, 6, 8, 10, 12, 14 and 16.", nameof(Tlen));

			if (Nlen < 7 || Nlen > 13)
				throw new ArgumentOutOfRangeException("Valid Nlen values are 7-13.", nameof(Nlen));

			this.aes = Aes.Create();
			this.t = Tlen;
			this.n = Nlen;
			this.q = 15 - Nlen;

			aes.BlockSize = 128;
			aes.KeySize = Klen;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.None;
		}

		/// <summary>
		/// Performs the XOR operation on a Data arary with a cyclic Mask array.
		/// </summary>
		/// <param name="Data">Data array.</param>
		/// <param name="Mask">Cyclic mask array.</param>
		/// <returns>Masked array, of the same size as <paramref name="Data"/>.</returns>
		public static byte[] XOR(byte[] Data, byte[] Mask)
		{
			int i, c = Data.Length;
			int d = Mask.Length;
			byte[] Result = new byte[c];

			for (i = 0; i < c; i++)
				Result[i] = (byte)(Data[i] ^ Mask[i % d]);

			return Result;
		}

		/// <summary>
		/// Performs the XOR operation on a Data arary with a cyclic Mask array.
		/// </summary>
		/// <param name="Data">Data array.</param>
		/// <param name="Offset">Offset into the data array to begin.</param>
		/// <param name="Count">Number of bytes to mask.</param>
		/// <param name="Mask">Cyclic mask array.</param>
		/// <param name="MaskOffset">Offset into mask.</param>
		/// <param name="MaskCount">Number of bytes in mask.</param>
		/// <returns>Masked array, of the same size as <paramref name="Data"/>.</returns>
		public static void XOR(byte[] Data, int Offset, int Count, byte[] Mask, int MaskOffset, int MaskCount)
		{
			int i;

			i = 0;
			while (Count > 0)
			{
				Data[Offset++] ^= Mask[(i % MaskCount) + MaskOffset];
				i++;
				Count--;
			}
		}

		private static void FillBlock16(ref int Pos)
		{
			int i = Pos & 15;
			if (i > 0)
				Pos += 16 - i;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.aes != null)
			{
				this.aes.Dispose();
				this.aes = null;
			}
		}

		/// <summary>
		/// Encrypts and signs data using the AES CCM algorithm.
		/// </summary>
		/// <param name="Plaintext">Content to encrypt.</param>
		/// <param name="AssociatedData">Associated data to sign.</param>
		/// <param name="Nonce">Unique nonce value.</param>
		/// <param name="Key">Secret key.</param>
		/// <returns>Signed ciphertext.</returns>
		public byte[] Encrypt(byte[] Plaintext, byte[] AssociatedData, byte[] Nonce, byte[] Key)
		{
			if (Nonce.Length != this.n)
				throw new ArgumentException("Nonce length mismatch.", nameof(Nonce));

			using (ICryptoTransform Aes = this.aes.CreateEncryptor(Key, this.iv))
			{
				// §A.2. Formatting of the Input Data 

				int a = AssociatedData == null ? 0 : AssociatedData.Length;
				byte[] B;
				int i, j, k;
				int Q = Plaintext.Length;

				// §A.2.2. Formatting of the Associated Data:

				if (a == 0)
				{
					j = 16;
					i = 16 + Q;
					FillBlock16(ref i);

					B = new byte[i];
				}
				else if (a < 65536 - 256)
				{
					j = 16 + 2 + a;
					FillBlock16(ref j);
					i = j + Q;
					FillBlock16(ref i);

					B = new byte[i];
					B[16] = (byte)(a >> 8);
					B[17] = (byte)a;
					Array.Copy(AssociatedData, 0, B, 18, a);
				}
				else
				{
					j = 16 + 6 + a;
					FillBlock16(ref j);
					i = j + Q;
					FillBlock16(ref i);

					B = new byte[i];
					i = a;
					B[21] = (byte)i;
					i >>= 8;
					B[20] = (byte)i;
					i >>= 8;
					B[19] = (byte)i;
					i >>= 8;
					B[18] = (byte)i;
					B[17] = 0xfe;
					B[16] = 0xff;
					Array.Copy(AssociatedData, 0, B, 22, a);
				}

				// §A.2.3. Formatting of the Payload:

				Array.Copy(Plaintext, 0, B, j, Q);

				// §A.2.1. Formatting of the Control Information and the Nonce:

				B[0] = (byte)((a > 0 ? 0x40 : 0) + (((this.t - 2) / 2) << 3) + (this.q - 1));
				Array.Copy(Nonce, 0, B, 1, this.n);

				i = 15;
				j = Q;
				while (i > this.n)
				{
					B[i] = (byte)j;
					j >>= 8;
					i--;
				}

				if (j != 0)
					throw new ArgumentException("Plaintext too large.", nameof(Plaintext));

				// §6.1. Generation-Encryption Process:

				byte[] Y = new byte[j = B.Length];
				byte[] Temp;

				Array.Copy(B, 0, Y, 0, j);

				for (i = 0; i < j; i += 16)
				{
					if (i > 0)
						XOR(Y, i, 16, Y, i - 16, 16);

					Temp = Aes.TransformFinalBlock(Y, i, 16);
					Array.Copy(Temp, 0, Y, i, 16);
				}

				byte[] Tag = new byte[this.t];
				Array.Copy(Y, j - 16, Tag, 0, this.t);

				// §A.3. Formatting of the Counter Blocks:

				byte[] Ctr = new byte[16];

				Ctr[0] = (byte)(B[0] & 7);
				Array.Copy(Nonce, 0, Ctr, 1, this.n);

				int m = (Q + 15) >> 4;

				byte[] S = Aes.TransformFinalBlock(Ctr, 0, 16);
				byte[] Result = new byte[Q + this.t];

				Array.Copy(Plaintext, 0, Result, 0, Q);
				Array.Copy(XOR(Tag, S), 0, Result, Q, t);

				for (i = 1; i <= m; i++)
				{
					for (j = i, k = 0; k < this.q; k++)
					{
						Ctr[15 - k] = (byte)j;
						j >>= 8;
					}

					S = Aes.TransformFinalBlock(Ctr, 0, 16);

					j = (i - 1) << 4;
					XOR(Result, j, Math.Min(16, Q - j), S, 0, 16);
				}

				return Result;
			}
		}

		/// <summary>
		/// Authenticates and decrypts data using the AES CCM algorithm.
		/// </summary>
		/// <param name="Ciphertext">Signed ciphertext.</param>
		/// <param name="AssociatedData">Associated data to sign.</param>
		/// <param name="Nonce">Unique nonce value.</param>
		/// <param name="Key">Secret key.</param>
		/// <returns>Verified and decrypted data, or null if <paramref name="Ciphertext"/> invalid.</returns>
		public byte[] AuthenticateAndDecrypt(byte[] Ciphertext, byte[] AssociatedData, byte[] Nonce, byte[] Key)
		{
			if (Nonce.Length != this.n)
				throw new ArgumentException("Nonce length mismatch.", nameof(Nonce));

			// §6.2 Decryption-Verification Process:

			int Clen = Ciphertext.Length;
			if (Clen <= this.t)
				return null;    // INVALID

			using (ICryptoTransform Aes = this.aes.CreateEncryptor(Key, this.iv))
			{
				int Q = Clen - this.t;
				int m = (Q + 15) >> 4;
				byte[] Ctr = new byte[16];

				Ctr[0] = (byte)(this.q - 1);
				Array.Copy(Nonce, 0, Ctr, 1, this.n);

				byte[] S0 = Aes.TransformFinalBlock(Ctr, 0, 16);
				int i, j, k;

				byte[] Result = new byte[Q];
				byte[] S;

				Array.Copy(Ciphertext, 0, Result, 0, Q);

				for (i = 1; i <= m; i++)
				{
					for (j = i, k = 0; k < this.q; k++)
					{
						Ctr[15 - k] = (byte)j;
						j >>= 8;
					}

					S = Aes.TransformFinalBlock(Ctr, 0, 16);

					j = (i - 1) << 4;
					XOR(Result, j, Math.Min(16, Q - j), S, 0, 16);
				}

				int a = AssociatedData == null ? 0 : AssociatedData.Length;
				byte[] B;

				// §A.2.2. Formatting of the Associated Data:

				if (a == 0)
				{
					j = 16;
					i = 16 + Q;
					FillBlock16(ref i);

					B = new byte[i];
				}
				else if (a < 65536 - 256)
				{
					j = 16 + 2 + a;
					FillBlock16(ref j);
					i = j + Q;
					FillBlock16(ref i);

					B = new byte[i];
					B[16] = (byte)(a >> 8);
					B[17] = (byte)a;
					Array.Copy(AssociatedData, 0, B, 18, a);
				}
				else
				{
					j = 16 + 6 + a;
					FillBlock16(ref j);
					i = j + Q;
					FillBlock16(ref i);

					B = new byte[i];
					i = a;
					B[21] = (byte)i;
					i >>= 8;
					B[20] = (byte)i;
					i >>= 8;
					B[19] = (byte)i;
					i >>= 8;
					B[18] = (byte)i;
					B[17] = 0xfe;
					B[16] = 0xff;
					Array.Copy(AssociatedData, 0, B, 22, a);
				}

				// §A.2.3. Formatting of the Payload:

				Array.Copy(Result, 0, B, j, Q);

				// §A.2.1. Formatting of the Control Information and the Nonce:

				B[0] = (byte)((a > 0 ? 0x40 : 0) + (((this.t - 2) / 2) << 3) + (this.q - 1));
				Array.Copy(Nonce, 0, B, 1, this.n);

				i = 15;
				j = Q;
				while (i > this.n)
				{
					B[i] = (byte)j;
					j >>= 8;
					i--;
				}

				if (j != 0)
					return null;

				// §6.1. Generation-Encryption Process:

				byte[] Y = new byte[j = B.Length];
				byte[] Temp;

				Array.Copy(B, 0, Y, 0, j);

				for (i = 0; i < j; i += 16)
				{
					if (i > 0)
						XOR(Y, i, 16, Y, i - 16, 16);

					Temp = Aes.TransformFinalBlock(Y, i, 16);
					Array.Copy(Temp, 0, Y, i, 16);
				}

				byte[] Tag = new byte[this.t];
				Array.Copy(Y, j - 16, Tag, 0, this.t);

				XOR(Tag, 0, this.t, S0, 0, this.t);

				for (i = 0; i < this.t; i++)
				{
					if (Tag[i] != Ciphertext[Clen - this.t + i])
						return null;	// Invalid tag.
				}

				return Result;
			}
		}

	}
}
