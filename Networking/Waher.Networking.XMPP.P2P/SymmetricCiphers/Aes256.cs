using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Security;

namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
    /// <summary>
    /// Implements support for the AES-256 cipher in hybrid End-to-End encryption schemes.
    /// </summary>
    public class Aes256 : E2eSymmetricCipher
    {
        private readonly static RandomNumberGenerator rnd = RandomNumberGenerator.Create();

        /// <summary>
        /// AES encryption object
        /// </summary>
        protected Aes aes;

        /// <summary>
        /// Abstract base class for AES-256 based End-to-End encryption schemes.
        /// </summary>
        public Aes256()
        {
            this.aes = Aes.Create();
            this.aes.BlockSize = 128;
            this.aes.KeySize = 256;
            this.aes.Mode = CipherMode.CBC;
            this.aes.Padding = PaddingMode.None;
        }

        /// <summary>
        /// Local name of the E2E symmetric cipher
        /// </summary>
        public override string LocalName => "aes";

        /// <summary>
        /// Namespace of the E2E symmetric cipher
        /// </summary>
        public override string Namespace => EndpointSecurity.IoTHarmonizationE2E;


        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this.aes?.Dispose();
            this.aes = null;
        }

        /// <summary>
        /// Gets AES Initiation Vector from stanza attributes.s
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <returns>AES Initiation vector.</returns>
        public byte[] GetIV(string Id, string Type, string From, string To, uint Counter)
        {
            byte[] IV = Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(Id + Type + From + To));
            Array.Resize<byte>(ref IV, 16);

            IV[12] = (byte)Counter;
            Counter >>= 8;
            IV[13] = (byte)Counter;
            Counter >>= 8;
            IV[14] = (byte)Counter;
            Counter >>= 8;
            IV[15] = (byte)Counter;

            return IV;
        }

        /// <summary>
        /// Encrypts binary data
        /// </summary>
        /// <param name="Data">Binary Data</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <returns>Encrypted Data</returns>
        public byte[] Encrypt(byte[] Data, byte[] Key, byte[] IV)
        {
            int c = Data.Length;
            int d = 0;
            int i = c;

            do
            {
                i >>= 7;
                d++;
            }
            while (i != 0);

            int ContentLen = c + d;
            int BlockLen = (ContentLen + 15) & ~0xf;
            byte[] Encrypted = new byte[BlockLen];
            int j = 0;

            i = c;

            do
            {
                Encrypted[j] = (byte)(i & 127);
                i >>= 7;
                if (i != 0)
                    Encrypted[j] |= 0x80;

                j++;
            }
            while (i != 0);

            Array.Copy(Data, 0, Encrypted, j, c);

            if (ContentLen < BlockLen)
            {
                c = BlockLen - ContentLen;
                byte[] Bin = new byte[c];

                lock (rnd)
                {
                    rnd.GetBytes(Bin);
                }

                Array.Copy(Bin, 0, Encrypted, ContentLen, c);
            }

            lock (this.aes)
            {
                using (ICryptoTransform Aes = this.aes.CreateEncryptor(Key, IV))
                {
                    Encrypted = Aes.TransformFinalBlock(Encrypted, 0, BlockLen);
                }
            }

            return Encrypted;
        }

        /// <summary>
        /// Decrypts binary data
        /// </summary>
        /// <param name="Data">Binary Data</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <returns>Decrypted Data</returns>
        public byte[] Decrypt(byte[] Data, byte[] Key, byte[] IV)
        {
            lock (this.aes)
            {
                using (ICryptoTransform Aes = this.aes.CreateDecryptor(Key, IV))
                {
                    Data = Aes.TransformFinalBlock(Data, 0, Data.Length);
                }
            }

            int c = 0;
            int i = 0;
            int Offset = 0;
            byte b;

            do
            {
                b = Data[i++];

                c |= (b & 127) << Offset;
                Offset += 7;
            }
            while (b >= 0x80);

            if (c < 0 || c + i > Data.Length)
                return null;

            byte[] Decrypted = new byte[c];

            Array.Copy(Data, i, Decrypted, 0, c);

            return Decrypted;
        }

        /// <summary>
        /// Encrypts binary data
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <param name="Data">Binary data to encrypt</param>
        /// <param name="Sender">Local endpoint performing the encryption.</param>
        /// <param name="Receiver">Remote endpoint performing the decryption.</param>
        /// <returns>Encrypted data</returns>
        public override byte[] Encrypt(string Id, string Type, string From, string To, uint Counter, byte[] Data,
            IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            byte[] Encrypted;
            byte[] EncryptedKey;
            byte[] Key;
            byte[] IV = GetIV(Id, Type, From, To, Counter);
            byte[] Signature;
            byte[] Block;
            int i, j, k, l;
            int c = 8;

            if (Sender.SupportsSharedSecrets)
            {
                Key = Sender.GetSharedSecret(Receiver);
                EncryptedKey = null;
                l = 0;
            }
            else
            {
                lock (this.aes)
                {
                    this.aes.GenerateKey();
                    Key = this.aes.Key;
                }

                EncryptedKey = Receiver.EncryptSecret(Key);
                l = EncryptedKey.Length;
                c += l + 1;
            }

            Encrypted = this.Encrypt(Data, Key, IV);
            i = Encrypted.Length;
            j = 0;
            c += i;

            if (Sender.SupportsSignatures)
            {
                Signature = Sender.Sign(Data);
                k = Signature.Length;
                c += k + 1;
            }
            else
            {
                k = 0;
                Signature = null;
            }

            Block = new byte[c];

            if (k > 0)
                Block[j++] = (byte)k;

            if (l > 0)
            {
                Block[j++] = (byte)l;
                Block[j++] = (byte)(l >> 8);
            }

            Block[j++] = (byte)i;
            Block[j++] = (byte)(i >> 8);
            Block[j++] = (byte)(i >> 16);
            Block[j++] = (byte)(i >> 24);

            Block[j++] = (byte)Counter;
            Block[j++] = (byte)(Counter >> 8);
            Block[j++] = (byte)(Counter >> 16);
            Block[j++] = (byte)(Counter >> 24);

            if (k > 0)
            {
                Array.Copy(Signature, 0, Block, j, k);
                j += k;
            }

            if (l > 0)
            {
                Array.Copy(EncryptedKey, 0, Block, j, l);
                j += l;
            }

            Array.Copy(Encrypted, 0, Block, j, i);

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
        /// <param name="Sender">Remote endpoint performing the encryption.</param>
        /// <param name="Receiver">Local endpoint performing the decryption.</param>
        /// <returns>Decrypted data</returns>
        public override byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data,
            IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            if (Data.Length < 8)
                return null;

            int SignatureLen;
            int KeyLen;
            int DataLen;
            uint Counter;
            int i = 0;

            if (Receiver.SupportsSignatures)
                SignatureLen = Data[i++];
            else
                SignatureLen = 0;

            if (Receiver.SupportsSharedSecrets)
                KeyLen = 0;
            else
            {
                KeyLen = Data[i++];
                KeyLen |= Data[i++] << 8;
            }

            if (i + 4 > Data.Length)
                return null;

            DataLen = Data[i++];
            DataLen |= Data[i++] << 8;
            DataLen |= Data[i++] << 16;
            DataLen |= Data[i++] << 24;

            Counter = Data[i++];
            Counter |= (uint)(Data[i++] << 8);
            Counter |= (uint)(Data[i++] << 16);
            Counter |= (uint)(Data[i++] << 24);

            if (Data.Length != i + SignatureLen + KeyLen + DataLen)
                return null;

            byte[] Signature = new byte[SignatureLen];
            byte[] EncryptedKey = KeyLen > 0 ? new byte[KeyLen] : null;
            byte[] Encrypted = new byte[DataLen];
            byte[] Key;

            Array.Copy(Data, i, Signature, 0, SignatureLen);
            i += SignatureLen;

            if (KeyLen > 0)
            {
                Array.Copy(Data, i, EncryptedKey, 0, KeyLen);
                i += KeyLen;
            }

            Array.Copy(Data, i, Encrypted, 0, DataLen);

            if (EncryptedKey is null)
                Key = Receiver.GetSharedSecret(Sender);
            else
                Key = Receiver.DecryptSecret(EncryptedKey);

            byte[] Decrypted;
            byte[] IV = GetIV(Id, Type, From, To, Counter);

            try
            {
                Decrypted = this.Decrypt(Encrypted, Key, IV);

                if (Decrypted != null &&
                    ((Sender.SupportsSignatures && Sender.Verify(Decrypted, Signature)) ||
                    (!Sender.SupportsSignatures && SignatureLen == 0)))
                {
                    return Decrypted;
                }
            }
            catch (Exception)
            {
                // Invalid key
            }

            if (!(Receiver.Previous is null))
            {
                try
                {
                    if (EncryptedKey is null)
                        Key = Receiver.Previous.GetSharedSecret(Sender);
                    else
                        Key = Receiver.Previous.DecryptSecret(EncryptedKey);

                    if (!(Key is null))
                    {
                        Decrypted = this.Decrypt(Encrypted, Key, IV);

                        if (Decrypted != null &&
                            ((Sender.SupportsSignatures && Sender.Verify(Decrypted, Signature)) ||
                            (!Sender.SupportsSignatures && SignatureLen == 0)))
                        {
                            return Decrypted;
                        }
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
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <param name="Data">Binary data to encrypt</param>
        /// <param name="Xml">XML output</param>
        /// <param name="Sender">Local endpoint performing the encryption.</param>
        /// <param name="Receiver">Remote endpoint performing the decryption.</param>
        /// <returns>If encryption was possible</returns>
        public override bool Encrypt(string Id, string Type, string From, string To, uint Counter, byte[] Data,
            StringBuilder Xml, IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            byte[] Encrypted;
            byte[] Key;
            byte[] IV = GetIV(Id, Type, From, To, Counter);

            Xml.Append("<aes xmlns=\"");
            Xml.Append(EndpointSecurity.IoTHarmonizationE2E);
            Xml.Append("\" r=\"");
            if (Sender.Namespace != EndpointSecurity.IoTHarmonizationE2E)
            {
                Xml.Append(Sender.Namespace);
                Xml.Append('#');
            }
            Xml.Append(Sender.LocalName);
            Xml.Append("\" c=\"");
            Xml.Append(Counter.ToString());

            if (Sender.SupportsSharedSecrets)
                Key = Sender.GetSharedSecret(Receiver);
            else
            {
                lock (this.aes)
                {
                    this.aes.GenerateKey();
                    Key = this.aes.Key;
                }

                byte[] EncryptedKey = Receiver.EncryptSecret(Key);

                Xml.Append("\" k=\"");
                Xml.Append(Convert.ToBase64String(EncryptedKey));
            }

            Encrypted = this.Encrypt(Data, Key, IV);

            if (Sender.SupportsSignatures)
            {
                byte[] Signature = Sender.Sign(Data);

                Xml.Append("\" s=\"");
                Xml.Append(Convert.ToBase64String(Signature));
            }

            Xml.Append("\">");
            Xml.Append(Convert.ToBase64String(Encrypted));
            Xml.Append("</aes>");

            return true;
        }

        /// <summary>
        /// Decrypts XML data
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Xml">XML element with encrypted data.</param>
        /// <param name="Sender">Remote endpoint performing the encryption.</param>
        /// <param name="Receiver">Local endpoint performing the decryption.</param>
        /// <returns>Decrypted XMLs</returns>
        public override string Decrypt(string Id, string Type, string From, string To, XmlElement Xml, IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            byte[] EncryptedKey = null;
            byte[] Signature = null;
            uint? Counter = null;

            foreach (XmlAttribute Attr in Xml.Attributes)
            {
                switch (Attr.Name)
                {
                    case "c":
                        if (!uint.TryParse(Attr.Value, out uint i))
                            return null;

                        Counter = i;
                        break;

                    case "s":
                        Signature = Convert.FromBase64String(Attr.Value);
                        break;

                    case "k":
                        EncryptedKey = Convert.FromBase64String(Attr.Value);
                        break;
                }
            }

            if (!Counter.HasValue)
                return null;

            byte[] Encrypted = Convert.FromBase64String(Xml.InnerText);
            byte[] Decrypted;
            byte[] Key;

            if (EncryptedKey is null)
                Key = Receiver.DecryptSecret(EncryptedKey);
            else
                Key = Receiver.GetSharedSecret(Sender);

            byte[] IV = this.GetIV(Id, Type, From, To, Counter.Value);

            try
            {
                Decrypted = this.Decrypt(Encrypted, Key, IV);

                if (Decrypted != null &&
                    ((Sender.SupportsSignatures && Sender.Verify(Decrypted, Signature)) ||
                    (!Sender.SupportsSignatures && Signature is null)))
                {
                    return Encoding.UTF8.GetString(Decrypted);
                }
            }
            catch (Exception)
            {
                // Invalid key
            }

            if (!(Receiver.Previous is null))
            {
                try
                {
                    if (EncryptedKey is null)
                        Key = Receiver.Previous.DecryptSecret(EncryptedKey);
                    else
                        Key = Receiver.Previous.GetSharedSecret(Sender);

                    if (Key != null)
                    {
                        Decrypted = this.Decrypt(Encrypted, Key, IV);

                        if (Decrypted != null &&
                            ((Sender.SupportsSignatures && Sender.Verify(Decrypted, Signature)) ||
                            (!Sender.SupportsSignatures && Signature is null)))
                        {
                            return Encoding.UTF8.GetString(Decrypted);
                        }
                    }
                }
                catch (Exception)
                {
                    // Invalid key
                }
            }

            return null;
        }

    }
}
