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
		private AesCcm aesCcm;

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
			this.aesCcm = new AesCcm(KeySize, TagLength, 12);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (this.aesCcm != null)
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
		/// <returns>Encrypted data.</returns>
		public override byte[] Encrypt(byte[] Data, byte[] Header)
		{
			if (this.IsClient)
				return this.aesCcm.Encrypt(Data, this.GetAssociatedData(Header), this.GetNonce(true), this.ClientWriteKey);
			else
				return this.aesCcm.Encrypt(Data, this.GetAssociatedData(Header), this.GetNonce(false), this.ServerWriteKey);
		}

		private byte[] GetAssociatedData(byte[] Header)
		{
			// §6.2.3.3, RFC 5246:
			// Key: client_write_key or server_write_key
			// A = Additional data: seq_num + TLSCompressed.type + TLSCompressed.version + TLSCompressed.length;
			// where "+" denotes concatenation.

			byte[] AssociatedData = new byte[13];

			Array.Copy(Header, 3, AssociatedData, 0, 8);    // seq_num
			Array.Copy(Header, 0, AssociatedData, 8, 3);    // Type, version
			Array.Copy(Header, 11, AssociatedData, 11, 2);  // length

			return AssociatedData;
		}

		/// <summary>
		/// Decrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <returns>Decrypted data, or null if authentication failed.</returns>
		public override byte[] Decrypt(byte[] Data, byte[] Header)
		{
			if (this.IsClient)
				return this.aesCcm.AuthenticateAndDecrypt(Data, this.GetAssociatedData(Header), this.GetNonce(true), this.ClientWriteKey);
			else
				return this.aesCcm.AuthenticateAndDecrypt(Data, this.GetAssociatedData(Header), this.GetNonce(false), this.ServerWriteKey);
		}
	}
}
