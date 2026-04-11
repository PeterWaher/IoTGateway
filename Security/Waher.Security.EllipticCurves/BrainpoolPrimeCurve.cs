using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Elliptic curves over a prime field defined by Brainpool.
	/// </summary>
	public abstract class BrainpoolPrimeCurve : WeierstrassCurve
	{
		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by Brainpool.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">A coefficient in Elliptic Curve.</param>
		/// <param name="B">B coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		public BrainpoolPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger A,
			BigInteger B, BigInteger Order)
			: base(Prime, BasePoint, A, B, Order, 1)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by Brainpool.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">A coefficient in Elliptic Curve.</param>
		/// <param name="B">B coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Secret">Secret.</param>
		public BrainpoolPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger A,
			BigInteger B, BigInteger Order, byte[] Secret)
			: base(Prime, BasePoint, A, B, Order, 1, Secret)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by Brainpool.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">A coefficient in Elliptic Curve.</param>
		/// <param name="B">B coefficient in Elliptic Curve.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Secret">Secret.</param>
		public BrainpoolPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger A,
			BigInteger B, BigInteger Order, uint[] Secret)
			: base(Prime, BasePoint, A, B, Order, 1, Secret)
		{
		}
    }
}
