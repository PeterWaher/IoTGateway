using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Brainpool P-256 Elliptic Curve, as defined in RFC5639:
	/// https://datatracker.ietf.org/doc/html/rfc5639
	/// </summary>
	public class BrainpoolP256 : BrainpoolPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Parse("0A9FB57DBA1EEA9BC3E660A909D838D726E3BF623D52620282013481D1F6E5377", NumberStyles.HexNumber);
		private static readonly BigInteger a = BigInteger.Parse("07D5A0975FC2C3057EEF67530417AFFE7FB8055C126DC5C6CE94A4B44F330B5D9", NumberStyles.HexNumber);
		private static readonly BigInteger b = BigInteger.Parse("026DC5C6CE94A4B44F330B5D9BBD77CBF958416295CF7E1CE6BCCDC18FF8C07B6", NumberStyles.HexNumber);
		private static readonly BigInteger x = BigInteger.Parse("08BD2AEB9CB7E57CB2C4B482FFC81B7AFB9DE27E1E3BD23C23A4453BD9ACE3262", NumberStyles.HexNumber);
		private static readonly BigInteger y = BigInteger.Parse("0547EF835C3DAC4FD97F8461A14611DC9C27745132DED8E545C1D54C72F046997", NumberStyles.HexNumber);
		private static readonly BigInteger q = BigInteger.Parse("0A9FB57DBA1EEA9BC3E660A909D838D718C397AA3B561A6F7901E0E82974856A7", NumberStyles.HexNumber);

		/// <summary>
		/// Brainpool P-256 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		public BrainpoolP256()
			: base(p0, new PointOnCurve(x, y), a, b, q)
		{
		}

		/// <summary>
		/// Brainpool P-256 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP256(byte[] Secret)
			: base(p0, new PointOnCurve(x, y), a, b, q, Secret)
		{
		}

		/// <summary>
		/// Brainpool P-256 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP256(uint[] Secret)
			: base(p0, new PointOnCurve(x, y), a, b, q, Secret)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "Brainpool P-256";

	}
}
