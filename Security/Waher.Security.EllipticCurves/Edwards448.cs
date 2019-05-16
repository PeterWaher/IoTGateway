using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Waher.Security.SHA3;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Edwards448 Elliptic Curve, as defined in RFC7748 & RFC8032:
    /// https://tools.ietf.org/html/rfc7748
    /// https://tools.ietf.org/html/rfc8032
    /// </summary>
    public class Edwards448 : EdwardsCurve
    {
        private static readonly BigInteger p0 = BigInteger.Pow(2, 448) - BigInteger.Pow(2, 224) - 1;
        private static readonly BigInteger d = p0 - 39081;
        private static readonly BigInteger n = BigInteger.Pow(2, 446) - BigInteger.Parse("8335dc163bb124b65129c96fde933d8d723a70aadc873d6d54a7bb0d", NumberStyles.HexNumber);
        private static readonly BigInteger BasePointX = BigInteger.Parse("224580040295924300187604334099896036246789641632564134246125461686950415467406032909029192869357953282578032075146446173674602635247710");
        private static readonly BigInteger BasePointY = BigInteger.Parse("298819210078481492676017930443930673437544040154080242095928241372331506189835876003536878655418784733982303233503462500531545062832660");
        private const int cofactor = 4;
        private SHAKE256 shake256_114;

        /// <summary>
        /// Edwards448 Elliptic Curve, as defined in RFC7748 & RFC8032:
        /// https://tools.ietf.org/html/rfc7748
        /// https://tools.ietf.org/html/rfc8032
        /// </summary>
        public Edwards448()
            : base(p0, new PointOnCurve(BasePointX, BasePointY), n, cofactor)
        {
            this.Init();
        }

        /// <summary>
        /// Edwards448 Elliptic Curve, as defined in RFC7748 & RFC8032:
        /// https://tools.ietf.org/html/rfc7748
        /// https://tools.ietf.org/html/rfc8032
        /// </summary>
        /// <param name="D">Private key.</param>
        public Edwards448(BigInteger D)
            : base(p0, new PointOnCurve(BasePointX, BasePointY), n, cofactor, D)
        {
            this.Init();
        }

        private void Init()
        {
            this.shake256_114 = new SHAKE256(114 << 3);
        }

        /// <summary>
        /// Name of curve.
        /// </summary>
        public override string CurveName => "Edwards448";

        /// <summary>
        /// d coefficient of Edwards curve.
        /// </summary>
        protected override BigInteger D => d;

        /// <summary>
        /// Number of bits used to encode the y-coordinate.
        /// </summary>
        public override int CoordinateBits => 447;

        /// <summary>
        /// Adds <paramref name="Q"/> to <paramref name="P"/>.
        /// </summary>
        /// <param name="P">Point 1.</param>
        /// <param name="Q">Point 2.</param>
        /// <returns>P+Q</returns>
        public override void AddTo(ref PointOnCurve P, PointOnCurve Q)
        {
            if (!P.IsHomogeneous)
                P.Z = BigInteger.One;

            if (!Q.IsHomogeneous)
                Q.Z = BigInteger.One;

            BigInteger A = this.Multiply(P.Z, Q.Z);
            BigInteger B = this.Multiply(A, A);
            BigInteger C = this.Multiply(P.X, Q.X);
            BigInteger D = this.Multiply(P.Y, Q.Y);
            BigInteger E = this.Multiply(this.Multiply(this.D, C), D);
            BigInteger F = this.Subtract(B, E);
            BigInteger G = this.Add(B, E);
            BigInteger H = this.Multiply(P.X + P.Y, Q.X + Q.Y);

            P.X = this.Multiply(A, this.Multiply(F, H - C - D));
            P.Y = this.Multiply(A, this.Multiply(G, D - C));
            P.Z = this.Multiply(F, G);
        }

        /// <summary>
        /// Doubles a point on the curve.
        /// </summary>
        /// <param name="P">Point</param>
        public override void Double(ref PointOnCurve P)
        {
            if (!P.IsHomogeneous)
                P.Z = BigInteger.One;

            /*BigInteger x1s = this.Multiply(P.X, P.X);
            BigInteger y1s = this.Multiply(P.Y, P.Y);
            BigInteger z1s = this.Multiply(P.Z, P.Z);
            BigInteger xys = this.Add(P.X, P.Y);
            BigInteger F = this.Add(x1s, y1s);
            BigInteger J2 = this.Subtract(F, this.Add(z1s, z1s));
            BigInteger X = this.Multiply(this.Subtract(this.Multiply(xys, xys), x1s + y1s), J2);
            BigInteger Y = this.Multiply(F, x1s - y1s);
            BigInteger Z = this.Multiply(F, J2);*/
                 
            BigInteger A = this.Add(P.X, P.Y);
            BigInteger B = this.Multiply(A, A);
            BigInteger C = this.Multiply(P.X, P.X);
            BigInteger D = this.Multiply(P.Y, P.Y);
            BigInteger E = this.Add(C, D);
            BigInteger H = this.Multiply(P.Z, P.Z);
            BigInteger J = this.Subtract(E, Two * H);

            P.X = this.Multiply(B - E, J);
            P.Y = this.Multiply(E, C - D);
            P.Z = this.Multiply(E, J);

            /*BigInteger A1 = this.Multiply(X, P.Z);
            BigInteger A2 = this.Multiply(P.X, Z);
            BigInteger B1 = this.Multiply(Y, P.Z);
            BigInteger B2 = this.Multiply(P.Y, Z);

            if (A1.Sign < 0)
                A1 += this.p;

            if (A2.Sign < 0)
                A2 += this.p;

            if (B1.Sign < 0)
                B1 += this.p;

            if (B2.Sign < 0)
                B2 += this.p;

            if (A1 != A2 || B1 != B2)
                throw new Exception("Double incorrect");*/
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            return EdDSA.Sign(Data, this.privateKey, 
                Bin => this.shake256_114.ComputeVariable(Bin),
                this.orderBits, this);
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
            return EdDSA.Verify(Data, PublicKey, 
                Bin => this.shake256_114.ComputeVariable(Bin), this.orderBits,
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

            BigInteger v = this.Multiply(this.D, y2) - BigInteger.One;
            BigInteger v2 = this.Multiply(v, v);
            BigInteger v3 = this.Multiply(v, v2);
            BigInteger u2 = this.Multiply(u, u);
            BigInteger u3 = this.Multiply(u, u2);
            BigInteger u5 = this.Multiply(u2, u3);
            BigInteger x = this.Multiply(this.Multiply(u3, v),
                BigInteger.ModPow(this.Multiply(u5, v3), this.P34, this.p));

            BigInteger x2 = this.Multiply(x, x);
            BigInteger Test = this.Multiply(v, x2);
            if (Test.Sign < 0)
                Test += this.p;

            if (Test != u)
                throw new ArgumentException("Not a valid point.", nameof(Y));

            if (X0)
            {
                if (x.IsZero)
                    throw new ArgumentException("Not a valid point.", nameof(Y));

                if (x.IsEven)
                    x = this.p - x;
            }
            else if (!x.IsEven)
                x = this.p - x;

            return x;
        }

    }
}
