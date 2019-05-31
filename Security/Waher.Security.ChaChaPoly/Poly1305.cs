using System;
using System.Numerics;

namespace Waher.Security.ChaChaPoly
{
    /// <summary>
    /// Poly1305 authenticator, as defined in RFC 8439:
    /// https://tools.ietf.org/html/rfc8439
    /// </summary>
    public class Poly1305
    {
        private readonly static BigInteger p = BigInteger.Pow(2, 130) - 5;
        private readonly BigInteger r;
        private readonly BigInteger s;

        /// <summary>
        /// Poly1305 authenticator, as defined in RFC 8439:
        /// https://tools.ietf.org/html/rfc8439
        /// </summary>
        /// <param name="Key">Key</param>
        public Poly1305(byte[] Key)
        {
            if (Key.Length != 32)
                throw new ArgumentException("Poly1305 keys must be 32 bytes (256 bits) long.", nameof(Key));

            byte[] rBin = new byte[(Key[15] & 0x80) != 0 ? 17 : 16];
            byte[] sBin = new byte[(Key[31] & 0x80) != 0 ? 17 : 16];

            Array.Copy(Key, 0, rBin, 0, 16);
            Array.Copy(Key, 16, sBin, 0, 16);

            rBin[3] &= 15;
            rBin[7] &= 15;
            rBin[11] &= 15;
            rBin[15] &= 15;
            rBin[4] &= 252;
            rBin[8] &= 252;
            rBin[12] &= 252;

            this.r = new BigInteger(rBin);
            this.s = new BigInteger(sBin);
        }

        /// <summary>
        /// Prepares a <see cref="Poly1305"/> instance using <see cref="ChaCha20"/>
        /// and Counter=0 to generate the corresponding key.
        /// </summary>
        /// <param name="Key">Key</param>
        /// <param name="Nonce">Nonce</param>
        /// <returns>Authenticator</returns>
        public static Poly1305 FromChaCha20(byte[] Key, byte[] Nonce)
        {
            ChaCha20 Cipher = new ChaCha20(Key, 0, Nonce);
            byte[] Key2 = Cipher.GetBytes(32);
            return new Poly1305(Key2);
        }

        /// <summary>
        /// Calculates a message authentication code (MAC).
        /// </summary>
        /// <param name="Data">Data</param>
        /// <returns>MAC</returns>
        public byte[] CalcMac(byte[] Data)
        {
            int i = 0;
            int c = Data.Length;
            byte[] Bin = new byte[17];
            int j;
            BigInteger Accumulator = BigInteger.Zero;

            while (i < c)
            {
                j = Math.Min(c - i, 16);
                Array.Copy(Data, i, Bin, 0, j);
                i += j;
                Bin[j++] = 1;
                while (j < 17)
                    Bin[j++] = 0;

                Accumulator = BigInteger.Remainder((Accumulator + new BigInteger(Bin)) * this.r, p);
            }

            Accumulator += this.s;

            byte[] Result = Accumulator.ToByteArray();

            if (Result.Length != 16)
                Array.Resize<byte>(ref Result, 16);

            return Result;
        }

    }
}
