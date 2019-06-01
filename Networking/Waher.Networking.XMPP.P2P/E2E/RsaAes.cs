using System;
using System.Collections.Generic;
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
        private RSA rsa;
        private readonly byte[] modulus;
        private readonly byte[] exponent;
        private readonly int keySize;
        private readonly string modulusBase64;
        private readonly string exponentBase64;

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        public RsaAes()
            : this(RSA.Create())
        {
        }

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        /// <param name="Rsa">RSA</param>
        public RsaAes(RSA Rsa)
            : base()
        {
            this.rsa = Rsa;

            RSAParameters P = this.rsa.ExportParameters(false);

            this.keySize = this.rsa.KeySize;
            this.modulus = P.Modulus;
            this.exponent = P.Exponent;

            this.modulusBase64 = Convert.ToBase64String(P.Modulus);
            this.exponentBase64 = Convert.ToBase64String(P.Exponent);
        }

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        /// <param name="KeySize">Size of key</param>
        /// <param name="Modulus">Modulus of RSA public key.</param>
        /// <param name="Exponent">Exponent of RSA public key.</param>
        public RsaAes(int KeySize, byte[] Modulus, byte[] Exponent)
            : base()
        {
            this.rsa = RSA.Create();
            this.rsa.KeySize = KeySize;

            this.keySize = KeySize;
            this.modulus = Modulus;
            this.exponent = Exponent;
            this.modulusBase64 = Convert.ToBase64String(Modulus);
            this.exponentBase64 = Convert.ToBase64String(Exponent);

            RSAParameters Param = new RSAParameters()
            {
                Modulus = Modulus,
                Exponent = Exponent
            };

            this.rsa.ImportParameters(Param);
        }

        /// <summary>
        /// Local name of the E2E encryption scheme
        /// </summary>
        public override string LocalName => "rsa";

        /// <summary>
        /// Size of key
        /// </summary>
        public int KeySize => this.keySize;

        /// <summary>
        /// Remote public key.
        /// </summary>
        public override byte[] PublicKey
        {
            get
            {
                int c = this.modulus.Length;
                int d = this.exponent.Length;
                byte[] Result = new byte[c + d];

                Array.Copy(this.modulus, 0, Result, 0, c);
                Array.Copy(this.exponent, 0, Result, c, d);

                return Result;
            }
        }

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
        /// Creates a new key.
        /// </summary>
        /// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
        /// <returns>New E2E endpoint.</returns>
        public override IE2eEndpoint Create(int SecurityStrength)
        {
            int KeySize;

            if (SecurityStrength <= 80)
                KeySize = 1024;
            else if (SecurityStrength <= 112)
                KeySize = 2048;
            else if (SecurityStrength <= 128)
                KeySize = 3072;
            else if (SecurityStrength <= 192)
                KeySize = 7680;
            else if (SecurityStrength <= 256)
                KeySize = 15360;
            else
                throw new ArgumentException("Key strength too high.", nameof(SecurityStrength));

            RSA Rsa = RSA.Create();
            Rsa.KeySize = KeySize;

            return new RsaAes(Rsa);
        }

        /// <summary>
        /// Parses endpoint information from an XML element.
        /// </summary>
        /// <param name="Xml">XML element.</param>
        /// <returns>Parsed key information, if possible, null if XML is not well-defined.</returns>
        public override IE2eEndpoint Parse(XmlElement Xml)
        {
            int? KeySize = null;
            byte[] Modulus = null;
            byte[] Exponent = null;

            foreach (XmlAttribute Attr in Xml.Attributes)
            {
                switch (Attr.Name)
                {
                    case "size":
                        if (int.TryParse(Attr.Value, out int i))
                            KeySize = i;
                        else
                            return null;
                        break;

                    case "mod":
                        Modulus = Convert.FromBase64String(Attr.Value);
                        break;

                    case "exp":
                        Exponent = Convert.FromBase64String(Attr.Value);
                        break;
                }
            }

            if (KeySize.HasValue && Modulus != null && Exponent != null)
                return new RsaAes(KeySize.Value, Modulus, Exponent);
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
            Xml.Append(" xmlns=\"");
            Xml.Append(this.Namespace);
            Xml.Append("\" size=\"");
            Xml.Append(this.keySize.ToString());
            Xml.Append("\" mod=\"");
            Xml.Append(this.modulusBase64);
            Xml.Append("\" exp=\"");
            Xml.Append(this.exponentBase64);
            Xml.Append("\"/>");
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
        /// <param name="LocalEndpoint">Local endpoint of same type.</param>
        /// <returns>Encrypted data</returns>
        public override byte[] Encrypt(string Id, string Type, string From, string To, byte[] Data, IE2eEndpoint LocalEndpoint)
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

            Signature = (LocalEndpoint as RsaAes)?.Sign(Data);
            if (Signature is null)
                return null;

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
        /// <param name="RemoteEndpoint">Remote endpoint of same type.</param>
        /// <returns>Decrypted data</returns>
        public override byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data, IE2eEndpoint RemoteEndpoint)
        {
            if (!(RemoteEndpoint is RsaAes RemoteRsaAes))
                return null;

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

            if (Data.Length != 10 + KeyLen + SignatureLen + DataLen)
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

            byte[] Decrypted;
            byte[] Key;
            byte[] IV = GetIV(Id, Type, From, To);

            try
            {
                Key = this.Decrypt(KeyEncrypted);
                Decrypted = this.Decrypt(Encrypted, Key, IV);
            }
            catch (Exception)
            {
                Decrypted = null;
            }

            if (Decrypted is null)
            {
                try
                {
                    Key = (this.Previous as RsaAes)?.Decrypt(KeyEncrypted);

                    if (Key != null && IV != null)
                    {
                        Decrypted = this.Decrypt(Encrypted, Key, IV);
                        if (Decrypted is null)
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

            lock (RemoteRsaAes.rsa)
            {
                if (!RemoteRsaAes.rsa.VerifyData(Decrypted, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
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
        /// <param name="LocalEndpoint">Local endpoint of same type.</param>
        /// <returns>If encryption was possible</returns>
        public override bool Encrypt(string Id, string Type, string From, string To, byte[] Data, StringBuilder Xml, IE2eEndpoint LocalEndpoint)
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

            Signature = (LocalEndpoint as RsaAes)?.Sign(Data);
            if (Signature is null)
                return false;

            Xml.Append("<aes xmlns=\"");
            Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
            Xml.Append("\" keyRsa=\"");
            Xml.Append(Convert.ToBase64String(KeyEncrypted));
            Xml.Append("\" signRsa=\"");
            Xml.Append(Convert.ToBase64String(Signature));
            Xml.Append("\">");
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
        /// <param name="RemoteEndpoint">Remote endpoint of same type.</param>
        /// <returns>Decrypted XMLs</returns>
        public override string Decrypt(string Id, string Type, string From, string To, XmlElement AesElement, IE2eEndpoint RemoteEndpoint)
        {
            if (!(RemoteEndpoint is RsaAes RemoteRsaAes))
                return null;

            byte[] KeyEncrypted = Convert.FromBase64String(XML.Attribute(AesElement, "keyRsa"));
            byte[] Signature = Convert.FromBase64String(XML.Attribute(AesElement, "signRsa"));
            byte[] Encrypted = Convert.FromBase64String(AesElement.InnerText);
            byte[] Decrypted;
            byte[] Key;
            byte[] IV = this.GetIV(Id, Type, From, To);

            try
            {
                Key = this.Decrypt(KeyEncrypted);
                Decrypted = this.Decrypt(Encrypted, Key, IV);

                if (Decrypted != null)
                {
                    lock (RemoteRsaAes.rsa)
                    {
                        if (!RemoteRsaAes.rsa.VerifyData(Decrypted, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
                            Decrypted = null;
                    }
                }
            }
            catch (Exception)
            {
                Decrypted = null;
            }

            if (Decrypted is null)
            {
                try
                {
                    Key = (this.Previous as RsaAes)?.Decrypt(KeyEncrypted);

                    if (Key != null)
                    {
                        Decrypted = this.Decrypt(Encrypted, Key, IV);
                        if (Decrypted is null)
                            return null;

                        lock (RemoteRsaAes.rsa)
                        {
                            if (!RemoteRsaAes.rsa.VerifyData(Decrypted, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
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

        /// <summary>
        /// Signs binary data using the local private key.
        /// </summary>
        /// <param name="Data">Binary data</param>
        /// <returns>Digital signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            return this.rsa.SignData(Data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }

        /// <summary>
        /// Verifies a signature.
        /// </summary>
        /// <param name="Data">Data that is signed.</param>
        /// <param name="Signature">Digital signature.</param>
        /// <returns>If signature is valid.</returns>
        public override bool Verify(byte[] Data, byte[] Signature)
        {
            return RsaAes.Verify(Data, Signature, this.keySize, this.modulus, this.exponent);
        }

        /// <summary>
        /// Verifies a signature.
        /// </summary>
        /// <param name="Data">Data that is signed.</param>
        /// <param name="Signature">Signature</param>
        /// <param name="KeySize">RSA key size</param>
        /// <param name="PublicKey">Public key.</param>
        /// <returns></returns>
        public static bool Verify(byte[] Data, byte[] Signature, int KeySize, byte[] PublicKey)
        {
            int c = KeySize >> 3;
            int d = PublicKey.Length - c;
            if (d <= 0)
                throw new ArgumentException("Invalid public key.", nameof(PublicKey));

            byte[] Modulus = new byte[c];
            byte[] Exponent = new byte[d];

            Array.Copy(PublicKey, 0, Modulus, 0, c);
            Array.Copy(PublicKey, c, Exponent, 0, d);

            return Verify(Data, Signature, KeySize, Modulus, Exponent);
        }

        /// <summary>
        /// Verifies a signature.
        /// </summary>
        /// <param name="Data">Data that is signed.</param>
        /// <param name="Signature">Signature</param>
        /// <param name="KeySize">RSA key size</param>
        /// <param name="Modulus">Modulus</param>
        /// <param name="Exponent">Exponent</param>
        /// <returns></returns>
        public static bool Verify(byte[] Data, byte[] Signature, int KeySize, byte[] Modulus,
            byte[] Exponent)
        {
            using (RSA Rsa = RSA.Create())
            {
                Rsa.KeySize = KeySize;

                RSAParameters P = new RSAParameters()
                {
                    Modulus = Modulus,
                    Exponent = Exponent
                };

                Rsa.ImportParameters(P);

                return Rsa.VerifyData(Data, Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            }
        }

        /// <summary>
        /// Decrypts a key using the local private RSA key.
        /// </summary>
        /// <param name="Key">Encrypted key</param>
        /// <returns>Decrypted key</returns>
        public byte[] Decrypt(byte[] Key)
        {
            return this.rsa.Decrypt(Key, RSAEncryptionPadding.OaepSHA256);
        }

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is RsaAes RsaAes &&
                this.keySize.Equals(RsaAes.keySize) &&
                this.modulusBase64.Equals(RsaAes.modulusBase64) &&
                this.exponentBase64.Equals(RsaAes.exponentBase64);
        }

        /// <summary>
        /// <see cref="Object.GetHashCode()"/>
        /// </summary>
        public override int GetHashCode()
        {
            int Result = this.keySize.GetHashCode();
            Result ^= Result << 5 ^ this.modulusBase64.GetHashCode();
            Result ^= Result << 5 ^ this.exponentBase64.GetHashCode();

            return Result;
        }

    }
}
