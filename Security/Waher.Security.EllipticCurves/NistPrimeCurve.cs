using System.IO;
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
		/// <param name="B">B coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger B,
            BigInteger Order)
			: base(Prime, BasePoint, -3, B, Order, 1)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="B">B coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Secret">Secret.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger B, 
            BigInteger Order, byte[] Secret)
			: base(Prime, BasePoint, -3, B, Order, 1, Secret)
		{
		}

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="BigEndian">Indicates if the signature should be in big-endian format.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data, bool BigEndian)
        {
            return ECDSA.Sign(Data, BigEndian, this.PrivateKey, 
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
				this.orderBytes, this.bigIntegerBytes, this.msbOrderMask, this);
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="BigEndian">Indicates if the signature should be in big-endian format.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(Stream Data, bool BigEndian)
        {
            return ECDSA.Sign(Data, BigEndian, this.PrivateKey,
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
				this.orderBytes, this.bigIntegerBytes, this.msbOrderMask, this);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="BigEndian">Indicates if the public key is in big-endian format.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(byte[] Data, byte[] PublicKey, bool BigEndian, byte[] Signature)
        {
            return ECDSA.Verify(Data, PublicKey, BigEndian, 
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
                this.orderBytes, this.bigIntegerBytes, this.msbOrderMask, this, Signature);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="BigEndian">Indicates if the public key is in big-endian format.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(Stream Data, byte[] PublicKey, bool BigEndian, byte[] Signature)
        {
            return ECDSA.Verify(Data, PublicKey, BigEndian,
                Bin => Hashes.ComputeHash(this.HashFunction, Bin),
                this.orderBytes, this.bigIntegerBytes, this.msbOrderMask, this, Signature);
        }

    }
}
