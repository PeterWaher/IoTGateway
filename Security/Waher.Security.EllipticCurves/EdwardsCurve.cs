using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of (twisted) Edwards curves over a prime field.
	/// </summary>
	public abstract class EdwardsCurve : CurvePrimeField
	{
        private BigInteger p58;
        private BigInteger p34;
        private BigInteger twoP14;
        private byte[] a;

        /// <summary>
        /// Base class of Edwards curves over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        public EdwardsCurve(BigInteger Prime, PointOnCurve BasePoint, 
            BigInteger Order, int Cofactor)
			: base(Prime, BasePoint, Order, Cofactor)
		{
            this.Init();
        }

        /// <summary>
        /// Base class of Edwards curves over a prime field.
        /// </summary>
		/// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="D">Private key.</param>
		public EdwardsCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order, 
            int Cofactor, BigInteger D)
			: base(Prime, BasePoint, Order, Cofactor, D)
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
        /// Number of bits used to encode the y-coordinate.
        /// </summary>
        public abstract int CoordinateBits
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
        /// Sets the private key (and therefore also the public key) of the curve.
        /// </summary>
        /// <param name="D">Private key.</param>
        public override void SetPrivateKey(BigInteger D)
        {
            base.SetPrivateKey(D);
            this.a = EdDSA.Encode(this.PublicKey, this);
        }

        /// <summary>
        /// Encoded public key, for EdDSA.
        /// </summary>
        public byte[] A => this.a;

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
