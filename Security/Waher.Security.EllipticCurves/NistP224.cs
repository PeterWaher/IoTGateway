using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// NIST P-224 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
	/// </summary>
	public class NistP224 : NistPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Pow(2, 224) - BigInteger.Pow(2, 96) + 1;
		private static readonly BigInteger n0 = BigInteger.Parse("26959946667150639794667015087019625940457807714424391721682722368061");
		private static readonly BigInteger BasePointX = ToBigInteger(new uint[]
		{
			0xb70e0cbd, 0x6bb4bf7f, 0x321390b9, 0x4a03c1d3, 0x56c21122, 0x343280d6, 0x115c1d21
		});
		private static readonly BigInteger BasePointY = ToBigInteger(new uint[]
		{
			0xbd376388, 0xb5f723fb, 0x4c22dfe6, 0xcd4375a0, 0x5a074764, 0x44d58199, 0x85007e34
		});

		/// <summary>
		/// NIST P-224 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
		/// </summary>
		public NistP224()
			: base(p0, new PointOnCurve(BasePointX, BasePointY), n0)
		{
		}

        /// <summary>
        /// NIST P-224 Elliptic Curve, as defined in NIST FIPS BUB 186-4:
        /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf
        /// </summary>
        /// <param name="Secret">Secret.</param>
        public NistP224(byte[] Secret)
            : base(p0, new PointOnCurve(BasePointX, BasePointY), n0, Secret)
        {
        }

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "NIST P-224";
	}
}
