using System;
using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Curve25519, as defined in RFC 7748:
	/// https://tools.ietf.org/html/rfc7748
	/// </summary>
	public class Curve25519 : MontgomeryCurve
	{
		private static readonly BigInteger p0 = BigInteger.Pow(2, 255) - 19;
		private static readonly BigInteger A = 486662;
		private static readonly BigInteger A24 = (A - 2) / 4;
		private static readonly BigInteger n = BigInteger.Pow(2, 252) + BigInteger.Parse("14def9dea2f79cd65812631a5cf5d3ed", NumberStyles.HexNumber);
		private static readonly BigInteger BasePointU = 9;
		private static readonly BigInteger BasePointV = BigInteger.Parse("43114425171068552920764898935933967039370386198203806730763910166200978582548");
		private static readonly BigInteger SqrtMinus486664 = ModulusP.SqrtModP(-486664, p0);
        private const int cofactor = 8;

        /// <summary>
        /// Curve25519, as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        public Curve25519()
			: base(p0, new PointOnCurve(BasePointU, BasePointV), A, n, cofactor)
		{
        }

        /// <summary>
        /// Curve25519, as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        /// <param name="D">Private key.</param>
        public Curve25519(BigInteger D)
			: base(p0, new PointOnCurve(BasePointU, BasePointV), A, n, cofactor, D)
		{
        }

        /// <summary>
        /// Name of curve.
        /// </summary>
        public override string CurveName => "Curve25519";


		/// <summary>
		/// Converts a pair of (U,V) coordinates to a pair of (X,Y) coordinates
		/// in the birational Edwards curve.
		/// </summary>
		/// <param name="UV">(U,V) coordinates.</param>
		/// <returns>(X,Y) coordinates.</returns>
		public override PointOnCurve ToXY(PointOnCurve UV)
		{
			return new PointOnCurve(
				this.Multiply(SqrtMinus486664, this.Divide(UV.X, UV.Y)),
				this.Divide(UV.X - BigInteger.One, UV.X + BigInteger.One));
		}

		/// <summary>
		/// Converts a pair of (X,Y) coordinates for the birational Edwards curve
		/// to a pair of (U,V) coordinates.
		/// </summary>
		/// <param name="XY">(X,Y) coordinates.</param>
		/// <returns>(U,V) coordinates.</returns>
		public override PointOnCurve ToUV(PointOnCurve XY)
		{
			BigInteger U = this.Divide(XY.Y + BigInteger.One, BigInteger.One - XY.Y);
			return new PointOnCurve(
				U,
				this.Multiply(SqrtMinus486664, this.Divide(U, XY.X)));
		}

		/// <summary>
		/// Returns the next random number, in the range [1, n-1].
		/// </summary>
		/// <returns>Random number.</returns>
		public override BigInteger NextRandomNumber()
		{
			byte[] B = new byte[32];
			BigInteger D;

			lock (rnd)
			{
				rnd.GetBytes(B);
			}

			B[0] &= 248;
			B[31] &= 127;
			B[31] |= 64;

			D = new BigInteger(B);

			return D;
		}

		/// <summary>
		/// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
		/// </summary>
		/// <param name="N">Scalar</param>
		/// <param name="U">U-coordinate of point</param>
		/// <returns><paramref name="N"/>*<paramref name="U"/></returns>
		public override BigInteger ScalarMultiplication(BigInteger N, BigInteger U)
		{
			return ScalarMultiplication(N, U, A24, this.p, 255);
		}

    }
}
