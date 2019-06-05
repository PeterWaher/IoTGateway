using System;
using System.Security.Cryptography;
using System.Text;
using Waher.Security;
using Waher.Security.ChaChaPoly;

namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
    /// <summary>
    /// Implements support for the AEAD-ChaCha20-Poly1305 cipher in hybrid End-to-End encryption schemes.
    /// </summary>
    public class AeadChaCha20Poly1305 : E2eSymmetricCipher
    {
        /// <summary>
        /// Implements support for the AEAD-ChaCha20-Poly1305 cipher in hybrid End-to-End encryption schemes.
        /// </summary>
        public AeadChaCha20Poly1305()
        {
        }

        /// <summary>
        /// Local name of the E2E symmetric cipher
        /// </summary>
        public override string LocalName => "acp";

        /// <summary>
        /// Namespace of the E2E symmetric cipher
        /// </summary>
        public override string Namespace => EndpointSecurity.IoTHarmonizationE2E;

        /// <summary>
        /// If Authenticated Encryption with Associated Data is used
        /// </summary>
        public override bool AuthenticatedEncryption => true;

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
            Array.Resize<byte>(ref IV, 12);

            IV[8] = (byte)Counter;
            Counter >>= 8;
            IV[9] = (byte)Counter;
            Counter >>= 8;
            IV[10] = (byte)Counter;
            Counter >>= 8;
            IV[11] = (byte)Counter;

            return IV;
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
            Security.ChaChaPoly.AeadChaCha20Poly1305 Acp = new Security.ChaChaPoly.AeadChaCha20Poly1305(Key, IV);

            byte[] Encrypted = Acp.Encrypt(Data, AssociatedData, out byte[] Mac);
            int c = Encrypted.Length;
            
            Array.Resize<byte>(ref Encrypted, c + 16);
            Array.Copy(Mac, 0, Encrypted, c, 16);

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
            int c = Data.Length;
            if (c < 16)
                return null;

            Security.ChaChaPoly.AeadChaCha20Poly1305 Acp = new Security.ChaChaPoly.AeadChaCha20Poly1305(Key, IV);
            byte[] Mac = new byte[16];
            Array.Copy(Data, 0, Mac, 0, 16);

            Array.Resize<byte>(ref Data, c - 16);

            return Acp.Decrypt(Data, AssociatedData, Mac);
        }


        /// <summary>
        /// Generates a new key. Used when the asymmetric cipher cannot calculate a shared secret.
        /// </summary>
        /// <returns>New key</returns>
        public override byte[] GenerateKey()
        {
            byte[] Key = new byte[32];

            lock (rnd)
            {
                rnd.GetBytes(Key);
            }

            return Key;
        }

    }
}
