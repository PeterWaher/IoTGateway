using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// NIST P-256 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
	/// </summary>
	public class NistP256 : NistPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Pow(2, 256) - BigInteger.Pow(2, 224) + BigInteger.Pow(2, 192) + BigInteger.Pow(2, 96) - 1;
		private static readonly BigInteger n = BigInteger.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369");
		private static readonly int nBits = CalcBits(n);
		private static readonly BigInteger BasePointX = ToBigInteger(new uint[]
		{
			0x6b17d1f2, 0xe12c4247, 0xf8bce6e5, 0x63a440f2, 0x77037d81, 0x2deb33a0, 0xf4a13945, 0xd898c296
		});
		private static readonly BigInteger BasePointY = ToBigInteger(new uint[]
		{
			0x4fe342e2, 0xfe1a7f9b, 0x8ee7eb4a, 0x7c0f9e16, 0x2bce3357, 0x6b315ece, 0xcbb64068, 0x37bf51f5
		});

		/// <summary>
		/// NIST P-256 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
		/// </summary>
		public NistP256()
			: base(p0, new PointOnCurve(BasePointX, BasePointY), n, nBits)
		{
		}

		/// <summary>
		/// NIST P-256 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
		/// </summary>
		/// <param name="D">Private key.</param>
		public NistP256(BigInteger D)
			: base(p0, new PointOnCurve(BasePointX, BasePointY), n, nBits, D)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "NIST P-256";
	}
}
