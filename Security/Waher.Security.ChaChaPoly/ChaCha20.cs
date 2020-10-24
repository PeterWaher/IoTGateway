using System;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Security.ChaChaPoly
{
    /// <summary>
    /// ChaCha20 encryptor, as defined in RFC 8439:
    /// https://tools.ietf.org/html/rfc8439
    /// </summary>
    public class ChaCha20
    {
        private readonly uint[] state;
        private readonly uint[] prev;
        private readonly byte[] block;
        private int pos;
        private uint blockCounter;
        private readonly uint[] nonce;
        private readonly uint[] key;

        /// <summary>
        /// ChaCha20 encryptor, as defined in RFC 8439:
        /// https://tools.ietf.org/html/rfc8439
        /// </summary>
        /// <param name="Key">Key</param>
        /// <param name="BlockCounter">Block counter</param>
        /// <param name="Nonce">Nonce value</param>
        public ChaCha20(byte[] Key, uint BlockCounter, byte[] Nonce)
        {
            if (Key.Length != 32)
                throw new ArgumentException("ChaCha20 keys must be 32 bytes (256 bits) long.", nameof(Key));

            if (Nonce.Length != 12)
                throw new ArgumentException("ChaCha20 nonces must be 12 bytes (96 bits) long.", nameof(Nonce));

            this.blockCounter = BlockCounter;
            this.key = new uint[]
            {
                ToUInt32(Key, 0),
                ToUInt32(Key, 4),
                ToUInt32(Key, 8),
                ToUInt32(Key, 12),
                ToUInt32(Key, 16),
                ToUInt32(Key, 20),
                ToUInt32(Key, 24),
                ToUInt32(Key, 28)
            };
            this.nonce = new uint[]
            {
                ToUInt32(Nonce, 0),
                ToUInt32(Nonce, 4),
                ToUInt32(Nonce, 8)
            };

            this.state = new uint[16];
            this.prev = new uint[16];
            this.block = new byte[64];
            this.pos = 64;
        }

        private static uint ToUInt32(byte[] Buffer, int Start)
        {
            uint Result;

            Start += 3;
            Result = Buffer[Start--];
            Result <<= 8;
            Result |= Buffer[Start--];
            Result <<= 8;
            Result |= Buffer[Start--];
            Result <<= 8;
            Result |= Buffer[Start];

            return Result;
        }

        private void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
        {
            uint i;

            a += b;
            d ^= a;
            i = d;
            d = (i << 16) | (i >> 16);
            c += d;
            b ^= c;
            i = b;
            b = (i << 12) | (i >> 20);
            a += b;
            d ^= a;
            i = d;
            d = (i << 8) | (i >> 24);
            c += d;
            b ^= c;
            i = b;
            b = (i << 7) | (i >> 25);
        }

        /// <summary>
        /// Generates a new block.
        /// </summary>
        public void NextBlock()
        {
            int i, j;
            uint k;

            this.state[0] = 0x61707865;
            this.state[1] = 0x3320646e;
            this.state[2] = 0x79622d32;
            this.state[3] = 0x6b206574;
            Array.Copy(this.key, 0, this.state, 4, 8);
            this.state[12] = this.blockCounter++;
            Array.Copy(this.nonce, 0, this.state, 13, 3);

            Array.Copy(this.state, 0, this.prev, 0, 16);

            for (i = 0; i < 10; i++)
            {
                this.QuarterRound(ref this.state[0], ref this.state[4], ref this.state[08], ref this.state[12]);
                this.QuarterRound(ref this.state[1], ref this.state[5], ref this.state[09], ref this.state[13]);
                this.QuarterRound(ref this.state[2], ref this.state[6], ref this.state[10], ref this.state[14]);
                this.QuarterRound(ref this.state[3], ref this.state[7], ref this.state[11], ref this.state[15]);
                this.QuarterRound(ref this.state[0], ref this.state[5], ref this.state[10], ref this.state[15]);
                this.QuarterRound(ref this.state[1], ref this.state[6], ref this.state[11], ref this.state[12]);
                this.QuarterRound(ref this.state[2], ref this.state[7], ref this.state[08], ref this.state[13]);
                this.QuarterRound(ref this.state[3], ref this.state[4], ref this.state[09], ref this.state[14]);
            }

            for (i = j = 0; i < 16; i++)
            {
                k = this.state[i] + this.prev[i];
                this.state[i] = k;

                this.block[j++] = (byte)k;
                k >>= 8;
                this.block[j++] = (byte)k;
                k >>= 8;
                this.block[j++] = (byte)k;
                k >>= 8;
                this.block[j++] = (byte)k;
            }

            this.pos = 0;
        }

        /// <summary>
        /// Gets the next byte in the stream.
        /// </summary>
        public byte GetNextByte()
        {
            if (this.pos >= 64)
                this.NextBlock();

            return this.block[this.pos++];
        }

        /// <summary>
        /// Gets the next number of bytes in the stream.
        /// </summary>
        public byte[] GetBytes(int NrBytes)
        {
            byte[] Result = new byte[NrBytes];
            int i = 0;
            int j;

            while (i < NrBytes)
            {
                if (this.pos >= 64)
                    this.NextBlock();

                j = Math.Min(NrBytes - i, 64 - this.pos);
                Array.Copy(this.block, this.pos, Result, i, j);
                this.pos += j;
                i += j;
            }

            return Result;
        }

        /// <summary>
        /// Encrypts or decrypts data.
        /// </summary>
        /// <param name="Data">Data to be encrypted or decrypted.</param>
        /// <returns>Encrypted or decrypted data.</returns>
        public byte[] EncryptOrDecrypt(byte[] Data)
        {
            int i, c = Data.Length;
            byte[] Result = this.GetBytes(c);

            for (i = 0; i < c; i++)
                Result[i] ^= Data[i];

            return Result;
        }

        /// <summary>
        /// Encrypts or decrypts data.
        /// </summary>
        /// <param name="Data">Data to be encrypted or decrypted.</param>
        /// <param name="Transformed">Transformed data</param>
        /// <returns>Encrypted or decrypted data.</returns>
        public async Task EncryptOrDecrypt(Stream Data, Stream Transformed)
        {
            byte[] Buf = new byte[65536];
            int i, c;

            while ((c = await Data.ReadAsync(Buf, 0, 65536)) > 0)
            {
                byte[] Result = this.GetBytes(c);

                for (i = 0; i < c; i++)
                    Result[i] ^= Buf[i];

                await Transformed.WriteAsync(Result, 0, c);
            }
        }

    }
}
