using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Elliptic curves over a prime field defined by NIST.
	/// </summary>
	public abstract class NistPrimeCurve : WeierstrassCurve
	{
		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order)
			: base(Prime, BasePoint, -3, Order, 1)
		{
		}

        /// <summary>
        /// Base class of Elliptic curves over a prime field defined by NIST.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Secret">Secret.</param>
        public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order,
            byte[] Secret)
			: base(Prime, BasePoint, -3, Order, 1, Secret)
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

            return ToInt(B);
        }

        /// <summary>
        /// Hash function to use in signatures.
        /// </summary>
        public virtual Security.HashFunction HashFunction => Security.HashFunction.SHA256;

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            return ECDSA.Sign(Data, this.PrivateKey, 
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
        public override bool Verify(byte[] Data, byte[] PublicKey, byte[] Signature)
        {
            return ECDSA.Verify(Data, PublicKey,
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
                this.orderBytes, this.msbOrderMask, this, Signature);
        }

    }
}
