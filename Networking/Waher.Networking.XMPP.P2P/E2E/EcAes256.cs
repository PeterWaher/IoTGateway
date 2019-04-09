using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Cache;
using Waher.Security;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Abstract base class for Elliptic Curve / AES-256 hybrid ciphers.
	/// </summary>
	public abstract class EcAes256 : Aes256
	{
		private static readonly Cache<string, byte[]> sharedSecrets = new Cache<string, byte[]>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromDays(1));

		/// <summary>
		/// Remote public key.
		/// </summary>
		protected readonly PointOnCurve publicKey;
		private readonly CurvePrimeField curve;
		private readonly bool hasPrivateKey;
		private readonly string keyString;

		/// <summary>
		/// Abstract base class for Elliptic Curve / AES-256 hybrid ciphers.s
		/// </summary>
		/// <param name="Curve">Curve instance</param>
		public EcAes256(CurvePrimeField Curve)
			: base()
		{
			this.curve = Curve;
			this.publicKey = Curve.PublicKey;
			this.hasPrivateKey = true;
			this.keyString = this.publicKey.X.ToString() + "," + this.publicKey.Y.ToString();
		}

		/// <summary>
		/// Abstract base class for Elliptic Curve / AES-256 hybrid ciphers.s
		/// </summary>
		/// <param name="X">X-coordinate of remote public key.</param>
		/// <param name="Y">Y-coordinate of remote public key.</param>
		/// <param name="ReferenceCurve">Reference curve</param>
		public EcAes256(byte[] X, byte[] Y, CurvePrimeField ReferenceCurve)
			: base()
		{
			this.publicKey = new PointOnCurve(FromNetwork(X), FromNetwork(Y));
			this.curve = ReferenceCurve;
			this.hasPrivateKey = false;
			this.keyString = this.publicKey.X.ToString() + "," + this.publicKey.Y.ToString();
		}

		/// <summary>
		/// If the key contains a private key.
		/// </summary>
		public bool HasPrivateKey => this.hasPrivateKey;

		/// <summary>
		/// Creates a new endpoint.
		/// </summary>
		/// <param name="D">Private key.</param>
		/// <returns>Endpoint object.</returns>
		public abstract EcAes256 Create(BigInteger D);

		/// <summary>
		/// Creates a new endpoint.
		/// </summary>
		/// <param name="X">X-coordinate of remote public key.</param>
		/// <param name="Y">Y-coordinate of remote public key.</param>
		/// <returns>Endpoint object.</returns>
		public abstract EcAes256 Create(byte[] X, byte[] Y);

		/// <summary>
		/// Parses endpoint information from an XML element.
		/// </summary>
		/// <param name="Xml">XML element.</param>
		/// <returns>Parsed key information, if possible, null if XML is not well-defined.</returns>
		public override IE2eEndpoint Parse(XmlElement Xml)
		{
			byte[] X = null;
			byte[] Y = null;

			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "x":
						X = Convert.FromBase64String(Attr.Value);
						break;

					case "y":
						Y = Convert.FromBase64String(Attr.Value);
						break;
				}
			}

			if (X != null && Y != null)
				return this.Create(X, Y);
			else
				return null;
		}

		/// <summary>
		/// Exports the public key information to XML.
		/// </summary>
		/// <param name="Xml">XML output</param>
		public override void ToXml(StringBuilder Xml)
		{
			Xml.Append('<');
			Xml.Append(this.LocalName);
			Xml.Append(" x=\"");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork(this.publicKey.X)));
			Xml.Append("\" xmlns=\"");
			Xml.Append(this.Namespace);
			Xml.Append("\" y=\"");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork(this.publicKey.Y)));
			Xml.Append("\"/>");
		}

		/// <summary>
		/// Remote public key.
		/// </summary>
		public PointOnCurve PublicKey => this.publicKey;

		/// <summary>
		/// Converts a <see cref="BigInteger"/> from network binary representation.
		/// </summary>
		/// <param name="Bin">Network binary representation</param>
		/// <returns><see cref="BigInteger"/> representation</returns>
		public static BigInteger FromNetwork(byte[] Bin)
		{
			int c = Bin.Length;
			bool ExtraZero = Bin[0] >= 0x80;
			byte[] Bin2 = new byte[ExtraZero ? c + 1 : c];

			Array.Copy(Bin, 0, Bin2, ExtraZero ? 1 : 0, c);
			Array.Reverse(Bin2);

			return new BigInteger(Bin2);
		}

		/// <summary>
		/// Name of elliptic curve
		/// </summary>
		public string CurveName => this.curve.CurveName;

		/// <summary>
		/// Elliptic Curve
		/// </summary>
		public CurvePrimeField Curve => this.curve;

		/// <summary>
		/// Previous Elliptic Curve
		/// </summary>
		public CurvePrimeField PrevCurve => (this.Previous as EcAes256)?.Curve;

		/// <summary>
		/// Shared secret, for underlying AES cipher.
		/// </summary>
		public static byte[] GetSharedKey(EcAes256 LocalKey, EcAes256 RemoteKey)
		{
			string Key = LocalKey.keyString + ";" + RemoteKey.keyString;

			if (sharedSecrets.TryGetValue(Key, out byte[] SharedKey))
				return SharedKey;

			SharedKey = LocalKey.curve.GetSharedKey(RemoteKey.publicKey, HashFunction.SHA256);
			sharedSecrets[Key] = SharedKey;

			return SharedKey;
		}

		/// <summary>
		/// Encrypts binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to encrypt</param>
		/// <param name="LocalEndpoint">Local endpoint of same type.</param>
		/// <returns>Encrypted data</returns>
		public override byte[] Encrypt(string Id, string Type, string From, string To, byte[] Data, IE2eEndpoint LocalEndpoint)
		{
			if (!(LocalEndpoint is EcAes256 LocalEcAes256))
				return null;

			byte[] Encrypted;
			byte[] Key = GetSharedKey(LocalEcAes256, this);
			byte[] IV = GetIV(Id, Type, From, To);
			KeyValuePair<BigInteger, BigInteger> Signature;

			Encrypted = this.Encrypt(Data, Key, IV);

			Signature = LocalEcAes256.Curve.Sign(Data, HashFunction.SHA256);

			byte[] Signature1 = ToNetwork(Signature.Key);
			byte[] Signature2 = ToNetwork(Signature.Value);

			byte[] Block = new byte[Signature1.Length + Signature2.Length + Encrypted.Length + 6];
			int i, j;

			j = 6;

			i = Signature1.Length;
			Array.Copy(Signature1, 0, Block, j, i);
			j += i;
			Block[0] = (byte)i;

			i = Signature2.Length;
			Array.Copy(Signature2, 0, Block, j, i);
			j += i;
			Block[1] = (byte)i;

			i = Encrypted.Length;
			Array.Copy(Encrypted, 0, Block, j, i);
			j += i;
			Block[2] = (byte)(i >> 24);
			Block[3] = (byte)(i >> 16);
			Block[4] = (byte)(i >> 8);
			Block[5] = (byte)i;

			return Block;
		}

		/// <summary>
		/// Decrypts binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to decrypt</param>
		/// <param name="RemoteEndpoint">Remote endpoint of same type.</param>
		/// <returns>Decrypted data</returns>
		public override byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data, IE2eEndpoint RemoteEndpoint)
		{
			if (!(RemoteEndpoint is EcAes256 RemoteEcAes256))
				return null;

			if (Data.Length < 6)
				return null;

			int Signature1Len;
			int Signature2Len;
			int DataLen;

			Signature1Len = Data[0];
			Signature2Len = Data[1];

			DataLen = Data[2];
			DataLen <<= 8;
			DataLen |= Data[3];
			DataLen <<= 8;
			DataLen |= Data[4];
			DataLen <<= 8;
			DataLen |= Data[5];

			if (Data.Length != 6 + Signature1Len + Signature2Len + DataLen)
				return null;

			byte[] Signature1 = new byte[Signature1Len];
			byte[] Signature2 = new byte[Signature2Len];
			byte[] Encrypted = new byte[DataLen];

			int i = 6;
			Array.Copy(Data, i, Signature1, 0, Signature1Len);
			i += Signature1Len;
			Array.Copy(Data, i, Signature2, 0, Signature2Len);
			i += Signature2Len;
			Array.Copy(Data, i, Encrypted, 0, DataLen);
			i += DataLen;

			byte[] Decrypted;
			byte[] Key = GetSharedKey(this, RemoteEcAes256);
			byte[] IV = GetIV(Id, Type, From, To);
			BigInteger r = FromNetwork(Signature1);
			BigInteger s = FromNetwork(Signature2);

			try
			{
				Decrypted = this.Decrypt(Encrypted, Key, IV);

				if (Decrypted != null && RemoteEcAes256.Curve.Verify(Decrypted, RemoteEcAes256.publicKey, HashFunction.SHA256, new KeyValuePair<BigInteger, BigInteger>(r, s)))
					return Decrypted;
			}
			catch (Exception)
			{
				// Invalid key
			}

			if (this.Previous is EcAes256 PrevEcAes256)
			{
				try
				{
					Key = GetSharedKey(PrevEcAes256, RemoteEcAes256);

					if (Key != null)
					{
						Decrypted = this.Decrypt(Encrypted, Key, IV);
						if (Decrypted != null && RemoteEcAes256.Curve.Verify(Decrypted, RemoteEcAes256.publicKey, HashFunction.SHA256, new KeyValuePair<BigInteger, BigInteger>(r, s)))
							return Decrypted;
					}
				}
				catch (Exception)
				{
					// Invalid key
				}
			}

			return null;
		}

		/// <summary>
		/// Encrypts Binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to encrypt</param>
		/// <param name="Xml">XML output</param>
		/// <param name="LocalEndpoint">Local endpoint of same type.</param>
		/// <returns>If encryption was possible</returns>
		public override bool Encrypt(string Id, string Type, string From, string To, byte[] Data, StringBuilder Xml, IE2eEndpoint LocalEndpoint)
		{
			if (!(LocalEndpoint is EcAes256 LocalEcAes256))
				return false;

			byte[] Encrypted;
			byte[] Key = GetSharedKey(LocalEcAes256, this);
			byte[] IV = GetIV(Id, Type, From, To);
			KeyValuePair<BigInteger, BigInteger> Signature;

			Encrypted = this.Encrypt(Data, Key, IV);

			Signature = LocalEcAes256.Curve.Sign(Data, HashFunction.SHA256);

			byte[] Signature1 = ToNetwork(Signature.Key);
			byte[] Signature2 = ToNetwork(Signature.Value);

			Xml.Append("<aes xmlns=\"");
			Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
			Xml.Append("\" ec=\"");
			Xml.Append(this.CurveName);
			Xml.Append("\" ecdsa1=\"");
			Xml.Append(Convert.ToBase64String(Signature1));
			Xml.Append("\" ecdsa2=\"");
			Xml.Append(Convert.ToBase64String(Signature2));
			Xml.Append("\">");
			Xml.Append(Convert.ToBase64String(Encrypted));
			Xml.Append("</aes>");

			return true;
		}

		/// <summary>
		/// Converts a <see cref="BigInteger"/> to network binary representation.
		/// </summary>
		/// <param name="n"><see cref="BigInteger"/> representation</param>
		/// <returns>Network binary representation</returns>
		public static byte[] ToNetwork(BigInteger n)
		{
			byte[] Bin = n.ToByteArray();
			int c = Bin.Length - 1;

			if (Bin[c] == 0)
				Array.Resize<byte>(ref Bin, c);

			Array.Reverse(Bin);

			return Bin;
		}

		/// <summary>
		/// If the scheme can decrypt a given XML element.
		/// </summary>
		/// <param name="AesElement">XML element with encrypted data.</param>
		/// <returns>If the scheme can decrypt the data.</returns>
		public override bool CanDecrypt(XmlElement AesElement)
		{
			if (AesElement.HasAttribute("ec"))
				return AesElement.GetAttribute("ec") == this.CurveName;
			else
				return false;
		}

		/// <summary>
		/// Decrypts XML data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="AesElement">XML element with encrypted data.</param>
		/// <param name="RemoteEndpoint">Remote endpoint of same type.</param>
		/// <returns>Decrypted XMLs</returns>
		public override string Decrypt(string Id, string Type, string From, string To, XmlElement AesElement, IE2eEndpoint RemoteEndpoint)
		{
			if (!(RemoteEndpoint is EcAes256 RemoteEcAes256))
				return null;

			byte[] Encrypted = Convert.FromBase64String(AesElement.InnerText);
			byte[] Decrypted;
			byte[] Key = GetSharedKey(this, RemoteEcAes256);
			byte[] IV = this.GetIV(Id, Type, From, To);
			byte[] Signature1 = Convert.FromBase64String(XML.Attribute(AesElement, "ecdsa1"));
			byte[] Signature2 = Convert.FromBase64String(XML.Attribute(AesElement, "ecdsa2"));
			BigInteger r = FromNetwork(Signature1);
			BigInteger s = FromNetwork(Signature2);

			try
			{
				Decrypted = this.Decrypt(Encrypted, Key, IV);

				if (Decrypted != null && RemoteEcAes256.Curve.Verify(Decrypted, RemoteEcAes256.publicKey, HashFunction.SHA256, new KeyValuePair<BigInteger, BigInteger>(r, s)))
					return Encoding.UTF8.GetString(Decrypted);
			}
			catch (Exception)
			{
				// Invalid key
			}

			if (this.Previous is EcAes256 PrevEcAes256)
			{
				try
				{
					Key = GetSharedKey(PrevEcAes256, RemoteEcAes256);

					if (Key != null)
					{
						Decrypted = this.Decrypt(Encrypted, Key, IV);
						if (Decrypted != null && RemoteEcAes256.Curve.Verify(Decrypted, RemoteEcAes256.publicKey, HashFunction.SHA256, new KeyValuePair<BigInteger, BigInteger>(r, s)))
							return Encoding.UTF8.GetString(Decrypted);
					}
				}
				catch (Exception)
				{
					// Invalid key
				}
			}

			return null;
		}

        /// <summary>
        /// Signs binary data using the local private key.
        /// </summary>
        /// <param name="Data">Binary data</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <returns>Signature (ECDSA) consisting of one or two large integers.</returns>
        public override KeyValuePair<byte[], byte[]> Sign(byte[] Data, HashFunction HashFunction)
		{
			if (!this.hasPrivateKey)
				throw new InvalidOperationException("Signing requires private key.");

			KeyValuePair<BigInteger, BigInteger> Signature = this.curve.Sign(Data, HashFunction);
			byte[] s1 = ToNetwork(Signature.Key);
			byte[] s2 = ToNetwork(Signature.Value);

			return new KeyValuePair<byte[], byte[]>(s1, s2);
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="X">Public key (X-coordinate)</param>
		/// <param name="Y">Public key (Y-coordinate)</param>
		/// <param name="Signature1">First integer in ECDSA signature.</param>
		/// <param name="Signature2">Second integer in ECDSA signature.</param>
		/// <param name="HashFunction">Hash function used in signature calculation.</param>
		/// <returns>If signature is valid.</returns>
		public bool Verify(byte[] Data, byte[] X, byte[] Y, byte[] Signature1,
			byte[] Signature2, HashFunction HashFunction)
		{
			return this.Verify(Data, new PointOnCurve(FromNetwork(X), FromNetwork(Y)), Signature1, Signature2, HashFunction);
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="PublicKey">Public key</param>
		/// <param name="Signature1">First integer in ECDSA signature.</param>
		/// <param name="Signature2">Second integer in ECDSA signature.</param>
		/// <param name="HashFunction">Hash function used in signature calculation.</param>
		/// <returns>If signature is valid.</returns>
		public bool Verify(byte[] Data, PointOnCurve PublicKey, byte[] Signature1, byte[] Signature2, HashFunction HashFunction)
		{
			KeyValuePair<BigInteger, BigInteger> Signature = new KeyValuePair<BigInteger, BigInteger>(
				FromNetwork(Signature1), FromNetwork(Signature2));

			return this.curve.Verify(Data, PublicKey, HashFunction, Signature);
		}

        /// <summary>
        /// Verifies a signature.
        /// </summary>
        /// <param name="Data">Data that is signed.</param>
        /// <param name="Signature1">First integer in ECDSA signature.</param>
        /// <param name="Signature2">Second integer in ECDSA signature.</param>
        /// <param name="HashFunction">Hash function used in signature calculation.</param>
        /// <returns>If signature is valid.</returns>
        public override bool Verify(byte[] Data, byte[] Signature1, byte[] Signature2, HashFunction HashFunction)
        {
            return this.Verify(Data, this.publicKey, Signature1, Signature2, HashFunction);
        }

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override bool Equals(object obj)
		{
			return obj is EcAes256 EcAes256 &&
				this.curve.CurveName.Equals(EcAes256.curve.CurveName) &&
				this.publicKey.X.Equals(EcAes256.publicKey.X) &&
				this.publicKey.Y.Equals(EcAes256.publicKey.Y);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.curve.CurveName.GetHashCode();
			Result ^= Result << 5 ^ this.publicKey.X.GetHashCode();
			Result ^= Result << 5 ^ this.publicKey.Y.GetHashCode();

			return Result;
		}

	}
}
