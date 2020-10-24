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
		private readonly int t;
		private AesCcm aesCcm;

		/// <summary>
		/// AES CCM cipher:
		/// http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38c.pdf
		/// </summary>
		/// <param name="EncKeyLength">Encryption key size.</param>
		/// <param name="FixedIvLength">Fixed IV length.</param>
		/// <param name="TagLength">Tag length.</param>
		public AesCcmCipher(int EncKeyLength, int FixedIvLength, int TagLength)
			: base(0, EncKeyLength, FixedIvLength)
		{
			this.t = TagLength;
			this.aesCcm = new AesCcm(EncKeyLength * 8, TagLength, 12);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.aesCcm is null))
			{
				this.aesCcm.Dispose();
				this.aesCcm = null;
			}
		}

		/// <summary>
		/// Encrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <param name="Start">Start offset of header.</param>
		/// <param name="State">Endpoint state.</param>
		/// <returns>Encrypted data.</returns>
		public override byte[] Encrypt(byte[] Data, byte[] Header, int Start, EndpointState State)
		{
			lock (this.aesCcm)
			{
				byte[] AeadCipher;
				byte[] CipherText;
				byte[] Nonce;
				byte[] AD = this.GetAssociatedData(Header, Start);

				if (State.isClient)
				{
					Nonce = this.CreateNonce(true, State);
					CipherText = this.aesCcm.Encrypt(Data, AD, Nonce, State.client_write_key);
				}
				else
				{
					Nonce = this.CreateNonce(false, State);
					CipherText = this.aesCcm.Encrypt(Data, AD, Nonce, State.server_write_key);
				}

				// On AEAD ciphers, see RFC 5246, §6.2.3.3

				int RecordIvLength = Nonce.Length - this.fixedIvLength;
				int N = CipherText.Length;

				AeadCipher = new byte[RecordIvLength + N];

				Array.Copy(Nonce, this.fixedIvLength, AeadCipher, 0, RecordIvLength);
				Array.Copy(CipherText, 0, AeadCipher, RecordIvLength, N);

				return AeadCipher;
			}
		}

		private byte[] GetAssociatedData(byte[] Header, int Start)
		{
			// §6.2.3.3, RFC 5246:
			// Key: client_write_key or server_write_key
			// A = Additional data: seq_num + TLSCompressed.type + TLSCompressed.version + TLSCompressed.length;
			// where "+" denotes concatenation.

			byte[] AssociatedData = new byte[13];

			Array.Copy(Header, Start + 3, AssociatedData, 0, 8);    // seq_num
			Array.Copy(Header, Start, AssociatedData, 8, 3);		// Type, version
			Array.Copy(Header, Start + 11, AssociatedData, 11, 2);  // length

			return AssociatedData;
		}

		/// <summary>
		/// Decrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <param name="Start">Start offset of header.</param>
		/// <param name="State">Endpoint state.</param>
		/// <returns>Decrypted data, or null if authentication failed.</returns>
		public override byte[] Decrypt(byte[] Data, byte[] Header, int Start, EndpointState State)
		{
			lock (this.aesCcm)
			{
				// On AEAD ciphers, see RFC 5246, §6.2.3.3

				int RecordIvLength = 12 - this.fixedIvLength;
				int N = Data.Length - RecordIvLength;
				byte[] CipherText = new byte[N];
				byte[] Nonce = new byte[12];
				byte[] AD = this.GetAssociatedData(Header, Start);
				byte[] PlainText;
				ushort Len;

				Len = AD[11];
				Len <<= 8;
				Len |= AD[12];

				Len -= (ushort)(this.t + RecordIvLength);

				AD[11] = (byte)(Len >> 8);
				AD[12] = (byte)Len;

				Array.Copy(Data, 0, Nonce, this.fixedIvLength, RecordIvLength);
				Array.Copy(Data, RecordIvLength, CipherText, 0, N);

				if (State.isClient)
				{
					Array.Copy(State.server_write_IV, 0, Nonce, 0, this.fixedIvLength);
					PlainText = this.aesCcm.AuthenticateAndDecrypt(CipherText, AD, Nonce, 
						State.server_write_key);
				}
				else
				{
					Array.Copy(State.client_write_IV, 0, Nonce, 0, this.fixedIvLength);
					PlainText = this.aesCcm.AuthenticateAndDecrypt(CipherText, AD, Nonce, 
						State.client_write_key);
				}

				return PlainText;
			}
		}
	}
}
