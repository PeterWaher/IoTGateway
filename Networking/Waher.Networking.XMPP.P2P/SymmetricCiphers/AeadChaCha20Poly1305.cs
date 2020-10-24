using System;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
    /// <summary>
    /// Implements support for the AEAD-ChaCha20-Poly1305 cipher in hybrid End-to-End encryption schemes.
    /// </summary>
    public class AeadChaCha20Poly1305 : ChaCha20
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
        /// If Authenticated Encryption with Associated Data is used
        /// </summary>
        public override bool AuthenticatedEncryption => true;

        /// <summary>
        /// Creates a new symmetric cipher object with the same settings as the current object.
        /// </summary>
        /// <returns>New instance</returns>
        public override IE2eSymmetricCipher CreteNew()
        {
            return new AeadChaCha20Poly1305();
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
        /// Encrypts binary data
        /// </summary>
        /// <param name="Data">Data to encrypt.</param>
        /// <param name="Encrypted">Encrypted data will be stored here.</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        public override async Task Encrypt(Stream Data, Stream Encrypted, byte[] Key, byte[] IV, byte[] AssociatedData)
        {
            Security.ChaChaPoly.AeadChaCha20Poly1305 Acp = new Security.ChaChaPoly.AeadChaCha20Poly1305(Key, IV);

            byte[] Mac = await Acp.Encrypt(Data, Encrypted, AssociatedData);
            await Encrypted.WriteAsync(Mac, 0, 16);
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
            Array.Copy(Data, c - 16, Mac, 0, 16);

            Array.Resize<byte>(ref Data, c - 16);

            return Acp.Decrypt(Data, AssociatedData, Mac);
        }

    }
}
