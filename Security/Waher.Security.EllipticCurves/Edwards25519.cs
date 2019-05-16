using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Edwards25519 Elliptic Curve, as defined in RFC7748 & RFC8032:
    /// https://tools.ietf.org/html/rfc7748
    /// https://tools.ietf.org/html/rfc8032
    /// </summary>
    public class Edwards25519 : EdwardsCurve
    {
        private static readonly BigInteger p0 = BigInteger.Pow(2, 255) - 19;
        private static readonly BigInteger d = BigInteger.Parse("37095705934669439343138083508754565189542113879843219016388785533085940283555");
        private static readonly BigInteger d2 = BigInteger.Remainder(BigInteger.Multiply(d, 2), p0);
        private static readonly BigInteger n = BigInteger.Pow(2, 252) + BigInteger.Parse("14def9dea2f79cd65812631a5cf5d3ed", NumberStyles.HexNumber);
        private static readonly BigInteger BasePointX = BigInteger.Parse("15112221349535400772501151409588531511454012693041857206046113283949847762202");
        private static readonly BigInteger BasePointY = BigInteger.Parse("46316835694926478169428394003475163141307993866256225615783033603165251855960");
        private const int cofactor = 8;

        /// <summary>
        /// Edwards25519 Elliptic Curve, as defined in RFC7748 & RFC8032:
        /// https://tools.ietf.org/html/rfc7748
        /// https://tools.ietf.org/html/rfc8032
        /// </summary>
        public Edwards25519()
            : base(p0, new PointOnCurve(BasePointX, BasePointY), n, cofactor)
        {
        }

        /// <summary>
        /// Edwards25519 Elliptic Curve, as defined in RFC7748 & RFC8032:
        /// https://tools.ietf.org/html/rfc7748
        /// https://tools.ietf.org/html/rfc8032
        /// </summary>
        /// <param name="D">Private key.</param>
        public Edwards25519(BigInteger D)
            : base(p0, new PointOnCurve(BasePointX, BasePointY), n, cofactor, D)
        {
        }

        /// <summary>
        /// Name of curve.
        /// </summary>
        public override string CurveName => "Edwards25519";

        /// <summary>
        /// Number of bits used to encode the y-coordinate.
        /// </summary>
        public override int CoordinateBits => 253;

        /// <summary>
        /// d coefficient of Edwards curve.
        /// </summary>
        protected override BigInteger D => d;

        /// <summary>
        /// Adds <paramref name="Q"/> to <paramref name="P"/>.
        /// </summary>
        /// <param name="P">Point 1.</param>
        /// <param name="Q">Point 2.</param>
        /// <returns>P+Q</returns>
        public override void AddTo(ref PointOnCurve P, PointOnCurve Q)
        {
            if (!P.IsHomogeneous)
                P.T = P.X * P.Y;

            if (!Q.IsHomogeneous)
                Q.T = Q.X * Q.Y;

            BigInteger A = this.Multiply(P.Y - P.X, Q.Y - Q.X);
            BigInteger B = this.Multiply(P.Y + P.X, Q.Y + Q.X);
            BigInteger C = this.Multiply(this.Multiply(d2, P.T), Q.T);
            BigInteger D = this.Multiply(2 * P.Z, Q.Z);
            BigInteger E = this.Subtract(B, A);
            BigInteger F = this.Subtract(D, C);
            BigInteger G = this.Add(D, C);
            BigInteger H = this.Add(B, A);

            P.X = this.Multiply(E, F);
            P.Y = this.Multiply(G, H);
            P.T = this.Multiply(E, H);
            P.Z = this.Multiply(F, G);
        }

        /// <summary>
        /// Doubles a point on the curve.
        /// </summary>
        /// <param name="P">Point</param>
        public override void Double(ref PointOnCurve P)
        {
            if (!P.IsHomogeneous)
                P.T = P.X * P.Y;

            BigInteger A = this.Multiply(P.X, P.X);
            BigInteger B = this.Multiply(P.Y, P.Y);
            BigInteger C = this.Multiply(2 * P.Z, P.Z);
            BigInteger H = this.Add(B, A);
            BigInteger E = P.X + P.Y;
            E = this.Subtract(H, this.Multiply(E, E));
            BigInteger G = this.Subtract(A, B);
            BigInteger F = this.Add(C, G);

            P.X = this.Multiply(E, F);
            P.Y = this.Multiply(G, H);
            P.T = this.Multiply(E, H);
            P.Z = this.Multiply(F, G);
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            return EdDSA.Sign(Data, this.privateKey, Hashes.ComputeSHA512Hash, this.orderBits, this);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(byte[] Data, PointOnCurve PublicKey, byte[] Signature)
        {
            return EdDSA.Verify(Data, PublicKey, Hashes.ComputeSHA512Hash, this.orderBits,
                this, Signature);
        }

        /// <summary>
        /// Gets the X-coordinate that corresponds to a given Y-coordainte, and the 
        /// first bit of the X-coordinate.
        /// </summary>
        /// <param name="Y">Y-coordinate.</param>
        /// <param name="X0">First bit of X-coordinate.</param>
        /// <returns>X-coordinate</returns>
        public override BigInteger GetX(BigInteger Y, bool X0)
        {
            BigInteger y2 = this.Multiply(Y, Y);
            BigInteger u = y2 - BigInteger.One;
            if (u.Sign < 0)
                u += this.p;

            BigInteger v = this.Multiply(this.D, y2) + BigInteger.One;
            BigInteger v2 = this.Multiply(v, v);
            BigInteger v3 = this.Multiply(v, v2);
            BigInteger v4 = this.Multiply(v2, v2);
            BigInteger v7 = this.Multiply(v3, v4);
            BigInteger x = this.Multiply(this.Multiply(u, v3),
                BigInteger.ModPow(this.Multiply(u, v7), this.P58, this.Prime));

            BigInteger x2 = this.Multiply(x, x);
            BigInteger Test = this.Multiply(v, x2);
            if (Test.Sign < 0)
                Test += this.Prime;

            if (Test != u)
            {
                if (Test == this.Prime - u)
                    x = this.Multiply(x, this.TwoP14);
                else
                    throw new ArgumentException("Not a valid point.", nameof(Y));
            }

            if (X0)
            {
                if (x.IsZero)
                    throw new ArgumentException("Not a valid point.", nameof(Y));

                if (x.IsEven)
                    x = this.Prime - x;
            }
            else if (!x.IsEven)
                x = this.Prime - x;

            return x;
        }

    }
}
