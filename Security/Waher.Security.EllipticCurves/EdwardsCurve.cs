using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Base class of Edwards curves (x²+y²=1+dx²y²) over a prime field.
    /// </summary>
    public abstract class EdwardsCurve : EdwardsCurveBase
	{
        private BigInteger p58;
        private BigInteger p34;
        private BigInteger twoP14;

        /// <summary>
        /// Base class of Edwards curves (x²+y²=1+dx²y²) over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
        /// <param name="d">Coefficient in the curve equation (x²+y²=1+dx²y²)</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        public EdwardsCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger d,
            BigInteger Order, int Cofactor)
			: base(Prime, BasePoint, d, Order, Cofactor)
		{
            this.Init();
        }

        /// <summary>
        /// Base class of Edwards curves (x²+y²=1+dx²y²) over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
        /// <param name="d">Coefficient in the curve equation (x²+y²=1+dx²y²)</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        /// <param name="Secret">Secret.</param>
        public EdwardsCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger d, 
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
        /// d coefficient of Edwards curve.
        /// </summary>
        protected abstract BigInteger D
        {
            get;
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
        /// Neutral point.
        /// </summary>
        public override PointOnCurve Zero
        {
            get
            {
                return new PointOnCurve(BigInteger.Zero, BigInteger.One);
            }
        }

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

            BigInteger A = this.modP.Multiply(P.Z, Q.Z);
            BigInteger B = this.modP.Multiply(A, A);
            BigInteger C = this.modP.Multiply(P.X, Q.X);
            BigInteger D = this.modP.Multiply(P.Y, Q.Y);
            BigInteger E = this.modP.Multiply(this.modP.Multiply(this.D, C), D);
            BigInteger F = this.modP.Subtract(B, E);
            BigInteger G = this.modP.Add(B, E);
            BigInteger H = this.modP.Multiply(P.X + P.Y, Q.X + Q.Y);

            P.X = this.modP.Multiply(A, this.modP.Multiply(F, H - C - D));
            P.Y = this.modP.Multiply(A, this.modP.Multiply(G, D - C));
            P.Z = this.modP.Multiply(F, G);
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

            BigInteger A = this.modP.Add(P.X, P.Y);
            BigInteger B = this.modP.Multiply(A, A);
            BigInteger C = this.modP.Multiply(P.X, P.X);
            BigInteger D = this.modP.Multiply(P.Y, P.Y);
            BigInteger E = this.modP.Add(C, D);
            BigInteger H = this.modP.Multiply(P.Z, P.Z);
            BigInteger J = this.modP.Subtract(E, H << 1);

            P.X = this.modP.Multiply(B - E, J);
            P.Y = this.modP.Multiply(E, C - D);
            P.Z = this.modP.Multiply(E, J);

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

            BigInteger v = this.modP.Multiply(this.D, y2) - BigInteger.One;
            BigInteger v2 = this.modP.Multiply(v, v);
            BigInteger v3 = this.modP.Multiply(v, v2);
            BigInteger u2 = this.modP.Multiply(u, u);
            BigInteger u3 = this.modP.Multiply(u, u2);
            BigInteger u5 = this.modP.Multiply(u2, u3);
            BigInteger x = this.modP.Multiply(this.modP.Multiply(u3, v),
                BigInteger.ModPow(this.modP.Multiply(u5, v3), this.P34, this.p));

            BigInteger x2 = this.modP.Multiply(x, x);
            BigInteger Test = this.modP.Multiply(v, x2);
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
