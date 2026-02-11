using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Brainpool P-512 Elliptic Curve, as defined in RFC5639:
	/// https://datatracker.ietf.org/doc/html/rfc5639
	/// </summary>
	public class BrainpoolP512 : BrainpoolPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Parse("0AADD9DB8DBE9C48B3FD4E6AE33C9FC07CB308DB3B3C9D20ED6639CCA703308717D4D9B009BC66842AECDA12AE6A380E62881FF2F2D82C68528AA6056583A48F3", NumberStyles.HexNumber);
		private static readonly BigInteger a = BigInteger.Parse("07830A3318B603B89E2327145AC234CC594CBDD8D3DF91610A83441CAEA9863BC2DED5D5AA8253AA10A2EF1C98B9AC8B57F1117A72BF2C7B9E7C1AC4D77FC94CA", NumberStyles.HexNumber);
		//private static readonly BigInteger b = BigInteger.Parse("03DF91610A83441CAEA9863BC2DED5D5AA8253AA10A2EF1C98B9AC8B57F1117A72BF2C7B9E7C1AC4D77FC94CADC083E67984050B75EBAE5DD2809BD638016F723", NumberStyles.HexNumber);
		private static readonly BigInteger x = BigInteger.Parse("081AEE4BDD82ED9645A21322E9C4C6A9385ED9F70B5D916C1B43B62EEF4D0098EFF3B1F78E2D0D48D50D1687B93B97D5F7C6D5047406A5E688B352209BCB9F822", NumberStyles.HexNumber);
		private static readonly BigInteger y = BigInteger.Parse("07DDE385D566332ECC0EABFA9CF7822FDF209F70024A57B1AA000C55B881F8111B2DCDE494A5F485E5BCA4BD88A2763AED1CA2B2FA8F0540678CD1E0F3AD80892", NumberStyles.HexNumber);
		private static readonly BigInteger q = BigInteger.Parse("0AADD9DB8DBE9C48B3FD4E6AE33C9FC07CB308DB3B3C9D20ED6639CCA70330870553E5C414CA92619418661197FAC10471DB1D381085DDADDB58796829CA90069", NumberStyles.HexNumber);

		/// <summary>
		/// Brainpool P-512 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		public BrainpoolP512()
			: base(p0, new PointOnCurve(x, y), a, q)
		{
		}

		/// <summary>
		/// Brainpool P-512 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP512(byte[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Brainpool P-512 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP512(uint[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "Brainpool P-512";

		/// <summary>
		/// Hash function to use in signatures.
		/// </summary>
		public override HashFunction HashFunction => HashFunction.SHA512;

	}
}
