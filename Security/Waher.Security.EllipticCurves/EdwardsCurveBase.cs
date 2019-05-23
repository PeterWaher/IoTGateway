using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Base class of different types of Edwards curves over a prime field.
    /// </summary>
    public abstract class EdwardsCurveBase : PrimeFieldCurve
    {
        /// <summary>
        /// Edwards curve coefficient
        /// </summary>
        protected BigInteger d;

        /// <summary>
        /// Edwards curve coefficient * 2 mod p
        /// </summary>
        protected BigInteger d2;

        /// <summary>
        /// Base class of Twisted Edwards curves (-x²+y²=1+dx²y²) over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
        /// <param name="d">Edwards curve coefficient</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        public EdwardsCurveBase(BigInteger Prime, PointOnCurve BasePoint,
            BigInteger d, BigInteger Order, int Cofactor)
            : this(Prime, BasePoint, d, Order, Cofactor, null)
        {
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
        public EdwardsCurveBase(BigInteger Prime, PointOnCurve BasePoint, BigInteger d,
            BigInteger Order, int Cofactor, byte[] Secret)
            : base(Prime, BasePoint, Order, Cofactor, Secret)
        {
            this.d = d;
            this.d2 = BigInteger.Remainder(d << 1, this.p);
        }

        /// <summary>
        /// Number of bits used to encode the y-coordinate.
        /// </summary>
        public abstract int CoordinateBits
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
        /// Gets the X-coordinate that corresponds to a given Y-coordainte, and the 
        /// first bit of the X-coordinate.
        /// </summary>
        /// <param name="Y">Y-coordinate.</param>
        /// <param name="X0">First bit of X-coordinate.</param>
        /// <returns>X-coordinate</returns>
        public abstract BigInteger GetX(BigInteger Y, bool X0);

    }
}
