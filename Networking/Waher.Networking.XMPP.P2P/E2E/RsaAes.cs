using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Security;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// RSA / AES-256 hybrid cipher.
	/// </summary>
	public class RsaAes : Aes256
	{
		private readonly EndpointSecurity localEndpoint;
		private RSA rsa;
		private readonly byte[] modulus;
		private readonly byte[] exponent;
		private readonly int keySize;

		/// <summary>
		/// RSA / AES-256 hybrid cipher.
		/// </summary>
		/// <param name="KeySize">Size of key</param>
		/// <param name="Modulus">Modulus of RSA public key.</param>
		/// <param name="Exponent">Exponent of RSA public key.</param>
		/// <param name="LocalEndpoint">Local security endpoint, if available.</param>
		public RsaAes(int KeySize, byte[] Modulus, byte[] Exponent, EndpointSecurity LocalEndpoint)
			: base()
		{
			this.rsa = RSA.Create();
			this.rsa.KeySize = KeySize;

			this.keySize = KeySize;
			this.modulus = Modulus;
			this.exponent = Exponent;
			this.localEndpoint = LocalEndpoint;

			RSAParameters Param = new RSAParameters()
			{
				Modulus = Modulus,
				Exponent = Exponent
			};

			this.rsa.ImportParameters(Param);
		}

		/// <summary>
		/// Size of key
		/// </summary>
		public int KeySize => this.keySize;

		/// <summary>
		/// Modulus of RSA public key.
		/// </summary>
		public byte[] Modulus => this.modulus;

		/// <summary>
		/// Exponent of RSA public key.
		/// </summary>
		public byte[] Exponent => this.exponent;

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength
		{
			get
			{
				if (this.keySize < 1024)
					return 0;
				else if (this.keySize < 2048)
					return 80;
				else if (this.keySize < 3072)
					return 112;
				else if (this.keySize < 7680)
					return 128;
				else if (this.keySize < 15360)
					return 192;
				else 
					return 256;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.rsa?.Dispose();
			this.rsa = null;
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
			byte[] Result;
			byte[] KeyEncrypted;
			byte[] Signature;
			byte[] IV = GetIV(Id, Type, From, To);

			lock (this.rsa)
			{
				this.aes.GenerateKey();
				this.aes.IV = IV;

				KeyEncrypted = this.rsa.Encrypt(this.aes.Key, RSAEncryptionPadding.OaepSHA256);
				Result = this.Encrypt(Data, this.aes.Key, IV);
			}

			Signature = this.localEndpoint.SignRsa(Data);

			byte[] Block = new byte[KeyEncrypted.Length + Signature.Length + Result.Length + 8];
			int i, j;

			j = 8;
			i = KeyEncrypted.Length;
			Array.Copy(KeyEncrypted, 0, Block, j, i);
			j += i;
			Block[0] = (byte)(i >> 8);
			Block[1] = (byte)i;

			i = Signature.Length;
			Array.Copy(Signature, 0, Block, j, i);
			j += i;
			Block[2] = (byte)(i >> 8);
			Block[3] = (byte)i;

			i = Result.Length;
			Array.Copy(Result, 0, Block, j, i);
			j += i;
			Block[4] = (byte)(i >> 24);
			Block[5] = (byte)(i >> 16);
			Block[6] = (byte)(i >> 8);
			Block[7] = (byte)i;

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
			if (Data.Length < 10)
				return null;

			int KeyLen;
			int SignatureLen;
			int DataLen;

			KeyLen = Data[0];
			KeyLen <<= 8;
			KeyLen |= Data[1];

			SignatureLen = Data[2];
			SignatureLen <<= 8;
			SignatureLen |= Data[3];

			DataLen = Data[4];
			DataLen <<= 8;
			DataLen |= Data[5];
			DataLen <<= 8;
			DataLen |= Data[6];
			DataLen <<= 8;
			DataLen |= Data[7];

			if (Data.Length != 8 + KeyLen + SignatureLen + DataLen)
				return null;

			byte[] KeyEncrypted = new byte[KeyLen];
			byte[] Signature = new byte[SignatureLen];
			byte[] Encrypted = new byte[DataLen];

			int i = 10;
			Array.Copy(Data, i, KeyEncrypted, 0, KeyLen);
			i += KeyLen;
			Array.Copy(Data, i, Signature, 0, SignatureLen);
			i += SignatureLen;
			Array.Copy(Data, i, Encrypted, 0, DataLen);
			i += DataLen;

			byte[] Decrypted;
			byte[] Key;
			byte[] IV = GetIV(Id, Type, From, To);

			try
			{
				Key = this.localEndpoint.DecryptRsa(KeyEncrypted);
				Decrypted = this.Decrypt(Encrypted, Key, IV);
			}
			catch (Exception)
			{
				Decrypted = null;
			}

			if (Decrypted == null)
			{
				try
				{
					Key = this.localEndpoint.DecryptOldRsa(KeyEncrypted);

					if (Key != null && IV != null)
					{
						Decrypted = this.Decrypt(Encrypted, Key, IV);
						if (Decrypted == null)
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

			lock (this.rsa)
			{
				if (!this.rsa.VerifyData(Decrypted, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
					return null;
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
			byte[] Result;
			byte[] KeyEncrypted;
			byte[] Signature;
			byte[] IV = GetIV(Id, Type, From, To);

			lock (this.rsa)
			{
				this.aes.GenerateKey();
				this.aes.IV = IV;

				KeyEncrypted = this.rsa.Encrypt(this.aes.Key, RSAEncryptionPadding.OaepSHA256);
				Result = this.Encrypt(Data, this.aes.Key, IV);
			}

			Signature = this.localEndpoint.SignRsa(Data);

			Xml.Append("<aes xmlns='");
			Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
			Xml.Append("' keyRsa='");
			Xml.Append(Convert.ToBase64String(KeyEncrypted));
			Xml.Append("' signRsa='");
			Xml.Append(Convert.ToBase64String(Signature));
			Xml.Append("'>");
			Xml.Append(Convert.ToBase64String(Result));
			Xml.Append("</aes>");

			return true;
		}

		/// <summary>
		/// If the scheme can decrypt a given XML element.
		/// </summary>
		/// <param name="AesElement">XML element with encrypted data.</param>
		/// <returns>If the scheme can decrypt the data.</returns>
		public override bool CanDecrypt(XmlElement AesElement)
		{
			return AesElement.HasAttribute("keyRsa");
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
			byte[] KeyEncrypted = Convert.FromBase64String(XML.Attribute(AesElement, "keyRsa"));
			byte[] Signature = Convert.FromBase64String(XML.Attribute(AesElement, "signRsa"));
			byte[] Encrypted = Convert.FromBase64String(AesElement.InnerText);
			byte[] Decrypted;
			byte[] Key;
			byte[] IV = this.GetIV(Id, Type, From, To);

			try
			{
				Key = this.localEndpoint.DecryptRsa(KeyEncrypted);
				Decrypted = this.Decrypt(Encrypted, Key, IV);

				if (Decrypted != null)
				{
					lock (this.rsa)
					{
						if (!this.rsa.VerifyData(Decrypted, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
							Decrypted = null;
					}
				}
			}
			catch (Exception)
			{
				Decrypted = null;
			}

			if (Decrypted == null)
			{
				try
				{
					Key = this.localEndpoint.DecryptOldRsa(KeyEncrypted);

					if (Key != null)
					{
						Decrypted = this.Decrypt(Encrypted, Key, IV);
						if (Decrypted == null)
							return null;

						lock (this.rsa)
						{
							if (!this.rsa.VerifyData(Decrypted, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
								return null;
						}
					}
					else
						return null;
				}
				catch (Exception)
				{
					return null;    // Invalid keys.
				}
			}

			return Encoding.UTF8.GetString(Decrypted);
		}

	}
}
