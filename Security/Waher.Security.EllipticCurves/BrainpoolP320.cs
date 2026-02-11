using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Brainpool P-320 Elliptic Curve, as defined in RFC5639:
	/// https://datatracker.ietf.org/doc/html/rfc5639
	/// </summary>
	public class BrainpoolP320 : BrainpoolPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Parse("0D35E472036BC4FB7E13C785ED201E065F98FCFA6F6F40DEF4F92B9EC7893EC28FCD412B1F1B32E27", NumberStyles.HexNumber);
		private static readonly BigInteger a = BigInteger.Parse("03EE30B568FBAB0F883CCEBD46D3F3BB8A2A73513F5EB79DA66190EB085FFA9F492F375A97D860EB4", NumberStyles.HexNumber);
		//private static readonly BigInteger b = BigInteger.Parse("0520883949DFDBC42D3AD198640688A6FE13F41349554B49ACC31DCCD884539816F5EB4AC8FB1F1A6", NumberStyles.HexNumber);
		private static readonly BigInteger x = BigInteger.Parse("043BD7E9AFB53D8B85289BCC48EE5BFE6F20137D10A087EB6E7871E2A10A599C710AF8D0D39E20611", NumberStyles.HexNumber);
		private static readonly BigInteger y = BigInteger.Parse("014FDD05545EC1CC8AB4093247F77275E0743FFED117182EAA9C77877AAAC6AC7D35245D1692E8EE1", NumberStyles.HexNumber);
		private static readonly BigInteger q = BigInteger.Parse("0D35E472036BC4FB7E13C785ED201E065F98FCFA5B68F12A32D482EC7EE8658E98691555B44C59311", NumberStyles.HexNumber);

		/// <summary>
		/// Brainpool P-320 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		public BrainpoolP320()
			: base(p0, new PointOnCurve(x, y), a, q)
		{
		}

		/// <summary>
		/// Brainpool P-320 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP320(byte[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Brainpool P-320 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP320(uint[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "Brainpool P-320";

		/// <summary>
		/// Hash function to use in signatures.
		/// </summary>
		public override HashFunction HashFunction => HashFunction.SHA384;

	}
}
