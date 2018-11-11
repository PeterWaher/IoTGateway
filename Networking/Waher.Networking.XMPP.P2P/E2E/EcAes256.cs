//#define DEBUG_AES
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Security;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Abstract base class for Elliptic Curve / AES-256 hybrid ciphers.
	/// </summary>
	public abstract class EcAes256 : Aes256
	{
		/// <summary>
		/// Remote public key.
		/// </summary>
		protected readonly PointOnCurve publicKey;

		/// <summary>
		/// Local security endpoint, if available.
		/// </summary>
		protected readonly EndpointSecurity localEndpoint;

		private CurvePrimeField lastCurve = null;
		private CurvePrimeField lastCurveOld = null;
		private byte[] sharedKey = null;
		private byte[] sharedKeyOld = null;

		/// <summary>
		/// Abstract base class for Elliptic Curve / AES-256 hybrid ciphers.s
		/// </summary>
		/// <param name="X">X-coordinate of remote public key.</param>
		/// <param name="Y">Y-coordinate of remote public key.</param>
		/// <param name="LocalEndpoint">Local security endpoint, if available.</param>
		public EcAes256(byte[] X, byte[] Y, EndpointSecurity LocalEndpoint)
			: base()
		{
			this.publicKey = new PointOnCurve(FromNetwork(X), FromNetwork(Y));
			this.localEndpoint = LocalEndpoint;
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
		public abstract string CurveName
		{
			get;
		}

		/// <summary>
		/// Elliptic Curve
		/// </summary>
		protected abstract CurvePrimeField Curve
		{
			get;
		}

		/// <summary>
		/// Previous Elliptic Curve
		/// </summary>
		protected abstract CurvePrimeField PrevCurve
		{
			get;
		}

		/// <summary>
		/// Shared secret, for underlying AES cipher.
		/// </summary>
		public byte[] Key
		{
			get
			{
				if (this.sharedKey == null || this.Curve != this.lastCurve)
				{
					this.sharedKey = this.Curve.GetSharedKey(this.publicKey, HashFunction.SHA256);
					this.lastCurve = this.Curve;
				}

				return this.sharedKey;
			}
		}

		/// <summary>
		/// Previous shared secret, for underlying AES cipher.
		/// </summary>
		public byte[] OldKey
		{
			get
			{
				if (this.sharedKeyOld == null || this.PrevCurve != this.lastCurveOld)
				{
					this.sharedKeyOld = this.PrevCurve?.GetSharedKey(this.publicKey, HashFunction.SHA256);
					this.lastCurveOld = this.PrevCurve;
				}

				return this.sharedKeyOld;
			}
		}

		/// <summary>
		/// Encrypts binary data
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data to encrypt</param>
		/// <returns>Encrypted data</returns>
		public override byte[] Encrypt(string Id, string Type, string From, string To, byte[] Data)
		{
			byte[] Encrypted;
			byte[] Key = this.Key;
			byte[] IV = GetIV(Id, Type, From, To);
			KeyValuePair<BigInteger, BigInteger> Signature;

			Encrypted = this.Encrypt(Data, Key, IV);

			Signature = this.Curve.Sign(Data, HashFunction.SHA256);

#if DEBUG_AES
			Log.Notice("Encrypting.",
				new KeyValuePair<string, object>("Id", Id),
				new KeyValuePair<string, object>("Type", Type),
				new KeyValuePair<string, object>("To", To),
				new KeyValuePair<string, object>("Local.X: ", this.Curve.PublicKey.X.ToString()),
				new KeyValuePair<string, object>("Local.Y: ", this.Curve.PublicKey.Y.ToString()),
				new KeyValuePair<string, object>("Remote.X: ", this.publicKey.X.ToString()),
				new KeyValuePair<string, object>("Remote.Y: ", this.publicKey.Y.ToString()),
				new KeyValuePair<string, object>("Key: ", Convert.ToBase64String(Key)),
				new KeyValuePair<string, object>("IV: ", Convert.ToBase64String(IV)),
				new KeyValuePair<string, object>("Signature1: ", Signature.Key.ToString()),
				new KeyValuePair<string, object>("Signature2: ", Signature.Value.ToString()));
#endif

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
		/// <returns>Decrypted data</returns>
		public override byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data)
		{
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
			byte[] Key = this.Key;
			byte[] IV = GetIV(Id, Type, From, To);
			BigInteger r = FromNetwork(Signature1);
			BigInteger s = FromNetwork(Signature2);

#if DEBUG_AES
			Log.Notice("Decrypting.",
				new KeyValuePair<string, object>("Id", Id),
				new KeyValuePair<string, object>("Type", Type),
				new KeyValuePair<string, object>("To", To),
				new KeyValuePair<string, object>("Local.X: ", this.Curve.PublicKey.X.ToString()),
				new KeyValuePair<string, object>("Local.Y: ", this.Curve.PublicKey.Y.ToString()),
				new KeyValuePair<string, object>("Remote.X: ", this.publicKey.X.ToString()),
				new KeyValuePair<string, object>("Remote.Y: ", this.publicKey.Y.ToString()),
				new KeyValuePair<string, object>("Key: ", Convert.ToBase64String(Key)),
				new KeyValuePair<string, object>("IV: ", Convert.ToBase64String(IV)),
				new KeyValuePair<string, object>("Signature1: ", r.ToString()),
				new KeyValuePair<string, object>("Signature2: ", s.ToString()));
#endif

			try
			{
				Decrypted = this.Decrypt(Encrypted, Key, IV);

				if (Decrypted != null && !this.Curve.Verify(Decrypted, this.publicKey, HashFunction.SHA256, new KeyValuePair<BigInteger, BigInteger>(r, s)))
					Decrypted = null;
			}
			catch (Exception)
			{
				Decrypted = null;
			}

			if (Decrypted == null)
			{
				try
				{
					Key = this.OldKey;

#if DEBUG_AES
					Log.Notice("Decrypting (old key).",
						new KeyValuePair<string, object>("Old.Local.X: ", this.PrevCurve.PublicKey.X.ToString()),
						new KeyValuePair<string, object>("Old.Local.Y: ", this.PrevCurve.PublicKey.Y.ToString()),
						new KeyValuePair<string, object>("Old.Key: ", Convert.ToBase64String(Key)));
#endif

					if (Key != null)
					{
						Decrypted = this.Decrypt(Encrypted, Key, IV);
						if (Decrypted == null)
							return null;
						else if (!this.PrevCurve.Verify(Decrypted, this.publicKey, HashFunction.SHA256, new KeyValuePair<BigInteger, BigInteger>(r, s)))
							return null;
					}
					else
						return null;
				}
				catch (Exception)
				{
					return null;    // Invalid keys.
				}
			}

			return Decrypted;
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
		/// <returns>If encryption was possible</returns>
		public override bool Encrypt(string Id, string Type, string From, string To, byte[] Data, StringBuilder Xml)
		{
			byte[] Encrypted;
			byte[] Key = this.Key;
			byte[] IV = GetIV(Id, Type, From, To);
			KeyValuePair<BigInteger, BigInteger> Signature;

			Encrypted = this.Encrypt(Data, Key, IV);
			Signature = this.Curve.Sign(Data, HashFunction.SHA256);

#if DEBUG_AES
			Log.Notice("Encrypting.",
				new KeyValuePair<string, object>("Id", Id),
				new KeyValuePair<string, object>("Type", Type),
				new KeyValuePair<string, object>("To", To),
				new KeyValuePair<string, object>("Local.X: ", this.Curve.PublicKey.X.ToString()),
				new KeyValuePair<string, object>("Local.Y: ", this.Curve.PublicKey.Y.ToString()),
				new KeyValuePair<string, object>("Remote.X: ", this.publicKey.X.ToString()),
				new KeyValuePair<string, object>("Remote.Y: ", this.publicKey.Y.ToString()),
				new KeyValuePair<string, object>("Key: ", Convert.ToBase64String(Key)),
				new KeyValuePair<string, object>("IV: ", Convert.ToBase64String(IV)),
				new KeyValuePair<string, object>("Data: ", Encoding.UTF8.GetString(Data)),
				new KeyValuePair<string, object>("Encrypted: ", Convert.ToBase64String(Encrypted)),
				new KeyValuePair<string, object>("Signature1: ", Signature.Key.ToString()),
				new KeyValuePair<string, object>("Signature2: ", Signature.Value.ToString()));
#endif
			byte[] Signature1 = ToNetwork(Signature.Key);
			byte[] Signature2 = ToNetwork(Signature.Value);

			Xml.Append("<aes xmlns='");
			Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
			Xml.Append("' ec='");
			Xml.Append(this.CurveName);
			Xml.Append("' ecdsa1='");
			Xml.Append(Convert.ToBase64String(Signature1));
			Xml.Append("' ecdsa2='");
			Xml.Append(Convert.ToBase64String(Signature2));
			Xml.Append("'>");
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
		/// <returns>Decrypted XMLs</returns>
		public override string Decrypt(string Id, string Type, string From, string To, XmlElement AesElement)
		{
			byte[] Encrypted = Convert.FromBase64String(AesElement.InnerText);
			byte[] Decrypted;
			byte[] Key = this.Key;
			byte[] IV = this.GetIV(Id, Type, From, To);
			byte[] Signature1 = Convert.FromBase64String(XML.Attribute(AesElement, "ecdsa1"));
			byte[] Signature2 = Convert.FromBase64String(XML.Attribute(AesElement, "ecdsa2"));
			BigInteger r = FromNetwork(Signature1);
			BigInteger s = FromNetwork(Signature2);


#if DEBUG_AES
			Log.Notice("Decrypting.",
				new KeyValuePair<string, object>("Id", Id),
				new KeyValuePair<string, object>("Type", Type),
				new KeyValuePair<string, object>("To", To),
				new KeyValuePair<string, object>("Local.X: ", this.Curve.PublicKey.X.ToString()),
				new KeyValuePair<string, object>("Local.Y: ", this.Curve.PublicKey.Y.ToString()),
				new KeyValuePair<string, object>("Remote.X: ", this.publicKey.X.ToString()),
				new KeyValuePair<string, object>("Remote.Y: ", this.publicKey.Y.ToString()),
				new KeyValuePair<string, object>("Key: ", Convert.ToBase64String(Key)),
				new KeyValuePair<string, object>("IV: ", Convert.ToBase64String(IV)),
				new KeyValuePair<string, object>("Encrypted: ", Convert.ToBase64String(Encrypted)),
				new KeyValuePair<string, object>("Signature1: ", r.ToString()),
				new KeyValuePair<string, object>("Signature2: ", s.ToString()));
#endif
			try
			{
				Decrypted = this.Decrypt(Encrypted, Key, IV);

				if (Decrypted != null && !this.Curve.Verify(Decrypted, this.publicKey, HashFunction.SHA256, new KeyValuePair<BigInteger, BigInteger>(r, s)))
					Decrypted = null;
			}
			catch (Exception)
			{
				Decrypted = null;
			}

			if (Decrypted == null)
			{
				try
				{
					Key = this.OldKey;

#if DEBUG_AES
					Log.Notice("Decrypting (old key).",
						new KeyValuePair<string, object>("Old.Local.X: ", this.PrevCurve.PublicKey.X.ToString()),
						new KeyValuePair<string, object>("Old.Local.Y: ", this.PrevCurve.PublicKey.Y.ToString()),
						new KeyValuePair<string, object>("Old.Key: ", Convert.ToBase64String(Key)));
#endif
					if (Key != null)
					{
						Decrypted = this.Decrypt(Encrypted, Key, IV);
						if (Decrypted == null)
							return null;
						else if (!this.PrevCurve.Verify(Decrypted, this.publicKey, HashFunction.SHA256, new KeyValuePair<BigInteger, BigInteger>(r, s)))
							return null;
					}
					else
						return null;
				}
				catch (Exception)
				{
					return null;    // Invalid keys.
				}
			}

#if DEBUG_AES
			Log.Notice("Decrypted: " + Encoding.UTF8.GetString(Decrypted));
#endif

			return Encoding.UTF8.GetString(Decrypted);
		}

	}
}
