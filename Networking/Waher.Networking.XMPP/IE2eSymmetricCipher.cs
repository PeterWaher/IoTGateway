using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Networking.XMPP
{
    /// <summary>
    /// Interface for symmetric ciphers.
    /// </summary>
    public interface IE2eSymmetricCipher : IDisposable
    {
        /// <summary>
        /// Local name of the symmetric cipher
        /// </summary>
        string LocalName
        {
            get;
        }

        /// <summary>
        /// Namespace of the E2E symmetric cipher
        /// </summary>
        string Namespace
        {
            get;
        }

        /// <summary>
        /// If Authenticated Encryption with Associated Data is used
        /// </summary>
        bool AuthenticatedEncryption
        {
            get;
        }

        /// <summary>
        /// Creates a new symmetric cipher object with the same settings as the current object.
        /// </summary>
        /// <returns>New instance</returns>
        IE2eSymmetricCipher CreteNew();

        /// <summary>
        /// Encrypts binary data
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <param name="Data">Binary data to encrypt</param>
        /// <param name="Sender">Local endpoint performing the encryption.</param>
        /// <param name="Receiver">Remote endpoint performing the decryption.</param>
        /// <returns>Encrypted data</returns>
        byte[] Encrypt(string Id, string Type, string From, string To, uint Counter, byte[] Data, IE2eEndpoint Sender, IE2eEndpoint Receiver);

        /// <summary>
        /// Encrypts binary data
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <param name="Data">Binary data to encrypt</param>
        /// <param name="Encrypted">Encrypted data will be stored here.</param>
        /// <param name="Sender">Local endpoint performing the encryption.</param>
        /// <param name="Receiver">Remote endpoint performing the decryption.</param>
        Task Encrypt(string Id, string Type, string From, string To, uint Counter, Stream Data, Stream Encrypted, IE2eEndpoint Sender, IE2eEndpoint Receiver);

        /// <summary>
        /// Decrypts binary data
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Data">Binary data to decrypt</param>
        /// <param name="Sender">Remote endpoint performing the encryption.</param>
        /// <param name="Receiver">Local endpoint performing the decryption.</param>
        /// <returns>Decrypted data</returns>
        byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data, IE2eEndpoint Sender, IE2eEndpoint Receiver);

        /// <summary>
        /// Encrypts Binary data
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <param name="Data">Binary data to encrypt</param>
        /// <param name="Xml">XML output</param>
        /// <param name="Sender">Local endpoint performing the encryption.</param>
        /// <param name="Receiver">Remote endpoint performing the decryption.</param>
        /// <returns>If encryption was possible</returns>
        bool Encrypt(string Id, string Type, string From, string To, uint Counter, byte[] Data, StringBuilder Xml, IE2eEndpoint Sender, IE2eEndpoint Receiver);

        /// <summary>
        /// Decrypts XML data
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Xml">XML element with encrypted data.</param>
        /// <param name="Sender">Remote endpoint performing the encryption.</param>
        /// <param name="Receiver">Local endpoint performing the decryption.</param>
        /// <returns>Decrypted XMLs</returns>
        string Decrypt(string Id, string Type, string From, string To, XmlElement Xml, IE2eEndpoint Sender, IE2eEndpoint Receiver);
    }
}
