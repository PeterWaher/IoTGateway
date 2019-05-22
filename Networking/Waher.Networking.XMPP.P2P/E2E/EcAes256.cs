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

			SharedKey = LocalKey.curve.GetSharedKey(RemoteKey.publicKey, Hashes.ComputeSHA256Hash);
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
			byte[] Signature;

			Encrypted = this.Encrypt(Data, Key, IV);
			Signature = LocalEcAes256.Curve.Sign(Data);

			byte[] Block = new byte[Signature.Length + Encrypted.Length + 5];
			int i, j;

			j = 5;

			i = Signature.Length;
			Array.Copy(Signature, 0, Block, j, i);
			j += i;
			Block[0] = (byte)i;

			i = Encrypted.Length;
			Array.Copy(Encrypted, 0, Block, j, i);

            Block[1] = (byte)(i >> 24);
			Block[2] = (byte)(i >> 16);
			Block[3] = (byte)(i >> 8);
			Block[4] = (byte)i;

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

			if (Data.Length < 5)
				return null;

			int SignatureLen;
			int DataLen;

			SignatureLen = Data[0];

			DataLen = Data[1];
			DataLen <<= 8;
			DataLen |= Data[2];
			DataLen <<= 8;
			DataLen |= Data[3];
			DataLen <<= 8;
			DataLen |= Data[4];

			if (Data.Length != 5 + SignatureLen + DataLen)
				return null;

			byte[] Signature = new byte[SignatureLen];
			byte[] Encrypted = new byte[DataLen];

			int i = 5;
			Array.Copy(Data, i, Signature, 0, SignatureLen);
			i += SignatureLen;
			Array.Copy(Data, i, Encrypted, 0, DataLen);

			byte[] Decrypted;
			byte[] Key = GetSharedKey(this, RemoteEcAes256);
			byte[] IV = GetIV(Id, Type, From, To);

			try
			{
				Decrypted = this.Decrypt(Encrypted, Key, IV);

				if (Decrypted != null && RemoteEcAes256.Curve.Verify(Decrypted, RemoteEcAes256.publicKey, Signature))
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
						if (Decrypted != null && RemoteEcAes256.Curve.Verify(Decrypted, RemoteEcAes256.publicKey, Signature))
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
			byte[] Signature;

			Encrypted = this.Encrypt(Data, Key, IV);
			Signature = LocalEcAes256.Curve.Sign(Data);

			Xml.Append("<aes xmlns=\"");
			Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
			Xml.Append("\" ec=\"");
			Xml.Append(this.CurveName);
			Xml.Append("\" s=\"");
			Xml.Append(Convert.ToBase64String(Signature));
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
			byte[] Signature = Convert.FromBase64String(XML.Attribute(AesElement, "s"));

			try
			{
				Decrypted = this.Decrypt(Encrypted, Key, IV);

				if (Decrypted != null && RemoteEcAes256.Curve.Verify(Decrypted, RemoteEcAes256.publicKey, Signature))
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
						if (Decrypted != null && RemoteEcAes256.Curve.Verify(Decrypted, RemoteEcAes256.publicKey, Signature))
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
        /// <returns>Digital signature.</returns>
        public override byte[] Sign(byte[] Data)
		{
			if (!this.hasPrivateKey)
				throw new InvalidOperationException("Signing requires private key.");

			byte[] Signature = this.curve.Sign(Data);

			return Signature;
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="X">Public key (X-coordinate)</param>
		/// <param name="Y">Public key (Y-coordinate)</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public bool Verify(byte[] Data, byte[] X, byte[] Y, byte[] Signature)
		{
			return this.Verify(Data, new PointOnCurve(FromNetwork(X), FromNetwork(Y)), Signature);
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="PublicKey">Public key</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public bool Verify(byte[] Data, PointOnCurve PublicKey, byte[] Signature)
		{
			return this.curve.Verify(Data, PublicKey, Signature);
		}

        /// <summary>
        /// Verifies a signature.
        /// </summary>
        /// <param name="Data">Data that is signed.</param>
        /// <param name="Signature">Digital signature.</param>
        /// <returns>If signature is valid.</returns>
        public override bool Verify(byte[] Data, byte[] Signature)
        {
            return this.Verify(Data, this.publicKey, Signature);
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
