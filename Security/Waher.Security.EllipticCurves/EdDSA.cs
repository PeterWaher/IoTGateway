using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
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
        /// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
        /// <param name="MsbMask">Mask for most significant byte.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <returns>Signature</returns>
        public static byte[] Sign(byte[] Data, BigInteger PrivateKey, HashFunction HashFunction,
            int ScalarBytes, EdwardsCurve Curve)
        {
            // 5.1.6 of RFC 8032

            Console.Out.WriteLine("Signing");
            Console.Out.WriteLine("------------");
            Console.Out.WriteLine("Private Key: " + PrivateKey.ToString());

            byte[] h = PrivateKey.ToByteArray();
            if (h.Length != ScalarBytes)
                Array.Resize<byte>(ref h, ScalarBytes);

            h = Hashes.ComputeHash(HashFunction, h);

            Console.Out.WriteLine("Hash: " + Hashes.BinaryToString(h));

            byte[] Bin = new byte[ScalarBytes];
            Array.Copy(h, 0, Bin, 0, ScalarBytes);

            Bin[31] &= 0x3f;
            Bin[31] |= 0x40;
            Bin[0] &= 0xf8;

            BigInteger s = new BigInteger(Bin);

            Console.Out.WriteLine("s: " + s.ToString());

            int c = Data.Length;
            Bin = new byte[ScalarBytes + c];                    // dom2(F, C) = blank string
            Array.Copy(h, ScalarBytes, Bin, 0, ScalarBytes);    // prefix
            Array.Copy(Data, 0, Bin, ScalarBytes, c);           // PH(M)=M

            h = Hashes.ComputeHash(HashFunction, Bin);

            Console.Out.WriteLine("Hash 2: " + PrivateKey.ToString());

            BigInteger r = ToBigInt(h);
            r = BigInteger.Remainder(r, Curve.Order);

            Console.Out.WriteLine("r: " + r.ToString());
            Console.Out.WriteLine("Basepoint: " + Curve.BasePoint.ToString());

            PointOnCurve P = Curve.ScalarMultiplication(r, Curve.BasePoint);
            P.Normalize(Curve);

            Console.Out.WriteLine("P: " + P.ToString());
            Console.Out.WriteLine("Public Key: " + Curve.PublicKey.ToString());
            Console.Out.WriteLine("A: " + Hashes.BinaryToString(Curve.A));

            byte[] R = Encode(P, ScalarBytes);

            Bin = new byte[(ScalarBytes << 1) + c];             // dom2(F, C) = blank string
            Array.Copy(R, 0, Bin, 0, ScalarBytes);
            Array.Copy(Curve.A, 0, Bin, ScalarBytes, ScalarBytes);
            Array.Copy(Data, 0, Bin, ScalarBytes << 1, c);      // PH(M)=M

            Console.Out.WriteLine("Data to hash: " + Hashes.BinaryToString(Bin));
            h = Hashes.ComputeHash(HashFunction, Bin);

            Console.Out.WriteLine("Hash 3: " + Hashes.BinaryToString(h));

            BigInteger k = ToBigInt(h);
            k = BigInteger.Remainder(k, Curve.Order);

            Console.Out.WriteLine("k: " + k.ToString());

            BigInteger s2 = Curve.ModulusN.Add(r, Curve.ModulusN.Multiply(k, s));
            Console.Out.WriteLine("S2: " + s2.ToString());

            byte[] S = s2.ToByteArray();
            if (S.Length != ScalarBytes)
                Array.Resize<byte>(ref S, ScalarBytes);

            byte[] Signature = new byte[ScalarBytes << 1];

            Array.Copy(R, 0, Signature, 0, ScalarBytes);
            Array.Copy(S, 0, Signature, ScalarBytes, ScalarBytes);

            Console.Out.WriteLine("Signature: " + Hashes.BinaryToString(Signature));

            return Signature;
        }

        /// <summary>
        /// Encodes a point on the curve in accordance with §5.1.2 of RFC 8032.
        /// </summary>
        /// <param name="P">Point</param>
        /// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
        /// <returns>Encoding</returns>
        public static byte[] Encode(PointOnCurve P, int ScalarBytes)
        {
            Console.Out.WriteLine("Encoding: " + P.ToString());

            byte[] y = P.Y.ToByteArray();
            if (y.Length != ScalarBytes)
                Array.Resize<byte>(ref y, ScalarBytes);

            byte[] x = P.X.ToByteArray();

            if ((x[0] & 1) != 0)
                y[ScalarBytes - 1] |= 0x80;
            else
                y[ScalarBytes - 1] &= 0x7f;

            Console.Out.WriteLine("Encoded: " + Hashes.BinaryToString(y));

            return y;
        }

        /// <summary>
        /// Decodes a point on the curve in accordance with §5.1.3 of RFC 8032.
        /// </summary>
        /// <param name="Encoded">Encoded point.</param>
        /// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <returns>Point on curve.</returns>
        public static PointOnCurve Decode(byte[] Encoded, int ScalarBytes, EdwardsCurve Curve)
        {
            Console.Out.WriteLine("Decoding: " + Hashes.BinaryToString(Encoded));

            if (Encoded.Length != ScalarBytes)
                throw new ArgumentException("Not encoded properly.", nameof(Encoded));

            bool x0 = (Encoded[ScalarBytes - 1] & 0x80) != 0;
            if (x0)
                Encoded[ScalarBytes - 1] &= 0x7f;

            Console.Out.WriteLine("x0: " + x0.ToString());

            BigInteger y = new BigInteger(Encoded);

            if (x0)
                Encoded[ScalarBytes - 1] |= 0x80;

            Console.Out.WriteLine("y: " + y.ToString());

            if (y >= Curve.Prime)
                throw new ArgumentException("Not a valid point.", nameof(Encoded));

            BigInteger y2 = Curve.Multiply(y, y);

            Console.Out.WriteLine("y2: " + y2.ToString());

            BigInteger u = y2 - BigInteger.One;

            if (u.Sign < 0)
                u += Curve.Prime;

            Console.Out.WriteLine("u: " + u.ToString());

            BigInteger v = Curve.Multiply(Curve.D, y2) + BigInteger.One;

            Console.Out.WriteLine("v: " + v.ToString());

            BigInteger v2 = Curve.Multiply(v, v);

            Console.Out.WriteLine("v2: " + v2.ToString());

            BigInteger v3 = Curve.Multiply(v, v2);

            Console.Out.WriteLine("v3: " + v3.ToString());

            BigInteger v4 = Curve.Multiply(v2, v2);

            Console.Out.WriteLine("v4: " + v4.ToString());

            BigInteger v7 = Curve.Multiply(v3, v4);

            Console.Out.WriteLine("v7: " + v7.ToString());

            BigInteger x = Curve.Multiply(Curve.Multiply(u, v3),
                BigInteger.ModPow(Curve.Multiply(u, v7), Curve.P58, Curve.Prime));

            Console.Out.WriteLine("x: " + x.ToString());

            BigInteger Check = BigInteger.ModPow(Curve.Divide(u, v), (Curve.Prime + 3) / 8, Curve.Prime);   // TODO: Remove
            Console.Out.WriteLine("Check: " + Check.ToString());

            BigInteger x2 = Curve.Multiply(x, x);

            Console.Out.WriteLine("x2: " + x2.ToString());

            BigInteger Test = Curve.Multiply(v, x2);
            if (Test.Sign < 0)
                Test += Curve.Prime;

            Console.Out.WriteLine("Test: " + Test.ToString());

            if (Test != u)
            {
                if (Test == Curve.Prime - u)
                    x = Curve.Multiply(x, Curve.TwoP14);
                else
                    throw new ArgumentException("Not a valid point.", nameof(Encoded));
            }

            if (x0)
            {
                if (x.IsZero)
                    throw new ArgumentException("Not a valid point.", nameof(Encoded));

                if (x.IsEven)
                    x = Curve.Prime - x;
            }
            else if (!x.IsEven)
                x = Curve.Prime - x;

            Console.Out.WriteLine("x: " + x.ToString());

            return new PointOnCurve(x, y);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public static bool Verify(byte[] Data, PointOnCurve PublicKey, HashFunction HashFunction,
            int ScalarBytes, EdwardsCurve Curve, byte[] Signature)
        {
            try
            {
                Console.Out.WriteLine();
                Console.Out.WriteLine("Verifying");
                Console.Out.WriteLine("------------");

                if (Signature.Length != ScalarBytes << 1)
                    return false;

                Console.Out.WriteLine("Signature: " + Hashes.BinaryToString(Signature));

                byte[] R = new byte[ScalarBytes];
                Array.Copy(Signature, 0, R, 0, ScalarBytes);
                PointOnCurve r = Decode(R, ScalarBytes, Curve);

                Console.Out.WriteLine("r: " + r.ToString());

                byte[] S = new byte[ScalarBytes];
                Array.Copy(Signature, ScalarBytes, S, 0, ScalarBytes);
                BigInteger s = ToBigInt(S);

                Console.Out.WriteLine("s: " + s.ToString());

                if (s.Sign < 0 || s >= Curve.Order)
                    return false;

                Console.Out.WriteLine("Public Key: " + PublicKey.ToString());

                byte[] A = Encode(PublicKey, ScalarBytes);

                Console.Out.WriteLine("A: " + Hashes.BinaryToString(A));

                int c = Data.Length;
                byte[] Bin = new byte[(ScalarBytes << 1) + c];              // dom2(F, C) = blank string
                Array.Copy(R, 0, Bin, 0, ScalarBytes);
                Array.Copy(A, 0, Bin, ScalarBytes, ScalarBytes);
                Array.Copy(Data, 0, Bin, ScalarBytes << 1, c);              // PH(M)=M

                Console.Out.WriteLine("Data to hash: " + Hashes.BinaryToString(Bin));
                byte[] h = Hashes.ComputeHash(HashFunction, Bin);
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
