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

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="OrderBits">Number of bits used to encode order.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order, int OrderBits)
			: base(Prime, BasePoint, a, Order, OrderBits)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field defined by NIST.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="OrderBits">Number of bits used to encode order.</param>
		/// <param name="D">Private key.</param>
		public NistPrimeCurve(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order, int OrderBits, BigInteger D)
			: base(Prime, BasePoint, a, Order, OrderBits, D)
		{
		}
	}
}
