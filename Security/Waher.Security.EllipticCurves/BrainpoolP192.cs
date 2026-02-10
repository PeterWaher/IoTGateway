using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Brainpool P-192 Elliptic Curve, as defined in RFC5639:
	/// https://datatracker.ietf.org/doc/html/rfc5639
	/// </summary>
	public class BrainpoolP192 : BrainpoolPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Parse("0C302F41D932A36CDA7A3463093D18DB78FCE476DE1A86297", NumberStyles.HexNumber);
		private static readonly BigInteger a = BigInteger.Parse("06A91174076B1E0E19C39C031FE8685C1CAE040E5C69A28EF", NumberStyles.HexNumber);
		//private static readonly BigInteger b = BigInteger.Parse("0469A28EF7C28CCA3DC721D044F4496BCCA7EF4146FBF25C9", NumberStyles.HexNumber);
		private static readonly BigInteger x = BigInteger.Parse("0C0A0647EAAB6A48753B033C56CB0F0900A2F5C4853375FD6", NumberStyles.HexNumber);
		private static readonly BigInteger y = BigInteger.Parse("014B690866ABD5BB88B5F4828C1490002E6773FA2FA299B8F", NumberStyles.HexNumber);
		private static readonly BigInteger q = BigInteger.Parse("0C302F41D932A36CDA7A3462F9E9E916B5BE8F1029AC4ACC1", NumberStyles.HexNumber);

		/// <summary>
		/// Brainpool P-192 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		public BrainpoolP192()
			: base(p0, new PointOnCurve(x, y), a, q)
		{
		}

		/// <summary>
		/// Brainpool P-192 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP192(byte[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "Brainpool P-192";

	}
}
