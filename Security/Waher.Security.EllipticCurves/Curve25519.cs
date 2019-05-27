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
        private static readonly BigInteger A0 = 486662;
        private static readonly BigInteger A24 = (A0 - 2) / 4;
        private static readonly BigInteger n0 = BigInteger.Pow(2, 252) + BigInteger.Parse("14def9dea2f79cd65812631a5cf5d3ed", NumberStyles.HexNumber);
        private static readonly BigInteger BasePointU = 9;
        private static readonly BigInteger BasePointV = BigInteger.Parse("14781619447589544791020593568409986887264606134616475288964881837755586237401");
        private static readonly BigInteger SqrtMinus486664 = ModulusP.SqrtModP(-486664, p0);

        /// <summary>
        /// Curve25519, as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        public Curve25519()
            : base(p0, new PointOnCurve(BasePointU, BasePointV), n0, Cofactor: 8)
        {
        }

        /// <summary>
        /// Curve25519, as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        /// <param name="Secret">Secret.</param>
        public Curve25519(byte[] Secret)
            : base(p0, new PointOnCurve(BasePointU, BasePointV), n0, Cofactor: 8, Secret)
        {
        }

        /// <summary>
        /// Name of curve.
        /// </summary>
        public override string CurveName => "Curve25519";

        /// <summary>
        /// a Coefficient in the definition of the curve E:	v²=u³+A*u²+u
        /// </summary>
        protected override BigInteger A => A0;

        /// <summary>
        /// Converts a pair of (U,V) coordinates to a pair of (X,Y) coordinates
        /// in the birational Edwards curve.
        /// </summary>
        /// <param name="UV">(U,V) coordinates.</param>
        /// <returns>(X,Y) coordinates.</returns>
        public override PointOnCurve ToXY(PointOnCurve UV)
        {
            BigInteger X = this.modP.Multiply(SqrtMinus486664, this.modP.Divide(UV.X, UV.Y));
            BigInteger Y = this.modP.Divide(UV.X - BigInteger.One, UV.X + BigInteger.One);

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
            BigInteger U = this.modP.Divide(XY.Y + BigInteger.One, BigInteger.One - XY.Y);
            BigInteger V = this.modP.Multiply(SqrtMinus486664, this.modP.Divide(U, XY.X));

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
        public override BigInteger ScalarMultiplication(byte[] N, BigInteger U)
        {
            return XFunction(N, U, A24, this.p, 255);
        }

        /// <summary>
        /// Calculates a private key from a secret.
        /// </summary>
        /// <param name="Secret">Binary secret.</param>
        /// <returns>Private key</returns>
        public override Tuple<byte[], byte[]> CalculatePrivateKey(byte[] Secret)
        {
            byte[] Bin = Secret;

            if (Bin.Length != 32)
                Bin = Hashes.ComputeSHA256Hash(Secret);

            Bin[0] &= 0xf8;
            Bin[31] &= 0x3f;
            Bin[31] |= 0x40;

            return new Tuple<byte[], byte[]>(Bin, null);
        }

        /// <summary>
        /// Creates the Edwards Curve pair.
        /// </summary>
        /// <returns>Edwards curve.</returns>
        public override EdwardsCurveBase CreatePair()
        {
            PointOnCurve PublicKeyUV = this.PublicKeyPoint;
            PointOnCurve PublicKeyXY = this.ToXY(PublicKeyUV);

            Edwards25519 Candidate = new Edwards25519(this.PrivateKey, false);
            PointOnCurve PublicKeyXY2 = Candidate.PublicKeyPoint;

            if (!PublicKeyXY.Y.Equals(PublicKeyXY2.Y))
                throw new InvalidOperationException("Unable to create pair curve.");

            return Candidate;
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the XEdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            return XEdDSA.Sign(Data, this.PrivateKey, Hashes.ComputeSHA512Hash, this);
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
            return XEdDSA.Verify(Data, PublicKey, Hashes.ComputeSHA512Hash, this, 
                Signature, 255, 253);
        }

    }
}
