using System;
using System.Numerics;
using System.Security.Cryptography;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Implements the XEdDSA signature algorithm for Montgomery curves, as specified by
    /// the Signal Protocol:
    /// https://signal.org/docs/specifications/xeddsa/
    /// </summary>
    public static class XEdDSA
    {
        private readonly static RandomNumberGenerator rnd;

        /// <summary>
        /// Signs data using the XEdDSA algorithm.
        /// </summary>
        /// <param name="Data">Data to be signed.</param>
        /// <param name="PrivateKey">Private key.</param>
        /// <param name="HashFunction">Hash function to use</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <returns>Signature</returns>
        public static byte[] Sign(byte[] Data, byte[] PrivateKey,
            HashFunction HashFunction, MontgomeryCurve Curve)
        {
            int ScalarBytes = PrivateKey.Length;

            EdwardsCurveBase Pair = Curve.Pair;
            PointOnCurve UV = Curve.PublicKeyPoint;
            PointOnCurve XY = Curve.ToXY(UV);   // E
            byte[] a = PrivateKey;
            byte[] A = EdDSA.Encode(XY, Pair);

            if ((A[ScalarBytes - 1] & 0x80) != 0)
            {
                A[ScalarBytes - 1] &= 0x7f;
                BigInteger a2 = Pair.Order - EllipticCurve.ToInt(a);
                if (a2.Sign < 0)
                    a2 += Pair.Order;

                a = a2.ToByteArray();
                if (a.Length != ScalarBytes)
                    Array.Resize<byte>(ref a, ScalarBytes);
            }

            int DataLen = Data.Length;
            byte[] Z = new byte[64];
            byte[] Bin = new byte[ScalarBytes + DataLen + 64];

            lock (rnd)
            {
                rnd.GetBytes(Z);
            }

            Array.Copy(a, 0, Bin, 0, ScalarBytes);
            Array.Copy(Data, 0, Bin, ScalarBytes, DataLen);
            Array.Copy(Z, 0, Bin, ScalarBytes + DataLen, 64);

            BigInteger r = EllipticCurve.ToInt(Hash1(Bin, ScalarBytes, HashFunction));
            r = BigInteger.Remainder(r, Pair.Order);
            if (r.Sign < 0)
                r += Pair.Order;

            PointOnCurve R = Pair.ScalarMultiplication(r, Pair.BasePoint, true);
            byte[] Rs = EdDSA.Encode(R, Pair);

            Bin = new byte[(ScalarBytes << 1) + DataLen];

            Array.Copy(Rs, 0, Bin, 0, ScalarBytes);
            Array.Copy(A, 0, Bin, ScalarBytes, ScalarBytes);
            Array.Copy(Data, 0, Bin, ScalarBytes << 1, DataLen);

            BigInteger h = EllipticCurve.ToInt(HashFunction(Bin));
            h = BigInteger.Remainder(h, Pair.Order);

            BigInteger s = Pair.ModulusN.Add(r, Pair.ModulusN.Multiply(h, EllipticCurve.ToInt(a)));
            byte[] ss = s.ToByteArray();
            byte[] Signature = new byte[ScalarBytes << 1];

            Array.Copy(Rs, 0, Signature, 0, ScalarBytes);
            Array.Copy(ss, 0, Signature, ScalarBytes, ss.Length);

            return Signature;
        }

        private static byte[] Hash1(byte[] Data, int ScalarBytes, HashFunction HashFunction)
        {
            int c = Data.Length;
            byte[] Bin = new byte[ScalarBytes + c];
            int i;

            Bin[0] = 0xfe;

            for (i = 1; i < ScalarBytes; i++)
                Bin[i] = 0xff;

            Array.Copy(Data, 0, Bin, ScalarBytes, c);

            return HashFunction(Data);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the XEdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <param name="Signature">Signature</param>
        /// <param name="PBits">Number of bits used to encode prime.</param>
        /// <param name="QBits">Number of bits used to encode order.</param>
        /// <returns>If the signature is valid.</returns>
        public static bool Verify(byte[] Data, byte[] PublicKey, HashFunction HashFunction,
            MontgomeryCurve Curve, byte[] Signature, int PBits, int QBits)
        {
            try
            {
                int ScalarBytes = Signature.Length;
                if ((ScalarBytes & 1) != 0)
                    return false;

                ScalarBytes >>= 1;

                PointOnCurve P = Curve.Decode(PublicKey);
                BigInteger U = P.X;
                if (U >= Curve.Prime)
                    return false;

                byte[] Rs = new byte[ScalarBytes];
                Array.Copy(Signature, 0, Rs, 0, ScalarBytes);
                EdwardsCurveBase Pair = Curve.Pair;
                PointOnCurve R = EdDSA.Decode(Rs, Pair);

                byte[] ss = new byte[ScalarBytes];
                Array.Copy(Signature, ScalarBytes, ss, 0, ScalarBytes);
                BigInteger s = EllipticCurve.ToInt(ss);

                if (ModulusP.CalcBits(R.Y) >= PBits)
                    return false;

                if (ModulusP.CalcBits(ss) >= QBits)
                    return false;

                byte[] Bin = U.ToByteArray();
                if (Bin.Length != ScalarBytes)
                    Array.Resize<byte>(ref Bin, ScalarBytes);

                int MaskBits = PBits & 7;
                if (MaskBits != 0)
                    Bin[ScalarBytes - 1] &= (byte)(0xff >> (8 - MaskBits));

                BigInteger UMasked = EllipticCurve.ToInt(Bin);
                PointOnCurve AP = Curve.ToXY(new PointOnCurve(U, Curve.CalcV(U)));

                byte[] A = EdDSA.Encode(AP, Pair);
                A[ScalarBytes - 1] &= 0x7f; // A.s=0

                int DataLen = Data.Length;

                Bin = new byte[(ScalarBytes << 1) + DataLen];
                Array.Copy(Rs, 0, Bin, 0, ScalarBytes);
                Array.Copy(A, 0, Bin, ScalarBytes, ScalarBytes);
                Array.Copy(Data, 0, Bin, ScalarBytes << 1, DataLen);

                BigInteger h = EllipticCurve.ToInt(HashFunction(Bin));
                h = BigInteger.Remainder(h, Pair.Order);

                PointOnCurve Rcheck = Pair.ScalarMultiplication(h, AP, true);
                Pair.Negate(ref Rcheck);
                Pair.AddTo(ref Rcheck, Pair.ScalarMultiplication(s, Pair.BasePoint, true));

                if (!Rcheck.Y.Equals(R.Y))
                    return false;

                if (!Rcheck.X.IsEven.Equals(R.X.IsEven))
                    return false;

                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

    }
}
