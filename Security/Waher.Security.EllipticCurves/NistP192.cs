using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// NIST P-192 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
	/// </summary>
	public class NistP192 : NistPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Pow(2, 192) - BigInteger.Pow(2, 64) - 1;
		private static readonly BigInteger n = BigInteger.Parse("6277101735386680763835789423176059013767194773182842284081");
		private static readonly int nBits = CalcBits(n);
		private static readonly BigInteger BasePointX = ToBigInteger(new uint[]
		{
			0x188da80e, 0xb03090f6, 0x7cbf20eb, 0x43a18800, 0xf4ff0afd, 0x82ff1012
		});
		private static readonly BigInteger BasePointY = ToBigInteger(new uint[]
		{
			0x07192b95, 0xffc8da78, 0x631011ed, 0x6b24cdd5, 0x73f977a1, 0x1e794811
		});

		/// <summary>
		/// NIST P-192 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
		/// </summary>
		public NistP192()
			: base(p0, new PointOnCurve(BasePointX, BasePointY), n, nBits)
		{
		}
    }
}
