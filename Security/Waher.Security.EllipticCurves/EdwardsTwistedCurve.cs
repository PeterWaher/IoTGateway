using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Base class of Twisted Edwards curves (-x²+y²=1+dx²y²) over a prime field.
    /// </summary>
    public abstract class EdwardsTwistedCurve : EdwardsCurveBase
    {
        private BigInteger p58;
        private BigInteger p34;
        private BigInteger twoP14;

        /// <summary>
        /// Base class of Twisted Edwards curves (-x²+y²=1+dx²y²) over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
        /// <param name="d">Coefficient in the curve equation (-x²+y²=1+dx²y²)</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        public EdwardsTwistedCurve(BigInteger Prime, PointOnCurve BasePoint,
            BigInteger d, BigInteger Order, int Cofactor)
            : base(Prime, BasePoint, d, Order, Cofactor)
        {
            this.Init();
        }

        /// <summary>
        /// Base class of Twisted Edwards curves (-x²+y²=1+dx²y²) over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
        /// <param name="d">Coefficient in the curve equation (-x²+y²=1+dx²y²)</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        /// <param name="Secret">Secret.</param>
        public EdwardsTwistedCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger d,
            BigInteger Order, int Cofactor, byte[] Secret)
            : base(Prime, BasePoint, d, Order, Cofactor, Secret)
        {
            this.Init();
        }

        private void Init()
        {
            this.p58 = (this.p - 5) / 8;
            this.p34 = (this.p - 3) / 4;
            this.twoP14 = BigInteger.ModPow(Two, (this.p - 1) / 4, this.p);
        }

        /// <summary>
        /// (p-5)/8
        /// </summary>
        public BigInteger P58 => this.p58;

        /// <summary>
        /// (p-3)/4
        /// </summary>
        public BigInteger P34 => this.p34;

        /// <summary>
        /// 2^((p-1)/4) mod p
        /// </summary>
        public BigInteger TwoP14 => this.twoP14;

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

            BigInteger A = this.modP.Multiply(P.Y - P.X, Q.Y - Q.X);
            BigInteger B = this.modP.Multiply(P.Y + P.X, Q.Y + Q.X);
            BigInteger C = this.modP.Multiply(this.modP.Multiply(d2, P.T), Q.T);
            BigInteger D = this.modP.Multiply(P.Z << 1, Q.Z);
            BigInteger E = this.modP.Subtract(B, A);
            BigInteger F = this.modP.Subtract(D, C);
            BigInteger G = this.modP.Add(D, C);
            BigInteger H = this.modP.Add(B, A);

            P.X = this.modP.Multiply(E, F);
            P.Y = this.modP.Multiply(G, H);
            P.T = this.modP.Multiply(E, H);
            P.Z = this.modP.Multiply(F, G);
        }

        /// <summary>
        /// Doubles a point on the curve.
        /// </summary>
        /// <param name="P">Point</param>
        public override void Double(ref PointOnCurve P)
        {
            if (!P.IsHomogeneous)
                P.T = P.X * P.Y;

            BigInteger A = P.Y - P.X;
            A = this.modP.Multiply(A, A);

            BigInteger B = P.Y + P.X;
            B = this.modP.Multiply(B, B);

            BigInteger C = this.modP.Multiply(this.modP.Multiply(d2, P.T), P.T);
            BigInteger D = this.modP.Multiply(P.Z << 1, P.Z);
            BigInteger E = this.modP.Subtract(B, A);
            BigInteger F = this.modP.Subtract(D, C);
            BigInteger G = this.modP.Add(D, C);
            BigInteger H = this.modP.Add(B, A);

            P.X = this.modP.Multiply(E, F);
            P.Y = this.modP.Multiply(G, H);
            P.T = this.modP.Multiply(E, H);
            P.Z = this.modP.Multiply(F, G);
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
            BigInteger y2 = this.modP.Multiply(Y, Y);
            BigInteger u = y2 - BigInteger.One;
            if (u.Sign < 0)
                u += this.p;

            BigInteger v = this.modP.Multiply(this.d, y2) + BigInteger.One;
            BigInteger v2 = this.modP.Multiply(v, v);
            BigInteger v3 = this.modP.Multiply(v, v2);
            BigInteger v4 = this.modP.Multiply(v2, v2);
            BigInteger v7 = this.modP.Multiply(v3, v4);
            BigInteger x = this.modP.Multiply(this.modP.Multiply(u, v3),
                BigInteger.ModPow(this.modP.Multiply(u, v7), this.P58, this.Prime));

            BigInteger x2 = this.modP.Multiply(x, x);
            BigInteger Test = this.modP.Multiply(v, x2);
            if (Test.Sign < 0)
                Test += this.Prime;

            if (Test != u)
            {
                if (Test == this.Prime - u)
                    x = this.modP.Multiply(x, this.TwoP14);
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
