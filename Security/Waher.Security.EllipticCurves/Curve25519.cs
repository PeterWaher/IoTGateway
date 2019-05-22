using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Curve25519, as defined in RFC 7748:
    /// https://tools.ietf.org/html/rfc7748
    /// </summary>
    public class Curve25519 : MontgomeryCurve
    {
        private static readonly BigInteger p0 = BigInteger.Pow(2, 255) - 19;
        private static readonly BigInteger A = 486662;
        private static readonly BigInteger A24 = (A - 2) / 4;
        private static readonly BigInteger n = BigInteger.Pow(2, 252) + BigInteger.Parse("14def9dea2f79cd65812631a5cf5d3ed", NumberStyles.HexNumber);
        private static readonly BigInteger BasePointU = 9;
        private static readonly BigInteger BasePointV = BigInteger.Parse("14781619447589544791020593568409986887264606134616475288964881837755586237401");
        private static readonly BigInteger SqrtMinus486664 = ModulusP.SqrtModP(-486664, p0);
        private const int cofactor = 8;

        /// <summary>
        /// Curve25519, as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        public Curve25519()
            : base(p0, A, new PointOnCurve(BasePointU, BasePointV), n, cofactor)
        {
        }

        /// <summary>
        /// Curve25519, as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        /// <param name="D">Private key.</param>
        public Curve25519(BigInteger D)
            : base(p0, A, new PointOnCurve(BasePointU, BasePointV), n, cofactor, D)
        {
        }

        /// <summary>
        /// Name of curve.
        /// </summary>
        public override string CurveName => "Curve25519";


        /// <summary>
        /// Converts a pair of (U,V) coordinates to a pair of (X,Y) coordinates
        /// in the birational Edwards curve.
        /// </summary>
        /// <param name="UV">(U,V) coordinates.</param>
        /// <returns>(X,Y) coordinates.</returns>
        public override PointOnCurve ToXY(PointOnCurve UV)
        {
            BigInteger X = this.Multiply(SqrtMinus486664, this.Divide(UV.X, UV.Y));
            BigInteger Y = this.Divide(UV.X - BigInteger.One, UV.X + BigInteger.One);

            if (X.Sign < 0)
                X += this.p;

            if (Y.Sign < 0)
                Y += this.p;

            return new PointOnCurve(X, Y);
        }

        /// <summary>
        /// Converts a pair of (X,Y) coordinates for the birational Edwards curve
        /// to a pair of (U,V) coordinates.
        /// </summary>
        /// <param name="XY">(X,Y) coordinates.</param>
        /// <returns>(U,V) coordinates.</returns>
        public override PointOnCurve ToUV(PointOnCurve XY)
        {
            BigInteger U = this.Divide(XY.Y + BigInteger.One, BigInteger.One - XY.Y);
            BigInteger V = this.Multiply(SqrtMinus486664, this.Divide(U, XY.X));

            if (U.Sign < 0)
                U += this.p;

            if (V.Sign < 0)
                V += this.p;

            return new PointOnCurve(U, V);
        }

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
        /// </summary>
        /// <param name="N">Scalar</param>
        /// <param name="U">U-coordinate of point</param>
        /// <returns><paramref name="N"/>*<paramref name="U"/></returns>
        public override BigInteger ScalarMultiplication(BigInteger N, BigInteger U)
        {
            return XFunction(N, U, A24, this.p, 255, 0xf8, 0x3f, 0x40);
        }

        /// <summary>
        /// Creates the Edwards Curve pair.
        /// </summary>
        /// <returns>Edwards curve.</returns>
        public override EdwardsCurve CreatePair()
        {
            PointOnCurve PublicKeyUV = this.PublicKey;
            PointOnCurve PublicKeyXY = this.ToXY(PublicKeyUV);

            byte[] Bin = this.privateKey.ToByteArray();
            if (Bin.Length != 32)
                Array.Resize<byte>(ref Bin, 32);

            Bin[0] &= 0xf8;
            Bin[31] &= 0x3f;
            Bin[31] |= 0x40;

            BigInteger PrivateKey = new BigInteger(Bin);
            BigInteger PrivateKey2 = BigInteger.Remainder(this.Order - PrivateKey, this.Order);
            if (PrivateKey2.Sign < 0)
                PrivateKey2 += this.Order;

            Edwards25519 Candidate = new Edwards25519(PrivateKey2);
            PointOnCurve PublicKeyXY2 = Candidate.PublicKey;

            if (PublicKeyXY.Y.Equals(PublicKeyXY2.Y))
                return Candidate;

            Candidate = new Edwards25519(PrivateKey);
            PublicKeyXY2 = Candidate.PublicKey;

            if (PublicKeyXY.Y.Equals(PublicKeyXY2.Y))
                return Candidate;

            throw new InvalidOperationException("Unable to create pair curve.");
        }

    }
}
