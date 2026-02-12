using System.IO;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Elliptic curves over a prime field defined by Brainpool.
	/// </summary>
	public abstract class BrainpoolPrimeCurve : WeierstrassCurve
	{
		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by Brainpool.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">A coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		public BrainpoolPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger A,
            BigInteger Order)
			: base(Prime, BasePoint, A, Order, 1)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by Brainpool.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">A coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Secret">Secret.</param>
		public BrainpoolPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger A, 
            BigInteger Order, byte[] Secret)
			: base(Prime, BasePoint, A, Order, 1, Secret)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by Brainpool.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">A coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Secret">Secret.</param>
		public BrainpoolPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger A,
			BigInteger Order, uint[] Secret)
			: base(Prime, BasePoint, A, Order, 1, Secret)
		{
		}

		/// <summary>
		/// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <returns>Signature.</returns>
		public override byte[] Sign(byte[] Data)
        {
            return ECDSA.Sign(Data, this.PrivateKey, 
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
                this.bigIntegerBytes, this.msbOrderMask, this);
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(Stream Data)
        {
            return ECDSA.Sign(Data, this.PrivateKey,
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
                this.bigIntegerBytes, this.msbOrderMask, this);
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
                this.bigIntegerBytes, this.msbOrderMask, this, Signature);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(Stream Data, byte[] PublicKey, byte[] Signature)
        {
            return ECDSA.Verify(Data, PublicKey,
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
                this.bigIntegerBytes, this.msbOrderMask, this, Signature);
        }

    }
}
