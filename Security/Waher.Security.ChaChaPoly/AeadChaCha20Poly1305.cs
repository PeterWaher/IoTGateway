using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.ChaChaPoly
{
    /// <summary>
    /// Class implementing the authenticated encryption with additional
    /// data algorithm AEAD_CHACHA20_POLY1305.
    /// </summary>
    public class AeadChaCha20Poly1305
    {
        private readonly ChaCha20 chaCha20;
        private readonly Poly1305 poly1305;

        /// <summary>
        /// Class implementing the authenticated encryption with additional
        /// data algorithm AEAD_CHACHA20_POLY1305.
        /// </summary>
        /// <param name="Key">Key</param>
        /// <param name="Nonce">Nonce value</param>
        public AeadChaCha20Poly1305(byte[] Key, byte[] Nonce)
        {
            this.chaCha20 = new ChaCha20(Key, 0, Nonce);
            this.poly1305 = new Poly1305(this.chaCha20.GetBytes(32));
            this.chaCha20.NextBlock();
        }

        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="Data">Data to be encrypted.</param>
        /// <param name="AdditionalData">Additional data</param>
        /// <param name="Mac">Message Authentication Code.</param>
        /// <returns>Encrypted data.</returns>
        public byte[] Encrypt(byte[] Data, byte[] AdditionalData, out byte[] Mac)
        {
            return this.Process(Data, AdditionalData, true, out Mac);
        }

        private byte[] Process(byte[] Data, byte[] AdditionalData, bool Encrypting, out byte[] Mac)
        {
            byte[] Encrypted = this.chaCha20.EncryptOrDecrypt(Data);
            int c = AdditionalData.Length;
            int i = c & 15;
            if (i != 0)
                c += 16 - i;

            c += Encrypted.Length;
            i = c & 15;
            if (i != 0)
                c += 16 - i;

            c += 16;

            byte[] Bin = new byte[c];
            Array.Copy(AdditionalData, 0, Bin, 0, AdditionalData.Length);

            c = AdditionalData.Length;
            i = c & 15;
            if (i != 0)
                c += 16 - i;

            Array.Copy(Encrypting ? Encrypted : Data, 0, Bin, c, Encrypted.Length);

            i = Bin.Length - 16;
            c = AdditionalData.Length;

            Bin[i++] = (byte)c;
            c >>= 8;
            Bin[i++] = (byte)c;
            c >>= 8;
            Bin[i++] = (byte)c;
            c >>= 8;
            Bin[i++] = (byte)c;

            i += 4;
            c = Encrypted.Length;

            Bin[i++] = (byte)c;
            c >>= 8;
            Bin[i++] = (byte)c;
            c >>= 8;
            Bin[i++] = (byte)c;
            c >>= 8;
            Bin[i] = (byte)c;

            Mac = this.poly1305.CalcMac(Bin);

            return Encrypted;
        }

        /// <summary>
        /// Decrypt data.
        /// </summary>
        /// <param name="Data">Data to be encrypted.</param>
        /// <param name="AdditionalData">Additional data</param>
        /// <param name="Mac">Message Authentication Code.</param>
        /// <returns>Decrypted data, if successful, null otherwise.</returns>
        public byte[] Decrypt(byte[] Data, byte[] AdditionalData, byte[] Mac)
        {
            byte[] Decrypted = this.Process(Data, AdditionalData, false, out byte[] Mac2);

            int i, c = Mac2.Length;
            if (c != Mac.Length)
                return null;

            for (i = 0; i < c; i++)
            {
                if (Mac[i] != Mac2[i])
                    return null;
            }

            return Decrypted;
        }

    }
}
