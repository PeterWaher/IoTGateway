using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Elliptic curves over a prime field defined by NIST.
	/// </summary>
	public abstract class NistPrimeCurve : CurvePrimeField
	{
		private static readonly BigInteger a = new BigInteger(-3);  // a Coefficient in the definition of the curve E:	y^2=x^3+a*x+b
        private const int cofactor = 1;

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order)
			: base(Prime, BasePoint, Order, cofactor)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="D">Private key.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order, BigInteger D)
			: base(Prime, BasePoint, Order, cofactor, D)
		{
		}

        /// <summary>
        /// Converts a sequence of unsigned 32-bit integers to a <see cref="BigInteger"/>.
        /// </summary>
        /// <param name="BigEndianDWords">Sequence of unsigned 32-bit integers to a <see cref="BigInteger"/>, most significant word first.</param>
        /// <returns><see cref="BigInteger"/> value.</returns>
        protected static BigInteger ToBigInteger(uint[] BigEndianDWords)
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

            return new BigInteger(B);
        }

        /// <summary>
        /// Adds <paramref name="Q"/> to <paramref name="P"/>.
        /// </summary>
        /// <param name="P">Point 1.</param>
        /// <param name="Q">Point 2.</param>
        /// <returns>P+Q</returns>
        public override void AddTo(ref PointOnCurve P, PointOnCurve Q)
        {
            if (P.NonZero)
            {
                if (Q.NonZero)
                {
                    BigInteger sDividend = this.Subtract(P.Y, Q.Y);
                    BigInteger sDivisor = this.Subtract(P.X, Q.X);
                    BigInteger s, xR, yR;

                    if (sDivisor.IsZero)
                    {
                        if (sDividend.IsZero)   // P=Q
                            this.Double(ref P);
                        else
                            P = this.Zero;
                    }
                    else
                    {
                        s = this.Divide(sDividend, sDivisor);
                        xR = this.Subtract(this.Multiply(s, s), this.Add(P.X, Q.X));
                        yR = this.Add(P.Y, this.Multiply(s, this.Subtract(xR, P.X)));

                        P.X = xR;
                        P.Y = this.p - yR;
                    }
                }
            }
            else
                P.CopyFrom(Q);
        }

        /// <summary>
        /// Doubles a point on the curve.
        /// </summary>
        /// <param name="P">Point</param>
        public override void Double(ref PointOnCurve P)
        {
            if (P.NonZero)
            {
                BigInteger sDividend = this.Add(3 * this.Multiply(P.X, P.X), a);
                BigInteger sDivisor = this.Multiply(Two, P.Y);

                BigInteger s = this.Divide(sDividend, sDivisor);
                BigInteger xR = this.Subtract(this.Multiply(s, s), this.Add(P.X, P.X));
                BigInteger yR = this.Add(P.Y, this.Multiply(s, this.Subtract(xR, P.X)));

                P.X = xR;
                P.Y = this.p - yR;
            }
        }

        /// <summary>
        /// Hash function to use in signatures.
        /// </summary>
        public virtual Security.HashFunction HashFunction => Security.HashFunction.SHA256;

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            return ECDSA.Sign(Data, this.privateKey, 
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
                this.orderBytes, this.msbOrderMask, this);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(byte[] Data, PointOnCurve PublicKey, byte[] Signature)
        {
            return ECDSA.Verify(Data, PublicKey,
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
                this.orderBytes, this.msbOrderMask, this, Signature);
        }

    }
}
