using System;
using System.Text;
using System.Xml;
using Waher.Security;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Abstract base class for End-to-End encryption schemes.
	/// </summary>
	public abstract class E2eEndpoint : IE2eEndpoint
	{
		private IE2eEndpoint prev = null;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public abstract int SecurityStrength
		{
			get;
		}

		/// <summary>
		/// Previous keys.
		/// </summary>
		public IE2eEndpoint Previous
		{
			get => this.prev;
			set => this.prev = value;
		}

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public abstract string LocalName
		{
			get;
		}

		/// <summary>
		/// Namespace of the E2E encryption scheme
		/// </summary>
		public virtual string Namespace
		{
			get { return EndpointSecurity.IoTHarmonizationE2E; }
		}

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public abstract IE2eEndpoint Create(int SecurityStrength);

		/// <summary>
		/// Parses endpoint information from an XML element.
		/// </summary>
		/// <param name="Xml">XML element.</param>
		/// <returns>Parsed key information, if possible, null if XML is not well-defined.</returns>
		public abstract IE2eEndpoint Parse(XmlElement Xml);

		/// <summary>
		/// Exports the public key information to XML.
		/// </summary>
		/// <param name="Xml">XML output</param>
		public abstract void ToXml(StringBuilder Xml);

		/// <summary>
		/// Encrypts binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to encrypt</param>
		/// <returns>Encrypted data</returns>
		public abstract byte[] Encrypt(string Id, string Type, string From, string To, byte[] Data);

		/// <summary>
		/// Decrypts binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to decrypt</param>
		/// <returns>Decrypted data</returns>
		public abstract byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data);

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
		public abstract bool Encrypt(string Id, string Type, string From, string To, byte[] Data, StringBuilder Xml);

		/// <summary>
		/// If the scheme can decrypt a given XML element.
		/// </summary>
		/// <param name="AesElement">XML element with encrypted data.</param>
		/// <returns>If the scheme can decrypt the data.</returns>
		public abstract bool CanDecrypt(XmlElement AesElement);

		/// <summary>
		/// Decrypts XML data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="AesElement">XML element with encrypted data.</param>
		/// <returns>Decrypted XMLs</returns>
		public abstract string Decrypt(string Id, string Type, string From, string To, XmlElement AesElement);

		/// <summary>
		/// Gets AES Initiation Vector from stanza attributes.s
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <returns>AES Initiation vector.</returns>
		protected byte[] GetIV(string Id, string Type, string From, string To)
		{
			byte[] IV = Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(Id + Type + From + To));
			Array.Resize<byte>(ref IV, 16);

			return IV;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder Xml = new StringBuilder();
			this.ToXml(Xml);
			return Xml.ToString();
		}

	}
}
