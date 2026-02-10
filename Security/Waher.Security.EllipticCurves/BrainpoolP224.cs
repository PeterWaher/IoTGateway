using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Brainpool P-224 Elliptic Curve, as defined in RFC5639:
	/// https://datatracker.ietf.org/doc/html/rfc5639
	/// </summary>
	public class BrainpoolP224 : BrainpoolPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Parse("0D7C134AA264366862A18302575D1D787B09F075797DA89F57EC8C0FF", NumberStyles.HexNumber);
		private static readonly BigInteger a = BigInteger.Parse("068A5E62CA9CE6C1C299803A6C1530B514E182AD8B0042A59CAD29F43", NumberStyles.HexNumber);
		//private static readonly BigInteger b = BigInteger.Parse("02580F63CCFE44138870713B1A92369E33E2135D266DBB372386C400B", NumberStyles.HexNumber);
		private static readonly BigInteger x = BigInteger.Parse("00D9029AD2C7E5CF4340823B2A87DC68C9E4CE3174C1E6EFDEE12C07D", NumberStyles.HexNumber);
		private static readonly BigInteger y = BigInteger.Parse("058AA56F772C0726F24C6B89E4ECDAC24354B9E99CAA3F6D3761402CD", NumberStyles.HexNumber);
		private static readonly BigInteger q = BigInteger.Parse("0D7C134AA264366862A18302575D0FB98D116BC4B6DDEBCA3A5A7939F", NumberStyles.HexNumber);

		/// <summary>
		/// Brainpool P-224 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		public BrainpoolP224()
			: base(p0, new PointOnCurve(x, y), a, q)
		{
		}

		/// <summary>
		/// Brainpool P-224 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP224(byte[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "Brainpool P-224";

	}
}
