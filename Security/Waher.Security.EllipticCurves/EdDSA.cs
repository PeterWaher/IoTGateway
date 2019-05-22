using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Delegate to hash function.
    /// </summary>
    /// <param name="Data">Data to be hashed.</param>
    /// <returns>Hash digest</returns>
    public delegate byte[] HashFunction(byte[] Data);

    /// <summary>
    /// Implements the EdDSA algorithm, as defined in RFC 8032.
    /// https://tools.ietf.org/html/rfc8032
    /// </summary>
    public static class EdDSA
    {
        /// <summary>
        /// Signs data using the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Data to be signed.</param>
        /// <param name="PrivateKey">Private key.</param>
        /// <param name="HashFunction">Hash function to use</param>
        /// <param name="ScalarBits">Number of bits to use for scalars.</param>
        /// <param name="MsbMask">Mask for most significant byte.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <returns>Signature</returns>
        public static byte[] Sign(byte[] Data, BigInteger PrivateKey, HashFunction HashFunction,
            int ScalarBits, EdwardsCurve Curve)
        {
            // 5.1.6 of RFC 8032

            int ScalarBytes = (ScalarBits + 8) >> 3;

            Console.Out.WriteLine("Signing");
            Console.Out.WriteLine("------------");

            byte[] h = PrivateKey.ToByteArray();
            if (h.Length != ScalarBytes)
                Array.Resize<byte>(ref h, ScalarBytes);

            Console.Out.WriteLine("Secret: " + Hashes.BinaryToString(h));
            h = HashFunction(h);

            Console.Out.WriteLine("Hash: " + Hashes.BinaryToString(h));

            byte[] Bin = new byte[ScalarBytes];
            Array.Copy(h, 0, Bin, 0, ScalarBytes);

            Bin[31] &= 0x3f;
            Bin[31] |= 0x40;
            Bin[0] &= 0xf8;

            BigInteger a = new BigInteger(Bin);

            Console.Out.WriteLine("a: " + a.ToString());

            PointOnCurve P = Curve.ScalarMultiplication(a, Curve.BasePoint);
            P.Normalize(Curve);

            Console.Out.WriteLine("P: " + P.ToString());
            Console.Out.WriteLine("Public Key: " + Curve.PublicKey.ToString());

            byte[] A = Encode(P, Curve);

            Console.Out.WriteLine("A: " + Hashes.BinaryToString(A));

            int c = Data.Length;
            Bin = new byte[ScalarBytes + c];                    // dom2(F, C) = blank string
            Array.Copy(h, ScalarBytes, Bin, 0, ScalarBytes);    // prefix
            Array.Copy(Data, 0, Bin, ScalarBytes, c);           // PH(M)=M

            h = HashFunction(Bin);

            Console.Out.WriteLine("Hash 2: " + PrivateKey.ToString());

            BigInteger r = ToBigInt(h);
            r = BigInteger.Remainder(r, Curve.Order);

            Console.Out.WriteLine("r: " + r.ToString());
            Console.Out.WriteLine("Basepoint: " + Curve.BasePoint.ToString());

            PointOnCurve R = Curve.ScalarMultiplication(r, Curve.BasePoint);
            Console.Out.WriteLine("R: " + R.ToString());
            R.Normalize(Curve);

            Console.Out.WriteLine("Rnorm: " + R.ToString());

            byte[] Rs = Encode(R, Curve);
            Console.Out.WriteLine("Rs: " + Hashes.BinaryToString(Rs));

            Bin = new byte[(ScalarBytes << 1) + c];             // dom2(F, C) = blank string
            Array.Copy(Rs, 0, Bin, 0, ScalarBytes);
            Array.Copy(A, 0, Bin, ScalarBytes, ScalarBytes);
            Array.Copy(Data, 0, Bin, ScalarBytes << 1, c);      // PH(M)=M

            Console.Out.WriteLine("Data to hash: " + Hashes.BinaryToString(Bin));
            h = HashFunction(Bin);

            Console.Out.WriteLine("Hash 3: " + Hashes.BinaryToString(h));

            BigInteger k = ToBigInt(h);
            k = BigInteger.Remainder(k, Curve.Order);

            Console.Out.WriteLine("k: " + k.ToString());

            BigInteger s = Curve.ModulusN.Add(r, Curve.ModulusN.Multiply(k, a));
            Console.Out.WriteLine("s: " + s.ToString());

            Bin = s.ToByteArray();
            if (Bin.Length != ScalarBytes)
                Array.Resize<byte>(ref Bin, ScalarBytes);

            byte[] Signature = new byte[ScalarBytes << 1];

            Array.Copy(Rs, 0, Signature, 0, ScalarBytes);
            Array.Copy(Bin, 0, Signature, ScalarBytes, ScalarBytes);

            Console.Out.WriteLine("Signature: " + Hashes.BinaryToString(Signature));

            return Signature;
        }

        /// <summary>
        /// Encodes a point on the curve in accordance with §5.1.2 of RFC 8032.
        /// </summary>
        /// <param name="P">Point</param>
        /// <param name="Curve">Edwards curve.</param>
        /// <returns>Encoding</returns>
        public static byte[] Encode(PointOnCurve P, EdwardsCurve Curve)
        {
            int ScalarBits = Curve.CoordinateBits;
            int ScalarBytes = (ScalarBits + 9) >> 3;

            byte[] y = P.Y.ToByteArray();
            if (y.Length != ScalarBytes)
                Array.Resize<byte>(ref y, ScalarBytes);

            byte[] x = P.X.ToByteArray();
            int Msb = (ScalarBits + 1) & 7;

            byte Mask = (byte)(0xff >> (8 - Msb));
            y[ScalarBytes - 1] &= Mask;

            if ((x[0] & 1) != 0)
                y[ScalarBytes - 1] |= 0x80;     // Always MSB

            return y;
        }

        /// <summary>
        /// Decodes a point on the curve in accordance with §5.1.3 of RFC 8032.
        /// </summary>
        /// <param name="Encoded">Encoded point.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <returns>Point on curve.</returns>
        public static PointOnCurve Decode(byte[] Encoded, EdwardsCurve Curve)
        {
            int ScalarBits = Curve.CoordinateBits;
            int ScalarBytes = (ScalarBits + 9) >> 3;

            if (Encoded.Length != ScalarBytes)
                throw new ArgumentException("Not encoded properly.", nameof(Encoded));

            bool x0 = (Encoded[ScalarBytes - 1] & 0x80) != 0;
            if (x0)
                Encoded[ScalarBytes - 1] &= 0x7f;

            BigInteger y = new BigInteger(Encoded);
            if (y >= Curve.Prime)
                throw new ArgumentException("Not a valid point.", nameof(Encoded));

            if (x0)
                Encoded[ScalarBytes - 1] |= 0x80;

            BigInteger x = Curve.GetX(y, x0);

            return new PointOnCurve(x, y);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <param name="ScalarBits">Number of bits to use for scalars.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public static bool Verify(byte[] Data, PointOnCurve PublicKey, HashFunction HashFunction,
            int ScalarBits, EdwardsCurve Curve, byte[] Signature)
        {
            try
            {
                int ScalarBytes = (ScalarBits + 8) >> 3;

                Console.Out.WriteLine();
                Console.Out.WriteLine("Verifying");
                Console.Out.WriteLine("------------");

                if (Signature.Length != ScalarBytes << 1)
                    return false;

                Console.Out.WriteLine("Signature: " + Hashes.BinaryToString(Signature));

                byte[] R = new byte[ScalarBytes];
                Array.Copy(Signature, 0, R, 0, ScalarBytes);
                PointOnCurve r = Decode(R, Curve);

                Console.Out.WriteLine("r: " + r.ToString());

                byte[] S = new byte[ScalarBytes];
                Array.Copy(Signature, ScalarBytes, S, 0, ScalarBytes);
                BigInteger s = ToBigInt(S);

                Console.Out.WriteLine("s: " + s.ToString());

                if (s.Sign < 0 || s >= Curve.Order)
                    return false;

                Console.Out.WriteLine("Public Key: " + PublicKey.ToString());

                byte[] A = Encode(PublicKey, Curve);

                Console.Out.WriteLine("A: " + Hashes.BinaryToString(A));

                int c = Data.Length;
                byte[] Bin = new byte[(ScalarBytes << 1) + c];              // dom2(F, C) = blank string
                Array.Copy(R, 0, Bin, 0, ScalarBytes);
                Array.Copy(A, 0, Bin, ScalarBytes, ScalarBytes);
                Array.Copy(Data, 0, Bin, ScalarBytes << 1, c);              // PH(M)=M

                Console.Out.WriteLine("Data to hash: " + Hashes.BinaryToString(Bin));
                byte[] h = HashFunction(Bin);
                Console.Out.WriteLine("Hash: " + Hashes.BinaryToString(h));

                BigInteger k = BigInteger.Remainder(ToBigInt(h), Curve.Order);

                Console.Out.WriteLine("k: " + k.ToString());

                PointOnCurve P1 = Curve.ScalarMultiplication(s, Curve.BasePoint);
                PointOnCurve P2 = Curve.ScalarMultiplication(k, PublicKey);
                Curve.AddTo(ref P2, r);

                /*PointOnCurve P1 = Curve.ScalarMultiplication(8,
                    Curve.ScalarMultiplication(s, Curve.BasePoint));
                PointOnCurve P2 = Curve.ScalarMultiplication(8, 
                    Curve.ScalarMultiplication(k, PublicKey));
                Curve.AddTo(ref P2, Curve.ScalarMultiplication(8, r));*/

                P1.Normalize(Curve);
                P2.Normalize(Curve);

                Console.Out.WriteLine("P1: " + P1.ToString());
                Console.Out.WriteLine("P2: " + P2.ToString());

                return P1.Equals(P2);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static BigInteger ToBigInt(byte[] Bin)
        {
            int c = Bin.Length;

            if ((Bin[c - 1] & 0x80) != 0)
                Array.Resize<byte>(ref Bin, c + 1);

            return new BigInteger(Bin);
        }

    }
}
