using System.IO;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Base class of Weierstrass curves (y²=x³+ax+b) over a prime field.
    /// </summary>
    public abstract class WeierstrassCurve : PrimeFieldCurve
	{
		private readonly BigInteger a;
		private readonly BigInteger b;

		/// <summary>
		/// Base class of Weierstrass curves (y²=x³+ax+b) over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="a">Coefficient in the Weierstrass equation.</param>
		/// <param name="b">Coefficient in the Weierstrass equation.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		public WeierstrassCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger a, 
            BigInteger b, BigInteger Order, int Cofactor)
            : this(Prime, BasePoint, a, b, Order, Cofactor, (byte[])null)
		{
		}

		/// <summary>
		/// Base class of Weierstrass curves (y²=x³+ax+b) over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="a">Coefficient in the Weierstrass equation.</param>
		/// <param name="b">Coefficient in the Weierstrass equation.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="Secret">Secret.</param>
		public WeierstrassCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger a,
			BigInteger b, BigInteger Order, int Cofactor, byte[] Secret)
			: base(Prime, BasePoint, Order, Cofactor, Secret)
		{
            this.a = a;
            this.b = b;
		}

		/// <summary>
		/// Base class of Weierstrass curves (y²=x³+ax+b) over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="a">Coefficient in the Weierstrass equation.</param>
		/// <param name="b">Coefficient in the Weierstrass equation.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="Secret">Secret.</param>
		public WeierstrassCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger a,
			BigInteger b, BigInteger Order, int Cofactor, uint[] Secret)
			: base(Prime, BasePoint, Order, Cofactor, Secret)
		{
			this.a = a;
            this.b = b;
		}

		/// <summary>
		/// Coefficient a.
		/// </summary>
		public BigInteger A => this.a;

		/// <summary>
		/// Coefficient b.
		/// </summary>
		public BigInteger B => this.b;

		/// <summary>
		/// Negates a point on the curve.
		/// </summary>
		/// <param name="P">Point</param>
		public void Negate(ref PointOnCurve P)
        {
            P.Y = this.p - P.Y;
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
                    BigInteger sDividend = this.modP.Subtract(P.Y, Q.Y);
                    BigInteger sDivisor = this.modP.Subtract(P.X, Q.X);
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
                        s = this.modP.Divide(sDividend, sDivisor);
                        xR = this.modP.Subtract(this.modP.Multiply(s, s), this.modP.Add(P.X, Q.X));
                        yR = this.modP.Add(P.Y, this.modP.Multiply(s, this.modP.Subtract(xR, P.X)));

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
                BigInteger sDividend = this.modP.Add(3 * this.modP.Multiply(P.X, P.X), this.a);
                BigInteger sDivisor = this.modP.Multiply(Two, P.Y);

                BigInteger s = this.modP.Divide(sDividend, sDivisor);
                BigInteger xR = this.modP.Subtract(this.modP.Multiply(s, s), this.modP.Add(P.X, P.X));
                BigInteger yR = this.modP.Add(P.Y, this.modP.Multiply(s, this.modP.Subtract(xR, P.X)));

                P.X = xR;
                P.Y = this.p - yR;
            }
        }

		/// <summary>
		/// Checks if a point is on the curve.
		/// </summary>
		/// <param name="Point">Point</param>
		/// <returns>If the point is on the curve.</returns>
		public override bool IsPoint(PointOnCurve Point)
		{
			// Check if point matches y²=x³+ax+b=x(x²+a)+b

			BigInteger Left = this.modP.Multiply(Point.Y, Point.Y);

            BigInteger Right = this.modP.Add(
                this.modP.Multiply(
                    Point.X,
                    this.modP.Add(
                        this.modP.Multiply(Point.X, Point.X),
                        this.a)),
                this.b);

			return Left == Right;
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
				Bin => Hashes.ComputeHash(this.HashFunction, Bin), this);
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
				Bin => Hashes.ComputeHash(this.HashFunction, Bin), this);
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
				Bin => Hashes.ComputeHash(this.HashFunction, Bin), this, Signature);
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
				Bin => Hashes.ComputeHash(this.HashFunction, Bin), this, Signature);
		}
	}
}
