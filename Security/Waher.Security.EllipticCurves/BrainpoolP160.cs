using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Brainpool P-160 Elliptic Curve, as defined in RFC5639:
	/// https://datatracker.ietf.org/doc/html/rfc5639
	/// </summary>
	public class BrainpoolP160 : BrainpoolPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Parse("0E95E4A5F737059DC60DFC7AD95B3D8139515620F", NumberStyles.HexNumber);
		private static readonly BigInteger a = BigInteger.Parse("0340E7BE2A280EB74E2BE61BADA745D97E8F7C300", NumberStyles.HexNumber);
		//private static readonly BigInteger b = BigInteger.Parse("01E589A8595423412134FAA2DBDEC95C8D8675E58", NumberStyles.HexNumber);
		private static readonly BigInteger x = BigInteger.Parse("0BED5AF16EA3F6A4F62938C4631EB5AF7BDBCDBC3", NumberStyles.HexNumber);
		private static readonly BigInteger y = BigInteger.Parse("01667CB477A1A8EC338F94741669C976316DA6321", NumberStyles.HexNumber);
		private static readonly BigInteger q = BigInteger.Parse("0E95E4A5F737059DC60DF5991D45029409E60FC09", NumberStyles.HexNumber);

		/// <summary>
		/// Brainpool P-160 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		public BrainpoolP160()
			: base(p0, new PointOnCurve(x, y), a, q)
		{
		}

		/// <summary>
		/// Brainpool P-160 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP160(byte[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Brainpool P-160 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP160(uint[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "Brainpool P-160";

	}
}
