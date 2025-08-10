﻿using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Abstract base class for End-to-End encryption schemes.
	/// </summary>
	public abstract class E2eEndpoint : IE2eEndpoint
	{
		private IE2eSymmetricCipher defaultSymmetricCipher;
		private IE2eEndpoint prev = null;
		private uint counter = 0;

		/// <summary>
		/// Abstract base class for End-to-End encryption schemes.
		/// </summary>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public E2eEndpoint(IE2eSymmetricCipher DefaultSymmetricCipher)
		{
			this.defaultSymmetricCipher = DefaultSymmetricCipher;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			this.defaultSymmetricCipher?.Dispose();
			this.defaultSymmetricCipher = null;
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
		public virtual string Namespace => EndpointSecurity.IoTHarmonizationE2ECurrent;

		/// <summary>
		/// Remote public key.
		/// </summary>
		public abstract byte[] PublicKey
		{
			get;
		}

		/// <summary>
		/// Remote public key, as a Base64 string.
		/// </summary>
		public abstract string PublicKeyBase64
		{
			get;
		}

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public abstract IE2eEndpoint Create(int SecurityStrength);

		/// <summary>
		/// Creates a new endpoint given a private key.
		/// </summary>
		/// <param name="Secret">Secret.</param>
		/// <returns>Endpoint object.</returns>
		public abstract IE2eEndpoint CreatePrivate(byte[] Secret);

		/// <summary>
		/// Creates a new endpoint given a public key.
		/// </summary>
		/// <param name="PublicKey">Remote public key.</param>
		/// <returns>Endpoint object.</returns>
		public abstract IE2eEndpoint CreatePublic(byte[] PublicKey);

		/// <summary>
		/// Parses endpoint information from an XML element.
		/// </summary>
		/// <param name="Xml">XML element.</param>
		/// <returns>Parsed key information, if possible, null if XML is not well-defined.</returns>
		public IE2eEndpoint Parse(XmlElement Xml)
		{
			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "pub":
						return this.CreatePublic(Convert.FromBase64String(Attr.Value));

					case "d":
						return this.CreatePrivate(Convert.FromBase64String(Attr.Value));
				}
			}

			return null;
		}

		/// <summary>
		/// Exports the public key information to XML.
		/// </summary>
		/// <param name="Xml">XML output</param>
		/// <param name="ParentNamespace">Namespace of parent element.</param>
		public void ToXml(StringBuilder Xml, string ParentNamespace)
		{
			Xml.Append('<');
			Xml.Append(this.LocalName);
			Xml.Append(" pub=\"");
			Xml.Append(this.PublicKeyBase64);

			string ns = this.Namespace;
			if (ns != ParentNamespace)
			{
				Xml.Append("\" xmlns=\"");
				Xml.Append(ns);
			}

			Xml.Append("\"/>");
		}

		/// <summary>
		/// Gets a shared secret for encryption, and optionally a corresponding cipher text.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint</param>
		/// <param name="Cipher">Symmetric cipher to use for encryption.</param>
		/// <param name="CipherText">Optional cipher text required by the recipient to
		/// be able to generate the same shared secret.</param>
		/// <returns>Shared secret.</returns>
		public abstract byte[] GetSharedSecretForEncryption(IE2eEndpoint RemoteEndpoint,
			IE2eSymmetricCipher Cipher, out byte[] CipherText);

		/// <summary>
		/// Gets a shared secret for decryption.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint</param>
		/// <param name="CipherText">Optional cipher text required by the recipient to
		/// be able to generate the same shared secret.</param>
		/// <returns>Shared secret.</returns>
		public abstract byte[] GetSharedSecretForDecryption(IE2eEndpoint RemoteEndpoint,
			byte[] CipherText);

		/// <summary>
		/// If the recipient needs a cipher text to generate the same shared secret.
		/// </summary>
		public abstract bool SharedSecretUseCipherText
		{
			get;
		}

		/// <summary>
		/// If signatures are supported.
		/// </summary>
		public virtual bool SupportsSignatures => true;

		/// <summary>
		/// Signs binary data using the local private key.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <returns>Digital signature.</returns>
		public abstract byte[] Sign(byte[] Data);

		/// <summary>
		/// Signs binary data using the local private key.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <returns>Digital signature.</returns>
		public abstract byte[] Sign(Stream Data);

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public abstract bool Verify(byte[] Data, byte[] Signature);

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public abstract bool Verify(Stream Data, byte[] Signature);

		/// <summary>
		/// If endpoint is considered safe (i.e. there are no suspected backdoors)
		/// </summary>
		public virtual bool Safe => true;

		/// <summary>
		/// If implementation is slow, compared to other options.
		/// </summary>
		public virtual bool Slow => false;

		/// <summary>
		/// If post-quantum cryptography is used.
		/// </summary>
		public virtual bool PostQuantumCryptography => false;

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder Xml = new StringBuilder();
			this.ToXml(Xml, string.Empty);
			return Xml.ToString();
		}

		/// <inheritdoc/>
		public override abstract bool Equals(object obj);

		/// <inheritdoc/>
		public override abstract int GetHashCode();

		/// <summary>
		/// Provides a score for the endpoint. More features, higher score.
		/// </summary>
		public int Score
		{
			get
			{
				int Result = 0;

				if (!this.SharedSecretUseCipherText)
					Result++;

				if (this.SupportsSignatures)
					Result++;

				if (this.Safe)
					Result++;

				if (!this.Slow)
					Result++;

				if (this.PostQuantumCryptography)
					Result += 2;	// To overcome requirement of using cipher texts to derive shared secrets.

				return Result;
			}
		}

		/// <summary>
		/// Default symmetric cipher.
		/// </summary>
		public virtual IE2eSymmetricCipher DefaultSymmetricCipher => this.defaultSymmetricCipher;

		/// <summary>
		/// Gets the next counter value.
		/// </summary>
		/// <returns>Counter value.</returns>
		public uint GetNextCounter()
		{
			return ++this.counter;
		}
	}
}
