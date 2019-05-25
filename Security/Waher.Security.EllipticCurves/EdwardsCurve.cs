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
        private readonly BigInteger p34;

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
			: this(Prime, BasePoint, d, Order, Cofactor, null)
		{
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
            this.p34 = (this.p - 3) / 4;
        }

        /// <summary>
        /// d coefficient of Edwards curve.
        /// </summary>
        protected abstract BigInteger D
        {
            get;
        }

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
                BigInteger.ModPow(this.modP.Multiply(u5, v3), this.p34, this.p));

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
