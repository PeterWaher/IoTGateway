using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// NIST P-384 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
	/// </summary>
	public class NistP384 : NistPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Pow(2, 384) - BigInteger.Pow(2, 128) - BigInteger.Pow(2, 96) + BigInteger.Pow(2, 32) - 1;
		private static readonly BigInteger n = BigInteger.Parse("39402006196394479212279040100143613805079739270465446667946905279627659399113263569398956308152294913554433653942643");
		private static readonly int nBits = CalcBits(n);
		private static readonly BigInteger BasePointX = ToBigInteger(new uint[]
		{
			0xaa87ca22, 0xbe8b0537, 0x8eb1c71e, 0xf320ad74, 0x6e1d3b62, 0x8ba79b98, 0x59f741e0, 0x82542a38, 0x5502f25d, 0xbf55296c, 0x3a545e38, 0x72760ab7
		});
		private static readonly BigInteger BasePointY = ToBigInteger(new uint[]
		{
			0x3617de4a, 0x96262c6f, 0x5d9e98bf, 0x9292dc29, 0xf8f41dbd, 0x289a147c, 0xe9da3113, 0xb5f0b8c0, 0x0a60b1ce, 0x1d7e819d, 0x7a431d7c, 0x90ea0e5f
		});

		/// <summary>
		/// NIST P-384 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
		/// </summary>
		public NistP384()
			: base(p0, new PointOnCurve(BasePointX, BasePointY), n, nBits)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "NIST P-192";
	}
}
