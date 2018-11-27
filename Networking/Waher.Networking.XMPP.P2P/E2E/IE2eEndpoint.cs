using System;
using System.Text;
using System.Xml;
using Waher.Security;

namespace Waher.Networking.XMPP.P2P.E2E
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
		/// Local name of the E2E encryption scheme
		/// </summary>
		string LocalName
		{
			get;
		}

		/// <summary>
		/// Namespace of the E2E encryption scheme
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
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		IE2eEndpoint Create(int SecurityStrength);

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
		void ToXml(StringBuilder Xml);

		/// <summary>
		/// Encrypts binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to encrypt</param>
		/// <returns>Encrypted data</returns>
		byte[] Encrypt(string Id, string Type, string From, string To, byte[] Data);

		/// <summary>
		/// Decrypts binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to decrypt</param>
		/// <returns>Decrypted data</returns>
		byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data);

		/// <summary>
		/// Encrypts Binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to encrypt</param>
		/// <param name="Xml">XML output</param>
		/// <returns>If encryption was possible</returns>
		bool Encrypt(string Id, string Type, string From, string To, byte[] Data, StringBuilder Xml);

		/// <summary>
		/// If the scheme can decrypt a given XML element.
		/// </summary>
		/// <param name="AesElement">XML element with encrypted data.</param>
		/// <returns>If the scheme can decrypt the data.</returns>
		bool CanDecrypt(XmlElement AesElement);

		/// <summary>
		/// Decrypts XML data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="AesElement">XML element with encrypted data.</param>
		/// <returns>Decrypted XMLs</returns>
		string Decrypt(string Id, string Type, string From, string To, XmlElement AesElement);

	}
}
