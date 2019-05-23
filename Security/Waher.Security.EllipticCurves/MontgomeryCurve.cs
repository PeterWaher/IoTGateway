using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Base class of Montgomery curves (y²=x³+Ax²+x), with biratinal Edwards equivalent 
    /// over a prime field.
    /// </summary>
    public abstract class MontgomeryCurve : PrimeFieldCurve
    {
        private EdwardsCurveBase pair = null;

        /// <summary>
        /// Base class of Montgomery curves (y²=x³+Ax²+x), with biratinal Edwards equivalent 
        /// over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (U,V) coordinates.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        public MontgomeryCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order, 
            int Cofactor)
            : base(Prime, BasePoint, Order, Cofactor)
        {
        }

        /// <summary>
        /// Base class of Montgomery curves, with biratinal Edwards equivalent 
        /// over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (U,V) coordinates.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        /// <param name="Secret">Secret.</param>
        public MontgomeryCurve(BigInteger Prime,  PointOnCurve BasePoint, BigInteger Order, 
            int Cofactor, byte[] Secret)
            : base(Prime, BasePoint, Order, Cofactor, Secret)
        {
        }

        /// <summary>
        /// a Coefficient in the definition of the curve E:	v²=u³+A*u²+u
        /// </summary>
        protected abstract BigInteger A
        {
            get;
        }

        /// <summary>
        /// Converts a pair of (U,V) coordinates to a pair of (X,Y) coordinates
        /// in the birational Edwards curve.
        /// </summary>
        /// <param name="UV">(U,V) coordinates.</param>
        /// <returns>(X,Y) coordinates.</returns>
        public abstract PointOnCurve ToXY(PointOnCurve UV);

        /// <summary>
        /// Converts a pair of (X,Y) coordinates for the birational Edwards curve
        /// to a pair of (U,V) coordinates.
        /// </summary>
        /// <param name="XY">(X,Y) coordinates.</param>
        /// <returns>(U,V) coordinates.</returns>
        public abstract PointOnCurve ToUV(PointOnCurve XY);

        /// <summary>
        /// Adds <paramref name="Q"/> to <paramref name="P"/>.
        /// </summary>
        /// <param name="P">Point 1.</param>
        /// <param name="Q">Point 2.</param>
        /// <returns>P+Q</returns>
        public override void AddTo(ref PointOnCurve P, PointOnCurve Q)
        {
            this.Double(ref P);
        }

        /// <summary>
        /// Doubles a point on the curve.
        /// </summary>
        /// <param name="P">Point</param>
        public override void Double(ref PointOnCurve P)
        {
            throw new NotSupportedException("Scalar  multiplication is performed using a Montgomery ladder.");
        }

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
        /// </summary>
        /// <param name="N">Scalar</param>
        /// <param name="U">U-coordinate of point</param>
        /// <returns><paramref name="N"/>*<paramref name="U"/></returns>
        public BigInteger ScalarMultiplication(BigInteger N, BigInteger U)
        {
            return this.ScalarMultiplication(N.ToByteArray(), U);
        }

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
        /// </summary>
        /// <param name="N">Scalar</param>
        /// <param name="U">U-coordinate of point</param>
        /// <returns><paramref name="N"/>*<paramref name="U"/></returns>
        public abstract BigInteger ScalarMultiplication(byte[] N, BigInteger U);

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="P"/>.
        /// </summary>
        /// <param name="N">Scalar, in binary, little-endian form.</param>
        /// <param name="P">Point</param>
        /// <param name="Normalize">If normalized output is expected.</param>
        /// <returns><paramref name="N"/>*<paramref name="P"/></returns>
        public override PointOnCurve ScalarMultiplication(byte[] N, PointOnCurve P, bool Normalize)
        {
            return new PointOnCurve(this.ScalarMultiplication(N, P.X), BigInteger.Zero);
        }

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
        /// </summary>
        /// <param name="N">Scalar</param>
        /// <param name="U">U-coordinate of point.</param>
        /// <param name="A24">(A-2)/4</param>
        /// <param name="p">Prime</param>
        /// <param name="Bits">Number of bits</param>
        /// <param name="LsbMask">Mask for Least Significant Byte.</param>
        /// <param name="MsbMask">Mask for Most Significant Byte.</param>
        /// <param name="MsbBit">Most Significat Bit to set.</param>
        /// <returns><paramref name="N"/>*<paramref name="U"/></returns>
        public static BigInteger XFunction(byte[] N, BigInteger U,
            BigInteger A24, BigInteger p, int Bits, byte LsbMask, byte MsbMask, byte MsbBit)
        {
            BigInteger x1 = U;
            BigInteger x2 = BigInteger.One;
            BigInteger z2 = BigInteger.Zero;
            BigInteger x3 = U;
            BigInteger z3 = BigInteger.One;
            BigInteger A, AA, B, BB, E, C, D, DA, CB;
            int kt;
            int swap = 0;

            kt = (Bits + 7) >> 3;
            if (N.Length < kt)
                Array.Resize<byte>(ref N, kt);

            //N[0] &= LsbMask;
            //N[--kt] &= MsbMask;
            //N[kt] |= MsbBit;

            while (--Bits >= 0)
            {
                kt = (N[Bits >> 3] >> (Bits & 7)) & 1;
                swap ^= kt;
                ConditionalSwap(swap, ref x2, ref x3);
                ConditionalSwap(swap, ref z2, ref z3);
                swap = kt;

                A = BigInteger.Remainder(x2 + z2, p);
                AA = BigInteger.Remainder(A * A, p);
                B = BigInteger.Remainder(x2 - z2, p);
                BB = BigInteger.Remainder(B * B, p);
                E = BigInteger.Remainder(AA - BB, p);
                C = BigInteger.Remainder(x3 + z3, p);
                D = BigInteger.Remainder(x3 - z3, p);
                DA = BigInteger.Remainder(D * A, p);
                CB = BigInteger.Remainder(C * B, p);

                x3 = DA + CB;
                x3 = BigInteger.Remainder(x3 * x3, p);
                z3 = DA - CB;
                z3 = BigInteger.Remainder(x1 * BigInteger.Remainder(z3 * z3, p), p);
                x2 = BigInteger.Remainder(AA * BB, p);
                z2 = BigInteger.Remainder(E * (AA + BigInteger.Remainder(A24 * E, p)), p);
            }

            ConditionalSwap(swap, ref x2, ref x3);
            ConditionalSwap(swap, ref z2, ref z3);

            BigInteger Result = BigInteger.Remainder(x2 * BigInteger.ModPow(z2, p - Two, p), p);
            if (Result.Sign < 0)
                Result += p;

            return Result;
        }

        /// <summary>
        /// Swaps <paramref name="I2"/> and <paramref name="I3"/> if 
        /// <paramref name="swap"/> != 0, in a way that minimizes the risk
        /// of a side-channel attack measuring CPU timing, to deduce the bits
        /// used in private keys.
        /// </summary>
        /// <param name="swap">If integers are to be swapped.</param>
        /// <param name="I2">First integer.</param>
        /// <param name="I3">Second integer.</param>
        private static void ConditionalSwap(int swap, ref BigInteger I2, ref BigInteger I3)
        {
            byte[] x2 = I2.ToByteArray();
            byte[] x3 = I3.ToByteArray();
            int i, c = x2.Length, d = x3.Length;
            byte Dummy;
            byte Mask;
            bool Sign;

            if (c < d)
            {
                Sign = (x2[c - 1] & 0x80) != 0;
                Array.Resize<byte>(ref x2, d);

                if (Sign)
                {
                    while (c < d)
                        x2[c++] = 0xff;
                }
                else
                    c = d;
            }
            else if (d < c)
            {
                Sign = (x3[d - 1] & 0x80) != 0;
                Array.Resize<byte>(ref x3, c);

                if (Sign)
                {
                    while (d < c)
                        x3[d++] = 0xff;
                }
                //else
                //	d = c;
            }

            Mask = (byte)(0xff * swap);

            for (i = 0; i < c; i++)
            {
                Dummy = (byte)(Mask & (x2[i] ^ x3[i]));
                x2[i] ^= Dummy;
                x3[i] ^= Dummy;
            }

            I2 = ToInt(x2);
            I3 = ToInt(x3);
        }

        /// <summary>
        /// Edwards Curve pair.
        /// </summary>
        public EdwardsCurveBase Pair
        {
            get
            {
                if (this.pair is null)
                    this.pair = this.CreatePair();

                return this.pair;
            }
        }

        /// <summary>
        /// Creates the Edwards Curve pair.
        /// </summary>
        /// <returns>Edwards curve.</returns>
        public abstract EdwardsCurveBase CreatePair();

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            return this.Pair.Sign(Data);
        }

        /// <summary>
        /// Public key.
        /// </summary>
        public override PointOnCurve PublicKeyPoint
        {
            get
            {
                BigInteger V = this.publicKeyPoint.Y;

                if (V.IsZero)
                {
                    BigInteger U = this.publicKeyPoint.X;
                    BigInteger U2 = this.modP.Multiply(U, U);
                    BigInteger U3 = this.modP.Multiply(U, U2);
                    BigInteger V2 = BigInteger.Remainder(U3 + this.modP.Multiply(this.A, U2) + U, this.Prime);

                    BigInteger V1 = this.modP.Sqrt(V2);
                    if (V1.Sign < 0)
                        V1 += this.Prime;

                    V = this.Prime - V1;
                    if (V1 < V)
                        V = V1;

                    this.publicKeyPoint.Y = V;
                }

                return this.publicKeyPoint;
            }
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(byte[] Data, byte[] PublicKey, byte[] Signature)
        {
            PointOnCurve UV = this.Decode(PublicKey);
            PointOnCurve XY = this.ToXY(UV);
            EdwardsCurveBase Pair = this.Pair;

            return Pair.Verify(Data, Pair.Encode(XY), Signature);
        }

    }
}
