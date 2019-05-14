using System;
using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Curve448, as defined in RFC 7748:
	/// https://tools.ietf.org/html/rfc7748
	/// </summary>
	public class Curve448 : MontgomeryCurve
	{
		private static readonly BigInteger p0 = BigInteger.Pow(2, 448) - BigInteger.Pow(2, 224) - 1;
		private static readonly BigInteger A = 156326;
        private static readonly BigInteger A24 = (A - 2) / 4;
		private static readonly BigInteger n = BigInteger.Pow(2, 446) - BigInteger.Parse("8335dc163bb124b65129c96fde933d8d723a70aadc873d6d54a7bb0d", NumberStyles.HexNumber);
		private const int cofactor = 4;
		private static readonly BigInteger BasePointU = 5;
		private static readonly BigInteger BasePointV = BigInteger.Parse("355293926785568175264127502063783334808976399387714271831880898435169088786967410002932673765864550910142774147268105838985595290606362");
		private static readonly BigInteger Sqrt156324 = ModulusP.SqrtModP(156324, p0);

		/// <summary>
		/// Curve448, as defined in RFC 7748:
		/// https://tools.ietf.org/html/rfc7748
		/// </summary>
		public Curve448()
			: base(p0, A, new PointOnCurve(BasePointU, BasePointV), n, cofactor)
		{
		}

		/// <summary>
		/// Curve448, as defined in RFC 7748:
		/// https://tools.ietf.org/html/rfc7748
		/// </summary>
		/// <param name="D">Private key.</param>
		public Curve448(BigInteger D)
			: base(p0, A, new PointOnCurve(BasePointU, BasePointV), n, cofactor, D)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "Curve448";


		/// <summary>
		/// Converts a pair of (U,V) coordinates to a pair of (X,Y) coordinates
		/// in the birational Edwards curve.
		/// </summary>
		/// <param name="UV">(U,V) coordinates.</param>
		/// <returns>(X,Y) coordinates.</returns>
		public override PointOnCurve ToXY(PointOnCurve UV)
		{
			return new PointOnCurve(
				this.Multiply(Sqrt156324, this.Divide(UV.X, UV.Y)),
				this.Divide(UV.X + BigInteger.One, BigInteger.One - UV.X));
		}

		/// <summary>
		/// Converts a pair of (X,Y) coordinates for the birational Edwards curve
		/// to a pair of (U,V) coordinates.
		/// </summary>
		/// <param name="XY">(X,Y) coordinates.</param>
		/// <returns>(U,V) coordinates.</returns>
		public override PointOnCurve ToUV(PointOnCurve XY)
		{
			BigInteger U = this.Divide(XY.Y - BigInteger.One, XY.Y + BigInteger.One);
			return new PointOnCurve(
				U,
				this.Multiply(Sqrt156324, this.Divide(U, XY.X)));
		}

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
        /// </summary>
        /// <param name="N">Scalar</param>
        /// <param name="U">U-coordinate of point</param>
        /// <returns><paramref name="N"/>*<paramref name="U"/></returns>
        public override BigInteger ScalarMultiplication(BigInteger N, BigInteger U)
		{
			return XFunction(N, U, A24, this.p, 448, 0xfc, 0x7f, 0x80);
		}

        /// <summary>
        /// Creates the Edwards Curve pair.
        /// </summary>
        /// <returns>Edwards curve.</returns>
        public override EdwardsCurve CreatePair()
        {
            throw new NotImplementedException();
        }

    }
}
