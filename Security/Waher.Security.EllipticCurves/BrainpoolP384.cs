using System.Globalization;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Brainpool P-384 Elliptic Curve, as defined in RFC5639:
	/// https://datatracker.ietf.org/doc/html/rfc5639
	/// </summary>
	public class BrainpoolP384 : BrainpoolPrimeCurve
	{
		private static readonly BigInteger p0 = BigInteger.Parse("08CB91E82A3386D280F5D6F7E50E641DF152F7109ED5456B412B1DA197FB71123ACD3A729901D1A71874700133107EC53", NumberStyles.HexNumber);
		private static readonly BigInteger a = BigInteger.Parse("07BC382C63D8C150C3C72080ACE05AFA0C2BEA28E4FB22787139165EFBA91F90F8AA5814A503AD4EB04A8C7DD22CE2826", NumberStyles.HexNumber);
		//private static readonly BigInteger b = BigInteger.Parse("004A8C7DD22CE28268B39B55416F0447C2FB77DE107DCD2A62E880EA53EEB62D57CB4390295DBC9943AB78696FA504C11", NumberStyles.HexNumber);
		private static readonly BigInteger x = BigInteger.Parse("01D1C64F068CF45FFA2A63A81B7C13F6B8847A3E77EF14FE3DB7FCAFE0CBD10E8E826E03436D646AAEF87B2E247D4AF1E", NumberStyles.HexNumber);
		private static readonly BigInteger y = BigInteger.Parse("08ABE1D7520F9C2A45CB1EB8E95CFD55262B70B29FEEC5864E19C054FF99129280E4646217791811142820341263C5315", NumberStyles.HexNumber);
		private static readonly BigInteger q = BigInteger.Parse("08CB91E82A3386D280F5D6F7E50E641DF152F7109ED5456B31F166E6CAC0425A7CF3AB6AF6B7FC3103B883202E9046565", NumberStyles.HexNumber);

		/// <summary>
		/// Brainpool P-384 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		public BrainpoolP384()
			: base(p0, new PointOnCurve(x, y), a, q)
		{
		}

		/// <summary>
		/// Brainpool P-384 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP384(byte[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Brainpool P-384 Elliptic Curve, as defined in RFC5639:
		/// https://datatracker.ietf.org/doc/html/rfc5639
		/// </summary>
		/// <param name="Secret">Secret.</param>
		public BrainpoolP384(uint[] Secret)
			: base(p0, new PointOnCurve(x, y), a, q, Secret)
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => "Brainpool P-384";

		/// <summary>
		/// Hash function to use in signatures.
		/// </summary>
		public override HashFunction HashFunction => HashFunction.SHA384;

	}
}
