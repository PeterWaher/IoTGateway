using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Base class of Elliptic curves over a prime field.
    /// </summary>
    public abstract class PrimeFieldCurve : EllipticCurve
    {
        /// <summary>
        /// Arithmetic modulus p
        /// </summary>
        protected readonly ModulusP modP;

        /// <summary>
        /// Arithmetic modulus n
        /// </summary>
        protected readonly ModulusP modN;

        /// <summary>
        /// Prime p
        /// </summary>
        protected readonly BigInteger p;

        /// <summary>
        /// Base class of Elliptic curves over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        public PrimeFieldCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order,
            int Cofactor)
            : this(Prime, BasePoint, Order, Cofactor, null)
        {
        }

        /// <summary>
        /// Base class of Elliptic curves over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        /// <param name="Secret">Secret.</param>
        public PrimeFieldCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order,
            int Cofactor, byte[] Secret)
            : base(BasePoint, Order, Cofactor, Secret)
        {
            if (Prime <= BigInteger.One)
                throw new ArgumentException("Invalid prime base.", nameof(Prime));

            this.p = Prime;
            this.modP = new ModulusP(Prime);
            this.modN = new ModulusP(Order);
        }

        /// <summary>
        /// Prime of curve.
        /// </summary>
        public BigInteger Prime => this.p;

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="P"/>.
        /// </summary>
        /// <param name="N">Scalar, in binary, little-endian form.</param>
        /// <param name="P">Point</param>
        /// <param name="Normalize">If normalized output is expected.</param>
        /// <returns><paramref name="N"/>*<paramref name="P"/></returns>
        public override PointOnCurve ScalarMultiplication(byte[] N, PointOnCurve P, bool Normalize)
        {
            PointOnCurve Result = base.ScalarMultiplication(N, P, Normalize);

            if (Normalize)
                Result.Normalize(this);

            return Result;
        }

        /// <summary>
        /// Generates a new secret.
        /// </summary>
        /// <returns>Generated secret.</returns>
        public override byte[] GenerateSecret()
        {
            byte[] B = new byte[this.orderBytes];
            BigInteger D;

            do
            {
                lock (rnd)
                {
                    rnd.GetBytes(B);
                }

                B[this.orderBytes - 1] &= this.msbOrderMask;

                D = ToInt(B);
            }
            while (D.IsZero || D >= this.n);

            return B;
        }

        /// <summary>
        /// Arithmetic modulus p (the prime)
        /// </summary>
        public ModulusP ModulusP => this.modP;

        /// <summary>
        /// Arithmetic modulus n (the order)
        /// </summary>
        public ModulusP ModulusN => this.modN;

    }
}
