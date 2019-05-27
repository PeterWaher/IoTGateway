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
    /// Implements the Edwards curve Digital Signature Algorithm (EdDSA), as defined in RFC 8032.
    /// https://tools.ietf.org/html/rfc8032
    /// </summary>
    public static class EdDSA
    {
        /// <summary>
        /// Signs data using the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Data to be signed.</param>
        /// <param name="PrivateKey">Private key.</param>
        /// <param name="Prefix">Prefix</param>
        /// <param name="HashFunction">Hash function to use</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <returns>Signature</returns>
        public static byte[] Sign(byte[] Data, byte[] PrivateKey, byte[] Prefix,
            HashFunction HashFunction, EdwardsCurveBase Curve)
        {
            // 5.1.6 of RFC 8032

            int ScalarBytes = PrivateKey.Length;

            if (Prefix.Length != ScalarBytes)
                throw new ArgumentException("Invalid prefix.", nameof(Prefix));

            BigInteger a = EllipticCurve.ToInt(PrivateKey);
            PointOnCurve P = Curve.ScalarMultiplication(PrivateKey, Curve.BasePoint, true);
            byte[] A = Encode(P, Curve);
            int c = Data.Length;
            byte[] Bin = new byte[ScalarBytes + c];             // dom2(F, C) = blank string
            Array.Copy(Prefix, 0, Bin, 0, ScalarBytes);         // prefix
            Array.Copy(Data, 0, Bin, ScalarBytes, c);           // PH(M)=M

            byte[] h = HashFunction(Bin);
            BigInteger r = BigInteger.Remainder(EllipticCurve.ToInt(h), Curve.Order);
            PointOnCurve R = Curve.ScalarMultiplication(r, Curve.BasePoint, true);
            byte[] Rs = Encode(R, Curve);

            Bin = new byte[(ScalarBytes << 1) + c];             // dom2(F, C) = blank string
            Array.Copy(Rs, 0, Bin, 0, ScalarBytes);
            Array.Copy(A, 0, Bin, ScalarBytes, ScalarBytes);
            Array.Copy(Data, 0, Bin, ScalarBytes << 1, c);      // PH(M)=M

            h = HashFunction(Bin);

            BigInteger k = BigInteger.Remainder(EllipticCurve.ToInt(h), Curve.Order);
            BigInteger s = Curve.ModulusN.Add(r, Curve.ModulusN.Multiply(k, a));

            Bin = s.ToByteArray();
            if (Bin.Length != ScalarBytes)
                Array.Resize<byte>(ref Bin, ScalarBytes);

            byte[] Signature = new byte[ScalarBytes << 1];

            Array.Copy(Rs, 0, Signature, 0, ScalarBytes);
            Array.Copy(Bin, 0, Signature, ScalarBytes, ScalarBytes);

            return Signature;
        }

        /// <summary>
        /// Encodes a point on the curve in accordance with §5.1.2 of RFC 8032.
        /// </summary>
        /// <param name="P">Point</param>
        /// <param name="Curve">Edwards curve.</param>
        /// <returns>Encoding</returns>
        public static byte[] Encode(PointOnCurve P, EdwardsCurveBase Curve)
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
        public static PointOnCurve Decode(byte[] Encoded, EdwardsCurveBase Curve)
        {
            int ScalarBits = Curve.CoordinateBits;
            int ScalarBytes = (ScalarBits + 9) >> 3;

            if (Encoded.Length != ScalarBytes)
                throw new ArgumentException("Not encoded properly.", nameof(Encoded));

            bool x0 = (Encoded[ScalarBytes - 1] & 0x80) != 0;
            if (x0)
                Encoded[ScalarBytes - 1] &= 0x7f;

            BigInteger y = EllipticCurve.ToInt(Encoded);
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
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public static bool Verify(byte[] Data, byte[] PublicKey, HashFunction HashFunction,
            EdwardsCurveBase Curve, byte[] Signature)
        {
            try
            {
                int ScalarBytes = Signature.Length;
                if ((ScalarBytes & 1) != 0)
                    return false;

                ScalarBytes >>= 1;

                byte[] R = new byte[ScalarBytes];
                Array.Copy(Signature, 0, R, 0, ScalarBytes);
                PointOnCurve r = Decode(R, Curve);
                byte[] S = new byte[ScalarBytes];
                Array.Copy(Signature, ScalarBytes, S, 0, ScalarBytes);
                BigInteger s = EllipticCurve.ToInt(S);

                if (s >= Curve.Order)
                    return false;

                int c = Data.Length;
                byte[] Bin = new byte[(ScalarBytes << 1) + c];              // dom2(F, C) = blank string
                Array.Copy(R, 0, Bin, 0, ScalarBytes);
                Array.Copy(PublicKey, 0, Bin, ScalarBytes, ScalarBytes);
                Array.Copy(Data, 0, Bin, ScalarBytes << 1, c);              // PH(M)=M

                byte[] h = HashFunction(Bin);

                BigInteger k = BigInteger.Remainder(EllipticCurve.ToInt(h), Curve.Order);
                PointOnCurve P1 = Curve.ScalarMultiplication(s, Curve.BasePoint, false);
                PointOnCurve P2 = Curve.ScalarMultiplication(k, Curve.Decode(PublicKey), false);
                Curve.AddTo(ref P2, r);

                P1.Normalize(Curve);
                P2.Normalize(Curve);

                return P1.Equals(P2);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

    }
}
