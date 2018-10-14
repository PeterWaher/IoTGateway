using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// NIST P-521 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
	/// </summary>
	public class NistP521 : NistPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Pow(2, 521) - 1;
		private static readonly BigInteger n = BigInteger.Parse("6864797660130609714981900799081393217269435300143305409394463459185543183397655394245057746333217197532963996371363321113864768612440380340372808892707005449");
		private static readonly int nBits = CalcBits(n);
		private static readonly BigInteger BasePointX = ToBigInteger(new uint[]
		{
			0xc6, 0x858e06b7, 0x0404e9cd, 0x9e3ecb66, 0x2395b442, 0x9c648139, 0x053fb521, 0xf828af60, 0x6b4d3dba,
			0xa14b5e77, 0xefe75928, 0xfe1dc127, 0xa2ffa8de, 0x3348b3c1, 0x856a429b, 0xf97e7e31, 0xc2e5bd66
		});
		private static readonly BigInteger BasePointY = ToBigInteger(new uint[]
		{
			0x118, 0x39296a78, 0x9a3bc004, 0x5c8a5fb4, 0x2c7d1bd9, 0x98f54449, 0x579b4468, 0x17afbd17, 0x273e662c,
			0x97ee7299, 0x5ef42640, 0xc550b901, 0x3fad0761, 0x353c7086, 0xa272c240, 0x88be9476, 0x9fd16650
		});

		/// <summary>
		/// NIST P-521 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
		/// </summary>
		public NistP521()
			: base(p0, new PointOnCurve(BasePointX, BasePointY), n, nBits)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "NIST P-521";
	}
}
