using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Implements the Elliptic Curve Digital Signature Algorithm (ECDSA).
    /// </summary>
    public static class ECDSA
    {
        /// <summary>
        /// Signs data using the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Data to be signed.</param>
        /// <param name="PrivateKey">Private key.</param>
        /// <param name="HashFunction">Hash function to use</param>
        /// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
        /// <param name="MsbMask">Mask for most significant byte.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <returns>Signature</returns>
        public static byte[] Sign(byte[] Data, byte[] PrivateKey, HashFunction HashFunction,
            int ScalarBytes, byte MsbMask, PrimeFieldCurve Curve)
        {
            BigInteger e = CalcE(Data, HashFunction, ScalarBytes, MsbMask);
            BigInteger r, s, PrivateKeyInt = EllipticCurve.ToInt(PrivateKey);
            PointOnCurve P1;
            byte[] k;

            do
            {
                do
                {
                    k = Curve.GenerateSecret();
                    P1 = Curve.ScalarMultiplication(k, Curve.BasePoint, true);
                }
                while (P1.IsXZero);

                r = BigInteger.Remainder(P1.X, Curve.Order);
                s = Curve.ModulusN.Divide(Curve.ModulusN.Add(e,
                    Curve.ModulusN.Multiply(r, PrivateKeyInt)), EllipticCurve.ToInt(k));
            }
            while (s.IsZero);

            if (r.Sign < 0)
                r += Curve.Prime;

            P1.Normalize(Curve);

            byte[] Signature = new byte[ScalarBytes << 1];

            byte[] S = r.ToByteArray();
            if (S.Length != ScalarBytes)
                Array.Resize<byte>(ref S, ScalarBytes);

            Array.Copy(S, 0, Signature, 0, ScalarBytes);

            S = s.ToByteArray();
            if (S.Length != ScalarBytes)
                Array.Resize<byte>(ref S, ScalarBytes);

            Array.Copy(S, 0, Signature, ScalarBytes, ScalarBytes);

            return Signature;
        }

        private static BigInteger CalcE(byte[] Data, HashFunction HashFunction,
            int ScalarBytes, byte MsbMask)
        {
            byte[] Hash = HashFunction(Data);
            int c = Hash.Length;

            if (c != ScalarBytes)
                Array.Resize<byte>(ref Hash, ScalarBytes);

            Hash[ScalarBytes - 1] &= MsbMask;

            return EllipticCurve.ToInt(Hash);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <param name="Curve">Elliptic curve</param>
        /// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
        /// <param name="MsbMask">Mask for most significant byte.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public static bool Verify(byte[] Data, byte[] PublicKey, HashFunction HashFunction,
            int ScalarBytes, byte MsbMask, PrimeFieldCurve Curve, byte[] Signature)
        {
            int c = Signature.Length;
            if (c != ScalarBytes << 1)
                return false;

            c >>= 1;

            byte[] Bin = new byte[c];
            Array.Copy(Signature, 0, Bin, 0, c);

            BigInteger r = EllipticCurve.ToInt(Bin);

            Bin = new byte[c];
            Array.Copy(Signature, c, Bin, 0, c);

            BigInteger s = EllipticCurve.ToInt(Bin);
            PointOnCurve PublicKeyPoint = Curve.Decode(PublicKey);

            if (!PublicKeyPoint.NonZero || r.IsZero || s.IsZero || r >= Curve.Order || s >= Curve.Order)
                return false;

            BigInteger e = CalcE(Data, HashFunction, ScalarBytes, MsbMask);
            BigInteger w = Curve.ModulusN.Invert(s);
            BigInteger u1 = Curve.ModulusN.Multiply(e, w);
            BigInteger u2 = Curve.ModulusN.Multiply(r, w);
            PointOnCurve P2 = Curve.ScalarMultiplication(u1, Curve.BasePoint, true);
            PointOnCurve P3 = Curve.ScalarMultiplication(u2, PublicKeyPoint, true);
            Curve.AddTo(ref P2, P3);

            if (!P2.NonZero)
                return false;

            P2.Normalize(Curve);

            BigInteger Compare = BigInteger.Remainder(P2.X, Curve.Order);
            if (Compare.Sign < 0)
                Compare += Curve.Order;

            return Compare == r;
        }

    }
}
