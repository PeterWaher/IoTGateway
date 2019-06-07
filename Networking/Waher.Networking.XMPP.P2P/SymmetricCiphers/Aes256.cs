using System;
using System.Security.Cryptography;
using System.Text;
using Waher.Security;

namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
    /// <summary>
    /// Implements support for the AES-256 cipher in hybrid End-to-End encryption schemes.
    /// </summary>
    public class Aes256 : E2eSymmetricCipher
    {
        /// <summary>
        /// AES encryption object
        /// </summary>
        protected Aes aes;

        /// <summary>
        /// Implements support for the AES-256 cipher in hybrid End-to-End encryption schemes.
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
        /// Local name of the E2E symmetric cipher
        /// </summary>
        public override string LocalName => "aes";

        /// <summary>
        /// Namespace of the E2E symmetric cipher
        /// </summary>
        public override string Namespace => EndpointSecurity.IoTHarmonizationE2E;

        /// <summary>
        /// Creates a new symmetric cipher object with the same settings as the current object.
        /// </summary>
        /// <returns>New instance</returns>
        public override IE2eSymmetricCipher CreteNew()
        {
            return new Aes256();
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this.aes?.Dispose();
            this.aes = null;
        }

        /// <summary>
        /// Gets an Initiation Vector from stanza attributes.
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <returns>Initiation vector.</returns>
        protected override byte[] GetIV(string Id, string Type, string From, string To, uint Counter)
        {
            byte[] IV = Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(Id + Type + From + To));
            Array.Resize<byte>(ref IV, 16);

            IV[12] = (byte)Counter;
            Counter >>= 8;
            IV[13] = (byte)Counter;
            Counter >>= 8;
            IV[14] = (byte)Counter;
            Counter >>= 8;
            IV[15] = (byte)Counter;

            return IV;
        }

        /// <summary>
        /// Calculates the minimum size of encrypted data, given the size of the content.
        /// </summary>
        /// <param name="ContentLength">Size of content.</param>
        /// <returns>Minimum size of encrypted data.</returns>
        protected override int GetEncryptedLength(int ContentLength)
        {
            return (ContentLength + 15) & ~0xf;
        }

        /// <summary>
        /// Encrypts binary data
        /// </summary>
        /// <param name="Data">Binary Data</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        /// <returns>Encrypted Data</returns>
        public override byte[] Encrypt(byte[] Data, byte[] Key, byte[] IV, byte[] AssociatedData)
        {
            byte[] Encrypted = base.Encrypt(Data, Key, IV, AssociatedData);

            lock (this.aes)
            {
                using (ICryptoTransform Aes = this.aes.CreateEncryptor(Key, IV))
                {
                    Encrypted = Aes.TransformFinalBlock(Encrypted, 0, Encrypted.Length);
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
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        /// <returns>Decrypted Data</returns>
        public override byte[] Decrypt(byte[] Data, byte[] Key, byte[] IV, byte[] AssociatedData)
        {
            lock (this.aes)
            {
                using (ICryptoTransform Aes = this.aes.CreateDecryptor(Key, IV))
                {
                    Data = Aes.TransformFinalBlock(Data, 0, Data.Length);
                }
            }

            return base.Decrypt(Data, Key, IV, AssociatedData);
        }


        /// <summary>
        /// Generates a new key. Used when the asymmetric cipher cannot calculate a shared secret.
        /// </summary>
        /// <returns>New key</returns>
        public override byte[] GenerateKey()
        {
            lock (this.aes)
            {
                this.aes.GenerateKey();
                return this.aes.Key;
            }
        }

    }
}
