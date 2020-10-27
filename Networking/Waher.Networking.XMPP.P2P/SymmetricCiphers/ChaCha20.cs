using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Temporary;
using Waher.Security;

namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
    /// <summary>
    /// Implements support for the ChaCha20 cipher in hybrid End-to-End encryption schemes.
    /// </summary>
    public class ChaCha20 : E2eSymmetricCipher
    {
        /// <summary>
        /// Implements support for the ChaCha20 cipher in hybrid End-to-End encryption schemes.
        /// </summary>
        public ChaCha20()
        {
        }

        /// <summary>
        /// Local name of the E2E symmetric cipher
        /// </summary>
        public override string LocalName => "cha";

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
            return new ChaCha20();
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
            Security.ChaChaPoly.ChaCha20 ChaCha20 = new Security.ChaChaPoly.ChaCha20(Key, 1, IV);
            return ChaCha20.EncryptOrDecrypt(Data);
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
            return this.Encrypt(Data, Key, IV, AssociatedData);
        }

        /// <summary>
        /// Encrypts binary data
        /// </summary>
        /// <param name="Data">Data to encrypt.</param>
        /// <param name="Encrypted">Encrypted data will be stored here.</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        public override Task Encrypt(Stream Data, Stream Encrypted, byte[] Key, byte[] IV, byte[] AssociatedData)
        {
            Security.ChaChaPoly.ChaCha20 ChaCha20 = new Security.ChaChaPoly.ChaCha20(Key, 1, IV);
            return ChaCha20.EncryptOrDecrypt(Data, Encrypted);
        }

        /// <summary>
        /// Decrypts binary data
        /// </summary>
        /// <param name="Data">Binary Data</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        /// <returns>Decrypted Data</returns>
        public override async Task<Stream> Decrypt(Stream Data, byte[] Key, byte[] IV, byte[] AssociatedData)
        {
            TemporaryStream Decrypted = new TemporaryStream();

            try
            {
                Security.ChaChaPoly.ChaCha20 ChaCha20 = new Security.ChaChaPoly.ChaCha20(Key, 1, IV);
                await ChaCha20.EncryptOrDecrypt(Data, Decrypted);

                return Decrypted;
            }
            catch (Exception ex)
            {
                Decrypted.Dispose();
                ExceptionDispatchInfo.Capture(ex).Throw();
                return null;
            }
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
