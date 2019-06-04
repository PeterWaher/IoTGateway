using System;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
    /// <summary>
    /// Abstract base class for symmetric ciphers.
    /// </summary>
    public abstract class E2eSymmetricCipher
    {
        /// <summary>
        /// Local name of the E2E symmetric cipher
        /// </summary>
        public abstract string LocalName
        {
            get;
        }

        /// <summary>
        /// Namespace of the E2E symmetric cipher
        /// </summary>
        public virtual string Namespace
        {
            get { return EndpointSecurity.IoTHarmonizationE2E; }
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public virtual void Dispose()
        {
        }

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
        public abstract byte[] Encrypt(string Id, string Type, string From, string To, uint Counter, byte[] Data, IE2eEndpoint Sender, IE2eEndpoint Receiver);

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
        public abstract byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data, IE2eEndpoint Sender, IE2eEndpoint Receiver);

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
        public abstract bool Encrypt(string Id, string Type, string From, string To, uint Counter, byte[] Data, StringBuilder Xml, IE2eEndpoint Sender, IE2eEndpoint Receiver);

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
        public abstract string Decrypt(string Id, string Type, string From, string To, XmlElement Xml, IE2eEndpoint Sender, IE2eEndpoint Receiver);
    }
}
