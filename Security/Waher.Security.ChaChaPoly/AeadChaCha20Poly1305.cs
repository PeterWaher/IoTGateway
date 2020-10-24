using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Runtime.Temporary;

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
            Array.Copy(AdditionalData, 0, Bin, 0, c = AdditionalData.Length);

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
        /// Encrypts data.
        /// </summary>
        /// <param name="Data">Data to be encrypted.</param>
        /// <param name="Encrypted">Encrypted data will be stored here.</param>
        /// <param name="AdditionalData">Additional data</param>
        /// <returns>Message Authentication Code.</returns>
        public Task<byte[]> Encrypt(Stream Data, Stream Encrypted, byte[] AdditionalData)
        {
            return this.Process(Data, Encrypted, AdditionalData, true);
        }

        private async Task<byte[]> Process(Stream Data, Stream Encrypted, byte[] AdditionalData, bool Encrypting)
        {
            await this.chaCha20.EncryptOrDecrypt(Data, Encrypted);

            using (TemporaryStream Temp = new TemporaryStream())
            {
                byte[] Padding;

                int i = AdditionalData.Length;
                await Temp.WriteAsync(AdditionalData, 0, i);
                i &= 15;
                if (i != 0)
                {
                    i = 16 - i;
                    Padding = new byte[i];
                    await Temp.WriteAsync(Padding, 0, i);
                }

                if (Encrypting)
                {
                    Encrypted.Position = 0;
                    await Encrypted.CopyToAsync(Temp);
                }
                else
                {
                    Data.Position = 0;
                    await Data.CopyToAsync(Temp);
                }

                i = AdditionalData.Length;

                Temp.WriteByte((byte)i);
                i >>= 8;
                Temp.WriteByte((byte)i);
                i >>= 8;
                Temp.WriteByte((byte)i);
                i >>= 8;
                Temp.WriteByte((byte)i);

                Padding = new byte[4];
                await Temp.WriteAsync(Padding, 0, 4);

                i = (int)Encrypted.Length;

                Temp.WriteByte((byte)i);
                i >>= 8;
                Temp.WriteByte((byte)i);
                i >>= 8;
                Temp.WriteByte((byte)i);
                i >>= 8;
                Temp.WriteByte((byte)i);

                await Temp.WriteAsync(Padding, 0, 4);

                Temp.Position = 0;

                return await this.poly1305.CalcMac(Temp);
            }
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
