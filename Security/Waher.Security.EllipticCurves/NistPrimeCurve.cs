using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Elliptic curves over a prime field defined by NIST.
	/// </summary>
	public abstract class NistPrimeCurve : CurvePrimeField
	{
		private static readonly BigInteger a = new BigInteger(-3);
		private const int cofactor = 1;

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order)
			: base(Prime, BasePoint, a, Order, cofactor)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="D">Private key.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order, BigInteger D)
			: base(Prime, BasePoint, a, Order, cofactor, D)
		{
		}
	}
}
