using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Edwards curves over a prime field.
	/// </summary>
	public abstract class EdwardsCurve : CurvePrimeField
	{
        private readonly BigInteger d;
        private readonly BigInteger p58;
        private readonly BigInteger twoP14;
        private byte[] a;

        /// <summary>
        /// Base class of Edwards curves over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="d">d coefficient of Edwards curve.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        public EdwardsCurve(BigInteger Prime, BigInteger d, PointOnCurve BasePoint, 
            BigInteger Order, int Cofactor)
			: base(Prime, BasePoint, Order, Cofactor)
		{
            this.d = d;
            this.p58 = (Prime - 5) / 8;
            this.twoP14 = BigInteger.ModPow(Two, (Prime - 1) / 4, Prime);
        }

        /// <summary>
        /// Base class of Edwards curves over a prime field.
        /// </summary>
		/// <param name="Prime">Prime base of field.</param>
        /// <param name="d">d coefficient of Edwards curve.</param>
        /// <param name="BasePoint">Base-point in (X,Y) coordinates.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="D">Private key.</param>
		public EdwardsCurve(BigInteger Prime, BigInteger d, PointOnCurve BasePoint, BigInteger Order, 
            int Cofactor, BigInteger D)
			: base(Prime, BasePoint, Order, Cofactor, D)
		{
            this.d = d;
            this.p58 = (Prime - 5) / 8;
            this.twoP14 = BigInteger.ModPow(Two, (Prime - 1) / 4, Prime);
        }

        /// <summary>
        /// d coefficient of Edwards curve.
        /// </summary>
        public BigInteger D => this.d;

        /// <summary>
        /// (p-5)/8
        /// </summary>
        public BigInteger P58 => this.p58;

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
            this.a = EdDSA.Encode(this.PublicKey, this.orderBytes);
        }

        /// <summary>
        /// Encoded public key, for EdDSA.
        /// </summary>
        public byte[] A => this.a;

    }
}
