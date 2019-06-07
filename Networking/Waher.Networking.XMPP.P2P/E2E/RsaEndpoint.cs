using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;

namespace Waher.Networking.XMPP.P2P.E2E
{
    /// <summary>
    /// RSA / AES-256 hybrid cipher.
    /// </summary>
    public class RsaEndpoint : E2eEndpoint
    {
        private RSA rsa;
        private readonly byte[] modulus;
        private readonly byte[] exponent;
        private readonly int keySize;
        private byte[] publicKey;
        private string publicKeyBase64;
        private string modulusBase64;
        private string exponentBase64;

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        public RsaEndpoint()
            : this(4096)
        {
        }

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        /// <param name="KeySize">Size of key</param>
        public RsaEndpoint(int KeySize)
            : this(CreateRSA(KeySize))
        {
        }

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        /// <param name="KeySize">Size of key</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public RsaEndpoint(int KeySize, IE2eSymmetricCipher SymmetricCipher)
            : this(CreateRSA(KeySize), SymmetricCipher)
        {
        }

        private static RSA CreateRSA(int KeySize)
        {
            RSA Result = RSA.Create();

            if (Result.KeySize != KeySize)
                Result.KeySize = KeySize;

            return Result;
        }

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        /// <param name="Rsa">RSA</param>
        public RsaEndpoint(RSA Rsa)
            : this(Rsa, new Aes256())
        {
        }

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        /// <param name="Rsa">RSA</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public RsaEndpoint(RSA Rsa, IE2eSymmetricCipher SymmetricCipher)
            : base(SymmetricCipher)
        {
            this.rsa = Rsa;

            RSAParameters P = this.rsa.ExportParameters(false);

            this.keySize = this.rsa.KeySize;
            this.modulus = P.Modulus;
            this.exponent = P.Exponent;

            this.Init();
        }

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        /// <param name="KeySize">Size of key</param>
        /// <param name="Modulus">Modulus of RSA public key.</param>
        /// <param name="Exponent">Exponent of RSA public key.</param>
        public RsaEndpoint(int KeySize, byte[] Modulus, byte[] Exponent)
            : this(KeySize, Modulus, Exponent, new Aes256())
        {
        }

        /// <summary>
        /// RSA / AES-256 hybrid cipher.
        /// </summary>
        /// <param name="KeySize">Size of key</param>
        /// <param name="Modulus">Modulus of RSA public key.</param>
        /// <param name="Exponent">Exponent of RSA public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public RsaEndpoint(int KeySize, byte[] Modulus, byte[] Exponent,
            IE2eSymmetricCipher SymmetricCipher)
            : base(SymmetricCipher)
        {
            this.rsa = CreateRSA(KeySize);

            this.keySize = KeySize;
            this.modulus = Modulus;
            this.exponent = Exponent;

            RSAParameters Param = new RSAParameters()
            {
                Modulus = Modulus,
                Exponent = Exponent
            };

            this.rsa.ImportParameters(Param);

            this.Init();
        }

        private void Init()
        {
            this.modulusBase64 = Convert.ToBase64String(this.modulus);
            this.exponentBase64 = Convert.ToBase64String(this.exponent);

            int c = this.modulus.Length;
            int d = this.exponent.Length;

            this.publicKey = new byte[2 + c + d];

            this.publicKey[0] = (byte)(this.keySize);
            this.publicKey[1] = (byte)(this.keySize >> 8);
            Array.Copy(this.modulus, 0, this.publicKey, 2, c);
            Array.Copy(this.exponent, 0, this.publicKey, c + 2, d);

            this.publicKeyBase64 = Convert.ToBase64String(this.publicKey);
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
        public override byte[] PublicKey => this.publicKey;

        /// <summary>
        /// Remote public key, as a Base64 string.
        /// </summary>
        public override string PublicKeyBase64 => this.publicKeyBase64;

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

            RSA Rsa = CreateRSA(KeySize);

            return new RsaEndpoint(Rsa, this.DefaultSymmetricCipher.CreteNew());
        }

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
        {
            RSAParameters P = new RSAParameters();
            string s = Encoding.ASCII.GetString(Secret);
            string[] Parts = s.Split(',');
            string Name;
            byte[] Value;
            int i;

            foreach (string Part in Parts)
            {
                i = Part.IndexOf('=');
                if (i < 0)
                    continue;

                Name = Part.Substring(0, i);
                Value = Convert.FromBase64String(Part.Substring(i + 1));

                switch (Name)
                {
                    case "D": P.D = Value; break;
                    case "DP": P.DP = Value; break;
                    case "DQ": P.DQ = Value; break;
                    case "Exponent": P.Exponent = Value; break;
                    case "InverseQ": P.InverseQ = Value; break;
                    case "Modulus": P.Modulus = Value; break;
                    case "P": P.P = Value; break;
                    case "Q": P.Q = Value; break;
                }
            }

            RSA Rsa = RSA.Create();
            Rsa.ImportParameters(P);

            return new RsaEndpoint(Rsa, this.DefaultSymmetricCipher.CreteNew());
        }

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
        {
            if (PublicKey.Length < 2)
                throw new ArgumentException("Invalid public key.", nameof(PublicKey));

            int KeySize = PublicKey[1];
            KeySize <<= 8;
            KeySize |= PublicKey[0];

            int ModSize = KeySize >> 3;
            if (PublicKey.Length < 2 + ModSize)
                throw new ArgumentException("Invalid public key.", nameof(PublicKey));

            int ExpSize = PublicKey.Length - 2 - ModSize;
            if (ExpSize <= 0)
                throw new ArgumentException("Invalid public key.", nameof(PublicKey));

            byte[] Modulus = new byte[ModSize];
            byte[] Exponent = new byte[ExpSize];

            Array.Copy(PublicKey, 2, Modulus, 0, ModSize);
            Array.Copy(PublicKey, 2 + ModSize, Exponent, 0, ExpSize);

            return new RsaEndpoint(KeySize, Modulus, Exponent, this.DefaultSymmetricCipher.CreteNew());
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
        /// If shared secrets can be calculated from the endpoints keys.
        /// </summary>
        public override bool SupportsSharedSecrets => false;

        /// <summary>
        /// Gets a shared secret
        /// </summary>
        /// <param name="RemoteEndpoint">Remote endpoint</param>
        /// <returns>Shared secret.</returns>
        public override byte[] GetSharedSecret(IE2eEndpoint RemoteEndpoint)
        {
            throw new NotSupportedException("Getting shared secrets using RSA not supported.");
        }

        /// <summary>
        /// Encrypts a secret. Used if shared secrets cannot be calculated.
        /// </summary>
        /// <param name="Secret">Secret</param>
        /// <returns>Encrypted secret.</returns>
        public override byte[] EncryptSecret(byte[] Secret)
        {
            lock (this.rsa)
            {
                return this.rsa.Encrypt(Secret, RSAEncryptionPadding.OaepSHA256);
            }
        }

        /// <summary>
        /// Decrypts a secret. Used if shared secrets cannot be calculated.
        /// </summary>
        /// <param name="Secret">Encrypted secret</param>
        /// <returns>Decrypted secret.</returns>
        public override byte[] DecryptSecret(byte[] Secret)
        {
            lock (this.rsa)
            {
                return this.rsa.Decrypt(Secret, RSAEncryptionPadding.OaepSHA256);
            }
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
            return RsaEndpoint.Verify(Data, Signature, this.keySize, this.modulus, this.exponent);
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
        public static bool Verify(byte[] Data, byte[] Signature, int KeySize, byte[] Modulus, byte[] Exponent)
        {
            using (RSA Rsa = CreateRSA(KeySize))
            {
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
        /// If implementation is slow, compared to other options.
        /// </summary>
        public override bool Slow => true;

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is RsaEndpoint RsaEndpoint &&
                this.keySize.Equals(RsaEndpoint.keySize) &&
                this.modulusBase64.Equals(RsaEndpoint.modulusBase64) &&
                this.exponentBase64.Equals(RsaEndpoint.exponentBase64);
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
