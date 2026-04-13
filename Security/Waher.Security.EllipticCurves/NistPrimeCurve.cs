using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Elliptic curves over a prime field defined by NIST.
	/// </summary>
	public abstract class NistPrimeCurve : WeierstrassCurve
	{
		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="B">B coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger B,
            BigInteger Order)
			: base(Prime, BasePoint, Prime - 3, B, Order, 1)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="B">B coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Secret">Secret.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger B, 
            BigInteger Order, byte[] Secret)
			: base(Prime, BasePoint, Prime - 3, B, Order, 1, Secret)
		{
		}
    }
}
