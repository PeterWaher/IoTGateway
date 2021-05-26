using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Temporary;
using Waher.Security;

namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
    /// <summary>
    /// Abstract base class for symmetric ciphers.
    /// </summary>
    public abstract class E2eSymmetricCipher : IE2eSymmetricCipher
    {
        /// <summary>
        /// Random number generator.
        /// </summary>
        protected readonly static RandomNumberGenerator rnd = RandomNumberGenerator.Create();

        /// <summary>
        /// Local name of the E2E symmetric cipher
        /// </summary>
        public abstract string LocalName
        {
            get;
        }

        /// <summary>
        /// Namespace of the E2E symmetric cipher
        /// </summary>
        public virtual string Namespace
        {
            get { return EndpointSecurity.IoTHarmonizationE2E; }
        }

        /// <summary>
        /// If Authenticated Encryption with Associated Data is used
        /// </summary>
        public virtual bool AuthenticatedEncryption => false;

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Creates a new symmetric cipher object with the same settings as the current object.
        /// </summary>
        /// <returns>New instance</returns>
        public abstract IE2eSymmetricCipher CreteNew();

        /// <summary>
        /// Gets an Initiation Vector from stanza attributes.
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <returns>Initiation vector.</returns>
        protected abstract byte[] GetIV(string Id, string Type, string From, string To, uint Counter);

        /// <summary>
        /// Calculates the minimum size of encrypted data, given the size of the content.
        /// </summary>
        /// <param name="ContentLength">Size of content.</param>
        /// <returns>Minimum size of encrypted data.</returns>
        protected virtual long GetEncryptedLength(long ContentLength)
        {
            return ContentLength;
        }

        /// <summary>
        /// Encrypts binary data
        /// </summary>
        /// <param name="Data">Binary Data</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        /// <returns>Encrypted Data</returns>
        public virtual byte[] Encrypt(byte[] Data, byte[] Key, byte[] IV, byte[] AssociatedData)
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
            int BlockLen = (int)this.GetEncryptedLength(ContentLen);
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

            return Encrypted;
        }

        /// <summary>
        /// Decrypts binary data
        /// </summary>
        /// <param name="Data">Binary Data</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        /// <returns>Decrypted Data</returns>
        public virtual byte[] Decrypt(byte[] Data, byte[] Key, byte[] IV, byte[] AssociatedData)
        {
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
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Encrypted">Encrypted data will be stored here.</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        public virtual async Task Encrypt(Stream Data, Stream Encrypted, byte[] Key, byte[] IV, byte[] AssociatedData)
        {
            long c = Data.Length;
            int d = 0;
            long i = c;

            do
            {
                i >>= 7;
                d++;
            }
            while (i != 0);

            long ContentLen = c + d;
            long BlockLen = this.GetEncryptedLength(ContentLen);
            byte b;

            i = c;

            do
            {
                b = (byte)(i & 127);
                i >>= 7;
                if (i != 0)
                    b |= 0x80;

                Encrypted.WriteByte(b);
            }
            while (i != 0);

            await Data.CopyToAsync(Encrypted);

            if (ContentLen < BlockLen)
            {
                c = BlockLen - ContentLen;
                byte[] Bin = new byte[c];

                lock (rnd)
                {
                    rnd.GetBytes(Bin);
                }

                await Encrypted.WriteAsync(Bin, 0, (int)c);
            }
        }

        /// <summary>
        /// Decrypts binary data
        /// </summary>
        /// <param name="Data">Binary Data</param>
        /// <param name="Key">Encryption Key</param>
        /// <param name="IV">Initiation Vector</param>
        /// <param name="AssociatedData">Any associated data used for authenticated encryption (AEAD).</param>
        /// <returns>Decrypted Data</returns>
        public virtual async Task<Stream> Decrypt(Stream Data, byte[] Key, byte[] IV, byte[] AssociatedData)
        {
            int c = 0;
            int i;
            int Offset = 0;

            do
            {
                i = Data.ReadByte();
                if (i < 0)
                    return null;

                c |= (i & 127) << Offset;
                Offset += 7;
            }
            while (i >= 0x80);

            if (c < 0 || c + Data.Position > Data.Length)
                return null;

            TemporaryStream Decrypted = new TemporaryStream();
            try
            {
                await Crypto.CopyAsync(Data, Decrypted, c);
                return Decrypted;
            }
            catch (Exception)
			{
                Decrypted.Dispose();
                return null;
			}
        }

        /// <summary>
        /// Generates a new key. Used when the asymmetric cipher cannot calculate a shared secret.
        /// </summary>
        /// <returns>New key</returns>
        public abstract byte[] GenerateKey();

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
        public virtual byte[] Encrypt(string Id, string Type, string From, string To, uint Counter, byte[] Data, IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            byte[] Encrypted;
            byte[] EncryptedKey;
            byte[] Key;
            byte[] IV = this.GetIV(Id, Type, From, To, Counter);
            byte[] AssociatedData = this.AuthenticatedEncryption ? Encoding.UTF8.GetBytes(From) : null;
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
                Key = this.GenerateKey();
                EncryptedKey = Receiver.EncryptSecret(Key);
                l = EncryptedKey.Length;
                c += l + 2;
            }

            Encrypted = this.Encrypt(Data, Key, IV, AssociatedData);
            i = Encrypted.Length;
            j = 0;
            c += i;

            if (Sender.SupportsSignatures)
            {
                Signature = Sender.Sign(Data);
                k = Signature.Length;
                c += k + 1;
                if (k >= 128)
                    c++;
            }
            else
            {
                k = 0;
                Signature = null;
            }

            Block = new byte[c];

            if (k > 0)
			{
                if (k < 128)
                    Block[j++] = (byte)k;
                else
                {
                    Block[j++] = (byte)(k | 128);
                    Block[j++] = (byte)(k >> 7);
                }
            }

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
        /// <returns>Decrypted data, if able, null otherwise.</returns>
        public virtual byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data, IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            if (Data.Length < 8)
                return null;

            int SignatureLen;
            int KeyLen;
            int DataLen;
            uint Counter;
            int i = 0;

            if (Receiver.SupportsSignatures)
            {
                SignatureLen = Data[i++];
                if ((SignatureLen & 128) != 0)
                {
                    SignatureLen &= 127;
                    SignatureLen |= Data[i++] << 7;
                }
            }
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
            byte[] IV = this.GetIV(Id, Type, From, To, Counter);
            byte[] AssociatedData = this.AuthenticatedEncryption ? Encoding.UTF8.GetBytes(From) : null;

            try
            {
                Decrypted = this.Decrypt(Encrypted, Key, IV, AssociatedData);

                if (!(Decrypted is null) &&
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
                        Decrypted = this.Decrypt(Encrypted, Key, IV, AssociatedData);

                        if (!(Decrypted is null) &&
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
        /// Encrypts binary data
        /// </summary>
        /// <param name="Id">Id attribute</param>
        /// <param name="Type">Type attribute</param>
        /// <param name="From">From attribute</param>
        /// <param name="To">To attribute</param>
        /// <param name="Counter">Counter. Can be reset every time a new key is generated.
        /// A new key must be generated before the counter wraps.</param>
        /// <param name="Data">Binary data to encrypt</param>
        /// <param name="Encrypted">Encrypted data will be stored here.</param>
        /// <param name="Sender">Local endpoint performing the encryption.</param>
        /// <param name="Receiver">Remote endpoint performing the decryption.</param>
        public virtual async Task Encrypt(string Id, string Type, string From, string To, uint Counter, Stream Data, Stream Encrypted, IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            using (TemporaryStream TempEncrypted = new TemporaryStream())
            {
                byte[] EncryptedKey;
                byte[] Key;
                byte[] IV = this.GetIV(Id, Type, From, To, Counter);
                byte[] AssociatedData = this.AuthenticatedEncryption ? Encoding.UTF8.GetBytes(From) : null;
                byte[] Signature;
                long i;
                int k, l;

                if (Sender.SupportsSharedSecrets)
                {
                    Key = Sender.GetSharedSecret(Receiver);
                    EncryptedKey = null;
                    l = 0;
                }
                else
                {
                    Key = this.GenerateKey();
                    EncryptedKey = Receiver.EncryptSecret(Key);
                    l = EncryptedKey.Length;
                }

                await this.Encrypt(Data, TempEncrypted, Key, IV, AssociatedData);
                i = TempEncrypted.Length;

                if (i > uint.MaxValue)
                    throw new NotSupportedException("Too large.");

                if (Sender.SupportsSignatures)
                {
                    Data.Position = 0;
                    Signature = Sender.Sign(Data);
                    k = Signature.Length;
                }
                else
                {
                    k = 0;
                    Signature = null;
                }

                if (k > 0)
				{
                    if (k < 128)
                        Encrypted.WriteByte((byte)k);
                    else
                    {
                        Encrypted.WriteByte((byte)(k | 128));
                        Encrypted.WriteByte((byte)(k >> 7));
                    }
                }

                if (l > 0)
                {
                    Encrypted.WriteByte((byte)l);
                    Encrypted.WriteByte((byte)(l >> 8));
                }

                Encrypted.WriteByte((byte)i);
                Encrypted.WriteByte((byte)(i >> 8));
                Encrypted.WriteByte((byte)(i >> 16));
                Encrypted.WriteByte((byte)(i >> 24));

                Encrypted.WriteByte((byte)Counter);
                Encrypted.WriteByte((byte)(Counter >> 8));
                Encrypted.WriteByte((byte)(Counter >> 16));
                Encrypted.WriteByte((byte)(Counter >> 24));

                if (k > 0)
                    await Encrypted.WriteAsync(Signature, 0, k);

                if (l > 0)
                    await Encrypted.WriteAsync(EncryptedKey, 0, l);

                TempEncrypted.Position = 0;
                await TempEncrypted.CopyToAsync(Encrypted);
            }
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
        /// <returns>Decrypted data, if able, null otherwise.</returns>
        public virtual async Task<Stream> Decrypt(string Id, string Type, string From, string To, Stream Data, IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            if (Data.Length < 8)
                return null;

            int SignatureLen;
            int KeyLen;
            int DataLen;
            uint Counter;

            if (Receiver.SupportsSignatures)
            {
                SignatureLen = Data.ReadByte();
                if ((SignatureLen & 128) != 0)
                {
                    SignatureLen &= 127;
                    SignatureLen |= Data.ReadByte() << 7;
                }
            }
            else
                SignatureLen = 0;

            if (Receiver.SupportsSharedSecrets)
                KeyLen = 0;
            else
            {
                KeyLen = Data.ReadByte();
                KeyLen |= Data.ReadByte() << 8;
            }

            if (Data.Position + 4 > Data.Length)
                return null;

            DataLen = Data.ReadByte();
            DataLen |= Data.ReadByte() << 8;
            DataLen |= Data.ReadByte() << 16;
            DataLen |= Data.ReadByte() << 24;

            Counter = (byte)Data.ReadByte();
            Counter |= (uint)(Data.ReadByte() << 8);
            Counter |= (uint)(Data.ReadByte() << 16);
            Counter |= (uint)(Data.ReadByte() << 24);

            if (Data.Length != Data.Position + SignatureLen + KeyLen + DataLen)
                return null;

            byte[] Signature = new byte[SignatureLen];
            byte[] EncryptedKey = KeyLen > 0 ? new byte[KeyLen] : null;
            byte[] Key;

            if (await Data.ReadAsync(Signature, 0, SignatureLen) != SignatureLen)
                return null;

            if (KeyLen > 0)
            {
                if (await Data.ReadAsync(EncryptedKey, 0, KeyLen) != KeyLen)
                    return null;
            }

            using (TemporaryStream Encrypted = new TemporaryStream())
            {
                await Crypto.CopyAsync(Data, Encrypted, DataLen);

                if (EncryptedKey is null)
                    Key = Receiver.GetSharedSecret(Sender);
                else
                    Key = Receiver.DecryptSecret(EncryptedKey);

                byte[] IV = this.GetIV(Id, Type, From, To, Counter);
                byte[] AssociatedData = this.AuthenticatedEncryption ? Encoding.UTF8.GetBytes(From) : null;
                Stream Decrypted = null;

                try
                {
                    Encrypted.Position = 0;
                    Decrypted = await this.Decrypt(Encrypted, Key, IV, AssociatedData);

                    if (!(Decrypted is null))
                    {
                        Decrypted.Position = 0;

                        if ((Sender.SupportsSignatures && Sender.Verify(Decrypted, Signature)) ||
                            (!Sender.SupportsSignatures && SignatureLen == 0))
                        {
                            return Decrypted;
                        }

                        Decrypted.Dispose();
                        Decrypted = null;
                    }
                }
                catch (Exception)
                {
                    // Invalid key

                    Decrypted?.Dispose();
                    Decrypted = null;
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
                            Encrypted.Position = 0;
                            Decrypted = await this.Decrypt(Encrypted, Key, IV, AssociatedData);

                            if (!(Decrypted is null))
                            {
                                Decrypted.Position = 0;

                                if ((Sender.SupportsSignatures && Sender.Verify(Decrypted, Signature)) ||
                                    (!Sender.SupportsSignatures && SignatureLen == 0))
                                {
                                    return Decrypted;
                                }

                                Decrypted.Dispose();
                                Decrypted = null;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Invalid key

                        Decrypted?.Dispose();
                        Decrypted = null;
                    }
                }

                return null;
            }
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
        public virtual bool Encrypt(string Id, string Type, string From, string To, uint Counter, byte[] Data,
            StringBuilder Xml, IE2eEndpoint Sender, IE2eEndpoint Receiver)
        {
            byte[] Encrypted;
            byte[] Key;
            byte[] IV = this.GetIV(Id, Type, From, To, Counter);
            byte[] AssociatedData = this.AuthenticatedEncryption ? Encoding.UTF8.GetBytes(From) : null;

            Xml.Append('<');
            Xml.Append(this.LocalName);
            Xml.Append(" xmlns=\"");
            Xml.Append(this.Namespace);
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
                Key = this.GenerateKey();

                byte[] EncryptedKey = Receiver.EncryptSecret(Key);

                Xml.Append("\" k=\"");
                Xml.Append(Convert.ToBase64String(EncryptedKey));
            }

            Encrypted = this.Encrypt(Data, Key, IV, AssociatedData);

            if (Sender.SupportsSignatures)
            {
                byte[] Signature = Sender.Sign(Data);

                Xml.Append("\" s=\"");
                Xml.Append(Convert.ToBase64String(Signature));
            }

            Xml.Append("\">");
            Xml.Append(Convert.ToBase64String(Encrypted));
            Xml.Append("</");
            Xml.Append(this.LocalName);
            Xml.Append('>');

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
        public virtual string Decrypt(string Id, string Type, string From, string To, XmlElement Xml,
            IE2eEndpoint Sender, IE2eEndpoint Receiver)
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
                Key = Receiver.GetSharedSecret(Sender);
            else
                Key = Receiver.DecryptSecret(EncryptedKey);

            byte[] IV = this.GetIV(Id, Type, From, To, Counter.Value);
            byte[] AssociatedData = this.AuthenticatedEncryption ? Encoding.UTF8.GetBytes(From) : null;

            try
            {
                Decrypted = this.Decrypt(Encrypted, Key, IV, AssociatedData);

                if (!(Decrypted is null) &&
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
                        Key = Receiver.Previous.GetSharedSecret(Sender);
                    else
                        Key = Receiver.Previous.DecryptSecret(EncryptedKey);

                    if (!(Key is null))
                    {
                        Decrypted = this.Decrypt(Encrypted, Key, IV, AssociatedData);

                        if (!(Decrypted is null) &&
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
