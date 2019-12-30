using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Abstract base class for End-to-End encryption schemes.
	/// </summary>
	public interface IE2eEndpoint : IDisposable
	{
		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		int SecurityStrength
		{
			get;
		}

		/// <summary>
		/// Local name of the E2E endpoint
		/// </summary>
		string LocalName
		{
			get;
		}

        /// <summary>
        /// Namespace of the E2E endpoint
        /// </summary>
        string Namespace
		{
			get;
		}

		/// <summary>
		/// Previous keys.
		/// </summary>
		IE2eEndpoint Previous
		{
			get;
			set;
		}

        /// <summary>
        /// Remote public key.
        /// </summary>
        byte[] PublicKey
        {
            get;
        }

        /// <summary>
        /// Remote public key, as a Base64 string.
        /// </summary>
        string PublicKeyBase64
        {
            get;
        }

        /// <summary>
        /// Creates a new key.
        /// </summary>
        /// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
        /// <returns>New E2E endpoint.</returns>
        IE2eEndpoint Create(int SecurityStrength);

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        IE2eEndpoint CreatePrivate(byte[] Secret);

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        IE2eEndpoint CreatePublic(byte[] PublicKey);

        /// <summary>
        /// Parses endpoint information from an XML element.
        /// </summary>
        /// <param name="Xml">XML element.</param>
        /// <returns>Parsed key information, if possible, null if XML is not well-defined.</returns>
        IE2eEndpoint Parse(XmlElement Xml);

		/// <summary>
		/// Exports the public key information to XML.
		/// </summary>
		/// <param name="Xml">XML output</param>
        /// <param name="ParentNamespace">Namespace of parent element.</param>
		void ToXml(StringBuilder Xml, string ParentNamespace);

        /// <summary>
        /// If shared secrets can be calculated from the endpoints keys.
        /// </summary>
        bool SupportsSharedSecrets
        {
            get;
        }

        /// <summary>
        /// Encrypts a secret. Used if shared secrets cannot be calculated.
        /// </summary>
        /// <param name="Secret">Secret</param>
        /// <returns>Encrypted secret.</returns>
        byte[] EncryptSecret(byte[] Secret);

        /// <summary>
        /// Decrypts a secret. Used if shared secrets cannot be calculated.
        /// </summary>
        /// <param name="Secret">Encrypted secret</param>
        /// <returns>Decrypted secret.</returns>
        byte[] DecryptSecret(byte[] Secret);

        /// <summary>
        /// Gets a shared secret
        /// </summary>
        /// <param name="RemoteEndpoint">Remote endpoint</param>
        /// <returns>Shared secret.</returns>
        byte[] GetSharedSecret(IE2eEndpoint RemoteEndpoint);

        /// <summary>
        /// If signatures are supported.
        /// </summary>
        bool SupportsSignatures
        {
            get;
        }

        /// <summary>
        /// Signs binary data using the local private key.
        /// </summary>
        /// <param name="Data">Binary data</param>
        /// <returns>Digital signature.</returns>
        byte[] Sign(byte[] Data);

        /// <summary>
        /// Signs binary data using the local private key.
        /// </summary>
        /// <param name="Data">Binary data</param>
        /// <returns>Digital signature.</returns>
        byte[] Sign(Stream Data);

        /// <summary>
        /// Verifies a signature.
        /// </summary>
        /// <param name="Data">Data that is signed.</param>
        /// <param name="Signature">Digital signature.</param>
        /// <returns>If signature is valid.</returns>
        bool Verify(byte[] Data, byte[] Signature);

        /// <summary>
        /// Verifies a signature.
        /// </summary>
        /// <param name="Data">Data that is signed.</param>
        /// <param name="Signature">Digital signature.</param>
        /// <returns>If signature is valid.</returns>
        bool Verify(Stream Data, byte[] Signature);

        /// <summary>
        /// If endpoint is considered safe (i.e. there are no suspected backdoors)
        /// </summary>
        bool Safe
        {
            get;
        }

        /// <summary>
        /// If implementation is slow, compared to other options.
        /// </summary>
        bool Slow
        {
            get;
        }
        /// <summary>
        /// Provides a score for the endpoint. More features, higher score (comparable to alternatives with the same security strength).
        /// </summary>
        int Score
        {
            get;
        }

        /// <summary>
        /// Default symmetric cipher.
        /// </summary>
        IE2eSymmetricCipher DefaultSymmetricCipher
        {
            get;
        }

        /// <summary>
        /// Gets the next counter value.
        /// </summary>
        /// <returns>Counter value.</returns>
        uint GetNextCounter();
    }
}
