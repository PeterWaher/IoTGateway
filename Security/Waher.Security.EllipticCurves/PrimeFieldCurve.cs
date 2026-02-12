using System;
using System.Numerics;

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
            : this(Prime, BasePoint, Order, Cofactor, (byte[])null)
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
		/// Base class of Elliptic curves over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="Secret">Secret.</param>
		public PrimeFieldCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order,
			int Cofactor, uint[] Secret)
			: this(Prime, BasePoint, Order, Cofactor, ToByteSecret(Secret))
		{
		}

		/// <summary>
		/// Prime of curve.
		/// </summary>
		public BigInteger Prime => this.p;

		/// <summary>
		/// Hash function to use in signatures.
		/// </summary>
		public virtual HashFunction HashFunction => HashFunction.SHA256;

		/// <summary>
		/// Converts a sequence of unsigned 32-bit integers to a <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="BigEndianDWords">Sequence of unsigned 32-bit integers to a <see cref="BigInteger"/>, most significant word first.</param>
		/// <returns><see cref="BigInteger"/> value.</returns>
		public static BigInteger ToBigInteger(uint[] BigEndianDWords)
		{
			return ToInt(ToByteSecret(BigEndianDWords));
		}

		/// <summary>
		/// Converts a sequence of unsigned 32-bit integers to a secret that can be used
		/// with <see cref="BigInteger"/>
		/// </summary>
		/// <param name="BigEndianDWords">Sequence of unsigned 32-bit integers to a 
		/// <see cref="BigInteger"/>, most significant word first.</param>
		/// <returns><see cref="BigInteger"/> value.</returns>
		public static byte[] ToByteSecret(uint[] BigEndianDWords)
		{
			int i, c = BigEndianDWords.Length;
			int j = c << 2;
			byte[] B = new byte[((BigEndianDWords[0] & 0x80000000) != 0) ? j + 1 : j];
			uint k;

			for (i = 0; i < c; i++)
			{
				k = BigEndianDWords[i];

				B[j - 4] = (byte)k;
				k >>= 8;
				B[j - 3] = (byte)k;
				k >>= 8;
				B[j - 2] = (byte)k;
				k >>= 8;
				B[j - 1] = (byte)k;

				j -= 4;
			}

			return B;
		}

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
            byte[] B = new byte[this.bigIntegerBytes];
            BigInteger D;

            do
            {
                lock (rnd)
                {
                    rnd.GetBytes(B);
                }

                B[this.bigIntegerBytes - 1] &= this.msbOrderMask;

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
